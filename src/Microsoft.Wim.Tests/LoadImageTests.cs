using System;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class LoadImageTests : TestBase
    {
        [Test]
        public void DeleteImageTest()
        {
            WimgApi.DeleteImage(TestWimHandle, TestWimImageCount);

            Assert.AreEqual(TestWimImageCount - 1, WimgApi.GetImageCount(TestWimHandle));
        }

        [Test]
        public void DeleteImageTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.DeleteImage(null, 1));
        }

        [Test]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            var indexOutOfRangeException = AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 10));

            Assert.AreEqual("There is no image at index 10.", indexOutOfRangeException.Message);
        }

        [Test]
        public void DeleteImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            var indexOutOfRangeException = AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.DeleteImage(TestWimHandle, 0));

            Assert.AreEqual("There is no image at index 0.", indexOutOfRangeException.Message);
        }

        [Test]
        public void LoadImageTest()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                Assert.IsNotNull(imageHandle);

                Assert.IsFalse(imageHandle.IsInvalid);

                Assert.IsFalse(imageHandle.IsClosed);
            }
        }

        [Test]
        public void LoadImageTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.LoadImage(null, 1));
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexMinusOne()
        {
            var indexOutOfRangeException = AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, -1));

            Assert.AreEqual("There is no image at index -1.", indexOutOfRangeException.Message);
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexOutOfRange()
        {
            var indexOutOfRangeException = AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 10));

            Assert.AreEqual("There is no image at index 10.", indexOutOfRangeException.Message);
        }

        [Test]
        public void LoadImageTest_ThrowsIndexOutOfRangeException_indexZero()
        {
            var indexOutOfRangeException = AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.LoadImage(TestWimHandle, 0));

            Assert.AreEqual("There is no image at index 0.", indexOutOfRangeException.Message);
        }
    }
}