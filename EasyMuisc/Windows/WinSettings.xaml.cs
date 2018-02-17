using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.ShareStaticResources;
using EasyMuisc.Tools;
using Un4seen.Bass;

namespace EasyMuisc.Windows
{
    /// <summary>
    /// WinSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : Window
    {
        public WinSettings()
        {
            InitializeComponent();
            chkOffset.IsChecked = set.SaveLrcOffsetByTag;
            chkPreferMusicInfo.IsChecked = set.PreferMusicInfo;
            chkLrcAnimation.IsChecked = set.LrcAnimation;
            txtAnimationFps .Text= set.AnimationFps.ToString();
            txtOffset.Text = set.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = set.UpdateSpeed.ToString();
            chkMusicSettings.IsChecked = set.MusicSettings;
            //txtSampleRate.Text = set.SampleRate.ToString();
            switch (set.UseListBoxLrcInsteadOfStackPanel)
            {
                case true:
                    chkListBoxLrc.IsChecked = true;
                    break;
                case false:
                    chkStackPanel.IsChecked = true;
                    return;
            }
            switch (set.TrayMode)
            {
                case 1:
                    chkCloseBtnToTray.IsChecked = true;
                    break;
                case 2:
                    chkMinimunBtnToTray.IsChecked = true;
                    break;
                case 3:
                    chkTrayBtnToTray.IsChecked = true;
                    break;
            }
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {


            if (!double.TryParse(txtUpdateSpeed.Text, out double speed) || speed <= 0)
            {
              ShowAlert("输入的速度值不是正数！");
                return;
            }
            //if (!int.TryParse(txtSampleRate.Text, out int sampleRate) || sampleRate <= 0)
            //{
            //    ShowAlert("输入的采样率不是正数！");
            //    return;
            //}
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
            //Bass.BASS_Free();
            //if(!Bass.BASS_Init(-1,sampleRate,BASSInit.BASS_DEVICE_DEFAULT,windowHandle))
            //{
            //    ShowAlert("采样率不支持！");
            //    return;
            //}

            set.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
            set.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
            set.LrcAnimation = (bool)chkLrcAnimation.IsChecked;
            set.UseListBoxLrcInsteadOfStackPanel = (bool)chkListBoxLrc.IsChecked;
            set.MusicSettings = chkMusicSettings.IsChecked.Value;
            if (fps!=set.AnimationFps)
            {
                MessageBox.Show("动画帧率将在下次启动后生效", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                set.AnimationFps = fps;
            }
            set.UpdateSpeed = speed;
            set.LrcDefautOffset = offset;
            set.TrayMode = chkCloseBtnToTray.IsChecked.Value ? 1 : (chkMinimunBtnToTray.IsChecked.Value ? 2 : 3);
            Close();
        }
        
    }
}
