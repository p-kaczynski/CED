using System.Collections.Generic;
using System.Configuration;
using JetBrains.Annotations;

namespace CED.Config
{
    [UsedImplicitly]
    public class EventConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("throwOnErrors", DefaultValue = true, IsRequired = false)]
        public bool ThrowOnErrors
        {
            get => (bool) base["throwOnErrors"];
            [UsedImplicitly]
            set => base["throwOnErrors"] = value;
        }

        [ConfigurationProperty("locator", IsRequired = false)]

        public LocatorConfigElement Locator
        {
            get => (LocatorConfigElement) base["locator"];
            set => base["locator"] = value;
        }

        [ConfigurationProperty("events", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventConfigElementCollection),
            AddItemName = "event",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public EventConfigElementCollection Events
        {
            get => (EventConfigElementCollection) base["events"];
            [UsedImplicitly]
            set => base["events"] = value;
        }

        
    }

    public class LocatorConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("qualifiedClassName", IsRequired = true)]
        public string QualifiedClassName
        {
            get => (string)this["qualifiedClassName"];
            [UsedImplicitly]
            set => this["qualifiedClassName"] = value;
        }

        [ConfigurationProperty("methodName", IsRequired = true)]
        public string MethodName
        {
            get => (string)this["methodName"];
            [UsedImplicitly]
            set => this["methodName"] = value;
        }
    }

    public abstract class ConfigurationElementCollection<TElement> : ConfigurationElementCollection, IEnumerable<TElement> where TElement : ConfigurationElement{
        public new IEnumerator<TElement> GetEnumerator()
        {
            foreach (var key in BaseGetAllKeys())
            {
                yield return (TElement)BaseGet(key);
            }
        }
    }

    public class EventConfigElementCollection : ConfigurationElementCollection<EventConfigElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EventConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var producer = ((EventConfigElement) element).Producer;
            return $"{producer.QualifiedClassName}.{producer.EventName}";
        }
    }

    public class EventConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("producer", IsRequired = true)]
        public ProducerConfigElement Producer
        {
            get => (ProducerConfigElement) this["producer"];
            [UsedImplicitly]
            set => this["producer"] = value;

        }

        [ConfigurationProperty("consumers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ConsumerConfigElementCollection),
            AddItemName = "consumer",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConsumerConfigElementCollection Consumers
        {
            get => (ConsumerConfigElementCollection) this["consumers"];
            [UsedImplicitly]
            set => this["consumers"] = value;
        }
    }

    [UsedImplicitly]
    public class ProducerConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("qualifiedClassName", IsRequired = true)]
        public string QualifiedClassName
        {
            get => (string)this["qualifiedClassName"];
            [UsedImplicitly]
            set => this["qualifiedClassName"] = value;
        }

        [ConfigurationProperty("eventName", IsRequired = true)]
        public string EventName
        {
            get => (string)this["eventName"];
            [UsedImplicitly]
            set => this["eventName"] = value;
        }

        [ConfigurationProperty("findInstance", DefaultValue = true, IsRequired = false)]
        public bool FindInstance
        {
            get => (bool) this["findInstance"];
            set => this["findInstance"] = value;
        }
    }

    public class ConsumerConfigElementCollection : ConfigurationElementCollection<ConsumerConfigElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConsumerConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var consumerElement = (ConsumerConfigElement) element;
            return $"{consumerElement.QualifiedClassName}.{consumerElement.MethodName}";
        }
    }

    public class ConsumerConfigElement : ConfigurationElement
    {
        internal const string AutoWiredMethodName = "autowired";

        [ConfigurationProperty("qualifiedClassName", IsRequired = true)]
        public string QualifiedClassName
        {
            get => (string) this["qualifiedClassName"];
            [UsedImplicitly]
            set => this["qualifiedClassName"] = value;
        }

        [ConfigurationProperty("methodName", DefaultValue = AutoWiredMethodName, IsRequired = false)]
        public string MethodName
        {
            get => (string) this["methodName"];
            [UsedImplicitly]
            set => this["methodName"] = value;
        }

        [ConfigurationProperty("findInstance", DefaultValue = true, IsRequired = false)]
        public bool FindInstance
        {
            get => (bool)this["findInstance"];
            set => this["findInstance"] = value;
        }

    }
}