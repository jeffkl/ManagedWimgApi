using System;
using System.IO;
using NUnit.Framework;

// ReSharper disable UnusedVariable

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CaptureImageTests : TestBase
    {
        private bool _captureWithCallbackCalled;
        private int _captureWithCallbackFileCount;

        #region Setup/Cleanup

        #endregion Setup/Cleanup

        [Test]
        public void CaptureImageTest()
        {
            using (var wimHandle = WimgApi.CreateFile(CaptureWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Xpress))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                using (var imageHandle = WimgApi.CaptureImage(wimHandle, CapturePath, WimCaptureImageOptions.None))
                {
                    var imageCount = WimgApi.GetImageCount(wimHandle);

                    Assert.AreEqual(1, imageCount);
                }
            }
        }

        [Test]
        public void CaptureImageTest_ThrowsArgumentNullException_path()
        {
            AssertThrows<ArgumentNullException>("path", () =>
                WimgApi.CaptureImage(TestWimHandle, null, WimCaptureImageOptions.None));
        }

        [Test]
        public void CaptureImageTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.CaptureImage(null, TempPath, WimCaptureImageOptions.None));
        }

        [Test]
        public void CaptureImageTest_ThrowsDirectoryNotFoundException_path()
        {
            AssertThrows<DirectoryNotFoundException>(() =>
                WimgApi.CaptureImage(TestWimHandle, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), WimCaptureImageOptions.None));
        }

        [Test]
        public void CaptureImageWithCallbackTest()
        {
            var userData = new CallbackObject();

            using (var wimHandle = WimgApi.CreateFile(CaptureWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Xpress))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimgApi.RegisterMessageCallback(wimHandle, CaptureImageWithCallbackTestCallback, userData);
                try
                {
                    using (var imageHandle = WimgApi.CaptureImage(wimHandle, CapturePath, WimCaptureImageOptions.None))
                    {
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, CaptureImageWithCallbackTestCallback);
                }
            }

            Assert.IsTrue(_captureWithCallbackCalled, "The callback was called");

            Assert.IsTrue(userData.WasCalled, "The callback set user data");

            Assert.AreEqual(TestWimFileCount, _captureWithCallbackFileCount);
        }

        private WimMessageResult CaptureImageWithCallbackTestCallback(WimMessageType messageType, object message, object userData)
        {
            _captureWithCallbackCalled = true;

            var callbackObject = userData as CallbackObject;

            if (callbackObject != null)
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