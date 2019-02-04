// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class MessageCallbackTests : TestBase
    {
        public MessageCallbackTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void GetMessageCallbackCount_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetMessageCallbackCount(null));
        }

        [Fact]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallback()
        {
            ShouldThrow<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(TestWimHandle, null));
        }

        [Fact]
        public void RegisterMessageCallback_ThrowsArgumentNullException_messageCallbackGlobal()
        {
            ShouldThrow<ArgumentNullException>("messageCallback", () =>
                WimgApi.RegisterMessageCallback(null));
        }

        [Fact]
        public void RegisterMessageCallback_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.RegisterMessageCallback(null, TestMessageCallback));
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void UnregisterMessageCallback_ThrowsArgumentOutOfRangeException_messageCallback()
        {
            Exception argumentOutOfRangeException = ShouldThrow<ArgumentOutOfRangeException>("messageCallback", () =>
                WimgApi.UnregisterMessageCallback(TestMessageCallback));

            argumentOutOfRangeException.Message.ShouldStartWith("Message callback is not registered.");
        }

        private WimMessageResult TestMessageCallback(WimMessageType messageType, object message, object userData)
        {
            return WimMessageResult.Done;
        }
    }
}