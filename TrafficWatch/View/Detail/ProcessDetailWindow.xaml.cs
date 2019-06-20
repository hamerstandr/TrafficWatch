using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrafficWatch.Services.Detail;

namespace TrafficWatch.View.Detail
{
    /// <summary>
    /// Interaction logic for ProcessDetailWindow.xaml
    /// </summary>
    public partial class ProcessDetailWindow : Window
    {
        public ProcessDetailWindow(int id) : this(new ProcessView(id))
        {

        }

        public ProcessDetailWindow(ProcessView tempP)
        {
            this.Process = tempP;
            InitializeComponent();
            ProcessID.Text = Process.ID + "";
            if (Process.SuccessGetInfo)
            {
                ProcessName.Text = Process.Name ?? Tool.GetStringResource("Unknown");
                ProcessIcon.Source = Process.Image;
                if (Process.FilePath == null && !Tool.IsAdministrator())
                {
                    OpenButtonImage.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Shield.Handle,
                        Int32Rect.Empty, BitmapSizeOptions.FromRotation(Rotation.Rotate0));
                    OpenButtonText.Text = Tool.GetStringResource("RunAsAdministratorToGetMoreInformation");
                    OpenButton.Click += OpenButton_RunAsAdmin_Click;
                }
                else
                {
                    if (Process.FilePath == null)
                    {
                        OpenButton.IsEnabled = false;
                    }
                    else
                    {
                        ProcessPath.Text = Process.FilePath;
                        OpenButton.Click += OpenButton_OpenPath_Click;
                    }
                }
            }
            else
            {
                ContentGrid.IsEnabled = false;
            }
        }

        private void OpenButton_OpenPath_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + Process.FilePath + "\"");
        }

        private void OpenButton_RunAsAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.FreeMutex();
                string exe = GetType().Assembly.Location;
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo(exe, "-processid " + Process.ID)
                    {
                        Verb = "runas",
                    },
                };
                try
                {
                    bool b = p.Start();
                    app.TryToExit();
                }
                catch (Exception)
                {

                }
            }
        }

        private ProcessView process;

        public ProcessView Process { get => process; set => process = value; }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (!Process.SuccessGetInfo)
            {
                Dispatcher.InvokeAsync(new Action(() =>
                {
                    MessageBox.Show(Tool.GetStringResource("CantGetInformationOfThisProcessMaybeItsNotRunningNow_"),
                        Tool.GetStringResource("ERROR"));
                }));
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {

            Monitor.MonitorProcess w = new Monitor.MonitorProcess(Process.ID);
            w.Show();
        }
    }
}
