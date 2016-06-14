using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class SplitFileTests : TestBase
    {
        [Test]
        public void SplitFileTest()
        {
            var splitWims = CreateSplitWim();

            splitWims.Count().ShouldNotBe(0);
        }

        [Test]
        public void SplitFileTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(null, "file.wim", 1000));
        }

        [Test]
        public void SplitFileTest_ThrowsArgumentNullException_partPath()
        {
            ShouldThrow<ArgumentNullException>("partPath", () =>
                WimgApi.SplitFile(TestWimHandle, null, 1000));
        }

        [Test]
        public void SplitFileTest_ThrowsDirectoryNotFoundException_partPath()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.SplitFile(TestWimHandle, Path.Combine(Guid.NewGuid().ToString(), "out.wim"), 1000));
        }

        [Test]
        public void SplitFileMinimumSizeTest()
        {
            var partPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "out.wim");

            var partSize = WimgApi.SplitFile(TestWimHandle, partPath);

            partSize.ShouldNotBe(0);
        }

        [Test]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(null, "file.wim"));
        }

        [Test]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_partPath()
        {
            ShouldThrow<ArgumentNullException>("partPath", () =>
                WimgApi.SplitFile(TestWimHandle, null));
        }

        [Test]
        public void SetReferenceFileTest()
        {
            var splitWims = CreateSplitWim().ToArray();

            using (var wimHandle = WimgApi.CreateFile(splitWims[0], WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                foreach (var referenceFile in splitWims.Skip(1))
                {
                    WimgApi.SetReferenceFile(wimHandle, referenceFile, WimSetReferenceMode.Append, WimSetReferenceOptions.None);
                }
            }
        }

        [Test]
        public void SetReferenceFileTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetReferenceFile(null, null, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Test]
        public void SetReferenceFileTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.SetReferenceFile(TestWimHandle, null, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Test]
        public void SetReferenceFileTest_ThrowsFileNotFoundException_path()
        {
            Should.Throw<FileNotFoundException>(() =>
                WimgApi.SetReferenceFile(TestWimHandle, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString()), WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        private IEnumerable<string> CreateSplitWim()
        {
            var partPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Split");

            if (Directory.Exists(partPath))
            {
                Directory.Delete(partPath, true);
            }

            Directory.CreateDirectory(partPath);

            var partSize = new FileInfo(TestWimPath).Length / 5;

            WimgApi.SplitFile(TestWimHandle, Path.Combine(partPath, "split.wim"), partSize);

            return Directory.EnumerateFiles(partPath, "split*.wim");
        }
    }
}