using NUnit.Framework;
using Shouldly;
using System;
using System.IO;

// ReSharper disable UnusedVariable

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CaptureImageTests : TestBase
    {
        private bool _captureWithCallbackCalled;
        private int _captureWithCallbackFileCount;

        [Test]
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

        [Test]
        public void CaptureImageTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.CaptureImage(TestWimHandle, null, WimCaptureImageOptions.None));
        }

        [Test]
        public void CaptureImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.CaptureImage(null, TempPath, WimCaptureImageOptions.None));
        }

        [Test]
        public void CaptureImageTest_ThrowsDirectoryNotFoundException_path()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.CaptureImage(TestWimHandle, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), WimCaptureImageOptions.None));
        }

        [Test]
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

            _captureWithCallbackFileCount.ShouldBe(TestWimFileCount);
        }

        private WimMessageResult CaptureImageWithCallbackTestCallback(WimMessageType messageType, object message, object userData)
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
            public bool WasCalled
            {
                get;
                set;
            }
        }
    }
}