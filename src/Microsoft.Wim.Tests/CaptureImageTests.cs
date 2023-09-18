// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class CaptureImageTests : TestBase
    {
        private bool _captureWithCallbackCalled;
        private int _captureWithCallbackFileCount;

        public CaptureImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void CaptureImageTest()
        {
            using (WimHandle wimHandle = WimgApi.CreateFile(CaptureWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Xpress))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                using (WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, CapturePath, WimCaptureImageOptions.None))
                {
                    int imageCount = WimgApi.GetImageCount(wimHandle);

                    imageCount.ShouldBe(1);
                }
            }
        }

        [Fact]
        public void CaptureImageTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.CaptureImage(TestWimHandle, path: null!, WimCaptureImageOptions.None));
        }

        [Fact]
        public void CaptureImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.CaptureImage(wimHandle: null!, TempPath, WimCaptureImageOptions.None));
        }

        [Fact]
        public void CaptureImageTest_ThrowsDirectoryNotFoundException_path()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.CaptureImage(TestWimHandle, Path.Combine(TestDirectory, Guid.NewGuid().ToString()), WimCaptureImageOptions.None));
        }

        [Fact]
        public void CaptureImageWithCallbackTest()
        {
            CallbackObject userData = new CallbackObject();

            using (WimHandle wimHandle = WimgApi.CreateFile(CaptureWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Xpress))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimgApi.RegisterMessageCallback(wimHandle, CaptureImageWithCallbackTestCallback, userData);
                try
                {
                    using (WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, CapturePath, WimCaptureImageOptions.None))
                    {
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, CaptureImageWithCallbackTestCallback);
                }
            }

            _captureWithCallbackCalled.ShouldBe(true, "The callback should have been called");

            userData.WasCalled.ShouldBe(true, "The callback should have set user data");

            _captureWithCallbackFileCount.ShouldBe(TestWimTemplate.FileCount);
        }

        private WimMessageResult CaptureImageWithCallbackTestCallback(WimMessageType messageType, object message, object? userData)
        {
            _captureWithCallbackCalled = true;

            if (userData is CallbackObject callbackObject)
            {
                callbackObject.WasCalled = true;
            }

            switch (messageType)
            {
                case WimMessageType.SetRange:
                    _captureWithCallbackFileCount = ((WimMessageSetRange)message).FileCount;
                    break;
            }

            return WimMessageResult.Success;
        }

        private class CallbackObject
        {
            public bool WasCalled { get; set; }
        }
    }
}