// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class SetBootImageTests : TestBase
    {
        public SetBootImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void SetBootImageTest()
        {
            const int bootImageIndex = TestWimTemplate.ImageCount;

            WimgApi.SetBootImage(TestWimHandle, bootImageIndex);

            WimgApi.GetAttributes(TestWimHandle).BootIndex.ShouldBe(bootImageIndex);
        }

        [Fact]
        public void SetBootImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetBootImage(null, 1));
        }

        [Fact]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, -1));
        }

        [Fact]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_outOfRange()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, TestWimTemplate.ImageCount + 2));
        }

        [Fact]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_zero()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, 0));
        }
    }
}