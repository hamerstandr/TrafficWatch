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
        int IDProcess;
        public MonitorProcess(int IDProcess)
        {
            InitializeComponent();
            this.IDProcess = IDProcess;
            this.Closing += MonytorProcess_Closing;
            init();
        }
        private Dictionary<int, ProcessView> idMap = new Dictionary<int, ProcessView>();
        private void MonytorProcess_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            idMap.Clear();
            if (Application.Current is App app)
            {
                app.NeedPortProcessMap(this, false);
            }
            Me = null;
        }

        void init()
        {
            Me = this;
            if (Application.Current is App app)
            {
                app.NeedPortProcessMap(this, true);
            }
        }
        public static MonitorProcess Me;
        public void NewData(List<UDOneItem> items, double timeSpan)
        {
            UDOneItem item = items.Where(x => x.ProcessID == IDProcess).FirstOrDefault();
            if (item != null)
            {
                down.Text = Tool.GetNetSpeedString(item.Download, timeSpan);
                up.Text = Tool.GetNetSpeedString(item.Upload, timeSpan);
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
                        Icon.Source = view.Image;
                    }
                }


            }
            else
            {
                up.Text = "0K/s";
                down.Text = "0K/s";
            }
        }
    }
}
