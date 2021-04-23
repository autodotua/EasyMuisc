using EasyMusic.Helper;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static EasyMusic.GlobalDatas;
using static FzLib.Control.Dialog.DialogBox;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinSettings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : WindowBase, INotifyPropertyChanged
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
            switch (Setting.MusicFxMode)
            {
                case Enum.MusicFxRemainMode.Not:
                    cbbMusicFx.SelectedIndex = 0;
                    break;

                case Enum.MusicFxRemainMode.All:
                    cbbMusicFx.SelectedIndex = 1;
                    break;

                case Enum.MusicFxRemainMode.Each:
                    cbbMusicFx.SelectedIndex = 2;
                    break;
            }
            chkShowTray.IsChecked = Setting.ShowTray;
            txtCurrentFontSize.Text = Setting.HighlightLrcFontSize.ToString();
            txtNormalFontSize.Text = Setting.NormalLrcFontSize.ToString();
            txtTextFontSize.Text = Setting.TextLrcFontSize.ToString();
            txtFloatCurrentFontSize.Text = Setting.FloatLyricsHighlightFontSize.ToString();
            txtFloatNormalFontSize.Text = Setting.FloatLyricsNormalFontSize.ToString();
            cbbFloatFontEffect.SelectedIndex = Setting.FloatLyricsFontEffect;
            floatFontColor.ColorBrush = Setting.FloatLyricsFontColor;
            floatBorderColor.ColorBrush = Setting.FloatLyricsBorderColor;
            cbbFloatFontEffect_SelectionChanged(null, null);
            txtFloatBlur.Text = Setting.FloatLyricsBlurRadius.ToString();
            txtFloatBorder.Text = Setting.FloatLyricsThickness.ToString();
            chkFloatBold.IsChecked = Setting.FloatLyricsFontBold;
            fontColor.ColorBrush = (Setting.LyricsFontColor);
            chkBold.IsChecked = Setting.LyricsFontBold;
            chkListenHitory.IsChecked = Setting.RecordListenHistory;
            chkOneLineFloatLyric.IsChecked = Setting.ShowOneLineInFloatLyric;
            txtListenHistoryValue.Text = Setting.ThresholdValueOfListenTime.ToString();
            mainColor.ColorBrush = Setting.BackgroundColor;
            if (!cbbFloatFont.SetSelectedFontByString(Setting.FloatLyricsFont) || !cbbFont.SetSelectedFontByString(Setting.LyricsFont))
            {
                trayIcon.ShowMessage("字体文件设置异常，请重新设置");
            }
            DefaultDialogOwner = this;

            HotKeyHelper.UnregistAll();
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

                double? floatBlur = txtFloatBlur.DoubleNumber;
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
                switch (cbbMusicFx.SelectedIndex)
                {
                    case 0:
                        Setting.MusicFxMode = Enum.MusicFxRemainMode.Not;
                        break;

                    case 1:
                        Setting.MusicFxMode = Enum.MusicFxRemainMode.All;
                        break;

                    case 2:
                        Setting.MusicFxMode = Enum.MusicFxRemainMode.Each;
                        break;
                }
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
                if (chkShowTray.IsChecked.Value == false && Setting.ShowTray)
                {
                    trayIcon.Hide();
                }
                else if (chkShowTray.IsChecked.Value && Setting.ShowTray == false)
                {
                    trayIcon.Show();
                }
                Setting.ShowTray = chkShowTray.IsChecked.Value;
                Setting.FloatLyricsFontEffect = cbbFloatFontEffect.SelectedIndex;
                Setting.FloatLyricsBorderColor = floatBorderColor.ColorBrush;
                Setting.FloatLyricsFontColor = floatFontColor.ColorBrush;
                Setting.FloatLyricsThickness = floatBorder.Value;
                Setting.FloatLyricsBlurRadius = floatBlur.Value;
                Setting.FloatLyricsFontBold = chkFloatBold.IsChecked.Value;
                Setting.FloatLyricsFont = cbbFloatFont.GetPreferChineseFontName();
                Setting.LyricsFontBold = chkBold.IsChecked.Value;
                Setting.LyricsFont = cbbFont.GetPreferChineseFontName();
                Setting.LyricsFontColor = fontColor.ColorBrush;
                Setting.RecordListenHistory = chkListenHitory.IsChecked.Value;
                Setting.ThresholdValueOfListenTime = listenValue.Value;
                Setting.BackgroundColor = mainColor.ColorBrush;
                Setting.ShowOneLineInFloatLyric = chkOneLineFloatLyric.IsChecked.Value;
                (App.Current as App).UpdateColor();
                if (MusicControlHelper.Music != null)
                {
                    MainWindow.Current.InitializeLrc();
                }
                Setting.Save();
            }
            catch (Exception ex)
            {
                ShowException("保存设置失败", ex);
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

            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                Title = "请选择保存位置",
                AlwaysAppendDefaultExtension = true,
            };

            int type = ShowMessage("请选择导出类型", FzLib.Control.Dialog.DialogType.Information, new string[] { "仅设置", "所有文件" });
            if (type == 0)
            {
                dialog.Filters.Add(new CommonFileDialogFilter("Json设置", "json"));
                dialog.DefaultExtension = "json";
            }
            else if (type == 1)
            {
                dialog.Filters.Add(new CommonFileDialogFilter("所有配置", "zip"));
                dialog.DefaultExtension = "zip";
            }
            else
            {
                return;
            }
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    if (dialog.FileName.EndsWith("zip"))
                    {
                        ZipFile.CreateFromDirectory(ConfigPath, dialog.FileName);
                    }
                    else if (dialog.FileName.EndsWith("json"))
                    {
                        File.Copy(ConfigPath + "\\Config.json", dialog.FileName);
                    }
                    else
                    {
                        ShowError("请好好选择");
                    }
                }
                catch (Exception ex)
                {
                    ShowException("导出失败", ex);
                }
            }
        }

        private void ButtonImportClickEventHandler(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = "请选择文件",
            };
            dialog.Filters.Add(new CommonFileDialogFilter("设置文件", "json"));
            dialog.Filters.Add(new CommonFileDialogFilter("所有配置文件", "zip"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    if (dialog.FileName.EndsWith("zip"))
                    {
                        ZipFile.CreateFromDirectory(ConfigPath, new FileInfo(ConfigPath).DirectoryName + "EasyMusic_OldFiles_" + DateTime.Now.ToString("yyyyMMdd_hhMMss"));
                        Directory.Delete(ConfigPath, true);
                        ZipFile.ExtractToDirectory(dialog.FileName, ConfigPath);
                    }
                    else if (dialog.FileName.EndsWith("json"))
                    {
                        if (File.Exists(ConfigPath + "\\Config.json"))
                        {
                            if (File.Exists(ConfigPath + "\\Config.json.bak"))
                            {
                                File.Delete(ConfigPath + "\\Config.json.bak");
                            }
                            File.Move(ConfigPath + "\\Config.json", ConfigPath + "\\Config.json.bak");
                        }
                        File.Copy(dialog.FileName, ConfigPath + "\\Config.json");
                    }
                    else
                    {
                        ShowError("只支持Json或Zip文件");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowException("导入失败", ex);
                    return;
                }
            }
            MainWindow.Current.SkipSavingSettings = true;
            ShowPrompt("导入成功，将重启以生效");
            //Process.Start(Application.ExecutablePath, "restart");
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetExecutingAssembly().Location + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Current.Shutdown();
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

        public HotKey NextHotKey
        {
            get => GetHotKey("下一曲");
            set
            {
                SetHotKey("下一曲", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextHotKey)));
            }
        }

        public HotKey LastHotKey
        {
            get => GetHotKey("上一曲");
            set
            {
                SetHotKey("上一曲", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastHotKey)));
            }
        }

        public HotKey VolumnUpHotKey
        {
            get => GetHotKey("音量加");
            set
            {
                SetHotKey("音量加", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VolumnUpHotKey)));
            }
        }

        public HotKey VolumnDownHotKey
        {
            get => GetHotKey("音量减");
            set
            {
                SetHotKey("音量减", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VolumnDownHotKey)));
            }
        }

        public HotKey PlayPauseHotKey
        {
            get => GetHotKey("播放暂停");
            set
            {
                SetHotKey("播放暂停", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayPauseHotKey)));
            }
        }

        public HotKey FloatLyricHotKey
        {
            get => GetHotKey("悬浮歌词");
            set
            {
                SetHotKey("悬浮歌词", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FloatLyricHotKey)));
            }
        }

        public HotKey ListHotKey
        {
            get => GetHotKey("收放列表");
            set
            {
                SetHotKey("收放列表", value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListHotKey)));
            }
        }

        private HotKey GetHotKey(string name)
        {
            if (HotKeyHelper.HotKeys.TryGetValue(name, out FzLib.Device.HotKey.HotKeyInfo value))
            {
                return value == null ? null : new HotKey(value.Key, value.Modifiers);
            }
            return null;
        }

        private void SetHotKey(string name, HotKey value)
        {
            if (value.Key == System.Windows.Input.Key.Escape)
            {
                HotKeyHelper.HotKeys[name] = null;
                return;
            }
            if (HotKeyHelper.HotKeys.Any(p => p.Key != name && p.Value != null && p.Value.Key == value.Key && p.Value.Modifiers == value.ModifierKeys))
            {
                ShowError("已存在相同热键");
            }
            else
            {
                HotKeyHelper.HotKeys[name] = new FzLib.Device.HotKey.HotKeyInfo(value.Key, value.ModifierKeys);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void WindowClosed(object sender, EventArgs e)
        {
            DefaultDialogOwner = MainWindow.Current;
            HotKeyHelper.RegistGolbalHotKey();
        }

        private void TestHotKeysButtonClick(object sender, RoutedEventArgs e)
        {
            if (HotKeyHelper.RegistGolbalHotKey())
            {
                ShowPrompt("测试通过");
            }
            HotKeyHelper.UnregistAll();
        }
    }
}