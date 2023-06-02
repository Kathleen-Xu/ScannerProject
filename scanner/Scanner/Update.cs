using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace scanner
{
    public class Update
    {
        public string trigger;
        public string mode;
        public List<Change> changes;

        public Update(string trigger, string mode, List<Change> changes)
        {
            this.trigger = trigger;
            this.mode = mode; 
            this.changes = changes;
        }   

        public void Apply(StmtTree tree)
        {
            StmtNode tNode = tree.root.Match(trigger);
            if (tNode == null) return;

            Regex regex = new Regex(trigger);
            Match match = regex.Match(tNode.text);
            Trace.Assert(match.Success);
            Dictionary<string, string> infos = new Dictionary<string, string>();
            for (int i = 0; i < match.Groups.Count; i++)
            {
                infos.Add(match.Groups[i].Name, match.Groups[i].Value);
            }
            
            // 在此之前，先用trigger对tnode.text进行分组捕获，使变量名在后面能够复用
            foreach (Change change in changes)
            {
                change.Init(infos).Apply(tNode);
            }

            if (mode == "loop")
            {
                Apply(tree);
            }
        }
    }
}
