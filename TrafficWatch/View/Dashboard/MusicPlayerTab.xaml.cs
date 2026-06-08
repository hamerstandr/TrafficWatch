using System.Windows.Controls;
using TrafficWatch.Models.Dashboard;

namespace TrafficWatch.View.Dashboard
{
    /// <summary>
    /// Interaction logic for MusicPlayerTab.xaml
    /// تب نمایش اطلاعات پخش کننده موسیقی (در حال توسعه)
    /// </summary>
    public partial class MusicPlayerTab : UserControl
    {
        private readonly AddonInfo _addonInfo;

        public MusicPlayerTab(AddonInfo addonInfo)
        {
            InitializeComponent();
            _addonInfo = addonInfo;
        }
    }
}
