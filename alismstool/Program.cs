using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;

namespace alismstool
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Choose the function:");
                Console.WriteLine("1. Send Message");
                Console.WriteLine("2. Get Returns");
                string method;
                try
                {
                    method = Console.ReadLine();
                }
                catch (Exception)
                {
                    Console.WriteLine("Unsupport Character");
                    continue;
                }
                switch (method)
                {
                    case "1":
                        SendMessage();
                        break;
                    case "2":
                        GetReturn();
                        break;
                    default:
                        break;
                }
            }
        }
        static void SendMessage()
        {
            Console.WriteLine("\nSend Message Method");

            // Start Read CSV
            Console.WriteLine("Enter Csv Path:");
            string csvPath;
            csvPath = Console.ReadLine();
            DataTable sendList = new DataTable();
            try
            {
                using (TextReader csvTextReader = File.OpenText(csvPath))
                {
                    CsvReader csv = new CsvReader(csvTextReader);
                    csv.Configuration.HasHeaderRecord = false;
                    // READ HEAD
                    csv.Read();
                    string head;
                    for (int i = 0; csv.TryGetField<string>(i, out head); i++)
                    {
                        sendList.Columns.Add(head, typeof(string));
                    }
                    while (csv.Read())
                    {
                        string value;
                        DataRow dataRow = sendList.NewRow();
                        for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                        {
                            dataRow[i] = value;
                        }
                        sendList.Rows.Add(dataRow);
                    }
                }
                Console.WriteLine("Read Csv Succeed");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERR: " + e.Message);
                return;
            }

            // Get template code
            string templateCode;
            Console.WriteLine("Enter Template Code:");
            templateCode = Console.ReadLine();

            // Is Add To Needding Reply List
            Console.WriteLine("Is add to need-reply list?[n]");
            bool isAddToNeedReply = Console.ReadLine().ToLower() == "y";

            // Start sending message
            // get params list
            Console.WriteLine("Select Columns in Params (Use space to split)");
            foreach (DataColumn column in sendList.Columns)
            {
                Console.WriteLine(sendList.Columns.IndexOf(column) + ": " + column.ColumnName);
            }
            string paramsIndex = Console.ReadLine();
            string[] paramsList = paramsIndex.Split(' ');
            List<string> errList = new List<string>();
            foreach (DataRow sendRow in sendList.Rows)
            {
                // Read Phone Number
                string phoneNumber;
                try
                {
                    phoneNumber = sendRow["phone"].ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERR: " + e.Message);
                    return;
                }
                // get param json list
                JObject jObject = new JObject();
                foreach (string tid in paramsList)
                {
                    DataColumn column = sendList.Columns[int.Parse(tid)];
                    jObject.Add(column.ColumnName, new JValue(sendRow[column].ToString()));
                }
                string param = jObject.ToString();
                // Send Message
                if (SendSMS.Send(templateCode, phoneNumber, param) != 0)
                    errList.Add(phoneNumber);
                // Add to Need Reply List
                if (isAddToNeedReply)
                {
                    string url = ConfigurationManager.AppSettings["ReplyServerURL"];
                    JObject needReplyJson = new JObject();
                    needReplyJson.Add("phone_number", phoneNumber);
                    needReplyJson.Add("send_time", DateTime.Now.ToString());
                    string returnJson = HttpPost(url, needReplyJson.ToString());
                    JObject rJson = JObject.Parse(returnJson);
                    if ((string)rJson["code"] != "0")
                    {
                        Console.WriteLine("Err when add to need reply list. " + phoneNumber);
                    }
                }
            }
            Console.WriteLine("Finished");
            if (errList.Count != 0)
            {
                Console.WriteLine("Err List:");
                Console.WriteLine(errList.ToString());
            }
            
        }
        static void GetReturn()
        {
            Console.WriteLine("\nGet Return Method");
            SmsUp.GetSmsUp();

        }
        static string HttpPost(string url, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
