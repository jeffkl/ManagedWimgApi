using System.Diagnostics.CodeAnalysis;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using NUnit.Framework;

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

        #region Setup/Cleanup

        [OneTimeTearDown]
        public virtual void Cleanup()
        {
            if (_testWimHandle != null)
            {
                _testWimHandle.Dispose();
            }

            if (_applyPath != null && Directory.Exists(_applyPath))
            {
                Directory.Delete(_applyPath, true);
            }
        }

        [OneTimeSetUp]
        public virtual void Setup()
        {
            _captureWimPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "capture.wim");
        }

        #endregion Setup/Cleanup

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

        public string CaptureWimPath
        {
            get
            {
                return _captureWimPath;
            }
        }

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
                    var capturePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestCapture");

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

        protected Exception AssertThrows<T>(Action action)
            where T : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                // TODO: Fix this
                //Assert.IsInstanceOfType(ex, typeof(T));
                return ex;
            }

            Assert.Fail(typeof(T) == new Exception().GetType() ? "Expected exception but no exception was thrown." : String.Format("Expected exception of type {0} but no exception was thrown.", typeof(T)));

            return null;
        }

        protected Exception AssertThrows<T>(string paramName, Action action)
            where T : ArgumentException
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            try
            {
                action();
            }
            catch (ArgumentException argumentException)
            {
                Assert.AreEqual(paramName, argumentException.ParamName, "Expected parameter name was found");
                return argumentException;
            }
            catch (Exception exception)
            {
                // TODO: Fix this
                //Assert.IsInstanceOfType(exception, typeof(T));

                return exception;
            }

            Assert.Fail(typeof(T) == new Exception().GetType() ? "Expected exception but no exception was thrown." : String.Format("Expected exception of type {0} but no exception was thrown.", typeof(T)));

            return null;
        }

        protected void CaptureTestImage(string imagePath, string capturePath)
        {
            if (String.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentNullException("imagePath");
            }

            if (!Directory.Exists(capturePath))
            {
                throw new DirectoryNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not find part of the path '{0}'", capturePath));
            }

            var xmlDocument = new XmlDocument();

            using (var wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Write, WimCreationDisposition.CreateNew, WimCreateFileOptions.None, WimCompressionType.Lzx))
            {
                WimgApi.SetTemporaryPath(wimHandle, TempPath);

                for (var i = 0; i < TestWimImageCount; i++)
                {
                    // ReSharper disable once UnusedVariable
                    using (var imageHandle = WimgApi.CaptureImage(wimHandle, capturePath, WimCaptureImageOptions.DisableDirectoryAcl | WimCaptureImageOptions.DisableFileAcl | WimCaptureImageOptions.DisableRPFix))
                    {
                    }
                }

                var xml = WimgApi.GetImageInformation(wimHandle).CreateNavigator();

                Assert.IsNotNull(xml, "xml should not be null");

                xmlDocument.LoadXml(xml.OuterXml);

                var imageNodes = xmlDocument.SelectNodes("//WIM/IMAGE");
                
                Assert.IsNotNull(imageNodes, "imageNodes should not be null");

                foreach (XmlElement imageNode in imageNodes)
                {
                    var fragment = xmlDocument.CreateDocumentFragment();

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

                    fragment.InnerXml = String.Format(@"<NAME>Test Image {0}</NAME>", imageNode.Attributes["INDEX"].Value);

                    imageNode.AppendChild(fragment);

                    fragment.InnerXml = String.Format(@"<DESCRIPTION>Test Image {0}</DESCRIPTION>", imageNode.Attributes["INDEX"].Value);

                    imageNode.AppendChild(fragment);

                    WimgApi.SetImageInformation(wimHandle, xmlDocument);
                }
            }
        }

        protected void CreateTestFiles(string path, int fileCount, int lineCount)
        {
            for (var i = 0; i < fileCount; i++)
            {
                var filePath = Path.Combine(path, String.Format("TestFile{0}.txt", Guid.NewGuid()));

                using (var fs = File.CreateText(filePath))
                {
                    for (var x = 0; x < lineCount; x++)
                    {
                        fs.WriteLine(Guid.NewGuid().ToString());
                    }
                }
            }
        }
    }
}