using System;
using System.IO;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class LogFileTests : TestBase
    {
        private string _logFilePath;

        #region Setup/Cleanup

        [TearDown]
        public override void Cleanup()
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }

            base.Cleanup();
        }

        [SetUp]
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
                File.Exists(_logFilePath).ShouldBeTrue();
            }
            finally
            {
                WimgApi.UnregisterLogFile(_logFilePath);
            }
        }

        [Test]
        public void RegisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            ShouldThrow<ArgumentNullException>("logFile", () =>
                WimgApi.RegisterLogFile(null));
        }

        [Test]
        public void UnregisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            ShouldThrow<ArgumentNullException>("logFile", () =>
                WimgApi.UnregisterLogFile(null));
        }
    }
}