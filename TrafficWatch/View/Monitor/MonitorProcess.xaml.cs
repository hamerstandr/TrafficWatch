using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrafficWatch.Services.Detail;
using TrafficWatch.View.Detail;

namespace TrafficWatch.View.Monitor
{
    /// <summary>
    /// Interaction logic for MonitorProcess.xaml
    /// </summary>
    public partial class MonitorProcess : Window
    {
        readonly int IDProcess;
        public MonitorProcess(int IDProcess)
        {
            InitializeComponent();
            this.IDProcess = IDProcess;
            this.Closing += MonytorProcess_Closing;
            Init();
        }
        private readonly Dictionary<int, ProcessView> idMap = new Dictionary<int, ProcessView>();
        private void MonytorProcess_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            idMap.Clear();
            if (Application.Current is App app)
            {
                app.NeedPortProcessMap(this, false);
            }
            Me = null;
        }

        private void Init()
        {
            Me = this;
            if (Application.Current is App app)
            {
                app.NeedPortProcessMap(this, true);
            }
            Chart1.Zoom = Control.Zooming.X;
            Chart1.Hoverable = true;
        }
        public static MonitorProcess Me;
        long Upload = 0;
        long Download = 0;
        public void NewData(List<UDOneItem> items, double timeSpan)
        {
            UDOneItem item = items.Where(x => x.ProcessID == IDProcess).FirstOrDefault();
            if (item != null)
            {
                down.Text = Tool.GetNetSpeedString(item.Download, timeSpan);
                up.Text = Tool.GetNetSpeedString(item.Upload, timeSpan);
                Upload += item.Upload;
                Download += item.Download;
                up.Text = Tool.ToString(Upload);
                down.Text = Tool.ToString(Download);
                Chart1.Uploaded(item.Upload);
                Chart1.Downloaded(item.Download);
                if (item.ProcessID == -1)
                {
                    Names.Text = "bridge";
                }
                else
                {
                    if (!idMap.TryGetValue(item.ProcessID, out ProcessView view))
                    {
                        view = new ProcessView(item.ProcessID);
                        idMap[item.ProcessID] = view;
                    }
                    Names.Text = view.Name ?? "Process ID: " + view.ID;
                    if (view.Image != null)
                    {
                        Icons.Source = view.Image;
                    }
                }


            }
            else
            {
                up.Text = "0K/s";
                down.Text = "0K/s";
                Chart1.Uploaded( 0);
                Chart1.Downloaded( 0);
            }
        }

        private void Chart1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Chart1.ResetrtZoom();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            int.TryParse(((TextBox)sender).Text, out int input);
            if (input > 0) Chart1.HistoryLenth = input;
        }
    }
}
