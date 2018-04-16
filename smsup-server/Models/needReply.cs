using MongoDB.Bson;
using System;

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