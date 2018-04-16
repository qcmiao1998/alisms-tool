using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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