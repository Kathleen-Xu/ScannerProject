using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scanner.MidXaml
{
    public abstract class MidXValue
    {
        public string valueText;

        public MidXValue(string value)
        {
            valueText = value;
        }

        public static MidXValue CreateMidXValue(string value)
        {
            string text = value.Trim();
            
            Regex regex = new Regex("^\\{(?<content>.*)\\}$");
            Match match = regex.Match(text);
            if (match.Success)
            {
                string content = match.Groups["content"].Value.Trim();

                regex = new Regex("^(?<bindingMode>Binding|TemplateBinding)\\b(?<infos>.*)$");
                match = regex.Match(content);
                if (match.Success)
                {
                    string bindingMode = match.Groups["bindingMode"].Value;
                    string infos = match.Groups["infos"].Value.Trim();
                    return new MidXBindingValue(bindingMode, infos, text);
                }

                regex = new Regex("^(?<resourceType>DynamicResource|StaticResource|RelativeSource|x\\s*\\:\\s*(Null|Static|Type|Array))\\b(?<resourceName>.*)?$");
                match = regex.Match(content);
                if (match.Success)
                {
                    string resourceType = match.Groups["resourceType"].Value;
                    string resourceName = match.Groups["infos"].Value != null ? match.Groups["resourceName"].Value.Trim() : "";
                    return new MidXResourceValue(resourceType, resourceName, text);
                }

                regex = new Regex("^(?<extension>\\w+\\s*\\:\\w+)\\b(?<infos>.*)$");
                match = regex.Match(content);
                if (match.Success)
                {
                    string extension = match.Groups["extension"].Value;
                    string infos = match.Groups["infos"].Value.Trim();
                    return new MidXExtensionValue(extension, infos, text);
                }

                throw new Exception("Fail to create value");

            } else
            {
                return new MidXRawValue(text);
            }

        }
    }

    public class MidXRawValue : MidXValue
    {
        public MidXRawValue(string value) : base(value)
        {

        }
    }

    public class MidXBindingValue : MidXValue
    {
        public string bindingMode;
        public Dictionary<string, MidXValue> bingdingInfos = new Dictionary<string, MidXValue>();

        public MidXBindingValue(string bindingMode, string infos, string value) : base(value)
        {
            this.bindingMode = bindingMode;
            string[] infoArray = infos.Split(',').Select(curr => curr.Trim()).ToArray();

            foreach (string info in infoArray)
            {
                int index = info.IndexOf('=');

                if (index == -1)
                {
                    bingdingInfos.Add("Path", CreateMidXValue(info.Trim()));
                } else
                {
                    string n = info.Substring(0, index);
                    string v = info.Substring(index + 1);
                    bingdingInfos.Add(n, CreateMidXValue(v));
                }
            }
        }
    }

    public class MidXResourceValue : MidXValue
    {
        public string resourceType;
        public string resourceName;

        public MidXResourceValue(string resourceType, string resourceName, string value) : base(value)
        {
            this.resourceType = resourceType;
            this.resourceName = resourceName;
        }
    }

    public class MidXExtensionValue : MidXValue
    {
        public string extension;
        public Dictionary<string, MidXValue> extensionInfos = new Dictionary<string, MidXValue>();

        public MidXExtensionValue(string extension, string infos, string value) : base(value)
        {
            this.extension = extension;
            string[] infoArray = infos.Split(',').Select(curr => curr.Trim()).ToArray();

            foreach (string info in infoArray)
            {
                string[] tmp = info.Split('=').Select(curr => curr.Trim()).ToArray();
                if (tmp.Length == 1)
                {
                    extensionInfos.Add("Path", CreateMidXValue(tmp[0]));
                }
                else
                {
                    Trace.Assert(tmp.Length == 2);
                    extensionInfos.Add(tmp[0], CreateMidXValue(tmp[1]));
                }
            }
        }
    }
}
