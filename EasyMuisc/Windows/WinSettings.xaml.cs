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
        public WinSettings(int pageIndex = 0)
        {
            InitializeComponent();
            tab.SelectedIndex = pageIndex;

            chkOffset.IsChecked = set.SaveLrcOffsetByTag;
            chkPreferMusicInfo.IsChecked = set.PreferMusicInfo;
            chkLrcAnimation.IsChecked = set.LrcAnimation;
            txtAnimationFps.Text = set.AnimationFps.ToString();
            txtOffset.Text = set.LrcDefautOffset.ToString();
            txtUpdateSpeed.Text = set.UpdateSpeed.ToString();
            chkMusicSettings.IsChecked = set.MusicSettings;
            cbbTrayMode.SelectedIndex = set.TrayMode;
            txtCurrentFontSize.Text = set.HighlightLrcFontSize.ToString();
            txtNormalFontSize.Text = set.NormalLrcFontSize.ToString();
            txtTextFontSize.Text = set.TextLrcFontSize.ToString();
            txtFloatCurrentFontSize.Text = set.FloatLyricsHighlightFontSize.ToString();
            txtFloatNormalFontSize.Text = set.FloatLyricsNormalFontSize.ToString();
            cbbFloatFontEffect.SelectedIndex = set.FloatLyricsFontEffect;
            floatFontColor.SetColor(set.FloatLyricsFontColor);
            floatBorderColor.SetColor(set.FloatLyricsBorderColor);
            cbbFloatFontEffect_SelectionChanged(null, null);
            txtFloatBlur.Text = set.FloatLyricsBlurRadius.ToString();
            txtFloatBorder.Text = set.FloatLyricsThickness.ToString();
            chkFloatBold.IsChecked = set.FloatLyricsFontBold;
            fontColor.SetColor(set.LyricsFontColor);
            chkBold.IsChecked = set.LyricsFontBold;
            chkListenHitory.IsChecked = set.RecordListenHistory;
            txtListenHistoryValue.Text = set.ThresholdValueOfListenTime.ToString();
            mainColor.SetColor(set.BackgroundColor);
            if(!cbbFloatFont.SetSelectedFontByString(set.FloatLyricsFont) || !cbbFont.SetSelectedFontByString(set.LyricsFont))
            {
                mainWindow.ShowTrayMessage("字体文件设置异常，请重新设置");
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

                set.SaveLrcOffsetByTag = (bool)chkOffset.IsChecked;
                set.PreferMusicInfo = (bool)chkPreferMusicInfo.IsChecked;
                set.LrcAnimation = (bool)chkLrcAnimation.IsChecked;
                set.MusicSettings = chkMusicSettings.IsChecked.Value;
                if (fps != set.AnimationFps)
                {
                    ShowPrompt("动画帧率将在下次启动后生效");
                    set.AnimationFps = fps.Value;
                }
                set.UpdateSpeed = speed.Value;
                set.LrcDefautOffset = offset.Value;
                set.NormalLrcFontSize = normal.Value;
                set.HighlightLrcFontSize = current.Value;
                set.FloatLyricsHighlightFontSize = floatCurrent.Value;
                set.FloatLyricsNormalFontSize = floatNormal.Value;
                if (cbbTrayMode.SelectedIndex == 0 && cbbTrayMode.SelectedIndex != set.TrayMode)
                {
                    trayIcon.Visible = false;
                }
                else if (cbbTrayMode.SelectedIndex != 0 && set.TrayMode == 0)
                {
                    trayIcon.Visible = true;
                }
                set.FloatLyricsFontEffect = cbbFloatFontEffect.SelectedIndex;
                set.FloatLyricsBorderColor = floatBorderColor.ColorBrush.ToString();
                set.FloatLyricsFontColor = floatFontColor.ColorBrush.ToString();
                set.TrayMode = cbbTrayMode.SelectedIndex;
                set.FloatLyricsThickness = floatBorder.Value;
                set.FloatLyricsBlurRadius = floatBlur.Value;
                set.FloatLyricsFontBold = chkFloatBold.IsChecked.Value;
                set.FloatLyricsFont = cbbFloatFont.GetPreferChineseFontName();
                set.LyricsFontBold = chkBold.IsChecked.Value;
                set.LyricsFont = cbbFont.GetPreferChineseFontName();
                set.LyricsFontColor = fontColor.ColorBrush.ToString();
                set.RecordListenHistory = chkListenHitory.IsChecked.Value;
                set.ThresholdValueOfListenTime = listenValue.Value;
                set.BackgroundColor = mainColor.ColorBrush.ToString();
                mainWindow.UpdateColor(mainColor.ColorBrush);
                if (mainWindow.path != null && File.Exists(mainWindow.path))
                {
                    mainWindow.InitialiazeLrc();
                }
                set.Save();
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
                set.Save();
            }
            catch (Exception ex)
            {
                ShowException("导入失败", ex);
                set.Reload();
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
