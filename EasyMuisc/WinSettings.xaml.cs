using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
            chkLrcAnimation.IsChecked = winMain.LrcAnimation;
            txtAnimationFps .Text= winMain.AnimationFps.ToString();
            txtOffset.Text = winMain.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = winMain.UpdateSpeed.ToString();
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {


            if (!double.TryParse(txtUpdateSpeed.Text, out double speed) || speed <= 0)
            {
                winMain.ShowAlert("输入的速度值不是正数！");
                return;
            }
            if (speed>60)
            {
                winMain.ShowAlert("输入的速度值过大！");
                return;

            }
            if (!int.TryParse(txtAnimationFps.Text, out int fps) || speed <= 0)
            {
                winMain.ShowAlert("输入的FPS不是正数！");
                return;
            }
            if (fps > 240)
            {
                winMain.ShowAlert("输入的速度值过大！");
                return;

            }
            if (!double.TryParse(txtOffset.Text, out double offset))
                {
                    winMain.ShowAlert("输入的偏移量不是数字！");
                return;
                }



            winMain.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
            winMain.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
            winMain.LrcAnimation = (bool)chkLrcAnimation.IsChecked;
            if(fps!=winMain.AnimationFps)
            {
                MessageBox.Show("动画帧率将在下次启动后生效", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                winMain.AnimationFps = fps;
            }
            winMain.UpdateSpeed = speed;
            winMain.LrcDefautOffset = offset;
            Close();
        }



    }
}
