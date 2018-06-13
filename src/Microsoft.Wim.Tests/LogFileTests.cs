using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class LogFileTests : TestBase
    {
        private string _logFilePath;

        public LogFileTests(TestWimTemplate template) : base(template)
        {
            _logFilePath = Path.Combine(TestDirectory, "test.log");

            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        public override void Dispose()
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }

            base.Dispose();
        }

        [Fact]
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

        [Fact]
        public void RegisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            ShouldThrow<ArgumentNullException>("logFile", () =>
                WimgApi.RegisterLogFile(null));
        }

        [Fact]
        public void UnregisterLogFileTest_ThrowsArgumentNullException_logFile()
        {
            ShouldThrow<ArgumentNullException>("logFile", () =>
                WimgApi.UnregisterLogFile(null));
        }
    }
}