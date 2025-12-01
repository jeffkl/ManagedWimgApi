// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Wim.Tests
{
    public class TestWimTemplate : IDisposable
    {
        public const int FileCount = 100;
        public const int FileLineCount = 1000;
        public const int ImageCount = 2;

        private readonly Lazy<string> _esdTemplatePathLazy;
        private readonly string _tempDirectory;
        private readonly string _templateDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}")).FullName;
        private readonly Lazy<string> _wimTemplatePathLazy;

        public TestWimTemplate()
        {
            _tempDirectory = Directory.CreateDirectory(Path.Combine(_templateDirectory, "temp")).FullName;

            _wimTemplatePathLazy = new Lazy<string>(() => CreateTemplateImage("test_template.wim", WimCreateFileOptions.None, WimCompressionType.Lzx));
            _esdTemplatePathLazy = new Lazy<string>(() => CreateTemplateImage("test_template.esd", WimCreateFileOptions.Chunked, WimCompressionType.Lzms));
        }

        public string EsdFullPath => _esdTemplatePathLazy.Value;

        public string WimFullPath => _wimTemplatePathLazy.Value;

        public static void CreateTestFiles(string path, int fileCount = FileCount, int lineCount = FileLineCount)
        {
            for (int i = 0; i < fileCount; i++)
            {
                string filePath = Path.Combine(path, $"TestFile{Guid.NewGuid()}.txt");

                using StreamWriter fs = File.CreateText(filePath);
                for (int x = 0; x < lineCount; x++)
                {
                    fs.WriteLine(Guid.NewGuid().ToString());
                }
            }
        }

        public void Dispose()
        {
            Directory.Delete(_templateDirectory, recursive: true);
        }

        private string CaptureTemplateImage(string filename, string capturePath, WimCreateFileOptions createFileOptions, WimCompressionType compressionType)
        {
            string imagePath = Path.Combine(_templateDirectory, filename);

            if (!Directory.Exists(capturePath))
            {
                throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, "Could not find part of the path '{0}'", capturePath));
            }

            using (WimHandle wimHandle = WimgApi.CreateFile(imagePath, WimFileAccess.Write, WimCreationDisposition.CreateNew, createFileOptions, compressionType))
            {
                WimgApi.SetTemporaryPath(wimHandle, _tempDirectory);

                for (int i = 0; i < ImageCount; i++)
                {
                    // ReSharper disable once UnusedVariable
                    using WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, capturePath, WimCaptureImageOptions.DisableDirectoryAcl | WimCaptureImageOptions.DisableFileAcl | WimCaptureImageOptions.DisableRPFix);
                }

                XmlDocument? xmlDocument = WimgApi.GetImageInformationAsXmlDocument(wimHandle);

                xmlDocument.ShouldNotBeNull();

                XmlNodeList? imageNodes = xmlDocument.SelectNodes("//WIM/IMAGE");

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

                    fragment.InnerXml = $@"<NAME>Test Image {imageNode.Attributes["INDEX"]?.Value}</NAME>";

                    imageNode.AppendChild(fragment);

                    fragment.InnerXml = $@"<DESCRIPTION>Test Image {imageNode.Attributes["INDEX"]?.Value}</DESCRIPTION>";

                    imageNode.AppendChild(fragment);

                    WimgApi.SetImageInformation(wimHandle, xmlDocument);
                }
            }

            return imagePath;
        }

        private string CreateTemplateImage(string filename, WimCreateFileOptions createFileOptions, WimCompressionType compressionType)
        {
            string capturePath = Directory.CreateDirectory(Path.Combine(_templateDirectory, Guid.NewGuid().ToString("N"))).FullName;

            CreateTestFiles(capturePath);
            try
            {
                return CaptureTemplateImage(filename, capturePath, createFileOptions, compressionType);
            }
            finally
            {
                Directory.Delete(capturePath, recursive: true);
            }
        }
    }
}