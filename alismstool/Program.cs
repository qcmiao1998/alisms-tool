using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                if (SendSMS.Send(templateCode, phoneNumber, param) != 0)
                    errList.Add(phoneNumber);
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
    }
}
