using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner.MidXaml
{
    public class SelectorSet : MidXElementNode
    {
        public List<Selector> selectorList = new List<Selector>();
        public SelectorSet(XmlElement xmlElement) : base(xmlElement)
        {
            if (name == "Style.Triggers")
            {
                foreach (var child in xmlElement.ChildNodes)
                {
                    Trace.Assert(child is XmlElement);
                    SimpleSelector selector = new SimpleSelector((XmlElement)child);
                    selector.ownerSet = this;
                    selectorList.Add(selector);
                }

            } else if (name == "ControlTemplate.Triggers")
            {
                foreach (var child in xmlElement.ChildNodes)
                {
                    Trace.Assert(child is XmlElement);
                    TemplateSelector selector = new TemplateSelector((XmlElement)child);
                    selector.ownerSet = this;
                    selectorList.Add(selector);
                }

            } else
            {
                throw new Exception("fail to parse selector set.");
            }

            
        }

        public override string ToTargetString(string leadingTrivia)
        {
            FindAncestor<ControlTheme>().selectorSets.Add(this);
            return "";
        }
    }

    public abstract class Selector : MidXElementNode
    {
        public SelectorSet ownerSet;
        public List<Condition> conditions = new List<Condition>();
        public Selector(XmlElement xmlElement) : base(xmlElement)
        {
            Condition tmpCondition;
            if (name == "Trigger")
            {
                tmpCondition = new Condition(attributes["Property"], attributes["Value"]);
                conditions.Add(tmpCondition);

            }
            else if (name == "MultiTrigger")
            {
                var conditionSet = childNodes.Where((curr) => ((MidXElementNode)curr)?.name == "MultiTrigger.Conditions").Single();
                Trace.Assert(conditionSet is MidXElementNode);
                ((MidXElementNode)conditionSet).childNodes.ForEach((curr) =>
                {
                    Trace.Assert(curr is MidXElementNode);
                    MidXElementNode c = (MidXElementNode)curr;
                    conditions.Add(new Condition(c.attributes["Property"], c.attributes["Value"]));
                });
            }

        }
    }

    public class SimpleSelector : Selector
    {
        List<Setter> setters = new List<Setter>();
        public SimpleSelector(XmlElement xmlElement) : base(xmlElement)
        {
            if (name == "Trigger")
            {
                foreach (var child in childNodes)
                {
                    Trace.Assert(child is Setter);
                    setters.Add((Setter)child);
                }

            } else if (name == "MultiTrigger")
            {
                foreach (var child in childNodes)
                {
                    if (child is Setter)
                    {
                        setters.Add((Setter)child);
                    }
                }
            }
        }

        public override string ToTargetString(string leadingTrivia)
        {
            return recursiveGenerator(0, leadingTrivia);
        }

        private string recursiveGenerator(int i, string leadingTrivia)
        {
            if (i == conditions.Count)
            {
                string childString = "";
                foreach (Setter setter in setters)
                {
                    childString += setter.ToTargetString(leadingTrivia);
                }
                return childString;
            }
            string attrString = $" Selector=\"{conditions[i].ToTargetString()}\"";
            return Combine(leadingTrivia, "Style", attrString, recursiveGenerator(i + 1, leadingTrivia + "    "));
        }
    }

    public class TemplateSelector : Selector
    {
        List<SingleTemplateSelector> singleTemplateSelectors = new List<SingleTemplateSelector>();
        Dictionary<string, string> targetMap = new Dictionary<string, string>();
        public TemplateSelector(XmlElement xmlElement) : base(xmlElement)
        {
        }

        public override string ToTargetString(string leadingTrivia)
        {
            Init();
            return recursiveGenerator(0, leadingTrivia);
        }
        private void Init()
        {
            if (name == "Trigger")
            {
                foreach (var child in childNodes)
                {
                    Trace.Assert(child is Setter);
                    InitSingleTemplateSelectors((Setter)child);
                }

            }
            else if (name == "MultiTrigger")
            {
                foreach (var child in childNodes)
                {
                    if (child is Setter)
                    {
                        InitSingleTemplateSelectors((Setter)child);
                    }
                }
            }
        }

        private string recursiveGenerator(int i, string leadingTrivia)
        {
            string attrString;
            if (i == conditions.Count - 1)
            {
                
                string OuterChildString = "";
                foreach (var target in targetMap)
                {
                    attrString = $" Selector=\"^{conditions[i].ToTargetString()} /template/ {target.Value}#{target.Key}\"";
                    string InnerChildString = "";
                    foreach (SingleTemplateSelector singleTemplateSelector in singleTemplateSelectors)
                    {
                        if (singleTemplateSelector.targetName == target.Key)
                        {
                            InnerChildString += singleTemplateSelector.setter.ToTargetString(leadingTrivia + "    ");
                        }
                    }
                    OuterChildString += Combine(leadingTrivia, "Style", attrString, InnerChildString);
                }
                
                return OuterChildString;
            }
            attrString = $" Selector=\"{conditions[i].ToTargetString()}\"";
            return Combine(leadingTrivia, "Style", attrString, recursiveGenerator(i + 1, leadingTrivia + "    "));
        }

        private void InitSingleTemplateSelectors(Setter setter)
        {
            string targetName = setter.attributes["TargetName"].valueText;
            string targetType = FindTargetType(targetName);
            singleTemplateSelectors.Add(new SingleTemplateSelector(targetName, targetType, setter));
            if (!targetMap.ContainsKey(targetName))
            {
                targetMap.Add(targetName, targetType);
            }           
        }

        private string FindTargetType(string targetName)
        {
            ControlTemplate controlTemplate = (ControlTemplate)ownerSet.parent;
            Trace.Assert(controlTemplate != null);
            return controlTemplate.FindChildNodeWithName(targetName).name;
        }
    }

    public class Condition
    {
        public string property;
        public string value;

        public Condition(MidXValue property, MidXValue value)
        {
            this.property = property.valueText;
            this.value = value.valueText;
        }

        public string ToTargetString()
        {
            return $"[{property}={value}]";
        }
    }

    public class SingleTemplateSelector
    {
        public string targetName;
        public string targetType;
        public Setter setter;

        public SingleTemplateSelector(string targetName, string targetType, Setter setter)
        {
            this.targetName = targetName;
            this.targetType = targetType;
            this.setter = setter;
        }   
    }


}
