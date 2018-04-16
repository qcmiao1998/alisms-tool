using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dybaseapi.Model.V20170525;
using Aliyun.MNS;
using Aliyun.MNS.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

namespace alismstool
{
    class SmsUp
    {
        private static int maxThread = 2;
        static String accessId = ConfigurationManager.AppSettings["AccessKeyId"];
        static String accessSecret = ConfigurationManager.AppSettings["AccessKeySecret"];
        static String messageType = "SmsUp";//消息类型目前有4种. 短信回执:SmsReport; 短信上行:SmsUp; 语音回执:VoiceReport; 流量回执:FlowReport;
        static String queueName = ConfigurationManager.AppSettings["queueName"];// 短信上行的队列名称. 格式是 "前缀(Alicom-Queue-)+阿里云uid+消息类型"
        static String domainForPop = "dybaseapi.aliyuncs.com";
        static String regionIdForPop = "cn-hangzhou";
        static String productName = "Dybaseapi";
        static IAcsClient acsClient = null;

        public static IAcsClient InitAcsClient(String regionIdForPop, String accessId, String accessSecret, String productName, String domainForPop)
        {
            IClientProfile profile = DefaultProfile.GetProfile(regionIdForPop, accessId, accessSecret);
            DefaultProfile.AddEndpoint(regionIdForPop, regionIdForPop, productName, domainForPop);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            return acsClient;
        }

        // 初始化环境
        private static void InitData()
        {
            acsClient = InitAcsClient(regionIdForPop, accessId, accessSecret, productName, domainForPop);
        }

        public static void GetSmsUp()
        {
            Console.WriteLine("Enter the save path:");
            string savePath;
            savePath = Console.ReadLine();
            SaveTask.Init(savePath);
            InitData();
            for (int i = 0; i < maxThread; i++)
            {
                MainTask testTask = new MainTask("PullMessageTask-thread-" + i, messageType, queueName, acsClient);
                Thread t = new Thread(new ThreadStart(testTask.Handle));
                //启动线程
                t.Start();
            }
            Console.WriteLine("Any Key To EXIT");
            Console.ReadKey();
            SaveTask.Close();
            Environment.Exit(0);
        }
    }

    class SaveTask
    {
        class SmsInfo
        {
            public string phone { get; set; }
            public string content { get; set; }
            public string time { get; set; }
            public override string ToString()
            {
                string re = string.Format("{0},{1},{2}", phone, content, time);
                //JObject jObject = new JObject();
                //jObject.Add("phone", phone);
                //jObject.Add("content", content);
                //jObject.Add("time", time);
                //return jObject.ToString();
                return re;
            }
        }
        //public static CsvWriter csv;
        private static dynamic csvWriter;
        public static void Init(string savePath)
        {
            //TextWriter csvTextWriter = File.CreateText(savePath);
            //dynamic textWriter = TextWriter.Synchronized(csvTextWriter);
            //csv = new CsvWriter(textWriter);
            //csv.WriteHeader<SmsInfo>();
            //csv.WriteHeader(String);
            //csv.NextRecord();
            StreamWriter writer = new StreamWriter(savePath, true);
            csvWriter = StreamWriter.Synchronized(writer); 
        }
        public static void Close()
        {
            csvWriter.Close();
        }
        public static bool WriteRecord(string json)
        {
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
                    SmsInfo smsRow = new SmsInfo();
                    smsRow.phone = (string)jrow["phone_number"];
                    smsRow.content = (string)jrow["content"];
                    smsRow.time = (string)jrow["send_time"];
                    Console.WriteLine(smsRow.ToString());
                    //csv.WriteRecord(smsRow);
                    //csv.NextRecord();
                    csvWriter.WriteLine(smsRow.ToString());
                }
            }
            catch (Exception e)
            {
                Console.Write("ERR: " + e.Message);
                return false;
            }
            return true;
        }
    }

    class MainTask
    {
        private object o = new object();
        private int sleepTime = 50;
        public String Name { get; private set; }
        public String MessageType { get; private set; }
        public String QueueName { get; private set; }
        public int TaskID { get; private set; }
        public IAcsClient AcsClient { get; private set; }

        public MainTask(String name, String messageType, String queueName, IAcsClient acsClient)
        {
            this.Name = name;
            this.MessageType = messageType;
            this.QueueName = queueName;
            this.AcsClient = acsClient;
        }
        long bufferTime = 60 * 2;//过期时间小于2分钟则重新获取，防止服务器时间误差
        String mnsAccountEndpoint = "https://1943695596114318.mns.cn-hangzhou.aliyuncs.com/";//阿里通信消息的endpoint,固定
        Dictionary<string, QueryTokenForMnsQueueResponse.MessageTokenDTO_> tokenMap = new Dictionary<string, QueryTokenForMnsQueueResponse.MessageTokenDTO_>();
        Dictionary<string, Queue> queueMap = new Dictionary<string, Queue>();

        public QueryTokenForMnsQueueResponse.MessageTokenDTO_ GetTokenByMessageType(IAcsClient acsClient, String messageType)
        {

            QueryTokenForMnsQueueRequest request = new QueryTokenForMnsQueueRequest();
            request.MessageType = messageType;
            QueryTokenForMnsQueueResponse queryTokenForMnsQueueResponse = acsClient.GetAcsResponse(request);
            QueryTokenForMnsQueueResponse.MessageTokenDTO_ token = queryTokenForMnsQueueResponse.MessageTokenDTO;
            return token;
        }

        /// 处理消息
        public void Handle()
        {
            while (true)
            {
                try
                {
                    QueryTokenForMnsQueueResponse.MessageTokenDTO_ token = null;
                    Queue queue = null;
                    lock (o)
                    {
                        if (tokenMap.ContainsKey(MessageType))
                        {
                            token = tokenMap[MessageType];
                        }
                        if (queueMap.ContainsKey(QueueName))
                        {
                            queue = queueMap[QueueName];
                        }
                        TimeSpan ts = new TimeSpan(0);
                        if (token != null)
                        {
                            DateTime b = Convert.ToDateTime(token.ExpireTime);
                            DateTime c = Convert.ToDateTime(DateTime.Now);
                            ts = b - c;
                        }
                        if (token == null || ts.TotalSeconds < bufferTime || queue == null)
                        {
                            token = GetTokenByMessageType(AcsClient, MessageType);
                            IMNS client = new MNSClient(token.AccessKeyId, token.AccessKeySecret, mnsAccountEndpoint, token.SecurityToken);
                            queue = client.GetNativeQueue(QueueName);
                            if (tokenMap.ContainsKey(MessageType))
                            {
                                tokenMap.Remove(MessageType);
                            }
                            if (queueMap.ContainsKey(QueueName))
                            {
                                queueMap.Remove(QueueName);
                            }
                            tokenMap.Add(MessageType, token);
                            queueMap.Add(QueueName, queue);
                        }
                    }
                    BatchReceiveMessageResponse batchReceiveMessageResponse = queue.BatchReceiveMessage(16);
                    List<Message> messages = batchReceiveMessageResponse.Messages;
                    for (int i = 0; i <= messages.Count - 1; i++)
                    {
                        try
                        {
                            byte[] outputb = Convert.FromBase64String(messages[i].Body);
                            string orgStr = Encoding.UTF8.GetString(outputb);
                            //Console.WriteLine(orgStr);
                            //TODO 具体消费逻辑,待客户自己实现.
                            if(SaveTask.WriteRecord(orgStr))
                                //消费成功的前提下删除消息
                                queue.DeleteMessage(messages[i].ReceiptHandle);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERR: " + e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Handle exception: " + e.Message);
                }
                Thread.Sleep(sleepTime);
            }
        }


    }
}
