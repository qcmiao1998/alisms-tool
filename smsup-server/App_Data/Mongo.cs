using MongoDB.Driver;
using smsup_server.Models;
using System;
using System.Configuration;

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
            database = client.GetDatabase(ConfigurationManager.AppSettings["dbname"]);
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
        public static void InsertNeedReply(string phone, string name, string sendTime)
        {
            NeedReply needReply = new NeedReply();
            needReply.Phone = phone;
            needReply.Name = name;
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