// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class CommitImageHandleTests : TestBase
    {
        public CommitImageHandleTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void CommitImageHandleTest()
        {
            using WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1);
            WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix);

            try
            {
                using (WimgApi.CommitImageHandle(imageHandle, false, WimCommitImageOptions.DisableDirectoryAcl | WimCommitImageOptions.DisableFileAcl | WimCommitImageOptions.DisableRPFix))
                {
                    WimgApi.GetImageCount(TestWimHandle).ShouldBe(TestWimTemplate.ImageCount);
                }
            }
            finally
            {
                WimgApi.UnmountImage(imageHandle);
            }
        }

        [Fact]
        public void CommitImageHandleTest_Append()
        {
            using WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1);
            WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix);

            try
            {
                using (WimgApi.CommitImageHandle(imageHandle, true, WimCommitImageOptions.DisableDirectoryAcl | WimCommitImageOptions.DisableFileAcl | WimCommitImageOptions.DisableRPFix))
                {
                    WimgApi.GetImageCount(TestWimHandle).ShouldBe(TestWimTemplate.ImageCount + 1);
                }
            }
            finally
            {
                WimgApi.UnmountImage(imageHandle);
            }
        }

        [Fact]
        public void CommitImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.CommitImageHandle(null!, false, WimCommitImageOptions.None));
        }
    }
}