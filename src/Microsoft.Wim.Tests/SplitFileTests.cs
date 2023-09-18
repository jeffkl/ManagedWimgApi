// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class SplitFileTests : TestBase
    {
        public SplitFileTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void SetReferenceFileTest()
        {
            string[] splitWims = CreateSplitWim().ToArray();

            using (WimHandle wimHandle = WimgApi.CreateFile(splitWims[0], WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                foreach (string referenceFile in splitWims.Skip(1))
                {
                    WimgApi.SetReferenceFile(wimHandle, referenceFile, WimSetReferenceMode.Append, WimSetReferenceOptions.None);
                }
            }
        }

        [Fact]
        public void SetReferenceFileTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.SetReferenceFile(TestWimHandle, path: null!, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Fact]
        public void SetReferenceFileTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetReferenceFile(wimHandle: null!, path: null!, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Fact]
        public void SetReferenceFileTest_ThrowsFileNotFoundException_path()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.SetReferenceFile(TestWimHandle, Path.Combine(TestDirectory, Guid.NewGuid().ToString()), WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Fact]
        public void SplitFileMinimumSizeTest()
        {
            string partPath = Path.Combine(TestDirectory, "out.wim");

            long partSize = WimgApi.SplitFile(TestWimHandle, partPath);

            partSize.ShouldNotBe(0);
        }

        [Fact]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_partPath()
        {
            ShouldThrow<ArgumentNullException>("partPath", () =>
                WimgApi.SplitFile(TestWimHandle, partPath: null!));
        }

        [Fact]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(wimHandle: null!, partPath: "file.wim"));
        }

        [Fact]
        public void SplitFileTest()
        {
            IEnumerable<string> splitWims = CreateSplitWim();

            splitWims.Count().ShouldNotBe(0);
        }

        [Fact]
        public void SplitFileTest_ThrowsArgumentNullException_partPath()
        {
            ShouldThrow<ArgumentNullException>("partPath", () =>
                WimgApi.SplitFile(TestWimHandle, partPath: null!, partSize: 1000));
        }

        [Fact]
        public void SplitFileTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(wimHandle: null!, partPath: "file.wim", partSize: 1000));
        }

        [Fact]
        public void SplitFileTest_ThrowsDirectoryNotFoundException_partPath()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.SplitFile(TestWimHandle, Path.Combine(Guid.NewGuid().ToString(), "out.wim"), 1000));
        }

        private IEnumerable<string> CreateSplitWim()
        {
            string partPath = Path.Combine(TestDirectory, "Split");

            if (Directory.Exists(partPath))
            {
                Directory.Delete(partPath, true);
            }

            Directory.CreateDirectory(partPath);

            long partSize = new FileInfo(TestWimPath).Length / 5;

            WimgApi.SplitFile(TestWimHandle, Path.Combine(partPath, "split.wim"), partSize);

            return Directory.EnumerateFiles(partPath, "split*.wim");
        }
    }
}