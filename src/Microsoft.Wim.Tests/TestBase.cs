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
        private DirectoryInfo _capturePath;
        private WimHandle _testEsdHandle;
        private string _testEsdPath;
        private WimHandle _testWimHandle;
        private string _testWimPath;

        protected TestBase(TestWimTemplate template)
        {
            Template = template;
            TempPath = Directory.CreateDirectory(Path.Combine(TestDirectory, "temp")).FullName;
            ApplyPath = Directory.CreateDirectory(Path.Combine(TestDirectory, "apply")).FullName;
            MountPath = Directory.CreateDirectory(Path.Combine(TestDirectory, "mount")).FullName;

            CaptureWimPath = Path.Combine(TestDirectory, "capture.wim");
        }

        protected string ApplyPath { get; }

        protected string CapturePath
        {
            get
            {
                if (_capturePath == null)
                {
                    _capturePath = Directory.CreateDirectory(Path.Combine(TestDirectory, "capture"));

                    TestWimTemplate.CreateTestFiles(_capturePath.FullName);
                }

                return _capturePath.FullName;
            }
        }

        protected string CaptureWimPath { get; }

        protected string MountPath { get; }

        protected TestWimTemplate Template { get; }

        protected string TempPath { get; }

        protected string TestDirectory { get; } = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}")).FullName;

        protected WimHandle TestEsdHandle
        {
            get
            {
                if (_testEsdHandle == null)
                {
                    _testEsdHandle = WimgApi.CreateFile(TestEsdPath, WimFileAccess.Read | WimFileAccess.Write | WimFileAccess.Mount, WimCreationDisposition.OpenExisting, WimCreateFileOptions.Chunked, WimCompressionType.Lzms);

                    WimgApi.SetTemporaryPath(_testEsdHandle, TempPath);
                }

                return _testEsdHandle;
            }
        }

        protected string TestEsdPath
        {
            get
            {
                if (_testEsdPath == null)
                {
                    _testEsdPath = Path.Combine(TestDirectory, "test.esd");

                    File.Copy(Template.EsdFullPath, _testEsdPath);
                }

                return _testEsdPath;
            }
        }

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
                    _testWimPath = Path.Combine(TestDirectory, "test.wim");

                    File.Copy(Template.WimFullPath, _testWimPath);
                }

                return _testWimPath;
            }
        }

        public virtual void Dispose()
        {
            _testWimHandle?.Dispose();
            _testEsdHandle?.Dispose();

            Directory.Delete(TestDirectory, recursive: true);
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