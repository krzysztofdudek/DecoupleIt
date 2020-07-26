using System;
using NUnit.Framework;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    [TestFixture]
    public sealed class EventHandlerTests
    {
        [Test]
        public void ArgumentExceptionOnEventOnInvalidType_OnEmissionEventHandler()
        {
            var eventHandler = new ExampleEventOnEmissionHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent()));
        }

        [Test]
        public void ArgumentExceptionOnEventOnInvalidType_OnFailureEventHandler()
        {
            var eventHandler = new ExampleEventOnFailureHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent(), new Exception()));
        }

        [Test]
        public void ArgumentExceptionOnEventOnInvalidType_OnSuccessEventHandler()
        {
            var eventHandler = new ExampleEventOnSuccessHandler();

            Assert.ThrowsAsync<ArgumentException>(async () => await eventHandler.HandleAsync(new ExceptionCausingEvent()));
        }
    }
}
