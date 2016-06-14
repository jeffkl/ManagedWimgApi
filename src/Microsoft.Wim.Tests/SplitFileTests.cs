using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class SplitFileTests : TestBase
    {
        [Test]
        public void SplitFileTest()
        {
            var splitWims = CreateSplitWim();

            Assert.AreNotEqual(0, splitWims.Count(), "Wim was split into multiple files");
        }

        [Test]
        public void SplitFileTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(null, "file.wim", 1000));
        }

        [Test]
        public void SplitFileTest_ThrowsArgumentNullException_partPath()
        {
            AssertThrows<ArgumentNullException>("partPath", () =>
                WimgApi.SplitFile(TestWimHandle, null, 1000));
        }

        [Test]
        public void SplitFileTest_ThrowsDirectoryNotFoundException_partPath()
        {
            AssertThrows<DirectoryNotFoundException>(() =>
                WimgApi.SplitFile(TestWimHandle, Path.Combine(Guid.NewGuid().ToString(), "out.wim"), 1000));
        }

        [Test]
        public void SplitFileMinimumSizeTest()
        {
            var partPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "out.wim");

            var partSize = WimgApi.SplitFile(TestWimHandle, partPath);

            Assert.AreNotEqual(0, partSize, "WingApi returned a valid minimum size for split files");
        }

        [Test]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.SplitFile(null, "file.wim"));
        }

        [Test]
        public void SplitFileMinimumSizeTest_ThrowsArgumentNullException_partPath()
        {
            AssertThrows<ArgumentNullException>("partPath", () =>
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
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetReferenceFile(null, null, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Test]
        public void SetReferenceFileTest_ThrowsArgumentNullException_path()
        {
            AssertThrows<ArgumentNullException>("path", () =>
                WimgApi.SetReferenceFile(TestWimHandle, null, WimSetReferenceMode.Append, WimSetReferenceOptions.None));
        }

        [Test]
        public void SetReferenceFileTest_ThrowsFileNotFoundException_path()
        {
            AssertThrows<FileNotFoundException>(() =>
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