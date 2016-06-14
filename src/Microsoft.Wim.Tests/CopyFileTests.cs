using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Microsoft.Win32;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CopyFileTests : TestBase
    {
        private const string CallbackText = "The callback user data was set correctly.";
        private bool _callbackCalled;
        private string _destinationPath;

        #region Setup/Cleanup

        [OneTimeSetUp]
        public override void Setup()
        {
            base.Setup();

            _destinationPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "copy.wim");

            if (File.Exists(_destinationPath))
            {
                File.Delete(_destinationPath);
            }
        }

        #endregion Setup/Cleanup

        [Test]
        public void CopyFileTest()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            Assert.IsTrue(File.Exists(_destinationPath), "File exists '{0}'", _destinationPath);
        }

        [Test]
        public void CopyFileTest_ThrowsArgumentNullException_destinationFile()
        {
            AssertThrows<ArgumentNullException>("destinationFile", () =>
                WimgApi.CopyFile("", null, WimCopyFileOptions.None));
        }

        [Test]
        public void CopyFileTest_ThrowsArgumentNullException_sourceFile()
        {
            AssertThrows<ArgumentNullException>("sourceFile", () =>
                WimgApi.CopyFile(null, "", WimCopyFileOptions.None));
        }

        [Test]
        public void CopyFileTest_ThrowsWin32Exception_FailIfExists()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            Assert.IsTrue(File.Exists(_destinationPath), "File exists '{0}'", _destinationPath);

            var win32Exception = AssertThrows<Win32Exception>(() =>
                WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.FailIfExists));

            Assert.AreEqual("The file exists", win32Exception.Message);
        }

        [Test]
        public void CopyFileWithCallbackTest()
        {
            var stringBuilder = new StringBuilder();

            CopyFileProgressCallback copyFileProgressCallback = delegate(CopyFileProgress progress, object userData)
            {
                _callbackCalled = true;

                ((StringBuilder)userData).Append(CallbackText);

                return CopyFileProgressAction.Quiet;
            };

            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None, copyFileProgressCallback, stringBuilder);

            Assert.IsTrue(_callbackCalled, "The callback was called");

            Assert.AreEqual(CallbackText, stringBuilder.ToString());
        }
    }
}