using System;
using NUnit.Framework;

// ReSharper disable UnusedVariable

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CommitImageHandleTests : TestBase
    {
        [Test]
        public void CommitImageHandleTest()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix);

                try
                {
                    using (var newImageHandle = WimgApi.CommitImageHandle(imageHandle, false, WimCommitImageOptions.DisableDirectoryAcl | WimCommitImageOptions.DisableFileAcl | WimCommitImageOptions.DisableRPFix))
                    {
                        Assert.AreEqual(TestWimImageCount, WimgApi.GetImageCount(TestWimHandle));
                    }
                }
                finally
                {
                    WimgApi.UnmountImage(imageHandle);
                }
            }
        }

        [Test]
        public void CommitImageHandleTest_Append()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix);

                try
                {
                    using (var newImageHandle = WimgApi.CommitImageHandle(imageHandle, true, WimCommitImageOptions.DisableDirectoryAcl | WimCommitImageOptions.DisableFileAcl | WimCommitImageOptions.DisableRPFix))
                    {
                        Assert.AreEqual(TestWimImageCount + 1, WimgApi.GetImageCount(TestWimHandle));
                    }
                }
                finally
                {
                    WimgApi.UnmountImage(imageHandle);
                }
            }
        }

        [Test]
        public void CommitImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.CommitImageHandle(null, false, WimCommitImageOptions.None));
        }
    }
}