using System.Configuration;
using System.Windows;

namespace EasyMuisc
{
    /// <summary>
    /// WinSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : Window
    {
        MainWindow winMain;
        public WinSettings(MainWindow winMain)
        {
            this.winMain = winMain;
            InitializeComponent();
            chkOffset.IsChecked = winMain.SaveLrcOffsetByTag;
            chkPreferMusicInfo.IsChecked = winMain.PreferMusicInfo;
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
            winMain.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
            winMain.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
            Close();
        }
    }
}
