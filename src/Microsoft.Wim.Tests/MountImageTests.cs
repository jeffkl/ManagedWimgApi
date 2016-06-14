using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NUnit.Framework;

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
                        Assert.IsNotNull(actualWimHandle, "GetMountedImageHandle() returned a WIM handle");

                        Assert.IsNotNull(actualImageHandle, "GetMountedImageHandle() returned an image handle");

                        var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        Assert.IsNotNull(wimMountInfo, "WimMountInfo is not null");

                        Assert.AreEqual(1, wimMountInfo.ImageIndex);
                        Assert.AreEqual(MountPath, wimMountInfo.MountPath);
                        Assert.AreEqual(TestWimPath, wimMountInfo.Path);
                        Assert.AreEqual(true, wimMountInfo.ReadOnly);
                        Assert.AreEqual(WimMountPointState.Mounted, wimMountInfo.State);
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
                        Assert.IsNotNull(actualWimHandle, "GetMountedImageHandle() returned a WIM handle");

                        Assert.IsNotNull(actualImageHandle, "GetMountedImageHandle() returned an image handle");

                        var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(actualImageHandle);

                        Assert.IsNotNull(wimMountInfo, "WimMountInfo is not null");

                        Assert.AreEqual(1, wimMountInfo.ImageIndex);
                        Assert.AreEqual(MountPath, wimMountInfo.MountPath);
                        Assert.AreEqual(TestWimPath, wimMountInfo.Path);
                        Assert.AreEqual(true, wimMountInfo.ReadOnly);
                        Assert.AreEqual(WimMountPointState.Mounted, wimMountInfo.State);
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

            AssertThrows<ArgumentNullException>("mountPath", () =>
                WimgApi.GetMountedImageHandle(null, false, out wimHandle));
        }

        [Test]
        public void GetMountedImageInfoFromHandleTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                Assert.IsNotNull(wimMountInfo, "WimMountInfo is not null");

                Assert.AreEqual(1, wimMountInfo.ImageIndex);
                Assert.AreEqual(MountPath, wimMountInfo.MountPath);
                Assert.AreEqual(TestWimPath, wimMountInfo.Path);
                Assert.AreEqual(true, wimMountInfo.ReadOnly);
                Assert.AreEqual(WimMountPointState.Mounted, wimMountInfo.State);
            });
        }

        [Test]
        public void GetMountedImageInfoFromHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.GetMountedImageInfoFromHandle(null));
        }

        [Test]
        public void GetMountedImageInfoTest()
        {
            ExecuteAgainstMountedImage((wimHandle, imageHandle) =>
            {
                var wimMountInfos = WimgApi.GetMountedImageInfo();

                Assert.IsNotNull(wimMountInfos, "GetMountedImageInfo() returned a value");

                Assert.AreEqual(1, wimMountInfos.Count, "1 image is mounted");

                var wimMountInfo = wimMountInfos.FirstOrDefault();

                Assert.IsNotNull(wimMountInfo, "WimMountInfo is not null");

                Assert.AreEqual(1, wimMountInfo.ImageIndex);
                Assert.AreEqual(MountPath, wimMountInfo.MountPath);
                Assert.AreEqual(TestWimPath, wimMountInfo.Path);
                Assert.AreEqual(true, wimMountInfo.ReadOnly);
                Assert.AreEqual(WimMountPointState.Mounted, wimMountInfo.State);
            });
        }

        [Test]
        public void MountImageHandleTest_ReadOnly()
        {
            const bool readOnly = false;

            ExecuteAgainstMountedImage(readOnly, (wimHandle, imageHandle) =>
            {
                Assert.IsNotNull(wimHandle);
                Assert.IsTrue(!wimHandle.IsInvalid);
                Assert.IsTrue(!wimHandle.IsClosed);
                Assert.IsNotNull(imageHandle);
                Assert.IsTrue(!imageHandle.IsInvalid);
                Assert.IsTrue(!imageHandle.IsClosed);

                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                Assert.AreEqual(readOnly, wimMountInfo.ReadOnly);
            });
        }

        [Test]
        public void MountImageHandleTest_ReadWrite()
        {
            const bool readOnly = false;

            ExecuteAgainstMountedImage(readOnly, (wimHandle, imageHandle) =>
            {
                Assert.IsNotNull(wimHandle);
                Assert.IsTrue(!wimHandle.IsInvalid);
                Assert.IsTrue(!wimHandle.IsClosed);
                Assert.IsNotNull(imageHandle);
                Assert.IsTrue(!imageHandle.IsInvalid);
                Assert.IsTrue(!imageHandle.IsClosed);

                var wimMountInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

                Assert.AreEqual(readOnly, wimMountInfo.ReadOnly);
            });
        }

        [Test]
        public void MountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.MountImage(null, MountPath, WimMountImageOptions.None));
        }

        [Test]
        public void MountImageHandleTest_ThrowsArgumentNullException_mountPath()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                AssertThrows<ArgumentNullException>("mountPath", () =>
                    WimgApi.MountImage(imageHandleCopy, null, WimMountImageOptions.None));
            }
        }

        [Test]
        public void MountImageHandleTest_ThrowsDirectoryNotFoundException_mountPath()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                AssertThrows<DirectoryNotFoundException>(() =>
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

                Assert.AreEqual(1, mountedImages.Count);
            }
            finally
            {
                WimgApi.UnmountImage(MountPath, TestWimPath, 1, false);
            }
        }

        [Test]
        public void MountImageTest_ThrowsArgumentNullException_imagePath()
        {
            AssertThrows<ArgumentNullException>("imagePath", () =>
                WimgApi.MountImage(MountPath, null, 1));
        }

        [Test]
        public void MountImageTest_ThrowsArgumentNullException_mountPath()
        {
            AssertThrows<ArgumentNullException>("mountPath", () =>
                WimgApi.MountImage(null, TestWimPath, 1));
        }

        [Test]
        public void MountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            AssertThrows<DirectoryNotFoundException>(() =>
                WimgApi.MountImage(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), TestWimPath, 1));
        }

        [Test]
        public void MountImageTest_ThrowsFileNotFoundException_imagePathDoesNotExist()
        {
            AssertThrows<FileNotFoundException>(() =>
                WimgApi.MountImage(MountPath, "NonExistentFile.wim", 1));
        }

        [Test]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 0));
        }

        [Test]
        public void MountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, -1));
        }

        [Test]
        public void MountImageTest_ThrowsWin32Exception_imageIndexOutOfRange()
        {
            var invalidParameterException = AssertThrows<Win32Exception>(() =>
                WimgApi.MountImage(MountPath, TestWimPath, 10));

            Assert.AreEqual("The parameter is incorrect", invalidParameterException.Message);
        }

        [Test]
        public void UnmountImageHandleTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.UnmountImage(null));
        }

        [Test]
        public void UnmountImageTest_ThrowsArgumentNullException_imagePath()
        {
            AssertThrows<ArgumentNullException>("imagePath", () =>
                WimgApi.UnmountImage("", null, 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsArgumentNullException_mountPath()
        {
            AssertThrows<ArgumentNullException>("mountPath", () =>
                WimgApi.UnmountImage(null, "", 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsDirectoryNotFoundException_mountPathDoesNotExist()
        {
            AssertThrows<DirectoryNotFoundException>(() =>
                WimgApi.UnmountImage(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), "", 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsFileNotFoundExceptionException_imagePathDoesNotExist()
        {
            AssertThrows<FileNotFoundException>(() =>
                WimgApi.UnmountImage(MountPath, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), 1, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_0()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.UnmountImage("", "", 0, false));
        }

        [Test]
        public void UnmountImageTest_ThrowsIndexOutOfRangeException_imageIndex_minusOne()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
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