using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace smsup_server.Controllers
{
    public class SMSUPController : ApiController
    {
        public string Post(dynamic jsonr)
        {
            string returnJson;
            string json = jsonr.ToString();
            try
            {
                if (!json.StartsWith("[") || !json.EndsWith("]"))
                {
                    json = "[" + json + "]";
                }
                JArray array = JArray.Parse(json);
                foreach (var item in array.Children())
                {
                    JObject jrow = JObject.Parse(item.ToString());
                    Data.Mongo.InsertUpMessage((string)jrow["phone_number"], (string)jrow["content"], (string)jrow["send_time"]);
                    Data.Mongo.UpdateReplyStatus((string)jrow["phone_number"], (string)jrow["send_time"]);
                }
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
