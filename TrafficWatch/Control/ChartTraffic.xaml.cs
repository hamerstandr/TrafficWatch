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
        private Canvas _chartCanvas;
        private Path _downloadPath;
        private Path _uploadPath;
        private List<double> _downloadValues = new List<double>();
        private List<double> _uploadValues = new List<double>();

        public ChartTraffic()
        {
            InitializeComponent();
            this.Loaded += ChartTraffic_Loaded;
            _chartCanvas = FindName("PART_ChartCanvas") as Canvas;
            if (_chartCanvas != null)
            {
                _chartCanvas.SizeChanged += (s, e) => DrawChart();
            }
        }

        private void ChartTraffic_Loaded(object sender, RoutedEventArgs e)
        {
            download1.History = History;
            HistoryLenth = 60;
        }

        private readonly DataChart download1 = new DataChart();

        public void Downloaded(double Value) 
        { 
            download1.Downloaded = Value;
            UpdateDownloadData(Value);
        }
        
        public void Uploaded(double Value) 
        { 
            download1.Uploaded = Value;
            UpdateUploadData(Value);
        }

        private void UpdateDownloadData(double value)
        {
            _downloadValues.Add(value);
            if (!History && _downloadValues.Count > 1)
                _downloadValues.RemoveAt(0);
            else if (History && _downloadValues.Count > HistoryLenth)
                _downloadValues.RemoveRange(0, _downloadValues.Count - HistoryLenth);
            
            if (_chartCanvas != null)
                DrawChart();
        }

        private void UpdateUploadData(double value)
        {
            _uploadValues.Add(value);
            if (!History && _uploadValues.Count > 1)
                _uploadValues.RemoveAt(0);
            else if (History && _uploadValues.Count > HistoryLenth)
                _uploadValues.RemoveRange(0, _uploadValues.Count - HistoryLenth);
            
            if (_chartCanvas != null)
                DrawChart();
        }

        public static readonly DependencyProperty HistoryProperty = DependencyProperty.Register
        ("History", typeof(bool), typeof(ChartTraffic), new PropertyMetadata(false, null));

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
            get => (Zooming)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }
        
        public double MaxValue 
        { 
            get => download1.MaxValue;
            set => download1.MaxValue = value; 
        }
        
        public double MinValue 
        { 
            get => download1.MinValue;
            set => download1.MinValue = value; 
        }

        public void ResetrtZoom()
        {
            Reset = true;
        }
        
        bool Reset = false;
        
        public bool Hoverable { get; set; } = true;

        private void DrawChart()
        {
            if (_chartCanvas == null || _downloadValues.Count == 0) return;

            _chartCanvas.Children.Clear();

            double width = _chartCanvas.ActualWidth;
            double height = _chartCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            // Calculate Y scale
            double yMin = MinValue.IsNaN() ? 0 : MinValue;
            double yMax = MaxValue.IsNaN() ? GetMaxValue() : MaxValue;
            if (yMax <= yMin) yMax = yMin + 1;

            int pointCount = Math.Max(_downloadValues.Count, _uploadValues.Count);
            double xStep = width / Math.Max(pointCount - 1, 1);

            // Create download path
            var downloadGeometry = new StreamGeometry();
            using (var context = downloadGeometry.Open())
            {
                context.BeginFigure(new Point(0, height), false, false);
                
                for (int i = 0; i < _downloadValues.Count; i++)
                {
                    double x = i * xStep;
                    double y = height - ((_downloadValues[i] - yMin) / (yMax - yMin)) * height;
                    y = Math.Max(0, Math.Min(height, y));
                    
                    if (i == 0)
                        context.LineTo(new Point(x, y), true, false);
                    else
                        context.LineTo(new Point(x, y), true, false);
                }
                
                context.LineTo(new Point(width, height), true, false);
                context.LineTo(new Point(0, height), true, false);
            }
            downloadGeometry.Freeze();

            _downloadPath = new Path
            {
                Data = downloadGeometry,
                Fill = new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)),
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeThickness = 2
            };
            _chartCanvas.Children.Add(_downloadPath);

            // Create upload path
            if (_uploadValues.Count > 0)
            {
                var uploadGeometry = new StreamGeometry();
                using (var context = uploadGeometry.Open())
                {
                    context.BeginFigure(new Point(0, height), false, false);
                    
                    for (int i = 0; i < _uploadValues.Count; i++)
                    {
                        double x = i * xStep;
                        double val = i < _uploadValues.Count ? _uploadValues[i] : 0;
                        double y = height - ((val - yMin) / (yMax - yMin)) * height;
                        y = Math.Max(0, Math.Min(height, y));
                        
                        if (i == 0)
                            context.LineTo(new Point(x, y), true, false);
                        else
                            context.LineTo(new Point(x, y), true, false);
                    }
                    
                    context.LineTo(new Point(width, height), true, false);
                    context.LineTo(new Point(0, height), true, false);
                }
                uploadGeometry.Freeze();

                _uploadPath = new Path
                {
                    Data = uploadGeometry,
                    Fill = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2
                };
                _chartCanvas.Children.Add(_uploadPath);
            }
        }

        private double GetMaxValue()
        {
            double max = 0;
            foreach (var v in _downloadValues)
                if (v > max) max = v;
            foreach (var v in _uploadValues)
                if (v > max) max = v;
            return max > 0 ? max : 100;
        }
    }
}
