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
using Run = TrafficWatch.Services.Detail.Run;

namespace TrafficWatch.View
{
    /// <summary>
    /// Interaction logic for DetailWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window
    {
        public DetailWindow(ICanMoveDetailWindowToRightPlace canMove)
        {
            this.canMove = canMove;
            asynShow = new Run(AsynShow);
            asynHide = new Run(AsynHide);
            InitializeComponent();
            InitializeContent();
            IsVisibleChanged += DetailWindow_IsVisibleChanged;
        }

        private void DetailWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                idMap.Clear();
            }
            if (Application.Current is App app)
            {
                app.NeedPortProcessMap(this, this.IsVisible);
            }
        }

        private void InitializeContent()
        {
            int rows = ContentGrid.RowDefinitions.Count;
            canvases = new Canvas[rows];
            icons = new Image[rows];
            names = new TextBlock[rows];
            downs = new TextBlock[rows];
            ups = new TextBlock[rows];
            labels = new Label[rows];
            for (int i = 0; i < rows; i++)
            {
                Canvas canvas = new Canvas();
                Grid.SetRow(canvas, i);
                Grid.SetColumnSpan(canvas, 6);
                ContentGrid.Children.Add(canvas);
                canvases[i] = canvas;

                Image icon = new Image();
                Grid.SetColumn(icon, 0);
                Grid.SetRow(icon, i);
                ContentGrid.Children.Add(icon);
                icons[i] = icon;

                TextBlock name = new TextBlock();
                Grid.SetColumn(name, 1);
                Grid.SetRow(name, i);
                name.VerticalAlignment = VerticalAlignment.Center;
                ContentGrid.Children.Add(name);
                names[i] = name;

                Label label = new Label();
                Grid.SetColumn(label, 2);
                Grid.SetRow(label, i);
                ContentGrid.Children.Add(label);
                labels[i] = label;
                label.MouseDown += DetailLabel_MouseDown;

                TextBlock down = new TextBlock();
                Grid.SetColumn(down, 3);
                Grid.SetRow(down, i);
                down.HorizontalAlignment = HorizontalAlignment.Right;
                down.VerticalAlignment = VerticalAlignment.Center;
                ContentGrid.Children.Add(down);
                downs[i] = down;

                TextBlock up = new TextBlock();
                Grid.SetColumn(up, 4);
                Grid.SetRow(up, i);
                up.HorizontalAlignment = HorizontalAlignment.Right;
                up.VerticalAlignment = VerticalAlignment.Center;
                ContentGrid.Children.Add(up);
                ups[i] = up;
            }
        }

        private void DetailLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                int row = Grid.GetRow(label);
                if (row >= 0 && row < localItems.Count)
                {
                    int id = localItems[row].ProcessID;
                    if (idMap.TryGetValue(id, out ProcessView process))
                    {
                        ProcessDetailWindow win = new ProcessDetailWindow(process);
                        win.Show();
                    }
                    else
                    {
                        ProcessDetailWindow win = new ProcessDetailWindow(id);
                        win.Show();
                    }
                }
            }
        }

        private void ClearViewContent()
        {
            for (int i = 0; i < ContentGrid.RowDefinitions.Count; i++)
            {
                icons[i].Source = null;
                names[i].Text = null;
                downs[i].Text = null;
                ups[i].Text = null;
            }
        }
        private string GatName(int IdProcss)
        {
            ProcessView value;
            if (idMap.TryGetValue(IdProcss, out value))
            {
                return value.Name;
            }
            else
            {
                return "";

            }
                
        }
        public void NewData(List<UDOneItem> items, double timeSpan)
        {
            if (items.Count == 0)
            {
                for (int i = 0; i < ContentGrid.RowDefinitions.Count; i++)
                {
                    if (names[i].Text == null || names[i].Text == "")
                    {
                        break;
                    }
                    else
                    {
                        ups[i].Text = "0K/s";
                        downs[i].Text = "0K/s";
                    }
                }
            }
            else
            {
                if(TextSearsh.Text!="")
                    if (items.Count>0)
                        items = items.Where(x => GatName(x.ProcessID).ToLower().IndexOf(TextSearsh.Text.ToLower()) !=-1).ToList();

                localItems.Clear();
                localItems.AddRange(items);
                ClearViewContent();
                for (int i = 0; i < ContentGrid.RowDefinitions.Count && i < items.Count; i++)
                {
                    UDOneItem item = items[i];
                    downs[i].Text = Tool.GetNetSpeedString(item.Download, timeSpan);
                    ups[i].Text = Tool.GetNetSpeedString(item.Upload, timeSpan);
                    if (item.ProcessID == -1)
                    {
                        names[i].Text = "bridge";
                    }
                    else
                    {
                        if (!idMap.TryGetValue(item.ProcessID, out ProcessView view))
                        {
                            view = new ProcessView(item.ProcessID);
                            idMap[item.ProcessID] = view;
                        }
                        names[i].Text = view.Name ?? "Process ID: " + view.ID;
                        if (view.Image != null)
                        {
                            icons[i].Source = view.Image;
                        }
                    }
                }
            }
            RefreshDetailButton(Mouse.GetPosition(ContentGrid));
        }

        public void OthersWantShow(bool now)
        {
            runManager.RemoveMission(asynHide);
            if (Visibility == Visibility.Visible)
            {
                runManager.RemoveMission(asynShow);
            }
            else
            {
                if (now)
                {
                    MyShow();
                    runManager.RemoveMission(asynShow);
                }
                else
                {
                    runManager.RunAfter(asynShow, 1000);
                }
            }
        }

        public void OthersWantHide(bool now)
        {
            if (Pin)
            {
                return;
            }
            runManager.RemoveMission(asynShow);
            if (Visibility == Visibility.Hidden)
            {
                runManager.RemoveMission(asynHide);
            }
            else
            {
                if (now)
                {
                    Hide();
                    runManager.RemoveMission(asynHide);
                }
                else
                {
                    runManager.RunAfter(asynHide, 1000);
                }
            }
        }

        private void MyShow()
        {
            canMove.MoveDetailWindowToRightPlace(this);
            Show();
        }

        private void AsynShow()
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                MyShow();
            }));
        }

        private void AsynHide()
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                Hide();
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tool.WindowMissFromMission(this, false);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            OthersWantShow(false);
            RefreshDetailButton(e.GetPosition(ContentGrid));
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            OthersWantHide(false);
            RefreshDetailButton(e.GetPosition(ContentGrid));
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            RefreshDetailButton(e.GetPosition(ContentGrid));
        }

        private void RefreshDetailButton(Point p)
        {
            foreach (var i in labels)
            {
                i.Visibility = Visibility.Hidden;
            }
            foreach (var i in canvases)
            {
                i.Background = null;
            }
            if (p.X < 0 || p.Y < 0 || p.X > ContentGrid.ActualHeight || p.Y > ContentGrid.ActualWidth)
            {
                return;
            }

            int row = (int)(p.Y / 20);
            if (row >= 0 && row < ContentGrid.RowDefinitions.Count && ups[row].Text != null && ups[row].Text != "")
            {
                labels[row].Visibility = Visibility.Visible;
                canvases[row].Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55555555"));
            }


        }



        private ICanMoveDetailWindowToRightPlace canMove;

        private Run asynShow, asynHide;

        private void CloseImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Topmost = false;
            Pin = false;
            OthersWantHide(true);
            PinWindows.IsChecked = false;
        }

        private DelayRunManager runManager = new DelayRunManager();
        private Dictionary<int, ProcessView> idMap = new Dictionary<int, ProcessView>();

        private Image[] icons;
        private TextBlock[] names;
        private TextBlock[] ups;
        private TextBlock[] downs;
        private Label[] labels;
        private Canvas[] canvases;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
        bool Pin = false;
        private void PinWindows_Click(object sender, RoutedEventArgs e)
        {
            runManager.RemoveMission(asynHide);
            Pin = !Pin;
            if (Pin)
            {
                PinWindows.ToolTip = "Pin";
                this.Topmost = true;
            }
            else{
                PinWindows.ToolTip = "UnPin";
                this.Topmost = false;
                runManager.RunAfter(asynHide, 1000);
            }
            
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            Pin = false;
            PinWindows.IsChecked = false;
            PinWindows.ToolTip = "UnPin";
            OthersWantHide(true);
            
        }

        private List<UDOneItem> localItems = new List<UDOneItem>();


    }

    public class ContentListViewItem
    {
        public string NAME { get; set; }
        public string DOWNLOAD { get; set; }
        public string UPLOAD { get; set; }
    }

    public interface ICanMoveDetailWindowToRightPlace
    {
        void MoveDetailWindowToRightPlace(DetailWindow dw);
    }
}

