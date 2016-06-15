using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ApplyImageTests : TestBase
    {
        private int _noApplyFileCount;

        [Test]
        public void ApplyImageTest()
        {
            using (var wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                using (var imageHandle = WimgApi.LoadImage(wimHandle, 1))
                {
                    WimgApi.ApplyImage(imageHandle, ApplyPath, WimApplyImageOptions.Index | WimApplyImageOptions.DisableDirectoryAcl | WimApplyImageOptions.DisableFileAcl | WimApplyImageOptions.DisableRPFix);
                }
            }

            Directory.EnumerateFiles(ApplyPath).Count().ShouldBe(TestWimFileCount);
        }

        [Test]
        public void ApplyImageTest_Abort()
        {
            using (var wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimMessageCallback messageCallback = (messageType, message, userData) => messageType == WimMessageType.Process ? WimMessageResult.Abort : WimMessageResult.Done;

                WimgApi.RegisterMessageCallback(wimHandle, messageCallback);

                try
                {
                    using (var imageHandle = WimgApi.LoadImage(wimHandle, 1))
                    {
                        var imageHandleCopy = imageHandle;

                        Should.Throw<OperationCanceledException>(() =>
                            WimgApi.ApplyImage(imageHandleCopy, ApplyPath, WimApplyImageOptions.NoApply));
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, messageCallback);
                }
            }
        }

        [Test]
        public void ApplyImageTest_NoApply()
        {
            WimMessageCallback messageCallback = (messageType, message, userData) =>
            {
                if (messageType == WimMessageType.SetRange)
                {
                    _noApplyFileCount = ((WimMessageSetRange) message).FileCount;
                }

                return WimMessageResult.Done;
            };

            var applyPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Apply");

            using (var wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimgApi.RegisterMessageCallback(wimHandle, messageCallback);

                try
                {
                    using (var imageHandle = WimgApi.LoadImage(wimHandle, 1))
                    {
                        WimgApi.ApplyImage(imageHandle, applyPath, WimApplyImageOptions.NoApply);
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, messageCallback);
                }
            }

            var fileCount = Directory.EnumerateFiles(ApplyPath).Count();

            fileCount.ShouldBe(0);

            _noApplyFileCount.ShouldBe(TestWimFileCount);
        }

        [Test]
        public void ApplyImageTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ApplyImage(null, "", WimApplyImageOptions.None));
        }
    }
}