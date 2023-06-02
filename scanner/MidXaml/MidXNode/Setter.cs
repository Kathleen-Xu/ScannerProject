using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner.MidXaml
{
    public abstract class Setter : MidXElementNode
    {
        public string property;
        public Setter(XmlElement xmlElement) : base(xmlElement)
        {
            Trace.Assert(xmlElement.Name == "Setter");
            property = attributes["Property"].valueText;
        }

        protected override string GetAttributesString()
        {
            string attrString = "";
            foreach (var attr in attributes)
            {
                if (attr.Key != "TargetName")
                {
                    attrString += $" {attr.Key}=\"{attr.Value.valueText}\"";
                }
            }
            return attrString;
        }
    }

    public class SimpleSetter : Setter
    {
        public MidXValue value;
        public SimpleSetter(XmlElement xmlElement) : base(xmlElement)
        {
            Trace.Assert(!xmlElement.HasChildNodes);
            value = attributes["Value"];
        }

    }

    public class NestedSetter : Setter
    {
        public MidXElementNode value;
        public NestedSetter(XmlElement xmlElement) : base(xmlElement)
        {
            Trace.Assert(childNodes.Count == 1);
            value = (MidXElementNode)childNodes[0];
            Trace.Assert(value.name == "Setter.Value");

        }

        protected override string GetChildNodesString(string leadingTrivia)
        {
            if (property == "Template")
            {
                return value.childNodes[0].ToTargetString(leadingTrivia + "    ");
            }
            return base.GetChildNodesString(leadingTrivia);
        }
    }
}
