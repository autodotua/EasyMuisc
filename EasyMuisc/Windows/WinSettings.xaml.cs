using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static EasyMusic.Tools.Tools;
using static EasyMusic.GlobalDatas;
using EasyMusic.Tools;
using Un4seen.Bass;
using static WpfControls.Dialog.DialogHelper;
using Microsoft.Win32;
using System.IO;
using System;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Text;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : Window
    {
        public WinSettings(int pageIndex = 0)
        {
            InitializeComponent();
            tab.SelectedIndex = pageIndex;

            chkOffset.IsChecked = Setting.SaveLrcOffsetByTag;
            chkPreferMusicInfo.IsChecked = Setting.PreferMusicInfo;
            chkLrcAnimation.IsChecked = Setting.LrcAnimation;
            txtAnimationFps.Text = Setting.AnimationFps.ToString();
            txtOffset.Text = Setting.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = Setting.UpdateSpeed.ToString();
            chkMusicSettings.IsChecked = Setting.MusicSettings;
            cbbTrayMode.SelectedIndex = Setting.TrayMode;
            txtCurrentFontSize.Text = Setting.HighlightLrcFontSize.ToString();
            txtNormalFontSize.Text = Setting.NormalLrcFontSize.ToString();
            txtTextFontSize.Text = Setting.TextLrcFontSize.ToString();
            txtFloatCurrentFontSize.Text = Setting.FloatLyricsHighlightFontSize.ToString();
            txtFloatNormalFontSize.Text = Setting.FloatLyricsNormalFontSize.ToString();
            cbbFloatFontEffect.SelectedIndex = Setting.FloatLyricsFontEffect;
            floatFontColor.SetColor(Setting.FloatLyricsFontColor);
            floatBorderColor.SetColor(Setting.FloatLyricsBorderColor);
            cbbFloatFontEffect_SelectionChanged(null, null);
            txtFloatBlur.Text = Setting.FloatLyricsBlurRadius.ToString();
            txtFloatBorder.Text = Setting.FloatLyricsThickness.ToString();
            chkFloatBold.IsChecked = Setting.FloatLyricsFontBold;
            fontColor.SetColor(Setting.LyricsFontColor);
            chkBold.IsChecked = Setting.LyricsFontBold;
            chkListenHitory.IsChecked = Setting.RecordListenHistory;
            txtListenHistoryValue.Text = Setting.ThresholdValueOfListenTime.ToString();
            mainColor.SetColor(Setting.BackgroundColor);
            if(!cbbFloatFont.SetSelectedFontByString(Setting.FloatLyricsFont) || !cbbFont.SetSelectedFontByString(Setting.LyricsFont))
            {
                trayIcon.ShowMessage("字体文件设置异常，请重新设置");
            }
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {

                int? speed = txtUpdateSpeed.IntNumber;
                if (!speed.HasValue || speed <= 0)
                {
                    ShowError("输入的速度值不是正数！");
                    return;
                }

                if (speed > 60)
                {
                    ShowError("输入的速度值过大！");
                    return;

                }

                int? fps = txtAnimationFps.IntNumber;
                if (!fps.HasValue || fps <= 0)
                {
                    ShowError("输入的FPS不是正数！");
                    return;
                }
                if (fps > 240)
                {
                    ShowError("输入的速度值过大！");
                    return;

                }

                double? offset = txtOffset.DoubleNumber;
                if (!offset.HasValue)
                {
                    ShowError("输入的偏移量不是数字！");
                    return;
                }

                double? current = txtCurrentFontSize.DoubleNumber;
                if (!current.HasValue)
                {
                    ShowError("输入的主界面当前歌词字体大小不是数字！");
                    return;
                }

                double? normal = txtNormalFontSize.DoubleNumber;
                if (!normal.HasValue)
                {
                    ShowError("输入的主界面非当前歌词字体大小不是数字！");
                    return;
                }
                double? text = txtTextFontSize.DoubleNumber;
                if (!text.HasValue)
                {
                    ShowError("输入的主界面文本歌词字体大小不是数字！");
                    return;
                }
                double? floatCurrent = txtFloatCurrentFontSize.DoubleNumber;
                if (!floatCurrent.HasValue)
                {
                    ShowError("输入的悬浮歌词当前歌词字体大小不是数字！");
                    return;
                }

                double? floatNormal = txtFloatNormalFontSize.DoubleNumber;
                if (!floatNormal.HasValue)
                {
                    ShowError("输入的悬浮歌词非当前歌词字体大小不是数字！");
                    return;
                }
                double? floatBorder = txtFloatBorder.DoubleNumber;
                if (!floatBorder.HasValue)
                {
                    ShowError("输入的悬浮歌词边框粗细不是数字！");
                    return;
                }

                double? floatBlur= txtFloatBlur.DoubleNumber;
                if (!floatBlur.HasValue)
                {
                    ShowError("输入的悬浮歌词阴影深度不是数字！");
                    return;
                }
                int? listenValue = txtListenHistoryValue.IntNumber;
                if (!floatBlur.HasValue)
                {
                    ShowError("输入的聆听历史阙值不是正整数！");
                    return;
                }

                Setting.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
                Setting.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
                Setting.LrcAnimation = (bool)chkLrcAnimation.IsChecked;
                Setting.MusicSettings = chkMusicSettings.IsChecked.Value;
                if (fps != Setting.AnimationFps)
                {
                    ShowPrompt("动画帧率将在下次启动后生效");
                    Setting.AnimationFps = fps.Value;
                }
                Setting.UpdateSpeed = speed.Value;
                Setting.LrcDefautOffset = offset.Value;
                Setting.NormalLrcFontSize = normal.Value;
                Setting.HighlightLrcFontSize = current.Value;
                Setting.FloatLyricsHighlightFontSize = floatCurrent.Value;
                Setting.FloatLyricsNormalFontSize = floatNormal.Value;
                if (cbbTrayMode.SelectedIndex == 0 && cbbTrayMode.SelectedIndex != Setting.TrayMode)
                {
                    trayIcon.Hide();
                }
                else if (cbbTrayMode.SelectedIndex != 0 && Setting.TrayMode == 0)
                {
                    trayIcon.Show();
                }
                Setting.FloatLyricsFontEffect = cbbFloatFontEffect.SelectedIndex;
                Setting.FloatLyricsBorderColor = floatBorderColor.ColorBrush.ToString();
                Setting.FloatLyricsFontColor = floatFontColor.ColorBrush.ToString();
                Setting.TrayMode = cbbTrayMode.SelectedIndex;
                Setting.FloatLyricsThickness = floatBorder.Value;
                Setting.FloatLyricsBlurRadius = floatBlur.Value;
                Setting.FloatLyricsFontBold = chkFloatBold.IsChecked.Value;
                Setting.FloatLyricsFont = cbbFloatFont.GetPreferChineseFontName();
                Setting.LyricsFontBold = chkBold.IsChecked.Value;
                Setting.LyricsFont = cbbFont.GetPreferChineseFontName();
                Setting.LyricsFontColor = fontColor.ColorBrush.ToString();
                Setting.RecordListenHistory = chkListenHitory.IsChecked.Value;
                Setting.ThresholdValueOfListenTime = listenValue.Value;
                Setting.BackgroundColor = mainColor.ColorBrush.ToString();
                WinMain.UpdateColor(mainColor.ColorBrush);
                if (WinMain.path != null && File.Exists(WinMain.path))
                {
                    WinMain.InitialiazeLrc();
                }
                Setting.Save();
            }
            catch (Exception ex)
            {
                ShowException("保存设置失败", ex, this);
            }
            finally
            {
                if ((sender as Button).Name == "btnOk")
                {
                    Close();
                }
            }

        }

        private void ButtonExportClickEventHandler(object sender, RoutedEventArgs e)
        {
            Setting.Save();
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
                        if (Setting[name] is string)
                        {
                            Setting[name] = value;
                        }
                        else if (Setting[name] is int)
                        {
                            Setting[name] = int.Parse(value);
                        }
                        else if (Setting[name] is double)
                        {
                            Setting[name] = double.Parse(value);
                        }
                        else
                        {
                            failedSettings.AppendLine(name + ": 格式不支持");
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
                Setting.Save();
            }
            catch (Exception ex)
            {
                ShowException("导入失败", ex);
                Setting.Reload();
            }
        }

        private void cbbFloatFontEffect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbFloatFontEffect.SelectedIndex == 0)
            {
                stkBorder.Visibility = Visibility.Visible;
                stkBlur.Visibility = Visibility.Collapsed;
            }
            else
            {
                stkBorder.Visibility = Visibility.Collapsed;
                stkBlur.Visibility = Visibility.Visible;
            }
        }
    }
}
