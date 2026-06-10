using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TrafficWatch.Control
{
    public class DataChart : INotifyPropertyChanged
    {
        public DataChart()
        {
            MaxValue = double.NaN;
            MinValue = double.NaN;
            YFormatter = value => value + "KB/S";
            Labels = new List<string>();
        }

        public List<string> Labels { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }

        public Func<double, string> YFormatter { get; set; }
        
        string _DateTime;
        public double Downloaded
        {
            set
            {
                _DateTime = DateTime.Now.ToString("hh:mm:ss");
                Labels.Add(_DateTime);
                if (!History)
                {
                    if (Labels.Count > 1)
                        Labels.RemoveAt(0);
                }
                else if (History && Labels.Count >= HistoryLenth)
                {
                    if (Labels.Count > HistoryLenth)
                        Labels.RemoveRange(0, Labels.Count - HistoryLenth);
                }
            }
        }
        
        public double Uploaded
        {
            set
            {
                // Upload handling if needed
            }
        }
        
        public bool History { get; internal set; }
        
        public int HistoryLenth
        {
            get => historyLenth;
            set { historyLenth = value; OnPropertyChanged("HistoryLenth"); }
        }

        private int historyLenth = 60;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
