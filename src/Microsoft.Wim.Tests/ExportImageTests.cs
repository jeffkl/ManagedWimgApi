// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class ExportImageTests : TestBase
    {
        public ExportImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void ExportEsdFromWimImageTest()
        {
            string exportWimPath = Path.Combine(TestDirectory, "export.esd");

            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                using (WimHandle wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.Chunked, WimCompressionType.Lzms))
                {
                    WimgApi.SetTemporaryPath(wimHandle, TempPath);

                    WimgApi.ExportImage(imageHandle, wimHandle, WimExportImageOptions.None);
                }
            }

            File.Exists(exportWimPath).ShouldBeTrue();

            new FileInfo(exportWimPath).Length.ShouldNotBe(0);

            using (WimHandle wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.Chunked, WimCompressionType.Lzms))
            {
                WimInfo wimInfo = WimgApi.GetAttributes(wimHandle);

                wimInfo.ShouldNotBeNull();

                wimInfo.Attributes.ShouldBe(WimInfoAttributes.Normal);
                wimInfo.ImageCount.ShouldBe(1);
            }
        }

        [Fact]
        public void ExportImageTest()
        {
            string exportWimPath = Path.Combine(TestDirectory, "export.wim");

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

        [Fact]
        public void ExportImageTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ExportImage(imageHandle: null!, wimHandle: null!, WimExportImageOptions.None));
        }

        [Fact]
        public void ExportImageTest_ThrowsArgumentNullException_wimHandle()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("wimHandle", () =>
                    WimgApi.ExportImage(imageHandleCopy, wimHandle: null!, WimExportImageOptions.None));
            }
        }

        [Fact]
        public void ExportWimFromEsdImageTest()
        {
            string exportWimPath = Path.Combine(TestDirectory, "export.wim");

            using (WimHandle imageHandle = WimgApi.LoadImage(TestEsdHandle, 1))
            {
                using (WimHandle wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.None))
                {
                    WimgApi.SetTemporaryPath(wimHandle, TempPath);

                    WimgApi.ExportImage(imageHandle, wimHandle, WimExportImageOptions.None);
                }
            }

            File.Exists(exportWimPath).ShouldBeTrue();

            using (WimHandle wimHandle = WimgApi.CreateFile(exportWimPath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimInfo wimInfo = WimgApi.GetAttributes(wimHandle);

                wimInfo.ShouldNotBeNull();

                wimInfo.Attributes.ShouldBe(WimInfoAttributes.Normal);
                wimInfo.ImageCount.ShouldBe(1);
            }
        }
    }
}