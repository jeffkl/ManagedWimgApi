
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class DeleteImageMountsTests : TestBase
    {
        public DeleteImageMountsTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void DeleteImageMountsTest_All()
        {
            WimgApi.DeleteImageMounts(true);
        }

        [Fact]
        public void DeleteImageMountsTest_InactiveOnly()
        {
            WimgApi.DeleteImageMounts(false);
        }
    }
}