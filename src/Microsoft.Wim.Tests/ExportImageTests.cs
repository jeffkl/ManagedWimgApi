using NUnit.Framework;
using Shouldly;
using System;
using System.IO;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ExportImageTests : TestBase
    {
        [Test]
        public void ExportImageTest()
        {
            string exportWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "export.wim");

            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                using (WimHandle wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Lzx))
                {
                    WimgApi.SetTemporaryPath(wimHandle, TempPath);

                    WimgApi.ExportImage(imageHandle, wimHandle, WimExportImageOptions.None);
                }
            }

            File.Exists(exportWimPath).ShouldBeTrue();

            new FileInfo(exportWimPath).Length.ShouldNotBe(0);
        }

        [Test]
        public void ExportImageTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ExportImage(null, null, WimExportImageOptions.None));
        }

        [Test]
        public void ExportImageTest_ThrowsArgumentNullException_wimHandle()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("wimHandle", () =>
                    WimgApi.ExportImage(imageHandleCopy, null, WimExportImageOptions.None));
            }
        }
    }
}