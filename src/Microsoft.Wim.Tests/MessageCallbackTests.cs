using System;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class MessageCallbackTests : TestBase
    {
        [Test]
        public void RegisterUnregisterAndCountMessageCallbackTest_Global()
        {
            Assert.AreEqual(0, WimgApi.GetMessageCallbackCount(), "There are no registered callbacks");

            WimgApi.RegisterMessageCallback(TestMessageCallback);

            try
            {
                Assert.AreEqual(1, WimgApi.GetMessageCallbackCount(), "There is 1 registered callback");
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestMessageCallback);
            }

            Assert.AreEqual(0, WimgApi.GetMessageCallbackCount(), "The callback was successfully unregistered");
        }

        [Test]
        public void RegisterUnregisterAndCountMessageCallbackTest_ImageHandle()
        {
            Assert.AreEqual(0, WimgApi.GetMessageCallbackCount(TestWimHandle), "There are no registered callbacks");

            WimgApi.RegisterMessageCallback(TestWimHandle, TestMessageCallback);

            try
            {
                Assert.AreEqual(1, WimgApi.GetMessageCallbackCount(TestWimHandle), "There is 1 registered callback");
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestWimHandle, TestMessageCallback);
            }

            Assert.AreEqual(0, WimgApi.GetMessageCallbackCount(TestWimHandle), "The callback was successfully unregistered");
        }

        [Test]
        public void UnregisterMessageCallback_ThrowsArgumentOutOfRangeException_messageCallback()
        {
            var argumentOutOfRangeException = AssertThrows<ArgumentOutOfRangeException>("messageCallback", () =>
                WimgApi.UnregisterMessageCallback(TestMessageCallback));

            StringAssert.Contains(argumentOutOfRangeException.Message, "Message callback is not registered.");
        }

        [Test]
        public void GetMessageCallbackCount_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetMessageCallbackCount(null));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.RegisterMessageCallback(null, TestMessageCallback));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallback()
        {
            AssertThrows<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(TestWimHandle, null));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallbackGlobal()
        {
            AssertThrows<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(null));
        }

        private WimMessageResult TestMessageCallback(WimMessageType messageType, object message, object userData)
        {
            return WimMessageResult.Done;
        }
    }
}