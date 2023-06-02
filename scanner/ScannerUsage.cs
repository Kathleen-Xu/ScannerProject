using Newtonsoft.Json.Linq;
using scanner.MidXaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace scanner
{
    public class ScannerUsage
    {
        static string updateListPath = @"D:\Code\code_scanner\FirstTransformationTry\ScannerProject\scanner\Example\updateListTest.json";
        Scanner scanner;
        string typeSuffix;
        string code;
        string[] otherFileCodes;
        JArray rules;

        public ScannerUsage(string typeSuffix, string code, string[] otherFileCodes, JArray rules)
        {
            this.typeSuffix = typeSuffix;
            this.code = code;
            this.otherFileCodes = otherFileCodes;
            this.rules = rules;
            
        }
        static public JObject GetRule()
        {
            string json = File.ReadAllText(updateListPath);
            return JObject.Parse(json);
        }
        public string Migrate()
        {
            if (typeSuffix == "cs")
            {
                scanner = new Scanner().LoadScanner(rules);
                StmtTree tree = new StmtTree(code, otherFileCodes);
                tree.Print();
                scanner.Scan(tree);
                tree.Print();
                return tree.root.ToString();
            } else
            {
                Trace.Assert(typeSuffix == "xaml");
                XmlDocument doc = XmlUtil.Parse(code);
                MidXTree xamlTree = new MidXTree(doc);
                //xamlTree.PrintSource();
                //xamlTree.PrintTarget();
                return xamlTree.root.ToTargetString("");
            }

            
        }
    }
}
