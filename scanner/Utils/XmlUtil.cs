using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner
{
    public class XmlUtil
    {
        public static XmlDocument Parse(string code)
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;

            xmlDocument.LoadXml(code);

            return xmlDocument;

        }
    }
}
