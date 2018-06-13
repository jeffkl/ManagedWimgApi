﻿using Shouldly;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class CopyFileTests : TestBase
    {
        private const string CallbackText = "The callback user data was set correctly.";
        private bool _callbackCalled;
        private string _destinationPath;

        public CopyFileTests(TestWimTemplate template)
            : base(template)
        {
            _destinationPath = Path.Combine(TestDirectory, "copy.wim");

            if (File.Exists(_destinationPath))
            {
                File.Delete(_destinationPath);
            }
        }

        [Fact]
        public void CopyFileTest()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            File.Exists(_destinationPath).ShouldBe(true);
        }

        [Fact]
        public void CopyFileTest_ThrowsArgumentNullException_destinationFile()
        {
            ShouldThrow<ArgumentNullException>("destinationFile", () =>
                WimgApi.CopyFile("", null, WimCopyFileOptions.None));
        }

        [Fact]
        public void CopyFileTest_ThrowsArgumentNullException_sourceFile()
        {
            ShouldThrow<ArgumentNullException>("sourceFile", () =>
                WimgApi.CopyFile(null, "", WimCopyFileOptions.None));
        }

        [Fact]
        public void CopyFileTest_ThrowsWin32Exception_FailIfExists()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            File.Exists(_destinationPath).ShouldBeTrue();

            Win32Exception win32Exception = Should.Throw<Win32Exception>(() =>
                WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.FailIfExists));

            win32Exception.Message.ShouldBe("The file exists");
        }

        [Fact]
        public void CopyFileWithCallbackTest()
        {
            StringBuilder stringBuilder = new StringBuilder();

            CopyFileProgressAction CopyFileProgressCallback(CopyFileProgress progress, object userData)
            {
                _callbackCalled = true;

                ((StringBuilder)userData).Append(CallbackText);

                return CopyFileProgressAction.Quiet;
            }

            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None, CopyFileProgressCallback, stringBuilder);

            _callbackCalled.ShouldBeTrue("The callback should have been called");

            stringBuilder.ToString().ShouldBe(CallbackText);
        }
    }
}