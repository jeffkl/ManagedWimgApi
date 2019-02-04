// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Wim.Tests
{
    [Collection(nameof(TestWimTemplate))]
    public abstract class TestBase : IDisposable
    {
        private readonly string _testDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}")).FullName;
        private string _capturePath;
        private WimHandle _testWimHandle;
        private string _testWimPath;

        protected TestBase(TestWimTemplate template)
        {
            Template = template;
            TempPath = Directory.CreateDirectory(Path.Combine(_testDirectory, "temp")).FullName;
            ApplyPath = Directory.CreateDirectory(Path.Combine(_testDirectory, "apply")).FullName;
            MountPath = Directory.CreateDirectory(Path.Combine(_testDirectory, "mount")).FullName;

            CaptureWimPath = Path.Combine(_testDirectory, "capture.wim");
        }

        protected string ApplyPath { get; }

        protected string CapturePath
        {
            get
            {
                if (_capturePath == null)
                {
                    _capturePath = Directory.CreateDirectory(Path.Combine(_testDirectory, "capture")).FullName;

                    TestWimTemplate.CreateTestFiles(_capturePath, TestWimTemplate.FileCount, TestWimTemplate.FileLineCount);
                }

                return _capturePath;
            }
        }

        protected string CaptureWimPath { get; }

        protected string MountPath { get; }

        protected TestWimTemplate Template { get; }

        protected string TempPath { get; }

        protected string TestDirectory => _testDirectory;

        protected WimHandle TestWimHandle
        {
            get
            {
                if (_testWimHandle == null)
                {
                    _testWimHandle = WimgApi.CreateFile(TestWimPath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None);

                    WimgApi.SetTemporaryPath(_testWimHandle, TempPath);
                }

                return _testWimHandle;
            }
        }

        protected string TestWimPath
        {
            get
            {
                if (_testWimPath == null)
                {
                    _testWimPath = Path.Combine(_testDirectory, "test.wim");

                    File.Copy(Template.FullPath, _testWimPath);
                }

                return _testWimPath;
            }
        }

        public virtual void Dispose()
        {
            _testWimHandle?.Dispose();

            Directory.Delete(_testDirectory, recursive: true);
        }

        protected Exception ShouldThrow<T>(string paramName, Action action)
                            where T : ArgumentException
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            T exception = Should.Throw<T>(action);

            exception.ParamName.ShouldBe(paramName);

            return exception;
        }
    }
}