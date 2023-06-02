using System;
using System.Diagnostics;
using System.Xml;

namespace scanner.MidXaml
{
    public class ControlTemplate : MidXElementNode
    {
        public ControlTemplate(XmlElement xmlElement) : base(xmlElement)
        {
            Trace.Assert(xmlElement.Name == "ControlTemplate");            
        }

        protected override string GetAttributesString()
        {
            return "";
        }
    }
}
