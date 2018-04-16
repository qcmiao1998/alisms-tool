using MongoDB.Bson;
using System;

namespace smsup_server.Models
{
    public class SmsUp
    {
        public ObjectId _id { get; set; }
        public string Phone { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }
}