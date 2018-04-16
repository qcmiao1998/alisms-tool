using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace smsup_server.Controllers
{
    public class NEEDREPLYController : ApiController
    {
        public string Post(dynamic jsonr)
        {
            string returnJson;
            try
            {
                string json = jsonr.ToString();
                JObject jrow = JObject.Parse(json.ToString());
                Data.Mongo.InsertNeedReply((string)jrow["phone_number"], (string)jrow["send_time"]);
                returnJson = @"{
                                ""code"": 0,
                                ""msg"": ""成功""
                            }";
            }
            catch (Exception)
            {
                returnJson = @"{
                                ""code"": 1,
                                ""msg"": ""失败""
                            }";
            }
            return returnJson;
        }
    }
}
