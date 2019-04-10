// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Shouldly;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace Microsoft.Wim.Tests
{
    public class ImageInfoTests : TestBase
    {
        public ImageInfoTests(TestWimTemplate template)
            : base(template)
        {
        }

        [Fact]
        public void GetAttributesTest()
        {
            WimInfo wimInfo = WimgApi.GetAttributes(TestWimHandle);
            wimInfo.ShouldNotBeNull();

            wimInfo.Attributes.ShouldBe(WimInfoAttributes.Normal);
            wimInfo.BootIndex.ShouldBe(0);
            wimInfo.CompressionType.ShouldBe(WimCompressionType.Lzx);
            wimInfo.Guid.ShouldNotBe(Guid.Empty);
            wimInfo.ImageCount.ShouldBe(TestWimTemplate.ImageCount);
            wimInfo.PartNumber.ShouldBe(1);
            wimInfo.TotalParts.ShouldBe(1);
        }

        [Fact]
        public void GetAttributesTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetAttributes(null));
        }

        [Fact]
        public void GetImageCountTest()
        {
            int imageCount = WimgApi.GetImageCount(TestWimHandle);
            imageCount.ShouldBe(TestWimTemplate.ImageCount);
        }

        [Fact]
        public void GetImageInformationAsStringTest()
        {
            string imageInformation = WimgApi.GetImageInformationAsString(TestWimHandle);

            imageInformation.ShouldStartWith("<WIM>");
        }

        [Fact]
        public void GetImageInformationAsXDocumentTest()
        {
            XDocument imageInformation = WimgApi.GetImageInformationAsXDocument(TestWimHandle);

            imageInformation?.Root.ShouldNotBeNull();

            XElement root = imageInformation.Root;

            root.Element("TOTALBYTES").Value.ShouldNotBeNullOrWhiteSpace();

            XElement image = root.Elements("IMAGE").FirstOrDefault();

            image.ShouldNotBeNull();

            image.Attribute("INDEX").Value.ShouldBe("1");
        }

        [Fact]
        public void GetImageInformationAsXmlDocumentTest()
        {
            XmlDocument imageInformation = WimgApi.GetImageInformationAsXmlDocument(TestWimHandle);

            imageInformation?.DocumentElement.ShouldNotBeNull();

            XmlElement root = imageInformation.DocumentElement;

            var totalBytesElement = root.GetElementsByTagName("TOTALBYTES").Cast<XmlElement>().FirstOrDefault();

            totalBytesElement.ShouldNotBeNull();

            totalBytesElement.InnerText.ShouldNotBeNullOrWhiteSpace();

            XmlElement image = root.GetElementsByTagName("IMAGE").Cast<XmlElement>().FirstOrDefault();

            image.ShouldNotBeNull();

            image.GetAttribute("INDEX").ShouldBe("1");
        }

        [Fact]
        public void GetImageInformationTest()
        {
            /*
                <WIM>
                  <TOTALBYTES>139846944</TOTALBYTES>
                  <IMAGE INDEX="1">
                    <DIRCOUNT>2703</DIRCOUNT>
                    <FILECOUNT>12369</FILECOUNT>
                    <TOTALBYTES>862190505</TOTALBYTES>
                    <HARDLINKBYTES>324280176</HARDLINKBYTES>
                    <CREATIONTIME>
                      <HIGHPART>0x01CE9F04</HIGHPART>
                      <LOWPART>0x5F9E1B18</LOWPART>
                    </CREATIONTIME>
                    <LASTMODIFICATIONTIME>
                      <HIGHPART>0x01CE9F04</HIGHPART>
                      <LOWPART>0x607BDB5B</LOWPART>
                    </LASTMODIFICATIONTIME>
                    <WINDOWS>
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
                    </WINDOWS>
                    <NAME>Microsoft Windows PE (x86)</NAME>
                    <DESCRIPTION>Microsoft Windows PE (x86)</DESCRIPTION>
                  </IMAGE>
                </WIM>
            */

            XmlDocument imageInformation = WimgApi.GetImageInformationAsXmlDocument(TestWimHandle);

            imageInformation.ShouldNotBeNull();

            VerifyXmlNodeText(imageInformation, "//WIM/TOTALBYTES/text()");

            XmlElement imageNode = VerifyXmlNode<XmlElement>(imageInformation, "//WIM/IMAGE[@INDEX = '1']");

            XmlElement windowsNode = VerifyXmlNode<XmlElement>(imageNode, "WINDOWS");

            VerifyXmlNodeText(imageNode, "DIRCOUNT/text()");
            VerifyXmlNodeText(imageNode, "FILECOUNT/text()");
            VerifyXmlNodeText(imageNode, "TOTALBYTES/text()");
            VerifyXmlNodeText(imageNode, "HARDLINKBYTES/text()");
            VerifyXmlNodeText(imageNode, "CREATIONTIME/HIGHPART/text()");
            VerifyXmlNodeText(imageNode, "CREATIONTIME/LOWPART/text()");
            VerifyXmlNodeText(imageNode, "LASTMODIFICATIONTIME/HIGHPART/text()");
            VerifyXmlNodeText(imageNode, "LASTMODIFICATIONTIME/LOWPART/text()");
            VerifyXmlNodeText(imageNode, "NAME/text()");
            VerifyXmlNodeText(imageNode, "DESCRIPTION/text()");
            VerifyXmlNodeText(windowsNode, "ARCH/text()");
            VerifyXmlNodeText(windowsNode, "PRODUCTNAME/text()");
            VerifyXmlNodeText(windowsNode, "EDITIONID/text()");
            VerifyXmlNodeText(windowsNode, "INSTALLATIONTYPE/text()");
            VerifyXmlNodeText(windowsNode, "PRODUCTTYPE/text()");
            VerifyXmlNodeText(windowsNode, "LANGUAGES/LANGUAGE/text()");
            VerifyXmlNodeText(windowsNode, "LANGUAGES/DEFAULT/text()");
            VerifyXmlNodeText(windowsNode, "VERSION/MAJOR/text()");
            VerifyXmlNodeText(windowsNode, "VERSION/MINOR/text()");
            VerifyXmlNodeText(windowsNode, "VERSION/BUILD/text()");
            VerifyXmlNodeText(windowsNode, "VERSION/SPBUILD/text()");
            VerifyXmlNodeText(windowsNode, "VERSION/SPLEVEL/text()");
            VerifyXmlNodeText(windowsNode, "SYSTEMROOT/text()");
        }

        [Fact]
        public void GetImageInformationTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetImageInformation(null));
        }

        [Fact]
        public void SetImageInformationFromStringTest()
        {
            WimgApi.SetImageInformation(TestWimHandle, @"<WIM><TEST>This is a test</TEST></WIM>");
        }

        [Fact]
        public void SetImageInformationTest()
        {
            XmlDocument xmlDocument = new XmlDocument()
            {
                XmlResolver = null
            };

            XmlDocumentFragment fragment = xmlDocument.CreateDocumentFragment();

            fragment.InnerXml = @"<WIM><TEST>This is a test</TEST></WIM>";

            xmlDocument.AppendChild(fragment);

            WimgApi.SetImageInformation(TestWimHandle, xmlDocument);
        }

        [Fact]
        public void SetImageInformationTest_ThrowsArgumentNullException_imageInfoXml()
        {
            ShouldThrow<ArgumentNullException>("imageInfoXml", () =>
                WimgApi.SetImageInformation(TestWimHandle, (IXPathNavigable)null));
        }

        [Fact]
        public void SetImageInformationTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetImageInformation(null, (IXPathNavigable)null));
        }

        private T VerifyXmlNode<T>(XmlNode parentNode, string xpath)
            where T : XmlNode
        {
            XmlNode node = parentNode.SelectSingleNode(xpath);

            node.ShouldNotBeNull($"Could not find node '{xpath}'");

            return node as T;
        }

        private void VerifyXmlNodeText(XmlNode parentNode, string xpath)
        {
            XmlText node = VerifyXmlNode<XmlText>(parentNode, xpath);

            node.Value.ShouldNotBeNullOrEmpty($"Node value '{xpath}' should not be empty");
        }
    }
}