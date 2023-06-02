using Newtonsoft.Json.Linq;
using scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace backend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MigrationController : ApiController
    {
        // GET: api/Migration
        public JObject Get()
        {
            return ScannerUsage.GetRule();
        }

        // GET: api/Migration/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Migration
        public string Post(JObject value)
        {
            string code = value["Code"].ToString();
            string typeSuffix = value["TypeSuffix"].ToString();
            string[] otherFileCodes = JArray.Parse(value["OtherFileCodes"].ToString())
                                            .Select((curr) => (string)curr)
                                            .ToArray();
            string s = value["Rules"].ToString();
            JArray rules = JArray.Parse(s);

            ScannerUsage scannerUsage = new ScannerUsage(typeSuffix, code, otherFileCodes, rules);
            return scannerUsage.Migrate();
        }

        // PUT: api/Migration/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Migration/5
        public void Delete(int id)
        {
        }
    }
}
