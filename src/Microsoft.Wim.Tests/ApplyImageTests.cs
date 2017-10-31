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
            using (WimHandle wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, 1))
                {
                    WimgApi.ApplyImage(imageHandle, ApplyPath, WimApplyImageOptions.Index | WimApplyImageOptions.DisableDirectoryAcl | WimApplyImageOptions.DisableFileAcl | WimApplyImageOptions.DisableRPFix);
                }
            }

            Directory.EnumerateFiles(ApplyPath).Count().ShouldBe(TestWimFileCount);
        }

        [Test]
        public void ApplyImageTest_Abort()
        {
            using (WimHandle wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimMessageResult MessageCallback(WimMessageType messageType, object message, object userData) => messageType == WimMessageType.Process ? WimMessageResult.Abort : WimMessageResult.Done;

                WimgApi.RegisterMessageCallback(wimHandle, MessageCallback);

                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, 1))
                    {
                        WimHandle imageHandleCopy = imageHandle;

                        Should.Throw<OperationCanceledException>(() =>
                            WimgApi.ApplyImage(imageHandleCopy, ApplyPath, WimApplyImageOptions.NoApply));
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, MessageCallback);
                }
            }
        }

        [Test]
        public void ApplyImageTest_NoApply()
        {
            WimMessageResult MessageCallback(WimMessageType messageType, object message, object userData)
            {
                if (messageType == WimMessageType.SetRange)
                {
                    _noApplyFileCount = ((WimMessageSetRange) message).FileCount;
                }

                return WimMessageResult.Done;
            }

            string applyPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Apply");

            using (WimHandle wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimgApi.RegisterMessageCallback(wimHandle, MessageCallback);

                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, 1))
                    {
                        WimgApi.ApplyImage(imageHandle, applyPath, WimApplyImageOptions.NoApply);
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, MessageCallback);
                }
            }

            int fileCount = Directory.EnumerateFiles(ApplyPath).Count();

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