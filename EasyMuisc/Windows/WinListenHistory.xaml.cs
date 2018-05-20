using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using static WpfControls.Dialog.DialogHelper;

namespace EasyMuisc.Windows
{
    /// <summary>
    /// WinListenHistory.xaml 的交互逻辑
    /// </summary>
    public partial class WinListenHistory : Window
    {

        //public static ObservableCollection<ListenHistory> listenHistories;

        public WinListenHistory()
        {
            InitializeComponent();
            try
            {
                lvwMain.ItemsSource = new ObservableCollection<ListenHistory>(listenHistory.GetListenHistories().OrderByDescending(p => p.ListenNumber));
            }
            catch (Exception ex)
            {
                ShowException("打开聆听历史文件发生错误", ex);
                Close();
            }
            lvwTime.Load(new string[] { "序号", "开始时间", "结束时间" });

        }

        private void LvwSelectionChangedEventHandler(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lvwTime.ClearRows();
                int index = 0;
                foreach (var time in (lvwMain.SelectedItem as ListenHistory).ListenTimes)
                {
                    DateTime begin = time.Key;
                    DateTime? end = time.Value;
                    if (end.HasValue)
                    {
                        if ((end.Value - begin).TotalSeconds < set.ThresholdValueOfListenTime)
                        {
                            continue;
                        }
                    }

                    lvwTime.AddRow(new List<string>() { (++index).ToString(), begin.ToString(), end.HasValue ? end.Value.ToString() : "" });
                }
            }
            catch (Exception ex)
            {
                ShowException("显示聆听时间详情", ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = ListenHistoryHelper.XmlPath;
            if (!File.Exists(path))
            {
                ShowError("文件不存在！", this);
                return;
            }
            try
            {
                Process.Start(ListenHistoryHelper.XmlPath);
            }
            catch (Exception ex)
            {
                ShowException("打开文件发生错误", ex);
            }
        }

        private void OrderModeSelectionChangedEventHandler(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lvwMain == null)
                {
                    return;
                }
                if (cbbOrderMode.SelectedIndex == 0)
                {
                    lvwMain.ItemsSource = new ObservableCollection<ListenHistory>(listenHistory.GetListenHistories().OrderByDescending(p => p.ListenNumber));
                }
                else
                {
                    lvwMain.ItemsSource = new ObservableCollection<ListenHistory>(listenHistory.GetListenHistories().OrderByDescending(p => p.ListenTimes.Last().Key));

                }
            }
            catch (Exception ex)
            {
                ShowException("打开聆听历史文件发生错误", ex);
                Close();
            }
        }
    }
}
