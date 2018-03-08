using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace CED.Config
{
    public class EventConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("events", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventConfigElementCollection),
            AddItemName = "event",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public EventConfigElementCollection Events
        {
            get => (EventConfigElementCollection) base["events"];
            set => base["events"] = value;
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
            set => this["consumers"] = value;
        }
    }

    public class ProducerConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("qualifiedClassName", IsRequired = true)]
        public string QualifiedClassName
        {
            get => (string)this["qualifiedClassName"];
            set => this["qualifiedClassName"] = value;
        }

        [ConfigurationProperty("eventName", IsRequired = true)]
        public string EventName
        {
            get => (string)this["eventName"];
            set => this["eventName"] = value;
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
        [ConfigurationProperty("qualifiedClassName", IsRequired = true)]
        public string QualifiedClassName
        {
            get => (string) this["qualifiedClassName"];
            set => this["qualifiedClassName"] = value;
        }

        [ConfigurationProperty("methodName", DefaultValue = "autowired", IsRequired = false)]
        public string MethodName
        {
            get => (string) this["methodName"];
            set => this["methodName"] = value;
        }

    }
}