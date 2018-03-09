using System;
using System.Diagnostics.CodeAnalysis;
using CED.Tests._461.Config;
using Should;
using Xunit;

namespace CED.Tests._461
{
    public class EventDispatcherTests : ConfigSectionBasedTests
    {
        [Fact]
        public void Basic()
        {
            EnableLogging();
            // prepare field
            var p = new Producer();
            var c = new Consumer();

            object Resolver(Type t)
            {
                if (t == typeof(Consumer)) return c;
                if (t == typeof(Producer)) return p;
                return null;
            }

            // load config
            var config = LoadEventConfigSection("Hook.config", "eventConfig");
            config.ShouldNotBeNull();

            // create dispatcher
            var dispatcher = new EventDispatcher(config, Resolver);
            dispatcher.HookAll();

            c.NoDataTriggered.ShouldBeFalse();
            p.OnNoDataEvent();
            c.NoDataTriggered.ShouldBeTrue();

            c.IntTriggered.ShouldBeFalse();
            p.OnIntEvent(1);
            c.IntTriggered.ShouldBeTrue();

            Consumer.StaticNoDataTriggered.ShouldBeFalse();
            Producer.OnStaticNoDataEvent();
            Consumer.StaticNoDataTriggered.ShouldBeTrue();

            Consumer.StaticIntTriggered.ShouldBeFalse();
            Producer.OnStaticIntEvent(1);
            Consumer.StaticIntTriggered.ShouldBeTrue();
        }
    }

    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global", Justification = "That's the point ;)")]
    internal class Producer
    {
        public event EventHandler NoDataEvent;
        public event EventHandler<int> IntEvent;

        public static event EventHandler StaticNoDataEvent;
        public static event EventHandler<int> StaticIntEvent;

        public void OnNoDataEvent()
        {
            NoDataEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OnIntEvent(int e)
        {
            IntEvent?.Invoke(this, e);
        }

        public static void OnStaticNoDataEvent()
        {
            StaticNoDataEvent?.Invoke(null, EventArgs.Empty);
        }

        public static void OnStaticIntEvent(int e)
        {
            StaticIntEvent?.Invoke(null, e);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "That's the point ;)")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification="Unused parameters are fine, this is just test code")]
    internal class Consumer
    {
        public bool NoDataTriggered { get; private set; }
        public bool IntTriggered { get; private set; }


        public static bool StaticNoDataTriggered { get; private set; }
        public static bool StaticIntTriggered { get; private set; }

        public void TriggerNoData(object sender, EventArgs args) => NoDataTriggered = true;
        public void TriggerInt(object sender, int i) => IntTriggered = true;

        public static void StaticTriggerNoData(object sender, EventArgs args) => StaticNoDataTriggered = true;
        public static void StaticTriggerInt(object sender, int i) => StaticIntTriggered = true;
    }
}