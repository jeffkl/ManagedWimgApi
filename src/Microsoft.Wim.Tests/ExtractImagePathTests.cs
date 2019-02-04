// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class ExtractImagePathTests : TestBase
    {
        public ExtractImagePathTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void ExtractImagePathTest()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                foreach (string file in GetImageFiles(imageHandle).Take(10))
                {
                    string fileName = Path.GetFileName(file);

                    fileName.ShouldNotBeNull();

                    // ReSharper disable once AssignNullToNotNullAttribute
                    string destinationPath = Path.Combine(TestDirectory, fileName);

                    WimgApi.ExtractImagePath(imageHandle, file, destinationPath);

                    File.Exists(destinationPath).ShouldBeTrue();
                }
            }
        }

        [Fact]
        public void ExtractImagePathTest_ThrowsArgumentNullException_destinationFile()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("destinationFile", () =>
                    WimgApi.ExtractImagePath(imageHandleCopy, string.Empty, null));
            }
        }

        [Fact]
        public void ExtractImagePathTest_ThrowsArgumentNullException_imageHandle()
        {
            ShouldThrow<ArgumentNullException>("imageHandle", () =>
                WimgApi.ExtractImagePath(null, string.Empty, string.Empty));
        }

        [Fact]
        public void ExtractImagePathTest_ThrowsArgumentNullException_sourceFile()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimHandle imageHandleCopy = imageHandle;

                ShouldThrow<ArgumentNullException>("sourceFile", () =>
                    WimgApi.ExtractImagePath(imageHandleCopy, null, string.Empty));
            }
        }

        private IEnumerable<string> GetImageFiles(WimHandle imageHandle)
        {
            List<string> files = new List<String>();

            WimMessageResult MessageCallback(WimMessageType messageType, object message, object userData)
            {
                if (messageType == WimMessageType.FileInfo)
                {
                    WimMessageFileInfo messageFileInfo = (WimMessageFileInfo)message;

                    if ((messageFileInfo.FileInfo.Attributes | FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        ((List<String>)userData).Add(messageFileInfo.Path.Replace(ApplyPath, string.Empty));
                    }
                }

                return WimMessageResult.Done;
            }

            WimgApi.RegisterMessageCallback(TestWimHandle, MessageCallback, files);

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
                WimgApi.UnregisterMessageCallback(TestWimHandle, MessageCallback);
            }

            return files;
        }
    }
}