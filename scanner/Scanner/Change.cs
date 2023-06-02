using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scanner
{
    public abstract class Change
    {
        public Position position;
        protected Dictionary<string, string> infoMap = new Dictionary<string, string>();

        public Change(Position position)
        {
            this.position = position;
        }

        public Change Init(Dictionary<string, string> infos)
        {
            infoMap.Clear();
            AddInfo(infos);
            Specialize();
            return this;
        }
        protected void AddInfo(Dictionary<string, string> infos)
        {
            if (infos == null) return;
            foreach (var info in infos)
            {
                infoMap.Add(info.Key, info.Value);
            }
        }
        public abstract void Apply(StmtNode target);
        protected virtual void Specialize()
        {
            position.Specialize(infoMap);
        }

    }

    public class NodeEditChange : Change
    {
        public string apiName;
        public Dictionary<string, string> overridePairs;
        public Dictionary<string, List<Change>> sideEffects;

        public NodeEditChange(string apiName, Dictionary<string, string> overridePairs, Dictionary<string, List<Change>> sideEffects, Position position) : base(position)
        {
            this.apiName = apiName;
            this.overridePairs = overridePairs;
            this.sideEffects = sideEffects;
        }

        public override void Apply(StmtNode tNode)
        {
            List<StmtNode> targetNodes = position.FindTargetNodes(tNode);

            RoslynEditHelper helper = new RoslynEditHelper();
            foreach (StmtNode target in targetNodes)
            {
                if (true)
                {
                    AddInfo(helper.EditAndGetInfo(target, apiName, overridePairs));
                    foreach (var sideEffect in sideEffects)
                    {
                        if (infoMap.ContainsKey(sideEffect.Key))
                        {
                            foreach (Change change in sideEffect.Value)
                            {
                                change.Init(infoMap).Apply(target);
                            }
                        }
                    }
                    
                }
                else
                {
                    //throw new Exception("Fail Edition.");
                }
            }
        }

        protected override void Specialize()
        {
            base.Specialize();
        }
    }


    public class TextEditChange : Change
    {
        public string oldPattern;
        public string newPattern;
        private string realOldPattern;
        private string realNewPattern;

        public TextEditChange(string oldPattern, string newPattern, Position position) : base(position)
        {
            this.oldPattern = oldPattern;
            this.newPattern = newPattern;
            this.realOldPattern = oldPattern;
            this.realNewPattern = newPattern;
        }

        public override void Apply(StmtNode tNode)
        {
            List<StmtNode> targetNodes = position.FindTargetNodes(tNode);
            foreach (StmtNode target in targetNodes)
            {
                if (Regex.IsMatch(target.text, realOldPattern))
                {
                    //target.text = newPattern;
                    target.text = Regex.Replace(target.text, realOldPattern, realNewPattern);
                }
                else
                {
                    //throw new Exception("Fail Edition.");
                }
            }
            
        }

        protected override void Specialize()
        {
            base.Specialize();
            realOldPattern = RegexUtil.Replace(oldPattern, infoMap);
            realNewPattern = RegexUtil.Replace(newPattern, infoMap);
        }
    }



    public class DeletionChange : Change
    {
        public string matcher;
        private string realMatcher;
        public DeletionChange(string matcher, Position position) : base(position)
        {
            this.matcher = matcher;
            this.realMatcher = matcher;
        }
        public override void Apply(StmtNode tNode)
        {
            List<StmtNode> targetNodes = position.FindTargetNodes(tNode);
            foreach (StmtNode target in targetNodes)
            {
                if (Regex.IsMatch(target.text, realMatcher) && target.parent != null)
                {
                    target.parent.RemoveChildNode(target);
                }
                else
                {
                    //throw new Exception("Fail Deletion.");
                }
            }
        }

        protected override void Specialize()
        {
            base.Specialize();
            realMatcher = RegexUtil.Replace(matcher, infoMap);
        }
    }

    public class AdditionChange : Change
    {
        public string codeToAdd;
        private string realCodeToAdd; 
        public string relativePos;
        public AdditionChange(string codeToAdd, string relativePos, Position position) : base(position)
        {
            this.codeToAdd = codeToAdd;
            this.realCodeToAdd = codeToAdd;
            this.relativePos = relativePos;
        }
        public override void Apply(StmtNode tNode)
        {
            List<StmtNode> targetNodes = position.FindTargetNodes(tNode);
            foreach (StmtNode target in targetNodes)
            {
                StmtNode r = new StmtNode(realCodeToAdd, true);
                if (relativePos == "inside")
                {
                    Trace.Assert(target.type != StmtType.Single);
                    foreach (StmtNode n in r.childNodes)
                    {
                        target.AddChildNode(n);
                    }
                }
                else if (relativePos == "before")
                {
                    Trace.Assert(target.parent != null);
                    foreach (StmtNode n in r.childNodes)
                    {
                        target.InsertNodeBefore(n);
                    }

                }
                else if (relativePos == "after")
                {
                    Trace.Assert(target.parent != null);
                    foreach (StmtNode n in r.childNodes)
                    {
                        target.InsertNodeAfter(n);
                    }
                }
                else
                {
                    //throw new Exception("Fail Addition.");
                }
            }
        }
        protected override void Specialize()
        {
            base.Specialize();
            realCodeToAdd = RegexUtil.Replace(codeToAdd, infoMap);
        }
    }
}