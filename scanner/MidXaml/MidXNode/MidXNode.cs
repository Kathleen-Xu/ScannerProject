using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace scanner.MidXaml
{
    public abstract class MidXNode
    {
        public MidXElementNode parent;

        public abstract string ToString(string leadingTrivia);
        public abstract string ToTargetString(string leadingTrivia);

        public static MidXNode CreateMidXNode(XmlNode xmlNode)
        {
            if (xmlNode.NodeType == XmlNodeType.Element)
            {
                XmlElement xmlElement = (XmlElement)xmlNode;

                switch (xmlElement.Name) 
                {
                    case "Style": return new ControlTheme(xmlElement);
                    case "Style.Triggers": return new SelectorSet(xmlElement);
                    case "Setter": 
                        { 
                            if (xmlElement.ChildNodes.Count == 0)
                            {
                                return new SimpleSetter(xmlElement);
                            } else
                            {
                                return new NestedSetter(xmlElement);
                            }
                        }
                    case "ControlTemplate": return new ControlTemplate(xmlElement);
                    case "ControlTemplate.Triggers": return new SelectorSet(xmlElement);
                    default: return new MidXElementNode(xmlElement);
                }
                
            }
            if (xmlNode.NodeType == XmlNodeType.Text)
            {
                return new MidXTextNode((XmlText)xmlNode);
            }
            return null;
        }
        
    }

    public class MidXTextNode : MidXNode
    {
        public string text;
        public MidXTextNode(XmlText xmlText)
        {
            text = xmlText.InnerText;
        }

        public override string ToString(string leadingTrivia)
        {
            return $"{leadingTrivia}{text}\n";
        }

        public override string ToTargetString(string leadingTrivia)
        {
            return ToString(leadingTrivia);
        }
    }

    public class MidXElementNode : MidXNode
    {
        public string name;
        public Dictionary<string, MidXValue> attributes = new Dictionary<string, MidXValue>();
        public List<MidXNode> childNodes = new List<MidXNode>();

        public MidXElementNode(XmlElement xmlElement)
        {
            name = xmlElement.Name;

            foreach (XmlAttribute attr in xmlElement.Attributes)
            {
                if (attr.Name == "RenderTransformOrigin")
                {
                    string x = attr.Value.Split(',')[0];
                    x = (float.Parse(x) * 100).ToString() + "%";
                    string y = attr.Value.Split(',')[1];
                    y = (float.Parse(y) * 100).ToString() + "%";
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue(x+","+y));

                } else if (attr.Value == "LayoutTransform")
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue("RenderTransform"));
                } else if (attr.Value == "Visibility")
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue("IsVisible"));
                }
                else if (attr.Value == "Visible")
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue("True"));
                }
                else if (attr.Value == "Hidden")
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue("False"));
                }
                else if (attr.Value == "Collapsed")
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue("False"));
                }
                else
                {
                    attributes.Add(attr.Name, MidXValue.CreateMidXValue(attr.Value));
                }
                
            }

            foreach (XmlNode child in xmlElement.ChildNodes)
            {
                MidXNode node = CreateMidXNode(child);
                node.parent = this;
                childNodes.Add(node);
            }

        }

        public override string ToString(string leadingTrivia)
        {
            string nameString = name;

            string attrString = "";
            foreach (var attr in attributes)
            {
                attrString += $" {attr.Key}=\"{attr.Value.valueText}\"";
            }
            string childString = "";
            foreach (MidXNode child in childNodes)
            {
                childString += child.ToString(leadingTrivia + "    ");
            }

            return Combine(leadingTrivia, nameString, attrString, childString);           
        }

        public override string ToTargetString(string leadingTrivia)
        {
            string nameString, attrString, childString;

            if (name == "VisualStateManager.VisualStateGroups")
            {
                FindAncestor<ControlTheme>().animationSet.Add(new Animation(XmlUtil.Parse(IndeterminateAnimation).DocumentElement));
                return "";
            }

            if (name == "Grid" && attributes.ContainsKey("ClipToBounds") && attributes["x:Name"].valueText == "PART_Indicator")
            {
                nameString = "Border";
                attrString = GetAttributesString();
                string indicator = "", animation = "";
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (childNodes[i] is MidXElementNode node)
                    {
                        if (node.name == "Border")
                        {
                            if (node.attributes["x:Name"].valueText == "Indicator")
                            {
                                attrString += $" Background=\"{node.attributes["Background"].valueText}\"";
                                indicator = Combine(leadingTrivia, nameString, attrString, "");
                            }
                            if (node.attributes["x:Name"].valueText == "Animation")
                            {
                                attrString = $" x:Name=\"PART_IndeterminateIndicator\"";
                                foreach (var attr in node.attributes)
                                {
                                    if (attr.Key != "x:Name")
                                    {
                                        attrString += $" {attr.Key}=\"{attr.Value.valueText}\"";
                                    }
                                }
                                attrString += $" IsVisible=\"{{TemplateBinding IsIndeterminate}}\"";
                                animation = Combine(leadingTrivia, nameString, attrString, "");
                            }
                        }
                    }
                }
                return indicator + animation;
            }


            nameString = GetNameString();
            attrString = GetAttributesString();
            childString = GetChildNodesString(leadingTrivia);

            return Combine(leadingTrivia, nameString, attrString, childString);
        }

        protected virtual string GetNameString()
        {
            return name;
        }

        protected virtual string GetAttributesString()
        {
            string attrString = "";
            foreach (var attr in attributes)
            {
                if (attr.Key == "xmlns" && attr.Value.valueText == @"http://schemas.microsoft.com/winfx/2006/xaml/presentation")
                {
                    attrString += $" {attr.Key}=\"https://github.com/avaloniaui\"";
                } else
                {
                    attrString += $" {attr.Key}=\"{attr.Value.valueText}\"";
                }
                
            }
            return attrString;
        }

        protected virtual string GetChildNodesString(string leadingTrivia)
        {
            string childString = "";
            foreach (MidXNode child in childNodes)
            {
                childString += child.ToTargetString(leadingTrivia + "    ");
            }
            return childString;
        }

        protected static string Combine(string leadingTrivia, string nameString, string attrString, string childString)
        {
            if (childString.Length > 0)
            {
                string startTag = $"{leadingTrivia}<{nameString}{attrString}>\n";
                string endTag = $"{leadingTrivia}</{nameString}>\n";
                return $"{startTag}{childString}{endTag}";
            }
            else
            {
                return $"{leadingTrivia}<{nameString}{attrString} />\n";
            }
        }


        public MidXElementNode FindChildNodeWithName(string name)
        {
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes[i] is MidXElementNode)
                {
                    MidXElementNode node = (MidXElementNode)childNodes[i];
                    if ((node.attributes.ContainsKey("Name") && node.attributes["Name"].valueText == name)
                        || (node.attributes.ContainsKey("x:Name") && node.attributes["x:Name"].valueText == name))
                    {
                        return node;
                    }
                    var result =  node.FindChildNodeWithName(name);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public T FindAncestor<T>()
        {
            if (this is T t) return t;
            if (parent == null) return default(T);
            return parent.FindAncestor<T>();
        }

        private static string IndeterminateAnimation = @"<Style Selector=""^ /template/ Border#PART_IndeterminateIndicator"">
			<Style.Animations>
				<Animation Easing=""LinearEasing""
							IterationCount=""Infinite""
							Duration=""0:0:2"">
					<KeyFrame Cue=""0%"">
						<Setter Property=""ScaleTransform.ScaleX"" Value=""0.25"" />
					    <Setter Property=""TranslateTransform.X"" Value=""-200"" />
					</KeyFrame>
					<KeyFrame Cue=""100%"">
						<Setter Property=""ScaleTransform.ScaleX"" Value=""0.25"" />
					    <Setter Property=""TranslateTransform.X"" Value=""200"" />
					</KeyFrame>
				</Animation>
			</Style.Animations>
		</Style>";

    }
}
