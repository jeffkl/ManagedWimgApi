using System;
using System.IO;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ExportImageTests : TestBase
    {
        [Test]
        public void ExportImageTest()
        {
            var exportWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "export.wim");

            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                using (var wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Lzx))
                {
                    WimgApi.SetTemporaryPath(wimHandle, TempPath);

                    WimgApi.ExportImage(imageHandle, wimHandle, WimExportImageOptions.None);
                }
            }

            Assert.IsTrue(File.Exists(exportWimPath));

            Assert.AreNotEqual(0, new FileInfo(exportWimPath).Length);
        }

        [Test]
        public void ExportImageTest_ThrowsArgumentNullException_imageHandle()
        {
            AssertThrows<ArgumentNullException>("imageHandle", () =>
                WimgApi.ExportImage(null, null, WimExportImageOptions.None));
        }

        [Test]
        public void ExportImageTest_ThrowsArgumentNullException_wimHandle()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                AssertThrows<ArgumentNullException>("wimHandle", () =>
                    WimgApi.ExportImage(imageHandleCopy, null, WimExportImageOptions.None));
            }
        }
    }
}