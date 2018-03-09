using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CED.Config;
using CED.Logging;
using JetBrains.Annotations;

namespace CED
{
    public class EventDispatcher
    {
        private static readonly ILog Log = LogProvider.For<EventDispatcher>();

        [PublicAPI]
        public const string DefaultConfigSectionName = "eventConfig";

        private readonly List<DelegatingHook> _hooks = new List<DelegatingHook>();

        [PublicAPI]
        public EventDispatcher(Func<Type, object> resolver = null) : this(DefaultConfigSectionName, resolver)
        {}

        [PublicAPI]
        public EventDispatcher(string configSectionName, Func<Type, object> resolver = null) : this(ConfigurationManager.GetSection(configSectionName) as EventConfigSection, resolver)
        {
        }

        [PublicAPI]
        public EventDispatcher(EventConfigSection eventConfigSection, Func<Type,object> resolver = null)
        {
            if (eventConfigSection == null)
                throw new ArgumentNullException(nameof(eventConfigSection));

            LoadFromConfig(eventConfigSection, resolver);

            Log.Info($"Loaded definitions of {_hooks.Count} hooks, {_hooks.Count(h=>h.CreatedSucessfully)} have been created sucessfully.");
        }

        [PublicAPI]
        public void HookAll()
        {
            foreach (var hook in _hooks.Where(h => h.CreatedSucessfully))
            {
                if (hook.Hook())
                    Log.Debug($"{hook.DebugName} hooked sucessfully");
                else
                    Log.Warn($"{hook.DebugName} was NOT hooked");
            }
        }

        [PublicAPI]
        public void UnhookAll()
        {
            foreach (var hook in _hooks.Where(h => h.CreatedSucessfully))
            {
                if (hook.Unhook())
                    Log.Debug($"{hook.DebugName} unhooked sucessfully");
                else
                    Log.Warn($"{hook.DebugName} was NOT unhooked");
            }
        }


        private void LoadFromConfig([NotNull] EventConfigSection eventConfigSection, Func<Type, object> resolver)
        {
            // Passed resolver renders defined locator obsolete
            if (resolver == null)
                resolver = GetResolverFromDefinedLocator(eventConfigSection);

            foreach (var eventConfig in eventConfigSection.Events)
            {
                try
                {
                    // Check if there is a point to even loading this one
                    if (eventConfig.Consumers.Count == 0)
                    {
                        Log.Warn($"Event {eventConfig.Producer.QualifiedClassName}.{eventConfig.Producer.EventName} has no defined consumers. Skipping...");
                        continue;
                    }

                    var (eventInfo, producerInstance) = LoadProducer(eventConfig.Producer, resolver,
                        eventConfigSection.ThrowOnErrors);

                    if (eventInfo == null)
                        continue; // the event was not loaded correctly and exception was NOT thrown
                    

                    // we have instance (or not), event (static or not, doesn't matter). We can now start iterating consumers
                    foreach (var consumer in eventConfig.Consumers)
                    {
                        var (methodInfo, consumerInstance) =
                            LoadConsumer(consumer, resolver, eventConfigSection.ThrowOnErrors);

                        if (methodInfo == null)
                            continue; // we were unable to load consumer AND exception was not thrown

                        // we good. We can create our hook
                        _hooks.Add(new DelegatingHook(eventInfo, producerInstance, methodInfo, consumerInstance, eventConfigSection.ThrowOnErrors));
                    }
                }
                catch (Exception exception)
                {
                    var message =
                        $"Error while processing {eventConfig.Producer.QualifiedClassName}.{eventConfig.Producer.EventName} event.";
                    Log.Error(exception, message);
                    if(eventConfigSection.ThrowOnErrors)
                        throw new EventDispatcherException(message, exception);
                }
            }
        }

        [CanBeNull]
        private static Func<Type, object> GetResolverFromDefinedLocator(EventConfigSection eventConfigSection)
        {
            Func<Type, object> resolver = null;
            if (eventConfigSection.Locator != null)
            {
                var locatorType = Type.GetType(eventConfigSection.Locator.QualifiedClassName, false);

                if (locatorType != null)
                {
                    var locator = locatorType.GetMethod(eventConfigSection.Locator.MethodName,
                        BindingFlags.Public | BindingFlags.Static);

                    if (locator == null)
                    {
                        var message =
                            $"Cannot find {nameof(EventConfigSection.Locator)} method {eventConfigSection.Locator.MethodName}";
                        Log.Warn(message);
                        if (eventConfigSection.ThrowOnErrors)
                            throw new EventDispatcherException(message);
                    }
                    else
                    {
                        // Here we have a locator - some static method of some class that should turn Type into object (of that type)
                        // Let's check if we really do
                        var parameters = locator.GetParameters();
                        if (locator.ReturnType == typeof(object) && parameters.Length == 1 &&
                            parameters.Single().ParameterType == typeof(Type))
                        {
                            // Ok, this will work
                            resolver = Expression
                                .Lambda<Func<Type, object>>(Expression.Call(locator,
                                    Expression.Parameter(typeof(Type), "type"))).Compile();
                        }
                        else
                        {
                            var message =
                                $"Cannot use {eventConfigSection.Locator.QualifiedClassName}.{eventConfigSection.Locator.MethodName} as {nameof(EventConfigSection.Locator)} as it is not a f(Type)=>object method";
                            if (eventConfigSection.ThrowOnErrors)
                                throw new EventDispatcherException(message);
                        }
                    }
                }
                else
                {
                    var message =
                        $"Cannot load {nameof(EventConfigSection.Locator)} type {eventConfigSection.Locator.QualifiedClassName}";
                    Log.Warn(message);
                    if (eventConfigSection.ThrowOnErrors)
                        throw new EventDispatcherException(message);
                }
            }

            return resolver;
        }

        private static (EventInfo eventInfo, object instance) LoadProducer(ProducerConfigElement producer,
            Func<Type, object> resolver, bool throwOnErrors)
        {
            // Get the type of the class (or interface) we want to use
            var producerType = Type.GetType(producer.QualifiedClassName, false);
            if (producerType == null)
            {
                var message =
                    $"Cannot find {nameof(EventConfigElement.Producer)} type {producer.QualifiedClassName}";
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            // make sure that we actually can get what we need
            if (producer.FindInstance && resolver == null)
            {
                var message =
                    $"Producer type {producer.QualifiedClassName} has {nameof(ProducerConfigElement.FindInstance)} property set to true, however resolver was not passed and {nameof(EventConfigSection.Locator)} was not set or was not loaded correctly";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            var producerInstance = producer.FindInstance ? resolver(producerType) : null;

            // we have instance (it might be null, that's ok if the user said so. We will check later when we get the event)

            // let's find the event now
            var eventInfo = producerType.GetEvent(producer.EventName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (eventInfo == null)
            {
                var message =
                    $"Event {producer.QualifiedClassName}.{producer.EventName} cannot be loaded";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null); 
            }

            // and do the check
            if (producer.FindInstance && eventInfo.GetAddMethod().IsStatic)
            {
                var message =
                    $"Producer type {producer.QualifiedClassName} is configured incorrectly: if the event is static, {nameof(ProducerConfigElement.FindInstance)} must be false.";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            return (eventInfo, producerInstance);
        }

        private static (MethodInfo methodInfo, object instance) LoadConsumer(ConsumerConfigElement consumer, Func<Type, object> resolver, bool throwOnErrors)
        {
            // Get the type of the class (or interface) we want to use
            var consumerType = Type.GetType(consumer.QualifiedClassName, false);
            if (consumerType == null)
            {
                var message =
                    $"Cannot find {nameof(EventConfigElement.Consumers)} type {consumer.QualifiedClassName}";
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            // make sure that we actually can get what we need
            if (consumer.FindInstance && resolver == null)
            {
                var message =
                    $"Consumer type {consumer.QualifiedClassName} has {nameof(ConsumerConfigElement.FindInstance)} property set to true, however resolver was not passed and {nameof(EventConfigSection.Locator)} was not set or was not loaded correctly";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            var consumerInstance = consumer.FindInstance ? resolver(consumerType) : null;

            // let's find the handler method now
            var methodInfo = consumerType.GetMethod(consumer.MethodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (methodInfo == null)
            {
                var message =
                    $"Consumer event handler {consumer.QualifiedClassName}.{consumer.MethodName} cannot be found.";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            // do the same check - if it's static, then instance must be null
            if (consumer.FindInstance && methodInfo.IsStatic)
            {
                var message =
                    $"Consumer type {consumer.QualifiedClassName} is configured incorrectly: if the method is static, {nameof(ConsumerConfigElement.FindInstance)} must be false.";
                Log.Error(message);
                if (throwOnErrors)
                    throw new EventDispatcherException(message);

                return (null, null);
            }

            return (methodInfo, consumerInstance);
        }
    }
}