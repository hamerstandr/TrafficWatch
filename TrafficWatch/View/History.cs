using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TrafficWatch.Services.Detail;

namespace TrafficWatch
{
    public class History
    {
        public Data Data = new Data();
        readonly string PathFileData;
        DataHistory Now;
        public History()
        {
            PathFileData = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData), typeof(App).Assembly.ManifestModule.Name.Replace(".exe", ""));
            Directory.CreateDirectory(PathFileData);
            PathFileData = Path.Combine(PathFileData, "Data.xml");
            Data = Data.Load(PathFileData);
        }
        public EventHandler<string> ChengEvent;
        public void Add(UDStatistic statistics, DateTime date)
        {
            Data.Total.Download += statistics.download;
            Data.Total.Upload += statistics.upload;
            HowGetNew(date);
            Now.Download += statistics.download;
            Now.Upload += statistics.upload;
            ChengEvent?.Invoke(this, "Total");
        }
        void HowGetNew(DateTime date)
        {
            Now = Data.ListHistory.Find(x => x.Date.Month == date.Month && x.Date.Day == date.Day);
            if (Now == null)
            {
                Now = new DataHistory() { Date = DateTime.Now };
                Data.ListHistory.Add(Now);
            }
        }
        internal void Save()
        {
            Data.Save(Data, PathFileData);
        }
    }
    [XmlRoot("Data", Namespace = "hamerstandr",
IsNullable = false)]
    public class Data
    {
        [XmlAttribute]
        public DateTime Date;
        [XmlElement]
        public DataHistory Total;
        [XmlArray("ListHistory")]
        public List<DataHistory> ListHistory;
        public static void Save(object obj, string path)
        {
            XmlSerializer s = new XmlSerializer(obj.GetType());
            using (StreamWriter writer = new StreamWriter(path))
            {
                s.Serialize(writer, obj);
            }
        }

        public static Data Load(string path)
        {
            try {
                XmlSerializer s = new XmlSerializer(typeof(Data));
                if (File.Exists(path))
                    using (StreamReader reader = new StreamReader(path))
                    {
                        object obj = s.Deserialize(reader);
                        return (Data)obj;
                    }
                else
                    return new Data()
                    {
                        ListHistory = new List<DataHistory>(),
                        Total = new DataHistory(),
                        Date = DateTime.Now
                    };
            }
            catch {
                return new Data()
                {
                    ListHistory = new List<DataHistory>(),
                    Total = new DataHistory(),
                    Date = DateTime.Now
                };
            }
            
        }
    }
    public class DataHistory
    {
        [XmlAttribute]
        public DateTime Date;
        [XmlAttribute]
        public double Download;
        [XmlAttribute]
        public double Upload;
    }
}