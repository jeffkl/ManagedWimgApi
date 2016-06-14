using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ExtractImagePathTests : TestBase
    {
        [Test]
        public void ExtractImagePathTest()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                foreach (var file in GetImageFiles(imageHandle).Take(10))
                {
                    var fileName = Path.GetFileName(file);

                    fileName.ShouldNotBeNull();

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var destinationPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);

                    WimgApi.ExtractImagePath(imageHandle, file, destinationPath);

                    File.Exists(destinationPath).ShouldBeTrue();
                }
            }
        }

        [Test]
        public void ExtractImagePathTest_ThrowsArgumentNullException_destinationFile()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("destinationFile", () =>
                    WimgApi.ExtractImagePath(imageHandleCopy, "", null));
            }
        }

        [Test]
        public void ExtractImagePathTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ExtractImagePath(null, "", ""));
        }

        [Test]
        public void ExtractImagePathTest_ThrowsArgumentNullException_sourceFile()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                var imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("sourceFile", () =>
                    WimgApi.ExtractImagePath(imageHandleCopy, null, ""));
            }
        }

        private IEnumerable<string> GetImageFiles(WimHandle imageHandle)
        {
            var files = new List<String>();

            WimMessageCallback messageCallback = delegate(WimMessageType messageType, object message, object userData)
            {
                if (messageType == WimMessageType.FileInfo)
                {
                    var messageFileInfo = (WimMessageFileInfo)message;

                    if ((messageFileInfo.FileInfo.Attributes | FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        ((List<String>)userData).Add(messageFileInfo.Path.Replace(ApplyPath, ""));
                    }
                }

                return WimMessageResult.Done;
            };

            WimgApi.RegisterMessageCallback(TestWimHandle, messageCallback, files);

            try
            {
                WimgApi.ApplyImage(imageHandle, ApplyPath, WimApplyImageOptions.NoApply | WimApplyImageOptions.FileInfo);
            }
            catch (Win32Exception win32Exception)
            {
                if (win32Exception.NativeErrorCode != 1235)
                {
                    throw;
                }
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(TestWimHandle, messageCallback);
            }

            return files;
        }
    }
}