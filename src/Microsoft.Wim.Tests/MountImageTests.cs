using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class MountImageTests : TestBase
    {
        [Test]
        public void GetMountedImageHandleTest_ReadOnly()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                WimHandle actualImageHandle;

                using (var actualWimHandle = WimgApi.GetMountedImageHandle(MountPath, true, out actualImageHandle))
                {
                    try
                    {
                        actualWimHandle.ShouldNotBeNull();

                        actualImageHandle.ShouldNotBeNull();

                        var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        wimMountInfo.ShouldNotBeNull();

                        wimMountInfo.ImageIndex.ShouldBe(1);
                        wimMountInfo.MountPath.ShouldBe(MountPath);
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

        [Test]
        public void GetMountedImageHandleTest_ReadWrite()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                WimHandle actualImageHandle;

                using (var actualWimHandle = WimgApi.GetMountedImageHandle(MountPath, false, out actualImageHandle))
                {
                    try
                    {
                        actualWimHandle.ShouldNotBeNull();

                        actualImageHandle.ShouldNotBeNull();

                        var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        wimMountInfo.ShouldNotBeNull();

                        wimMountInfo.ImageIndex.ShouldBe(1);
                        wimMountInfo.MountPath.ShouldBe(MountPath);
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

        [Test]
        public void GetMountedImageHandleTest_ThrowsArgumentNullException_mountPath()
        {
            WimHandle wimHandle;

            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.GetMountedImageHandle(null, false, out wimHandle));
        }

        [Test]
        public void GetMountedImageInfoFromHandleTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ShouldNotBeNull();

                wimMountInfo.ImageIndex.ShouldBe(1);
                wimMountInfo.MountPath.ShouldBe(MountPath);
                wimMountInfo.Path.ShouldBe(TestWimPath);
                wimMountInfo.ReadOnly.ShouldBeTrue();
                wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
            });
        }

        [Test]
        public void GetMountedImageInfoFromHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.GetMountedImageInfoFromHandle(null));
        }

        [Test]
        public void GetMountedImageInfoTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                var wimMountInfos = WimgApi.GetMountedImageInfo();

                wimMountInfos.ShouldNotBeNull();

                wimMountInfos.Count.ShouldBe(1);

                var wimMountInfo = wimMountInfos.FirstOrDefault();

                wimMountInfo.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                wimMountInfo.ImageIndex.ShouldBe(1);
                wimMountInfo.MountPath.ShouldBe(MountPath);
                wimMountInfo.Path.ShouldBe(TestWimPath);
                wimMountInfo.ReadOnly.ShouldBeTrue();
                wimMountInfo.State.ShouldBe(WimMountPointState.Mounted);
            });
        }

        [Test]
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

                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ReadOnly.ShouldBe(readOnly);
            });
        }

        [Test]
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

                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                wimMountInfo.ReadOnly.ShouldBe(readOnly);
            });
        }

        [Test]
        public void MountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.MountImage(null, MountPath, WimMountImageOptions.None));
        }

        [Test]
        public void MountImageHandleTest_ThrowsArgumentNullException_mountPath()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("mountPath", () =>
                    WimgApi.MountImage(imageHandleCopy, null, WimMountImageOptions.None));
            }
        }

        [Test]
        public void MountImageHandleTest_ThrowsDirectoryNotFoundException_mountPath()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                Should.Throw<DirectoryNotFoundException>(() =>
                    WimgApi.MountImage(imageHandleCopy, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), WimMountImageOptions.None));
            }
        }

        [Test]
        public void MountImageTest()
        {
            WimgApi.MountImage(MountPath, TestWimPath, 1);
            try
            {
                var mountedImages = WimgApi.GetMountedImageInfo();

                mountedImages.Count.ShouldBe(1);
            }
            finally
            {
                WimgApi.UnmountImage(MountPath, TestWimPath, 1, false);
            }
        }

        [Test]
        public void MountImageTest_ThrowsArgumentNullException_imagePath()
        {
            ShouldThrow<ArgumentNullException>("imagePath", () =>
                WimgApi.MountImage(MountPath, null, 1));
        }

        [Test]
        public void MountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.MountImage(null, TestWimPath, 1));
        }

        [Test]
        public void MountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.MountImage(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), TestWimPath, 1));
        }

        [Test]
        public void MountImageTest_ThrowsFileNotFoundException_imagePathDoesNotExist()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.MountImage(MountPath, "NonExistentFile.wim", 1));
        }

        [Test]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 0));
        }

        [Test]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, -1));
        }

        [Test]
        public void MountImageTest_ThrowsWin32Exception_imageIndexOutOfRange()
        {
            var invalidParameterException = Should.Throw<Win32Exception>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 10));

            invalidParameterException.Message.ShouldBe("The parameter is incorrect");
        }

        [Test]
        public void UnmountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.UnmountImage(null));
        }

        [Test]
        public void UnmountImageTest_ThrowsArgumentNullException_imagePath()
        {
            ShouldThrow<ArgumentNullException>("imagePath", () =>
                WimgApi.UnmountImage("", null, 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.UnmountImage(null, "", 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.UnmountImage(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), "", 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsFileNotFoundExceptionException_imagePathDoesNotExist()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.UnmountImage(MountPath, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.UnmountImage("", "", 0, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.UnmountImage("", "", -1, false));
        }

        internal static void ExecuteAgainstMountedImage(string imagePath, string mountPath, string tempPath, Action<WimHandle, WimHandle> action)
        {
            ExecuteAgainstMountedImage(imagePath, mountPath, tempPath, true, action);
        }

        internal static void ExecuteAgainstMountedImage(string imagePath, string mountPath, string tempPath, bool readOnly, Action<WimHandle, WimHandle> action)
        {
            using (var wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, tempPath);

                using (var imageHandle = WimgApi.LoadImage(wimHandle, 1))
                {
                    var flags = WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix;

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