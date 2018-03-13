using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.ShareStaticResources;
using EasyMuisc.Tools;
using Un4seen.Bass;
using static WpfControls.Dialog.DialogHelper;
using Microsoft.Win32;
using System.IO;
using System;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Text;

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
            txtAnimationFps.Text = set.AnimationFps.ToString();
            txtOffset.Text = set.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = set.UpdateSpeed.ToString();
            chkMusicSettings.IsChecked = set.MusicSettings;
            //txtSampleRate.Text = set.SampleRate.ToString();
            //switch (set.UseListBoxLrcInsteadOfStackPanel)
            //{
            //    case true:
            //        chkListBoxLrc.IsChecked = true;
            //        break;
            //    case false:
            //        chkStackPanel.IsChecked = true;
            //        return;
            //}
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

            //foreach (var i in mainWindow.musicList.stkMusiList.Children)
            //{
            //    if(i is UserControls.ToggleButton)
            //    {
            //        var btn = i as UserControls.ToggleButton;
            //        RadioButton chk = new RadioButton()
            //        {
            //            Content=btn.Text,
            //            IsChecked=btn.Text==set.DefautMusicList,
            //            Margin=new Thickness(0,0,0,8),
            //        };
            //        stkMusicList.Children.Add(chk);
            //    }
            //}
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {


            if (!double.TryParse(txtUpdateSpeed.Text, out double speed) || speed <= 0)
            {
                ShowError("输入的速度值不是正数！");
                return;
            }
            //if (!int.TryParse(txtSampleRate.Text, out int sampleRate) || sampleRate <= 0)
            //{
            //    ShowAlert("输入的采样率不是正数！");
            //    return;
            //}
            if (speed > 60)
            {
                ShowError("输入的速度值过大！");
                return;

            }
            if (!int.TryParse(txtAnimationFps.Text, out int fps) || speed <= 0)
            {
                ShowError("输入的FPS不是正数！");
                return;
            }
            if (fps > 240)
            {
                ShowError("输入的速度值过大！");
                return;

            }
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                ShowError("输入的偏移量不是数字！");
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
           // set.UseListBoxLrcInsteadOfStackPanel = (bool)chkListBoxLrc.IsChecked;
            set.MusicSettings = chkMusicSettings.IsChecked.Value;
            if (fps != set.AnimationFps)
            {
                MessageBox.Show("动画帧率将在下次启动后生效", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                set.AnimationFps = fps;
            }
            set.UpdateSpeed = speed;
            set.LrcDefautOffset = offset;
            set.TrayMode = chkCloseBtnToTray.IsChecked.Value ? 1 : (chkMinimunBtnToTray.IsChecked.Value ? 2 : 3);

            //foreach (var i in stkMusicList.Children)
            //{
            //    if(i is RadioButton && (i as RadioButton).IsChecked.Value)
            //    {
            //        set.DefautMusicList = (i as RadioButton).Content as string;
                    set.Save();
                    Close();
            //        return;
            //    }
            //}
            //ShowWarn("还未选择默认歌单！");
            

        }

        private void ButtonExportClickEventHandler(object sender, RoutedEventArgs e)
        {
            set.Save();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            SaveFileDialog dialog = new SaveFileDialog() { Filter = "XML文件|*.xml|所有文件|*.*" };
            if (dialog.ShowDialog() == true)
            {
                config.SaveAs(dialog.FileName);
            }
        }

        private void ButtonImportClickEventHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Filter = "XML文件|*.xml|所有文件|*.*" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            if (!File.Exists(dialog.FileName))
            {
                ShowError("文件不存在！");
                return;
            }

            try
            {
                StringBuilder failedSettings = new StringBuilder();
                // Open settings file as XML
                var import = XDocument.Load(dialog.FileName);
                // Get the <setting> elements
                var settings = import.XPathSelectElements("//setting");
                foreach (var setting in settings)
                {
                    string name = setting.Attribute("name").Value;
                    string value = setting.XPathSelectElement("value").FirstNode.ToString();

                    try
                    {
                        if (set[name] is string)
                        {
                            set[name] = value;
                        }
                        else if (set[name] is int)
                        {
                            set[name] = int.Parse(value);
                        }
                        else if (set[name] is double)
                        {
                            set[name] = double.Parse(value);
                        }
                        else
                        {
                            failedSettings.AppendLine(name + ": 格式不支持" );
                        }
                    }
                    catch (Exception ex)
                    {
                        failedSettings.AppendLine(name + ": " + ex.Message);
                    }
                }

                if (failedSettings.Length == 0)
                {
                    ShowPrompt("导入成功。部分设置需要重启生效。");
                }
                else
                {
                    ShowWarn("导入部分失败：" + Environment.NewLine + failedSettings);
                }
                set.Save();
            }
            catch (Exception ex)
            {
                ShowException("导入失败", ex);
                set.Reload();
            }
        }
    }
}
