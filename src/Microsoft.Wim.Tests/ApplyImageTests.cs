﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

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

            var fileCount = Directory.EnumerateFiles(ApplyPath).Count();

            Assert.AreEqual(TestWimFileCount, fileCount);
        }

        [Test]
        public void ApplyImageTest_Abort()
        {
            WimMessageCallback messageCallback = (messageType, message, userData) => messageType == WimMessageType.Process ? WimMessageResult.Abort : WimMessageResult.Done;

            WimgApi.RegisterMessageCallback(TestWimHandle, messageCallback);

            try
            {
                using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
                {
                    var imageHandleCopy = imageHandle;

                    AssertThrows<OperationCanceledException>(() =>
                        WimgApi.ApplyImage(imageHandleCopy, ApplyPath, WimApplyImageOptions.NoApply));
                }
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestWimHandle, messageCallback);
            }
        }

        [Test]
        public void ApplyImageTest_NoApply()
        {
            WimMessageCallback messageCallback = (messageType, message, userData) =>
            {
                if (messageType == WimMessageType.SetRange)
                {
                    _noApplyFileCount = ((WimMessageSetRange)message).FileCount;
                }

                return WimMessageResult.Done;
            };

            var applyPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Apply");

            using (var wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
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

            Assert.AreEqual(0, fileCount);

            Assert.AreEqual(TestWimFileCount, _noApplyFileCount);
        }

        [Test]
        public void ApplyImageTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.ApplyImage(null, "", WimApplyImageOptions.None));
        }
    }
}