using FluentAssertions;
using System.Linq;
using Xunit;

namespace CED.Tests.Core.Config
{
    public class EventConfigSectionTests : ConfigSectionBasedTests
    {
        [Fact]
        public void Basic()
        {
            var eventConfig = LoadEventConfigSection("Fake.config", "eventConfig");
            eventConfig.Should().NotBeNull();

            eventConfig.ThrowOnErrors.Should().BeFalse();

            eventConfig.Events.Should().NotBeNull();
            eventConfig.Events.Count.Should().Be(1);

            var singleEventConfig = eventConfig.Events.Single();

            singleEventConfig.Producer.Should().NotBeNull();
            singleEventConfig.Producer.EventName.Should().Be("testEventName");
            singleEventConfig.Producer.QualifiedClassName.Should().Be("testQualifiedClassName");
            singleEventConfig.Producer.FindInstance.Should().BeTrue();

            singleEventConfig.Consumers.Should().NotBeNull();
            singleEventConfig.Consumers.Count.Should().Be(1);

            var singleConsumer = singleEventConfig.Consumers.Single();
            singleConsumer.MethodName.Should().Be("testMethodName");
            singleConsumer.QualifiedClassName.Should().Be("testQualifiedClassName2");
            singleConsumer.FindInstance.Should().BeTrue();
        }


    }
}