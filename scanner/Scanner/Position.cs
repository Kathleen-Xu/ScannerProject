using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scanner
{
    public class Position
    {
        public string path;
        private string realPath;

        public Position(string path)
        {
            this.path = path;
            this.realPath = path;
        }

        public void Specialize(Dictionary<string, string> infos)
        {
            realPath = RegexUtil.Replace(path, infos);
        }

        public List<StmtNode> FindTargetNodes(StmtNode anchor)
        {
            List<StmtNode> res = new List<StmtNode>();

            string[] tmp = realPath.Split(':');
            string anchorType = tmp[0];

            switch (anchorType) // 忽略嵌套类和嵌套函数
            {
                case "C":
                    {
                        anchor = anchor.getClassAncestor();
                        break;
                    }
                case "F":
                    {
                        anchor = anchor.getFunctionAncestor();
                        break;
                    }
                case "S":
                    {
                        res.Add(anchor);
                        return res;
                    }
                default:
                    {
                        break;
                    }
            }

            res.Add(anchor);

            if (tmp.Length < 2) 
            {
                return res;
            }

            List<StmtNode> anchors = new List<StmtNode>();

            string[] steps = tmp[1].Split('/');
            foreach (string step in steps)
            {
                anchors.Clear();
                anchors.AddRange(res);
                res.Clear();

                tmp = step.Split('.');
                anchorType = tmp[0];
                string id = tmp.Length == 2 ? tmp[1] : ".*";

                switch (anchorType)
                {
                    case "C":
                        {
                            anchors.ForEach(item => res.AddRange(item.getClassChildNodes(id)));
                            break;
                        }
                    case "F":
                        {
                            anchors.ForEach(item => res.AddRange(item.getFunctionChildNodes(id)));
                            break;
                        }
                    case "FC":
                        {
                            anchors.ForEach(item => res.AddRange(item.getConstructorChildNodes()));
                            break;
                        }
                    case "FSC":
                        {
                            anchors.ForEach(item => res.AddRange(item.getStaticConstructorChildNodes()));
                            break;
                        }
                    case "S":
                        {
                            anchors.ForEach(item => res.AddRange(item.getSingleChildNodes(id))); //递归查找，不考虑内涵CF的情况
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return res;
        }
    }
}
