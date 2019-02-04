// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class LoadImageTests : TestBase
    {
        public LoadImageTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void DeleteImageTest()
        {
            WimgApi.DeleteImage(TestWimHandle, TestWimTemplate.ImageCount);

            WimgApi.GetImageCount(TestWimHandle).ShouldBe(TestWimTemplate.ImageCount - 1);
        }

        [Fact]
        public void DeleteImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.DeleteImage(null, 1));
        }

        [Fact]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            IndexOutOfRangeException indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 10));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 10.");
        }

        [Fact]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            IndexOutOfRangeException indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 0));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 0.");
        }

        [Fact]
        public void LoadImageTest()
        {
            using (WimHandle imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                imageHandle.ShouldNotBeNull();

                imageHandle.IsInvalid.ShouldBeFalse();

                imageHandle.IsClosed.ShouldBeFalse();
            }
        }

        [Fact]
        public void LoadImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.LoadImage(null, 1));
        }

        [Fact]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexMinusOne()
        {
            IndexOutOfRangeException indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, -1));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index -1.");
        }

        [Fact]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            IndexOutOfRangeException indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 10));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 10.");
        }

        [Fact]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            IndexOutOfRangeException indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 0));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 0.");
        }
    }
}