using System;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class ImageInfoTests : TestBase
    {
        [Test]
        [Description("Verifies that image attributes are correctly returned.")]
        public void GetAttributesTest()
        {
            var wimInfo = WimgApi.GetAttributes(TestWimHandle);

            Assert.IsNotNull(wimInfo, "WimInfo is not null.");
            Assert.AreEqual(WimInfoAttributes.Normal, wimInfo.Attributes);
            Assert.AreEqual(0, wimInfo.BootIndex);
            Assert.AreEqual(WimCompressionType.Lzx, wimInfo.CompressionType);
            Assert.AreNotEqual(Guid.Empty, wimInfo.Guid);
            Assert.AreEqual(TestWimImageCount, wimInfo.ImageCount);
            Assert.AreEqual(1, wimInfo.PartNumber);
            Assert.AreEqual(1, wimInfo.TotalParts);
        }

        [Test]
        [Description("Verifies that the GetAttributes throws an ArgumentNullException when wimHandle is null.")]
        public void GetAttributesTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetAttributes(null));
        }

        [Test]
        [Description("Verifies that image count is correctly returned.")]
        public void GetImageCountTest()
        {
            var imageCount = WimgApi.GetImageCount(TestWimHandle);

            Assert.AreEqual(TestWimImageCount, imageCount);
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

            var imageInformation = WimgApi.GetImageInformation(TestWimHandle);

            Assert.IsNotNull(imageInformation, "Image information is not null");

            var documentElement = imageInformation.CreateNavigator();

            Assert.IsNotNull(documentElement, "XPathNavigator is not null");

            VerifyXmlNodeText(documentElement, "//WIM/TOTALBYTES/text()");

            var imageNode = VerifyXmlNode(documentElement, "//WIM/IMAGE[@INDEX = '1']");

            var windowsNode = VerifyXmlNode(imageNode, "WINDOWS");

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
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.GetImageInformation(null));
        }

        [Test]
        public void SetImageInformationTest()
        {
            var xmlDocument = new XmlDocument();

            var fragment = xmlDocument.CreateDocumentFragment();

            fragment.InnerXml = @"<WIM><TEST>This is a test</TEST></WIM>";

            xmlDocument.AppendChild(fragment);

            WimgApi.SetImageInformation(TestWimHandle, xmlDocument);
        }

        [Test]
        public void SetImageInformationTest_ThrowsArgumentNullException_wimHandle()
        {
            AssertThrows<ArgumentNullException>("wimHandle", () =>
                WimgApi.SetImageInformation(null, null));
        }

        [Test]
        public void SetImageInformationTest_ThrowsArgumentNullException_imageInfoXml()
        {
            AssertThrows<ArgumentNullException>("imageInfoXml", () =>
                WimgApi.SetImageInformation(TestWimHandle, null));
        }

        private XPathNavigator VerifyXmlNode(XPathNavigator parentNode, string xpath)
        {
            var node = parentNode.SelectSingleNode(xpath);

            Assert.IsNotNull(node, String.Format("Found node '{0}'", xpath));

            return node;
        }

        private void VerifyXmlNodeText(XPathNavigator parentNode, string xpath)
        {
            var node = VerifyXmlNode(parentNode, xpath);

            Assert.IsTrue(!String.IsNullOrEmpty(node.Value), String.Format("Node value is null '{0}'", xpath));
        }
    }
}