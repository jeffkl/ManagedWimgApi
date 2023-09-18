// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class CreateFileTests : TestBase
    {
        public CreateFileTests(TestWimTemplate template)
            : base(template)
        {
            CreateWimPath = Path.Combine(TestDirectory, "create.wim");
        }

        protected string CreateWimPath { get; }

        [Theory]
        [InlineData(WimCompressionType.Lzx)]
        [InlineData(WimCompressionType.None)]
        [InlineData(WimCompressionType.Xpress)]
        public void CreateFileTest(WimCompressionType compressionType)
        {
            using (WimHandle wimHandle = WimgApi.CreateFile(CreateWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, compressionType))
            {
            }
        }

        [Fact]
        public void CreateFileTest_ThrowsArgumentNullException_path()
        {
            ShouldThrow<ArgumentNullException>("path", () =>
                WimgApi.CreateFile(path: null!, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None));
        }
    }
}