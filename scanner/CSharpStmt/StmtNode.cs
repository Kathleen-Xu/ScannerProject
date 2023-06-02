using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace scanner
{
    public class StmtNode
    {
        public string text;
        public string name;
        public List<StmtNode> childNodes;
        public StmtNode parent;
        public StmtType type;

        private StmtTree ownerTree;

        public StmtNode(string code, bool isRoot = false)
        {
            childNodes = new List<StmtNode>();

            if (isRoot)
            {
                text = "";
                type = StmtType.Root;
                InitChildNodes(code.Trim());
                parent = null;
            }
            else
            {
                code = code.Trim();
                int p1, p2;
                if (code.EndsWith("}"))
                {
                    p2 = code.Length - 1;
                    p1 = findFirstChar(code, '{');
                }
                else if (code.EndsWith("};"))
                {
                    p2 = code.Length - 2;
                    p1 = findFirstChar(code, '{');
                }
                else
                {
                    p1 = p2 = -1;
                }
                Trace.Assert(p1 == -1 && p2 == -1 || p1 != -1 && p2 != -1);
                if (p1 == -1)
                {
                    text = code;
                    type = StmtType.Single;
                }
                else
                {
                    text = code.Substring(0, p1).Trim();
                    if (Regex.IsMatch(text, @"^(\w+\s+)*enum .*"))
                    {
                        text = code;
                        type = StmtType.Single;
                        return;
                    }
                    if (Regex.IsMatch(text, @"^(\w+\s+)*class .*"))
                    {
                        type = StmtType.ClassDeclaration;
                        name = Regex.Replace(text, "^(\\w+\\s+)*class\\s+(?<name>\\S+)\\s*.*$", "${name}");
                    }
                    else if (Regex.IsMatch(text, @"^(if|while|else|foreach|for|switch|case|else\sif)(\s|\().*"))
                    {
                        type = StmtType.Other;
                    }
                    else if (Regex.IsMatch(text, @".*\w+\s*\(.*\)\s*"))
                    {
                        type = StmtType.FunctionDeclaration;
                        name = Regex.Replace(text, "^.*\\s+(?<name>\\S+)\\s*\\(.*$", "${name}");
                    }
                    else
                    {
                        type = StmtType.Other;
                    }
                    InitChildNodes(code.Substring(p1 + 1, p2 - p1 - 1).Trim());
                }

            }
        }

        public string ToString(string prefix = "")
        {
            string res;
            if (type == StmtType.Root)
            {
                res = "";
                foreach (StmtNode node in childNodes)
                {
                    res += node.ToString();
                }
            }
            else
            {
                res = prefix + text + "\n";
                if (type != StmtType.Single) // type == Sinlge
                {
                    res += prefix + "{\n";
                    foreach (StmtNode node in childNodes)
                    {
                        res += node.ToString(prefix + "    ");
                    }
                    res += prefix + "}\n";
                }
            }
            return res;
        }

        public StmtNode Match(string regex)
        {
            StmtNode res = null;
            if (Regex.IsMatch(text, regex))
            {
                res = this;
            }
            else
            {
                foreach (StmtNode node in childNodes)
                {
                    if ((res = node.Match(regex)) != null)
                    {
                        break;
                    }
                }
            }
            return res;
        }

        public void AddChildNode(StmtNode node)
        {
            childNodes.Add(node);
            node.parent = this;
        }

        public void InsertChildNode(int i, StmtNode node)
        {
            childNodes.Insert(i, node);
            node.parent = this;
        }

        public void RemoveChildNode(StmtNode node)
        {
            childNodes.Remove(node);
        }

        public int GetIndex()
        {
            return parent.childNodes.IndexOf(this);
        }
        public void InsertNodeBefore(StmtNode node)
        {
            Trace.Assert(parent != null);
            int index = parent.childNodes.IndexOf(this);
            parent.InsertChildNode(index, node);
        }

        public void InsertNodeAfter(StmtNode node)
        {
            Trace.Assert(parent != null);
            int index = parent.childNodes.IndexOf(this);
            parent.InsertChildNode(index + 1, node);
        }

        public List<StmtNode> selectChildNodes(StmtType t, string id)
        {
            return (List<StmtNode>)childNodes.Where(curr =>
            {
                if (curr.type != t)
                {
                    return false;
                }
                else
                {
                    return Regex.IsMatch(curr.text, id);
                }
            });
        }

        public StmtNode getClassAncestor()
        {
            if (type == StmtType.ClassDeclaration)
            {
                return this;
            }
            else
            {
                return parent.getClassAncestor();
            }
        }

        public StmtNode getFunctionAncestor()
        {
            if (type == StmtType.FunctionDeclaration)
            {
                return this;
            }
            else
            {
                return parent.getFunctionAncestor();
            }
        }
        public StmtNode getRootAncestor()
        {
            if (type == StmtType.Root)
            {
                return this;
            }
            else
            {
                return parent.getRootAncestor();
            }
        }

        public List<StmtNode> getClassChildNodes(string id)
        {
            List<StmtNode> res = new List<StmtNode>();
            foreach (StmtNode n in childNodes)
            {
                if (n.type == StmtType.ClassDeclaration && Regex.IsMatch(n.text, id))
                {
                    res.Add(n);
                }
            }
            return res;
        }

        public List<StmtNode> getFunctionChildNodes(string id)
        {
            List<StmtNode> res = new List<StmtNode>();
            foreach (StmtNode n in childNodes)
            {
                if (n.type == StmtType.FunctionDeclaration && Regex.IsMatch(n.text, id))
                {
                    res.Add(n);
                }
            }
            return res;
        }

        public List<StmtNode> getConstructorChildNodes()
        {
            Trace.Assert(type == StmtType.ClassDeclaration);
            List<StmtNode> res = new List<StmtNode>();
            foreach (StmtNode n in childNodes)
            {
                if (n.type == StmtType.FunctionDeclaration && Regex.IsMatch(n.text, name) && !Regex.IsMatch(n.text, "\\bstatic\\b"))
                {
                    res.Add(n);
                }
            }

            if (res.Count == 0)
            {
                StmtNode defaultConstructor = new StmtNode($"public {name}() {{}}");
                InsertChildNode(0, defaultConstructor);
                res.Add(defaultConstructor);
            }
            return res;
        }

        public List<StmtNode> getStaticConstructorChildNodes()
        {
            Trace.Assert(type == StmtType.ClassDeclaration);
            List<StmtNode> res = new List<StmtNode>();
            foreach (StmtNode n in childNodes)
            {
                if (n.type == StmtType.FunctionDeclaration && Regex.IsMatch(n.text, name) && Regex.IsMatch(n.text, "\\bstatic\\b"))
                {
                    res.Add(n);
                }
            }
            if (res.Count == 0)
            {
                StmtNode staticConstructor = new StmtNode($"static {name}() {{}}");
                InsertChildNode(0, staticConstructor);
                res.Add(staticConstructor);
            }
            return res;
        }

        public List<StmtNode> getSingleChildNodes(string id)
        {
            List<StmtNode> res = new List<StmtNode>();
            foreach (StmtNode n in childNodes)
            {
                if (n.type == StmtType.Single && Regex.IsMatch(n.text, id))
                {
                    res.Add(n); continue;
                }
                if (n.type == StmtType.Other)
                {
                    res.AddRange(n.getSingleChildNodes(id));
                    continue;
                }
            }
            return res;
        }
        
        public StmtNode getLastWSNode(string id)
        {
            if (parent == null) return null;

            Regex regex = new Regex($"\\s{id}\\s*=\\s*[^;,]+");
            for (int i = GetIndex(); i >= 0; i--)
            {
                StmtNode node = parent.childNodes[i];
                Match match = regex.Match(node.text);
                if (match.Success)
                {
                    return node;
                }
            }
            return parent.getLastWSNode(id);
        }
        public string getTypeFromContext(string exp)
        {
            Regex typeOfRegex = new Regex("typeof\\((?<type>.*)\\)");
            Match match = typeOfRegex.Match(exp);
            if (match.Success)
            {
                return match.Groups["type"].Value;
            } else
            {
                string id = exp;
                StmtNode lastWSNode = getLastWSNode(id);
                if (lastWSNode == null) return "";
                Regex valueRegex = new Regex($"\\s{id}\\s*=\\s*(?<value>[^;,]+)");
                Match valueMatch = valueRegex.Match(lastWSNode.text);
                if (valueMatch.Success)
                {
                    return getTypeFromContext(valueMatch.Groups["value"].Value.Trim());
                }
            }
            return "";
        }

        public void SetOwnerTreeForRoot(StmtTree tree)
        {
            Trace.Assert(type == StmtType.Root);
            ownerTree = tree;
        }

        public StmtTree GetOwnerTree()
        {
            return getRootAncestor().ownerTree;
        }


        private void InitChildNodes(string stmts)
        {

            int p1 = 0, p0 = 0;
            Stack<char> stack = new Stack<char>();
            for (p1 = 0; p1 < stmts.Length; p1++)
            {
                switch (stmts[p1])
                {
                    case ';':
                        {
                            if (stack.Count == 0)
                            {
                                childNodes.Add(new StmtNode(stmts.Substring(p0, p1 - p0 + 1).Trim()));  //??是否删除回车
                                p0 = p1 + 1;
                            }
                            break;
                        }
                    case '{':
                        {
                            if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                            stack.Push('{'); break;
                        }
                    case '}':
                        {
                            if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                            char c = stack.Pop();
                            Trace.Assert(c.Equals('{'));
                            if (stack.Count == 0)
                            {
                                childNodes.Add(new StmtNode(stmts.Substring(p0, p1 - p0 + 1).Trim()));  //??是否删除回车
                                p0 = p1 + 1;
                            }
                            break;
                        }
                    case '(':
                        {
                            if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                            stack.Push('('); break;
                        }
                    case ')':
                        {
                            if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                            char c = stack.Pop();
                            Trace.Assert(c.Equals('('));
                            break;
                        }
                    case '"':
                    case '\'':
                        {
                            if (stack.Count != 0 && stack.Peek().Equals(stmts[p1]))
                            {
                                if (stmts[p1 - 1] != '\\' || stmts[p1 - 2] == '\\')
                                {
                                    stack.Pop();
                                }
                            }
                            else
                            {
                                if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                                stack.Push(stmts[p1]);
                            }
                            break;
                        }

                }

            }
            foreach (StmtNode child in childNodes)
            {
                child.parent = this;
            }
        }

        

        private int findFirstChar(string s, char c)
        {
            int i = 0;
            Stack<char> stack = new Stack<char>();
            for (i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '{':
                        {
                            if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                            return i;
                        }
                    case '"':
                    case '\'':
                        {
                            if (stack.Count != 0 && stack.Peek().Equals(s[i]))
                            {
                                if (s[i - 1] != '\\' || s[i - 2] == '\\')
                                {
                                    stack.Pop();
                                }
                            }
                            else
                            {
                                if (stack.Count != 0 && (stack.Peek().Equals('\'') || stack.Peek().Equals('"'))) break;
                                stack.Push(s[i]);
                            }
                            break;
                        }

                }

            }

            return -1;
        }

    }


    public enum StmtType
    {
        Root,
        ClassDeclaration,
        FunctionDeclaration,
        Single,
        Other
    }
}
