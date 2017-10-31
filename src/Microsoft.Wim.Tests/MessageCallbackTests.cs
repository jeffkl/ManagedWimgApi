using System;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class MessageCallbackTests : TestBase
    {
        [Test]
        public void RegisterUnregisterAndCountMessageCallbackTest_Global()
        {
            WimgApi.GetMessageCallbackCount().ShouldBe(0);

            WimgApi.RegisterMessageCallback(TestMessageCallback);

            try
            {
                WimgApi.GetMessageCallbackCount().ShouldBe(1);
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestMessageCallback);
            }

            WimgApi.GetMessageCallbackCount().ShouldBe(0);
        }

        [Test]
        public void RegisterUnregisterAndCountMessageCallbackTest_ImageHandle()
        {
            WimgApi.GetMessageCallbackCount().ShouldBe(0);

            WimgApi.RegisterMessageCallback(TestWimHandle, TestMessageCallback);

            try
            {
                WimgApi.GetMessageCallbackCount(TestWimHandle).ShouldBe(1);
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestWimHandle, TestMessageCallback);
            }

            WimgApi.GetMessageCallbackCount().ShouldBe(0);
        }

        [Test]
        public void UnregisterMessageCallback_ThrowsArgumentOutOfRangeException_messageCallback()
        {
            Exception argumentOutOfRangeException = ShouldThrow<ArgumentOutOfRangeException>("messageCallback", () =>
                WimgApi.UnregisterMessageCallback(TestMessageCallback));

            argumentOutOfRangeException.Message.ShouldStartWith("Message callback is not registered.");
        }

        [Test]
        public void GetMessageCallbackCount_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetMessageCallbackCount(null));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.RegisterMessageCallback(null, TestMessageCallback));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallback()
        {
            ShouldThrow<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(TestWimHandle, null));
        }

        [Test]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallbackGlobal()
        {
            ShouldThrow<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(null));
        }

        private WimMessageResult TestMessageCallback(WimMessageType messageType, object message, object userData)
        {
            return WimMessageResult.Done;
        }
    }
}