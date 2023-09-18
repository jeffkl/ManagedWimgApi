// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class SetTemporaryPathTests : TestBase
    {
        public SetTemporaryPathTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void SetTemporaryPathTest()
        {
            WimgApi.SetTemporaryPath(TestWimHandle, TempPath);
        }

        [Fact]
        public void SetTemporaryPathTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.SetTemporaryPath(TestWimHandle, path: null!));
        }

        [Fact]
        public void SetTemporaryPathTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetTemporaryPath(wimHandle: null!, path: string.Empty));
        }

        [Fact]
        public void SetTemporaryPathTest_ThrowsDirectoryNotFoundException_pathDoesNotExist()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.SetTemporaryPath(TestWimHandle, Path.Combine(TestDirectory, Guid.NewGuid().ToString())));
        }
    }
}