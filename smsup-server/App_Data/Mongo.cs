using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB;
using smsup_server.Models;

namespace smsup_server.Data
{
    public class Mongo
    {
        static MongoClient client;
        static IMongoDatabase database;
        static IMongoCollection<SmsUp> upTable;
        static IMongoCollection<NeedReply> needReplyTable;
        public static void Init()
        {
            client = new MongoClient(ConfigurationManager.AppSettings["mongodb"]);
            database = client.GetDatabase("smsdb");
            upTable = database.GetCollection<SmsUp>("smsup");
            needReplyTable = database.GetCollection<NeedReply>("needreply");
        }
        public static void InsertUpMessage(string phone, string content, string time)
        {
            SmsUp sms = new SmsUp();
            sms.Phone = phone;
            sms.Content = content;
            sms.Time = DateTime.Parse(time);
            upTable.InsertOne(sms);
        }
        public static void InsertNeedReply(string phone, string sendTime)
        {
            NeedReply needReply = new NeedReply();
            needReply.Phone = phone;
            needReply.IsReply = false;
            needReply.SendTime = DateTime.Parse(sendTime);
            needReplyTable.InsertOne(needReply);
        }
        public static void UpdateReplyStatus(string phone, string replyTime)
        {
            var filter = Builders<NeedReply>.Filter.Eq("Phone", phone);
            var updateTime = Builders<NeedReply>.Update.Set("ReplyTime", DateTime.Parse(replyTime));
            var updateBool = Builders<NeedReply>.Update.Set("IsReply", true);
            needReplyTable.UpdateManyAsync(filter, updateTime);
            needReplyTable.UpdateManyAsync(filter, updateBool);
        }
    }
}