using Xunit;

namespace Microsoft.Wim.Tests
{
    [CollectionDefinition(nameof(TestWimTemplate))]
    public class TestWimTemplateCollectionFixture : ICollectionFixture<TestWimTemplate>
    {
    }
}