// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class ApplyImageTests : TestBase
    {
        private int _noApplyFileCount;

        public ApplyImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
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

            Directory.EnumerateFiles(ApplyPath).Count().ShouldBe(TestWimTemplate.FileCount);
        }

        [Fact]
        public void ApplyImageTest_Abort()
        {
            using (WimHandle wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimMessageResult MessageCallback(WimMessageType messageType, object message, object? userData) => messageType == WimMessageType.Process ? WimMessageResult.Abort : WimMessageResult.Done;

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

        [Fact]
        public void ApplyImageTest_NoApply()
        {
            WimMessageResult MessageCallback(WimMessageType messageType, object message, object? userData)
            {
                if (messageType == WimMessageType.SetRange)
                {
                    _noApplyFileCount = ((WimMessageSetRange)message).FileCount;
                }

                return WimMessageResult.Done;
            }

            using (WimHandle wimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                WimgApi.RegisterMessageCallback(wimHandle, MessageCallback);

                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, 1))
                    {
                        WimgApi.ApplyImage(imageHandle, ApplyPath, WimApplyImageOptions.NoApply);
                    }
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(wimHandle, MessageCallback);
                }
            }

            int fileCount = Directory.EnumerateFiles(ApplyPath).Count();

            fileCount.ShouldBe(0);

            _noApplyFileCount.ShouldBe(TestWimTemplate.FileCount);
        }

        [Fact]
        public void ApplyImageTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ApplyImage(imageHandle: null!, string.Empty, WimApplyImageOptions.None));
        }
    }
}