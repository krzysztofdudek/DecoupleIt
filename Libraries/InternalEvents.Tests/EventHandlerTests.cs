using System;
using Xunit;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    public sealed class EventHandlerTests
    {
        [Fact]
        public void ArgumentExceptionOnEventOnInvalidType_OnEmissionEventHandler()
        {
            var eventHandler = new ExampleEventOnEmissionHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent()));
        }

        [Fact]
        public void ArgumentExceptionOnEventOnInvalidType_OnFailureEventHandler()
        {
            var eventHandler = new ExampleEventOnFailureHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent(), new Exception()));
        }

        [Fact]
        public void ArgumentExceptionOnEventOnInvalidType_OnSuccessEventHandler()
        {
            var eventHandler = new ExampleEventOnSuccessHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent()));
        }
    }
}
