using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner.MidXaml
{
    public class ControlTheme : MidXElementNode
    {
        public MidXValue targetType;
        public MidXValue key;
        public List<SelectorSet> selectorSets = new List<SelectorSet>();
        public List<Animation> animationSet = new List<Animation>();
        public ControlTheme(XmlElement xmlElement) : base(xmlElement)
        {
            Trace.Assert(xmlElement.Name == "Style");
            if (attributes.ContainsKey("TargetType"))
            {
                targetType = attributes["TargetType"];
            } 
            if (attributes.ContainsKey("x:Key"))
            {
                key = attributes["x:Key"];
            }
            
            
        }

        protected override string GetNameString()
        {
            return "ControlTheme";
        }

        protected override string GetChildNodesString(string leadingTrivia)
        {
            string childString = base.GetChildNodesString(leadingTrivia);
            selectorSets.ForEach((set) =>
            {
                set.selectorList.ForEach((selector) =>
                {
                    childString += selector.ToTargetString(leadingTrivia + "    ");
                });
            });
            animationSet.ForEach((a) =>
            {
                childString += a.ToTargetString(leadingTrivia + "    ");
            });
            return childString;
        }

        public override string ToTargetString(string leadingTrivia)
        {
            if (childNodes.Count == 0) return "";
            return base.ToTargetString(leadingTrivia);
        }

    }
}
