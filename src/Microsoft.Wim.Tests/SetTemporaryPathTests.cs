using System;
using System.IO;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class SetTemporaryPathTests : TestBase
    {
        [Test]
        public void SetTemporaryPathTest()
        {
            WimgApi.SetTemporaryPath(TestWimHandle, TempPath);
        }

        [Test]
        public void SetTemporaryPathTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.SetTemporaryPath(TestWimHandle, null));
        }

        [Test]
        public void SetTemporaryPathTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetTemporaryPath(null, ""));
        }

        [Test]
        public void SetTemporaryPathTest_ThrowsDirectoryNotFoundException_pathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.SetTemporaryPath(TestWimHandle, Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString())));
        }
    }
}