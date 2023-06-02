using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner.MidXaml
{
    public class Animation : MidXElementNode
    {
        public Animation(XmlElement xmlElement) : base(xmlElement)
        {
            
        }

        public override string ToTargetString(string leadingTrivia)
        {
            string nameString, attrString, childString;
            nameString = GetNameString();
            attrString = GetAttributesString();
            childString = GetChildNodesString(leadingTrivia);

            return Combine(leadingTrivia, nameString, attrString, childString);
        }
    }
}
