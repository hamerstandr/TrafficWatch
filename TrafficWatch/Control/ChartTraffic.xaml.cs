using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrafficWatch.Control
{
    /// <summary>
    /// Interaction logic for ChartTraffic.xaml
    /// </summary>
    public partial class ChartTraffic : UserControl
    {
        public ChartTraffic()
        {
            InitializeComponent();
            Chart1.DataContext = download1;
            this.Loaded += ChartTraffic_Loaded;
        }

        private void ChartTraffic_Loaded(object sender, RoutedEventArgs e)
        {
            download1.History = History;
            HistoryLenth = 60;
        }

        private readonly DataChart download1 = new DataChart();

        public void Downloaded(double Value) { download1.Downloaded = Value;  }
        public void Uploaded(double Value) { download1.Uploaded = Value; }

        public static readonly DependencyProperty HistoryProperty = DependencyProperty.Register
        ("History", typeof(bool), typeof(ChartTraffic), new PropertyMetadata(false, null));
        //private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //}

        public bool History
        {
            get => (bool)GetValue(HistoryProperty);

            set
            {
                SetValue(HistoryProperty, value);
                download1.History = value;
            }
        }
        public static readonly DependencyProperty HistoryLenthProperty = DependencyProperty.Register
        ("HistoryLenth", typeof(int), typeof(ChartTraffic), new PropertyMetadata(60, null));
        //private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //}

        public int HistoryLenth
        {
            get => (int)GetValue(HistoryLenthProperty);

            set
            {
                SetValue(HistoryLenthProperty, value);
                download1.HistoryLenth = value;
            }
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register
        ("Zoom", typeof(Zooming), typeof(ChartTraffic), new PropertyMetadata(Zooming.None, null));

        public Zooming Zoom
        {
            get => (Zooming)GetValue(HistoryProperty);

            set
            {
                SetValue(ZoomProperty, value);
                switch (value)
                {
                    case Zooming.None:
                        Chart1.Zoom = ZoomingOptions.None;
                        break;
                    case Zooming.X:
                        Chart1.Zoom = ZoomingOptions.X;
                        break;
                    case Zooming.Y:
                        Chart1.Zoom = ZoomingOptions.Xy;
                        break;
                    case Zooming.XY:
                        Chart1.Zoom = ZoomingOptions.Xy;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public double MaxValue { get => download1.MaxValue;
            set => download1.MaxValue = value; }
        public double MinValue { get => download1.MinValue;
            set => download1.MinValue = value; }
        

        public void ResetrtZoom()
        {
            Reset = true;
            //download1.MinValue = 0;
            //download1.MaxValue = download1.Labels.Count - 1;
        }
        bool Reset = false;
        public bool Hoverable { get => Chart1.Hoverable; set => Chart1.Hoverable = value; }
        private void Axis_PreviewRangeChanged(LiveCharts.Events.PreviewRangeChangedEventArgs e)
        {
            if (Reset)
            {
                Reset = false;
                //if less than -0.5, cancel
                if (e.PreviewMinValue < -0.5) e.Cancel = true;

                //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
                if (e.PreviewMaxValue > download1.LastHourSeries[0].Values.Count - 0.5) e.Cancel = true;

                //finally if the axis range is less than 1, cancel the event
                if (e.PreviewMaxValue - e.PreviewMinValue < 1) e.Cancel = true;
            }
            
        }
    }
}
