using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static EasyMuisc.ShareStaticResources;

namespace EasyMuisc.Windows
{
    /// <summary>
    /// WinListenHistory.xaml 的交互逻辑
    /// </summary>
    public partial class WinListenHistory : Window
    {

        public static ObservableCollection<ListenHistory> listenHistories;

        public WinListenHistory()
        {
            InitializeComponent();
            lvwMain.ItemsSource = new ObservableCollection<ListenHistory>(listenHistory.GetListenHistories());
            
        }

        private void lvwMain_PreviewMouseLeftButtonDown(object sender, SelectionChangedEventArgs e)
        {
WpfControls.FlatStyle.ListView lvwTime = new WpfControls.FlatStyle.ListView();
            lvwTime.Load(new string[] { "序号", "开始时间", "结束时间" });
            int index = 0;
            foreach (var time in (lvwMain.SelectedItem as ListenHistory).ListenTimes)
            {
                DateTime begin = time.Key;
                DateTime? end = time.Value;
                if(end.HasValue)
                {
                    if((end.Value-begin).TotalSeconds<set.ThresholdValueOfListenTime)
                    {
                        continue;
                    }
                }

                lvwTime.AddRow(new List<string>() { (++index).ToString(),begin.ToString(),end.HasValue?end.Value.ToString():""});
            }
            frm.Content = lvwTime;
        }
    }
}
