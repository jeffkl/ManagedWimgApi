using NUnit.Framework;
using Shouldly;
using System;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ImageInfoTests : TestBase
    {
        [Test]
        [Description("Verifies that image attributes are correctly returned.")]
        public void GetAttributesTest()
        {
            WimInfo wimInfo = WimgApi.GetAttributes(TestWimHandle);
            wimInfo.ShouldNotBeNull();

            wimInfo.Attributes.ShouldBe(WimInfoAttributes.Normal);
            wimInfo.BootIndex.ShouldBe(0);
            wimInfo.CompressionType.ShouldBe(WimCompressionType.Lzx);
            wimInfo.Guid.ShouldNotBe(Guid.Empty);
            wimInfo.ImageCount.ShouldBe(TestWimImageCount);
            wimInfo.PartNumber.ShouldBe(1);
            wimInfo.TotalParts.ShouldBe(1);
        }

        [Test]
        [Description("Verifies that the GetAttributes throws an ArgumentNullException when wimHandle is null.")]
        public void GetAttributesTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetAttributes(null));
        }

        [Test]
        [Description("Verifies that image count is correctly returned.")]
        public void GetImageCountTest()
        {
            int imageCount = WimgApi.GetImageCount(TestWimHandle);
            imageCount.ShouldBe(TestWimImageCount);
        }

        [Test]
        [Description("Verifies that image information XML is correctly returned.")]
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

            IXPathNavigable imageInformation = WimgApi.GetImageInformation(TestWimHandle);

            imageInformation.ShouldNotBeNull();

            XPathNavigator documentElement = imageInformation.CreateNavigator();

            documentElement.ShouldNotBeNull();

            VerifyXmlNodeText(documentElement, "//WIM/TOTALBYTES/text()");

            XPathNavigator imageNode = VerifyXmlNode(documentElement, "//WIM/IMAGE[@INDEX = '1']");

            XPathNavigator windowsNode = VerifyXmlNode(imageNode, "WINDOWS");

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

        [Test]
        [Description("Verifies that the GetAttributes throws an ArgumentNullException when wimHandle is null.")]
        public void GetImageInformationTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetImageInformation(null));
        }

        [Test]
        public void SetImageInformationTest()
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlDocumentFragment fragment = xmlDocument.CreateDocumentFragment();

            fragment.InnerXml = @"<WIM><TEST>This is a test</TEST></WIM>";

            xmlDocument.AppendChild(fragment);

            WimgApi.SetImageInformation(TestWimHandle, xmlDocument);
        }

        [Test]
        public void SetImageInformationTest_ThrowsArgumentNullException_imageInfoXml()
        {
            ShouldThrow<ArgumentNullException>("imageInfoXml", () =>
                WimgApi.SetImageInformation(TestWimHandle, null));
        }

        [Test]
        public void SetImageInformationTest_ThrowsArgumentNullException_wimHandle()
        {
            ShouldThrow<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetImageInformation(null, null));
        }

        private XPathNavigator VerifyXmlNode(XPathNavigator parentNode, string xpath)
        {
            XPathNavigator node = parentNode.SelectSingleNode(xpath);

            node.ShouldNotBeNull($"Could not find node '{xpath}'");

            return node;
        }

        private void VerifyXmlNodeText(XPathNavigator parentNode, string xpath)
        {
            XPathNavigator node = VerifyXmlNode(parentNode, xpath);

            node.Value.ShouldNotBeNullOrEmpty($"Node value '{xpath}' should not be empty");
        }
    }
}