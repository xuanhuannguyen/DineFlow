using System.Windows;
using DineFlow.WPFApp.Features.Management.Tables;

namespace DineFlow.WPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(
            TableOverviewControl tableOverviewControl,
            AreaManagementControl areaManagementControl,
            TableManagementControl tableManagementControl)
        {
            InitializeComponent();

            ccOverview.Content = tableOverviewControl;
            ccArea.Content = areaManagementControl;
            ccTable.Content = tableManagementControl;
        }
    }
}
