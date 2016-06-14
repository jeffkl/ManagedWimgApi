using System;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class SetBootImageTests : TestBase
    {
        [Test]
        public void SetBootImageTest()
        {
            const int bootImageIndex = TestWimImageCount;

            WimgApi.SetBootImage(TestWimHandle, bootImageIndex);

            Assert.AreEqual(bootImageIndex, WimgApi.GetAttributes(TestWimHandle).BootIndex);
        }

        [Test]
        public void SetBootImageTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetBootImage(null, 1));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_zero()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, 0));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_minusOne()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, -1));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_outOfRange()
        {
            AssertThrows<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, TestWimImageCount + 2));
        }
    }
}