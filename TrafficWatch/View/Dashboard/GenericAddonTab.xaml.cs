using System.Windows.Controls;
using TrafficWatch.Models.Dashboard;

namespace TrafficWatch.View.Dashboard
{
    /// <summary>
    /// Interaction logic for GenericAddonTab.xaml
    /// تب پیش‌فرض برای افزونه‌هایی که view اختصاصی ندارند
    /// </summary>
    public partial class GenericAddonTab : UserControl
    {
        public GenericAddonTab(AddonInfo addonInfo)
        {
            InitializeComponent();
            TxtAddonName.Text = addonInfo.Name;
            TxtAddonDescription.Text = addonInfo.Description;
        }
    }
}
