using System;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class LoadImageTests : TestBase
    {
        [Test]
        public void DeleteImageTest()
        {
            WimgApi.DeleteImage(TestWimHandle, TestWimImageCount);

            WimgApi.GetImageCount(TestWimHandle).ShouldBe(TestWimImageCount - 1);
        }

        [Test]
        public void DeleteImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.DeleteImage(null, 1));
        }

        [Test]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            var indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 10));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 10.");
        }

        [Test]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            var indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 0));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 0.");
        }

        [Test]
        public void LoadImageTest()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                imageHandle.ShouldNotBeNull();

                imageHandle.IsInvalid.ShouldBeFalse();

                imageHandle.IsClosed.ShouldBeFalse();
            }
        }

        [Test]
        public void LoadImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.LoadImage(null, 1));
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexMinusOne()
        {
            var indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, -1));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index -1.");
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            var indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 10));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 10.");
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            var indexOutOfRangeException = Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 0));

            indexOutOfRangeException.Message.ShouldBe("There is no image at index 0.");
        }
    }
}