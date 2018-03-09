using System.Linq;
using Should;
using Xunit;

namespace CED.Tests._461.Config
{
    public class EventConfigSectionTests : ConfigSectionBasedTests
    {
        [Fact]
        public void Basic()
        {
            var eventConfig = LoadEventConfigSection("Fake.config", "eventConfig");
            eventConfig.ShouldNotBeNull();

            eventConfig.ThrowOnErrors.ShouldBeFalse();

            eventConfig.Events.ShouldNotBeNull();
            eventConfig.Events.Count.ShouldEqual(1);

            var singleEventConfig = eventConfig.Events.Single();

            singleEventConfig.Producer.ShouldNotBeNull();
            singleEventConfig.Producer.EventName.ShouldEqual("testEventName");
            singleEventConfig.Producer.QualifiedClassName.ShouldEqual("testQualifiedClassName");
            singleEventConfig.Producer.FindInstance.ShouldBeTrue();

            singleEventConfig.Consumers.ShouldNotBeNull();
            singleEventConfig.Consumers.Count.ShouldEqual(1);

            var singleConsumer = singleEventConfig.Consumers.Single();
            singleConsumer.MethodName.ShouldEqual("testMethodName");
            singleConsumer.QualifiedClassName.ShouldEqual("testQualifiedClassName2");
            singleConsumer.FindInstance.ShouldBeTrue();
        }


    }
}