using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanner
{
    public class StmtTree
    {
        public StmtNode root;
        public string[] otherFileCodes;
        public StmtTree(string code, string[] otherFileCodes)
        {
            this.otherFileCodes = otherFileCodes;

            root = new StmtNode(code, true);
            root.SetOwnerTreeForRoot(this);
        }

        public void Print()
        {
            Console.Write(root.ToString());
        }

        public void Scan(Scanner scanner)
        {
            foreach (Update update in scanner.updates)
            {
                Update(update);
            }
        }

        private void Update(Update update)
        {
            StmtNode tNode = root.Match(update.trigger);
            if (tNode == null) return;

            // 在此之前，先用trigger对tnode.text进行分组捕获，使变量名在后面能够复用
            foreach (Change change in update.changes)
            {
                List<StmtNode> targetNodes = change.position.FindTargetNodes(root);
                foreach (StmtNode target in targetNodes)
                {
                    change.Apply(target);
                }
                

            }
        }
    }
}
