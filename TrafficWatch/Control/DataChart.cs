using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TrafficWatch.Control
{
    public class DataChart : INotifyPropertyChanged
    {
        public DataChart()
        {
            MaxValue = double.NaN;
            MinValue = double.NaN;
            YFormatter = value => value + "KB/S";
            //varDownloadBrush = new LinearGradientBrush();
            //DownloadBrush.GradientStops.Add(new GradientStop(Colors.Green, 0));
            //DownloadBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

            //var UploadBrush = new LinearGradientBrush();
            //UploadBrush.GradientStops.Add(new GradientStop(Colors.Red, 0));
            //UploadBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            //DownloadBrush =new SolidColorBrush(Colors.Green);
            //DownloadMapper = Mappers.Xy<ObservableValue>()
            //.X((item, index) => index)
            //.Y(item => item.Value)
            //.Fill(item => item.Value > 200 ? DownloadBrush : null)
            //.Stroke(item => item.Value > 200 ? DownloadBrush : null);

            LastHourSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Download",
                        AreaLimit = 1,Stroke=new SolidColorBrush(Colors.Green),
                        Values = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),

                        }
                    },
                    new LineSeries
                    {
                        Title = "Upload",
                        Values = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),
                            new ObservableValue(0),

                        }
                    }
                };

            Labels = new List<string>() { "0:0:0", "0:0:0", "0:0:0", "0:0:0", "0:0:0", "0:0:0", "0:0:0", "0:0:0", "0:0:0" };
            //LastHourSeries[0].Values.Add(new ObservableValue(_trend));
            //LastHourSeries[0].Values.RemoveAt(0);
            //SetLecture();

            //_trend = 8;
            //_trend2 = 8;
            //r = new Random();
            //System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            //dispatcherTimer.Tick += DispatcherTimer_Tick;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            //dispatcherTimer.Start();
        }
        //public Brush DownloadBrush { get; set; }
        //public CartesianMapper<ObservableValue> DownloadMapper { get; set; }
        public List<string> Labels { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        //Random r;
        //double _trend = 0;
        //double _trend2 = 0;
        //private void DispatcherTimer_Tick(object sender, EventArgs e)
        //{

        //    _trend += (r.NextDouble() > 0.3 ? 1 : -1) * r.Next(0, 5);
        //    _trend2 += (r.NextDouble() > 0.3 ? 1 : -1) * r.Next(0, 5);
        //    LastHourSeries[0].Values.Add(new ObservableValue(_trend));
        //    LastHourSeries[0].Values.RemoveAt(0);
        //    LastHourSeries[1].Values.Add(new ObservableValue(_trend2));
        //    LastHourSeries[1].Values.RemoveAt(0);
        //    _DateTime = DateTime.Now;
        //    Labels.Add(_DateTime.Hour + ":" + _DateTime.Minute + ":" + _DateTime.Second);
        //    Labels.RemoveAt(0);
        //    //Action action = delegate
        //    //{
        //    //    LastHourSeries[0].Values.Add(new ObservableValue(_trend));
        //    //    LastHourSeries[0].Values.RemoveAt(0);
        //    //    SetLecture();
        //    //};
        //    //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
        //}

        public Func<double, string> YFormatter { get; set; }
        //double _Max = 0;
        //public double Max
        //{
        //    get { return _Max; }
        //    set
        //    {
        //        if (value > Max)
        //            _Max = value;
        //    }
        //}
        string _DateTime;
        public double Downloaded
        {
            set
            {
                LastHourSeries[0].Values.Add(new ObservableValue(value));
                _DateTime = DateTime.Now.ToString("hh:mm:ss");
                Labels.Add(_DateTime); //_DateTime.Hour+":"+ _DateTime.Minute + ":" + _DateTime.Second);
                if (!History)
                {
                    LastHourSeries[0].Values.RemoveAt(0);
                    Labels.RemoveAt(0);
                }
                else if (History && LastHourSeries[0].Values.Count >= HistoryLenth)
                {
                    for (int i = 0; i <= LastHourSeries[0].Values.Count - HistoryLenth; i++)
                        LastHourSeries[0].Values.RemoveAt(0);
                    if (Labels.Count >= HistoryLenth)
                        Labels.RemoveRange(0, Labels.Count - HistoryLenth);
                }

            }
        }
        public double Uploaded
        {
            set
            {
                LastHourSeries[1].Values.Add(new ObservableValue(value));
                if (!History)
                    LastHourSeries[1].Values.RemoveAt(0);
                else if (History && LastHourSeries[1].Values.Count >= HistoryLenth)
                {
                    for (int i = 0; i <= LastHourSeries[1].Values.Count - HistoryLenth; i++)
                        LastHourSeries[1].Values.RemoveAt(0);
                }
            }
        }
        public SeriesCollection LastHourSeries { get; set; }
        public bool History { get; internal set; }
        public int HistoryLenth
        {
            get => historyLenth;
            set { historyLenth = value; OnPropertyChanged("HistoryLenth"); }
        }

        private int historyLenth;

        //private double _trend;
        //private double _trend2;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
