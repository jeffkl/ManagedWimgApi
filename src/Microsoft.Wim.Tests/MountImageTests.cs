// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class MountImageTests : TestBase
    {
        public MountImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void GetMountedImageHandleTest_ReadOnly()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                using (WimHandle actualWimHandle = WimgApi.GetMountedImageHandle(MountPath, true, out WimHandle actualImageHandle))
                {
                    try
                    {
                        actualWimHandle.ShouldNotBeNull();

                        actualImageHandle.ShouldNotBeNull();

                        WimMountInfo wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        wimMountInfo.ShouldNotBeNull();

                        wimMountInfo.ImageIndex.ShouldBe(1);
                        wimMountInfo.MountPath.ShouldBe(MountPath, StringCompareShould.IgnoreCase);
                        wimMountInfo.Path.ShouldBe(TestWimPath);
                        wimMountInfo.ReadOnly.ShouldBeTrue();
                        wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
                    }
                    finally
                    {
                        actualImageHandle.Dispose();
                    }
                }
            });
        }

        [Fact]
        public void GetMountedImageHandleTest_ReadWrite()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                using (WimHandle actualWimHandle = WimgApi.GetMountedImageHandle(MountPath, false, out WimHandle actualImageHandle))
                {
                    try
                    {
                        actualWimHandle.ShouldNotBeNull();

                        actualImageHandle.ShouldNotBeNull();

                        WimMountInfo wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        wimMountInfo.ShouldNotBeNull();

                        wimMountInfo.ImageIndex.ShouldBe(1);
                        wimMountInfo.MountPath.ShouldBe(MountPath, StringCompareShould.IgnoreCase);
                        wimMountInfo.Path.ShouldBe(TestWimPath);
                        wimMountInfo.ReadOnly.ShouldBeTrue();
                        wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
                    }
                    finally
                    {
                        actualImageHandle.Dispose();
                    }
                }
            });
        }

        [Fact]
        public void GetMountedImageHandleTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.GetMountedImageHandle(null, false, out _));
        }

        [Fact]
        public void GetMountedImageInfoFromHandleTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                WimMountInfo wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ShouldNotBeNull();

                wimMountInfo.ImageIndex.ShouldBe(1);
                wimMountInfo.MountPath.ShouldBe(MountPath, StringCompareShould.IgnoreCase);
                wimMountInfo.Path.ShouldBe(TestWimPath);
                wimMountInfo.ReadOnly.ShouldBeTrue();
                wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
            });
        }

        [Fact]
        public void GetMountedImageInfoFromHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.GetMountedImageInfoFromHandle(null));
        }

        [Fact]
        public void GetMountedImageInfoTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                WimMountInfoCollection wimMountInfos = WimgApi.GetMountedImageInfo();

                wimMountInfos.ShouldNotBeNull();

                wimMountInfos.Count.ShouldBe(1);

                WimMountInfo wimMountInfo = wimMountInfos.FirstOrDefault();

                wimMountInfo.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                wimMountInfo.ImageIndex.ShouldBe(1);
                wimMountInfo.MountPath.ShouldBe(MountPath, StringCompareShould.IgnoreCase);
                wimMountInfo.Path.ShouldBe(TestWimPath);
                wimMountInfo.ReadOnly.ShouldBeTrue();
                wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
            });
        }

        [Fact]
        public void MountImageHandleTest_ReadOnly()
        {
            const bool readOnly = true;

            ExecuteAgainstMountedImage(readOnly, (wimHandle, imageHandle) =>
            {
                wimHandle.ShouldNotBeNull();
                wimHandle.IsInvalid.ShouldBeFalse();
                wimHandle.IsClosed.ShouldBeFalse();
                imageHandle.ShouldNotBeNull();
                imageHandle.IsClosed.ShouldBeFalse();
                imageHandle.IsInvalid.ShouldBeFalse();

                WimMountInfo wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ReadOnly.ShouldBe(readOnly);
            });
        }

        [Fact]
        public void MountImageHandleTest_ReadWrite()
        {
            const bool readOnly = false;

            ExecuteAgainstMountedImage(readOnly, (wimHandle, imageHandle) =>
            {
                wimHandle.ShouldNotBeNull();
                wimHandle.IsInvalid.ShouldBeFalse();
                wimHandle.IsClosed.ShouldBeFalse();
                imageHandle.ShouldNotBeNull();
                imageHandle.IsClosed.ShouldBeFalse();
                imageHandle.IsInvalid.ShouldBeFalse();

                WimMountInfo wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ReadOnly.ShouldBe(readOnly);
            });
        }

        [Fact]
        public void MountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.MountImage(null, MountPath, WimMountImageOptions.None));
        }

        [Fact]
        public void MountImageHandleTest_ThrowsArgumentNullException_mountPath()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("mountPath", () =>
                    WimgApi.MountImage(imageHandleCopy, null, WimMountImageOptions.None));
            }
        }

        [Fact]
        public void MountImageHandleTest_ThrowsDirectoryNotFoundException_mountPath()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                Should.Throw<DirectoryNotFoundException>(() =>
                    WimgApi.MountImage(imageHandleCopy, Path.Combine(TestDirectory, Guid.NewGuid().ToString()), WimMountImageOptions.None));
            }
        }

        [Fact]
        public void MountImageTest()
        {
            WimgApi.MountImage(MountPath, TestWimPath, 1);
            try
            {
                WimMountInfoCollection mountedImages = WimgApi.GetMountedImageInfo();

                mountedImages.Count.ShouldBe(1);
            }
            finally
            {
                WimgApi.UnmountImage(MountPath, TestWimPath, 1, false);
            }
        }

        [Fact]
        public void MountImageTest_ThrowsArgumentNullException_imagePath()
        {
            ShouldThrow<ArgumentNullException>("imagePath", () =>
                WimgApi.MountImage(MountPath, null, 1));
        }

        [Fact]
        public void MountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.MountImage(null, TestWimPath, 1));
        }

        [Fact]
        public void MountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.MountImage(Path.Combine(TestDirectory, Guid.NewGuid().ToString()), TestWimPath, 1));
        }

        [Fact]
        public void MountImageTest_ThrowsFileNotFoundException_imagePathDoesNotExist()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.MountImage(MountPath, "NonExistentFile.wim", 1));
        }

        [Fact]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 0));
        }

        [Fact]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, -1));
        }

        [Fact]
        public void MountImageTest_ThrowsWin32Exception_imageIndexOutOfRange()
        {
            Win32Exception invalidParameterException = Should.Throw<Win32Exception>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 10));

            invalidParameterException.Message.ShouldBe("The parameter is incorrect");
        }

        [Fact]
        public void UnmountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.UnmountImage(null));
        }

        [Fact]
        public void UnmountImageTest_ThrowsArgumentNullException_imagePath()
        {
            ShouldThrow<ArgumentNullException>("imagePath", () =>
                WimgApi.UnmountImage(string.Empty, null, 1, false));
        }

        [Fact]
        public void UnmountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.UnmountImage(null, string.Empty, 1, false));
        }

        [Fact]
        public void UnmountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.UnmountImage(Path.Combine(TestDirectory, Guid.NewGuid().ToString()), string.Empty, 1, false));
        }

        [Fact]
        public void UnmountImageTest_ThrowsFileNotFoundExceptionException_imagePathDoesNotExist()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.UnmountImage(MountPath, Path.Combine(TestDirectory, Guid.NewGuid().ToString()), 1, false));
        }

        [Fact]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.UnmountImage(string.Empty, string.Empty, 0, false));
        }

        [Fact]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.UnmountImage(string.Empty, string.Empty, -1, false));
        }

        internal static void ExecuteAgainstMountedImage(string imagePath, string mountPath, string tempPath, Action<WimHandle, WimHandle> action)
        {
            ExecuteAgainstMountedImage(imagePath, mountPath, tempPath, true, action);
        }

        internal static void ExecuteAgainstMountedImage(string imagePath, string mountPath, string tempPath, bool readOnly, Action<WimHandle, WimHandle> action)
        {
            using (WimHandle wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, tempPath);

                using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, 1))
                {
                    WimMountImageOptions flags = WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix;

                    if (readOnly)
                    {
                        flags |= WimMountImageOptions.ReadOnly;
                    }

                    WimgApi.MountImage(imageHandle, mountPath, flags);

                    try
                    {
                        action?.Invoke(wimHandle, imageHandle);
                    }
                    finally
                    {
                        WimgApi.UnmountImage(imageHandle);
                    }
                }
            }
        }

        private void ExecuteAgainstMountedImage(Action<WimHandle, WimHandle> action)
        {
            ExecuteAgainstMountedImage(true, action);
        }

        private void ExecuteAgainstMountedImage(bool readOnly, Action<WimHandle, WimHandle> action)
        {
            ExecuteAgainstMountedImage(TestWimPath, MountPath, TempPath, readOnly, action);
        }
    }
}