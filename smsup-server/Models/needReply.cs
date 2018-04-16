using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsup_server.Models
{
    public class NeedReply
    {
        public ObjectId _id { get; set; }
        public string Phone { get; set; }
        public bool IsReply { get; set; }
        public DateTime SendTime { get; set; }
        public DateTime ReplyTime { get; set; }
    }
}