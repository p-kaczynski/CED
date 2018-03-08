using System.Configuration;
using System.Linq;
using CED.Config;
using Should;
using Xunit;

namespace CED.Tests._461.Config
{
    public class EventConfigSectionTests
    {
        [Fact]
        public void Basic()
        {
            var eventConfig = ConfigurationManager.GetSection("eventConfig") as EventConfigSection;
            eventConfig.ShouldNotBeNull();

            eventConfig.Events.ShouldNotBeNull();
            eventConfig.Events.Count.ShouldEqual(1);

            var singleEventConfig = eventConfig.Events.Single();

            singleEventConfig.Producer.ShouldNotBeNull();
            singleEventConfig.Producer.EventName.ShouldEqual("testEventName");
            singleEventConfig.Producer.QualifiedClassName.ShouldEqual("testQualifiedClassName");

            singleEventConfig.Consumers.ShouldNotBeNull();
            singleEventConfig.Consumers.Count.ShouldEqual(1);

            var singleConsumer = singleEventConfig.Consumers.Single();
            singleConsumer.MethodName.ShouldEqual("testMethodName");
            singleConsumer.QualifiedClassName.ShouldEqual("testQualifiedClassName2");
        }
    }
}