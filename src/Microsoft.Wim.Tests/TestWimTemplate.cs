using Shouldly;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Wim.Tests
{
    public class TestWimTemplate : IDisposable
    {
        public const int FileCount = 100;
        public const int FileLineCount = 1000;
        public const int ImageCount = 2;

        private const string TestWimTemplateFilename = @"test_template.wim";

        private readonly string TestWimTemplateDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}")).FullName;

        private readonly Lazy<string> _testWimTemplatePathLazy;

        private readonly string _testWimTempPath;
        

        public TestWimTemplate()
        {
            _testWimTempPath = Directory.CreateDirectory(Path.Combine(TestWimTemplateDirectory, "temp")).FullName;

            _testWimTemplatePathLazy = new Lazy<string>(CreateTemplateImage, isThreadSafe: true);
        }

        public string FullPath => _testWimTemplatePathLazy.Value;

        private string CreateTemplateImage()
        {
            string capturePath = Directory.CreateDirectory(Path.Combine(TestWimTemplateDirectory, "capture")).FullName;

            CreateTestFiles(capturePath, FileCount, FileLineCount);
            try
            {
                return CaptureTemplateImage(capturePath);
            }
            finally
            {
                Directory.Delete(capturePath, recursive: true);
            }
        }

        private string CaptureTemplateImage(string capturePath)
        {
            string imagePath = Path.Combine(TestWimTemplateDirectory, TestWimTemplateFilename);

            if (!Directory.Exists(capturePath))
            {
                throw new DirectoryNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not find part of the path '{0}'", capturePath));
            }

            XmlDocument xmlDocument = new XmlDocument();

            using (WimHandle wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Write, WimCreationDisposition.CreateNew, WimCreateFileOptions.None, WimCompressionType.Lzx))
            {
                WimgApi.SetTemporaryPath(wimHandle, _testWimTempPath);

                for (int i = 0; i < ImageCount; i++)
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

            return imagePath;
        }

        public void Dispose()
        {
            Directory.Delete(TestWimTemplateDirectory, recursive: true);
        }

        public static void CreateTestFiles(string path, int fileCount, int lineCount)
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