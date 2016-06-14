using System;
using System.IO;
using NUnit.Framework;

// ReSharper disable UnusedVariable

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CreateFileTests : TestBase
    {
        #region Setup/Cleanup

        [OneTimeSetUp]
        public override void Setup()
        {
            base.Setup();

            CreateWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "create.wim");
        }

        #endregion Setup/Cleanup

        protected string CreateWimPath
        {
            get;
            private set;
        }

        [Test]
        public void CreateFileTest()
        {
            using (var wimHandle = WimgApi.CreateFile(CreateWimPath, WimFileAccess.Write, WimCreationDisposition.CreateAlways, WimCreateFileOptions.None, WimCompressionType.Xpress))
            {
            }
        }

        [Test]
        public void CreateFileTest_ThrowsArgumentNullException_path()
        {
            AssertThrows<ArgumentNullException>("path", () =>
                WimgApi.CreateFile(null, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None));
        }
    }
}