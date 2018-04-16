using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace smsup_server.Controllers
{
    public class NEEDREPLYController : ApiController
    {
        public HttpResponseMessage Post(dynamic jsonr)
        {
            string returnJson;
            JObject jObject = new JObject();
            try
            {
                string json = jsonr.ToString();
                JObject jrow = JObject.Parse(json.ToString());
                Data.Mongo.InsertNeedReply((string)jrow["phone_number"], (string)jrow["send_time"]);
                //returnJson = @"{""code"": 0, ""msg"": ""成功""}";
                jObject.Add("code", 0);
                jObject.Add("msg", "成功");
            }
            catch (Exception)
            {
                jObject.Add("code", 1);
                jObject.Add("msg", "失败");
                //returnJson = @"{""code"": 1,""msg"": ""失败""}";
            }
            returnJson = jObject.ToString().Replace("\r\n", "");
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(returnJson, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }
        //public string Get()
        //{
            
        //}
    }
}
