using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace scanner
{
    public class Scanner
    {
        public List<Update> updates = new List<Update>();

        private Change LoadChange(JObject ch)
        {
            JObject pos = JObject.Parse(ch["position"].ToString());
            string path = pos["path"].ToString();
            Position position = new Position(path);

            if (ch.ContainsKey("oldPattern"))
            {
                string oldPattern = ch["oldPattern"].ToString();
                string newPattern = ch["newPattern"].ToString();
                return new TextEditChange(oldPattern, newPattern, position);
            }
            else if (ch.ContainsKey("matcher"))
            {
                string matcher = ch["matcher"].ToString();
                return new DeletionChange(matcher, position);
            }
            else if (ch.ContainsKey("codeToAdd"))
            {
                string codeToAdd = ch["codeToAdd"].ToString();
                string relativePos = ch["relativePos"].ToString();
                return new AdditionChange(codeToAdd, relativePos, position);
            }
            else
            {
                Trace.Assert(ch.ContainsKey("apiName"));
                string apiName = ch["apiName"].ToString();

                Dictionary<string, string> overridePairs = new Dictionary<string, string>();
                JArray ps = JArray.Parse(ch["overridePairs"].ToString());
                foreach (var p in ps.Select((curr) => JObject.Parse(curr.ToString())))
                {
                    string oldApi = p["oldApi"].ToString();
                    string newApi = p["newApi"].ToString();
                    overridePairs.Add(oldApi, newApi);
                }

                Dictionary<string, List<Change>> sideEffects = new Dictionary<string, List<Change>>();
                JArray ses = JArray.Parse(ch["sideEffects"].ToString());
                foreach (var s in ses.Select((curr) => JObject.Parse(curr.ToString())))
                {
                    string condition = s["condition"].ToString();

                    JArray chs = JArray.Parse(s["attachedChanges"].ToString());
                    List<Change> attachedChanges = new List<Change>();
                    foreach(var c in chs.Select((curr) => JObject.Parse(curr.ToString())))
                    {
                        attachedChanges.Add(LoadChange(c));
                    }
                    sideEffects.Add(condition, attachedChanges);
                }

                return new NodeEditChange(apiName, overridePairs, sideEffects, position);
            }
        }

        private Update LoadUpate(JObject up)
        {
            string trigger = up["trigger"].ToString();
            string mode = up["mode"].ToString();

            List<Change> changes = new List<Change>();

            JArray chs = JArray.Parse(up["changes"].ToString());
            foreach (var ch in chs.Select((curr) => JObject.Parse(curr.ToString())))
            {
                changes.Add(LoadChange(ch));
            }
            return new Update(trigger, mode, changes);
        }

        public Scanner LoadScanner(string json)
        {
            JObject jobj = JObject.Parse(json);
            JArray ups = JArray.Parse(jobj["updates"].ToString());
            foreach (var up in ups.Select((curr) => JObject.Parse(curr.ToString())))
            {
                updates.Add(LoadUpate(up));
            }

            return this;
        }
        public Scanner LoadScanner(JArray ups)
        {
            foreach (var up in ups.Select((curr) => JObject.Parse(curr.ToString())))
            {
                updates.Add(LoadUpate(up));
            }

            return this;
        }
        public Scanner LoadScannerFromFilePath(string filePath)
        {
            string json = File.ReadAllText(filePath);
            JObject jobj = JObject.Parse(json);
            JArray ups = JArray.Parse(jobj["updates"].ToString());
            foreach (var up in ups.Select((curr)=> JObject.Parse(curr.ToString())))
            {
                updates.Add(LoadUpate(up));
            }

            return this;
        }

        public void Scan(StmtTree tree)
        {
            foreach (Update update in updates)
            {
                update.Apply(tree);
            }
        }
        
    }
}
