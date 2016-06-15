using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class DeleteImageMountsTests : TestBase
    {
        [Test]
        public void DeleteImageMountsTest_All()
        {
            WimgApi.DeleteImageMounts(true);
        }

        [Test]
        public void DeleteImageMountsTest_InactiveOnly()
        {
            WimgApi.DeleteImageMounts(false);
        }
    }
}