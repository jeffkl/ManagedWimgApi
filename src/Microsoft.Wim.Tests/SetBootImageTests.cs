using NUnit.Framework;
using Shouldly;
using System;

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

            WimgApi.GetAttributes(TestWimHandle).BootIndex.ShouldBe(bootImageIndex);
        }

        [Test]
        public void SetBootImageTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetBootImage(null, 1));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_minusOne()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, -1));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_outOfRange()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, TestWimImageCount + 2));
        }

        [Test]
        public void SetBootImageTest_ThrowsIndexOutOfRangeException_zero()
        {
            Should.Throw<IndexOutOfRangeException>(() =>
                WimgApi.SetBootImage(TestWimHandle, 0));
        }
    }
}