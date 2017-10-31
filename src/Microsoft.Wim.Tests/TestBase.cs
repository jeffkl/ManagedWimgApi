using System.Diagnostics.CodeAnalysis;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    public abstract class TestBase
    {
        protected const int TestWimFileCount = 100;
        protected const int TestWimFileLineCount = 1000;
        protected const int TestWimImageCount = 2;
        private const string TestWimFilename = @"test.wim";
        private const string TestWimTemplateFilename = @"test_template.wim";
        private string _applyPath;
        private string _capturePath;
        private string _captureWimPath;
        private string _mountPath;
        private string _tempPath;
        private WimHandle _testWimHandle;
        private string _testWimPath;
        private string _testWimTemplatePath;

        [TearDown]
        public virtual void Cleanup()
        {
            if (_applyPath != null && Directory.Exists(_applyPath))
            {
                Directory.Delete(_applyPath, true);
            }
        }

        [SetUp]
        public virtual void Setup()
        {
            _captureWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "capture.wim");
        }


        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            _testWimHandle?.Dispose();
        }

        public string ApplyPath
        {
            get
            {
                if (_applyPath == null)
                {
                    _applyPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Apply");
                }

                if (!Directory.Exists(_applyPath))
                {
                    Directory.CreateDirectory(_applyPath);
                }

                return _applyPath;
            }
        }

        public string CapturePath
        {
            get
            {
                if (_capturePath == null)
                {
                    _capturePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Capture");
                }

                if (!Directory.Exists(_capturePath))
                {
                    Directory.CreateDirectory(_capturePath);

                    CreateTestFiles(_capturePath, TestWimFileCount, TestWimFileLineCount);
                }

                return _capturePath;
            }
        }

        public string CaptureWimPath => _captureWimPath;

        public string MountPath
        {
            get
            {
                if (_mountPath == null)
                {
                    _mountPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Mount");
                }

                if (!Directory.Exists(_mountPath))
                {
                    Directory.CreateDirectory(_mountPath);
                }

                return _mountPath;
            }
        }

        protected string TempPath
        {
            get
            {
                if (_tempPath == null)
                {
                    _tempPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Temp");
                }

                if (!Directory.Exists(_tempPath))
                {
                    Directory.CreateDirectory(_tempPath);
                }

                return _tempPath;
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

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        protected string TestWimPath
        {
            get
            {
                if (_testWimTemplatePath == null)
                {
                    _testWimTemplatePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestWimTemplateFilename);
                }

                if (!File.Exists(_testWimTemplatePath))
                {
                    string capturePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestCapture");

                    Directory.CreateDirectory(capturePath);

                    CreateTestFiles(capturePath, TestWimFileCount, TestWimFileLineCount);

                    CaptureTestImage(_testWimTemplatePath, capturePath);

                    Directory.Delete(capturePath, true);
                }

                if (_testWimPath == null)
                {
                    _testWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestWimFilename);

                    File.Copy(_testWimTemplatePath, _testWimPath, true);

                    if (!File.Exists(_testWimPath))
                    {
                        throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not find part of the path '{0}'", _testWimPath));
                    }
                }

                return _testWimPath;
            }
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

        protected void CaptureTestImage(string imagePath, string capturePath)
        {
            if (String.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentNullException(nameof(imagePath));
            }

            if (!Directory.Exists(capturePath))
            {
                throw new DirectoryNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not find part of the path '{0}'", capturePath));
            }

            XmlDocument xmlDocument = new XmlDocument();

            using (WimHandle wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Write, WimCreationDisposition.CreateNew, WimCreateFileOptions.None, WimCompressionType.Lzx))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                for (int i = 0; i < TestWimImageCount; i++)
                {
                    // ReSharper disable once UnusedVariable
                    using (WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, capturePath, WimCaptureImageOptions.DisableDirectoryAcl | WimCaptureImageOptions.DisableFileAcl | WimCaptureImageOptions.DisableRPFix))
                    {
                    }
                }

                XPathNavigator xml = WimgApi.GetImageInformation(wimHandle).CreateNavigator();

                xml.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                xmlDocument.LoadXml(xml.OuterXml);

                XmlNodeList imageNodes = xmlDocument.SelectNodes("//WIM/IMAGE");

                imageNodes.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                foreach (XmlElement imageNode in imageNodes)
                {
                    XmlDocumentFragment fragment = xmlDocument.CreateDocumentFragment();

                    fragment.InnerXml =
                        @"<WINDOWS>
                              <ARCH>0</ARCH>
                              <PRODUCTNAME>Microsoft® Windows® Operating System</PRODUCTNAME>
                              <EDITIONID>WindowsPE</EDITIONID>
                              <INSTALLATIONTYPE>WindowsPE</INSTALLATIONTYPE>
                              <PRODUCTTYPE>WinNT</PRODUCTTYPE>
                              <PRODUCTSUITE></PRODUCTSUITE>
                              <LANGUAGES>
                                <LANGUAGE>en-US</LANGUAGE>
                                <DEFAULT>en-US</DEFAULT>
                              </LANGUAGES>
                              <VERSION>
                                <MAJOR>6</MAJOR>
                                <MINOR>3</MINOR>
                                <BUILD>9600</BUILD>
                                <SPBUILD>16384</SPBUILD>
                                <SPLEVEL>0</SPLEVEL>
                              </VERSION>
                              <SYSTEMROOT>WINDOWS</SYSTEMROOT>
                            </WINDOWS>";

                    imageNode.AppendChild(fragment);

                    fragment.InnerXml = $@"<NAME>Test Image {imageNode.Attributes["INDEX"].Value}</NAME>";

                    imageNode.AppendChild(fragment);

                    fragment.InnerXml = $@"<DESCRIPTION>Test Image {imageNode.Attributes["INDEX"].Value}</DESCRIPTION>";

                    imageNode.AppendChild(fragment);

                    WimgApi.SetImageInformation(wimHandle, xmlDocument);
                }
            }
        }

        protected void CreateTestFiles(string path, int fileCount, int lineCount)
        {
            for (int i = 0; i < fileCount; i++)
            {
                string filePath = Path.Combine(path, $"TestFile{Guid.NewGuid()}.txt");

                using (StreamWriter fs = File.CreateText(filePath))
                {
                    for (int x = 0; x < lineCount; x++)
                    {
                        fs.WriteLine(Guid.NewGuid().ToString());
                    }
                }
            }
        }
    }
}