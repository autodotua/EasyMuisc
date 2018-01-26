using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static EasyMuisc.Tools.Tools;
using EasyMuisc.Tools;

namespace EasyMuisc.Windows
{
    /// <summary>
    /// WinSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : Window
    {
        Properties.Settings set;
        public WinSettings(Properties.Settings set)
        {
            this.set = set;
            InitializeComponent();
            chkOffset.IsChecked = set.SaveLrcOffsetByTag;
            chkPreferMusicInfo.IsChecked = set.PreferMusicInfo;
            chkLrcAnimation.IsChecked = set.LrcAnimation;
            txtAnimationFps .Text= set.AnimationFps.ToString();
            txtOffset.Text = set.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = set.UpdateSpeed.ToString();
            switch (set.UseListBoxLrcInsteadOfStackPanel)
            {
                case true:
                    chkListBoxLrc.IsChecked = true;
                    break;
                case false:
                    chkStackPanel.IsChecked = true;
                    return;
            }
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {


            if (!double.TryParse(txtUpdateSpeed.Text, out double speed) || speed <= 0)
            {
              ShowAlert("输入的速度值不是正数！");
                return;
            }
            if (speed>60)
            {
              ShowAlert("输入的速度值过大！");
                return;

            }
            if (!int.TryParse(txtAnimationFps.Text, out int fps) || speed <= 0)
            {
              ShowAlert("输入的FPS不是正数！");
                return;
            }
            if (fps > 240)
            {
              ShowAlert("输入的速度值过大！");
                return;

            }
            if (!double.TryParse(txtOffset.Text, out double offset))
                {
                  ShowAlert("输入的偏移量不是数字！");
                return;
                }



            set.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
            set.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
            set.LrcAnimation = (bool)chkLrcAnimation.IsChecked;
            set.UseListBoxLrcInsteadOfStackPanel = (bool)chkListBoxLrc.IsChecked;
            if (fps!=set.AnimationFps)
            {
                MessageBox.Show("动画帧率将在下次启动后生效", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                set.AnimationFps = fps;
            }
            set.UpdateSpeed = speed;
            set.LrcDefautOffset = offset;
            Close();
        }
        
    }
}
