using System;
using System.IO;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class LogFileTests : TestBase
    {
        private string _logFilePath;

        #region Setup/Cleanup

        [OneTimeTearDown]
        public override void Cleanup()
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }

            base.Cleanup();
        }

        [OneTimeSetUp]
        public override void Setup()
        {
            base.Setup();

            _logFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test.log");

            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        #endregion Setup/Cleanup

        [Test]
        public void RegisterLogFileTest()
        {
            WimgApi.RegisterLogFile(_logFilePath);
            try
            {
                Assert.IsTrue(File.Exists(_logFilePath), "Registered log file exists");
            }
            finally
            {
                WimgApi.UnregisterLogFile(_logFilePath);
            }
        }

        [Test]
        public void RegisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            AssertThrows<ArgumentNullException>("logFile", () =>
                WimgApi.RegisterLogFile(null));
        }

        [Test]
        public void UnregisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            AssertThrows<ArgumentNullException>("logFile", () =>
                WimgApi.UnregisterLogFile(null));
        }
    }
}