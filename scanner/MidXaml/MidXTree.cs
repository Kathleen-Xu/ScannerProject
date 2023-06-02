using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner.MidXaml
{
    public class MidXTree
    {
        public MidXNode root;

        public MidXTree(XmlDocument xmlDocument)
        {
            root = MidXNode.CreateMidXNode(xmlDocument.DocumentElement);
        }

        public void PrintSource()
        {
            Console.Write(root.ToString(""));
        }
        public void PrintTarget()
        {
            Console.Write(root.ToTargetString(""));
        }
    }
}
