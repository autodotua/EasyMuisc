using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Un4seen.Bass;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Linq;
using System.Windows.Shell;
using EasyMusic.Windows;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using WpfCodes.Basic;
using EasyMusic.Info;
using System.Security.Principal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using WpfControls.Dialog;
using EasyMusic.UserControls;
using System.Threading.Tasks;
using WpfCodes.WindowsApi;
using EasyMusic.Enum;

namespace EasyMusic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;

        #region 字段
        ///// <summary>
        ///// 颜色选择器
        ///// </summary>
        //ColorPickerControlView colorPicker = new ColorPickerControlView();
        /// <summary>
        /// 
        /// </summary>
        public FloatLyrics floatLyric;

        #endregion


        #region 属性
        

        /// <summary>
        /// 是否显示加载动画
        /// </summary>
        public bool LoadingSpinner
        {
            set
            {
                loading.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                //if (value)
                //{
                //    grdLoading.Visibility = Visibility.Visible;
                //    NewDoubleAnimation(grdLoading, OpacityProperty, 0.5, 0.5, 0);

                //}
                //else
                //{
                //    NewDoubleAnimation(grdLoading, OpacityProperty, 0, 0.5, 0, (p1, p2) => grdLoading.Visibility = Visibility.Hidden);
                //}
            }
        }
        #endregion

        #region 设置

        ///// <summary>
        ///// 自动收放列表
        ///// </summary>
        //private bool AutoFurl
        //{
        //    get => bool.Parse(GetConfig("AutoFurl", "True"));
        //    set => SetConfig("AutoFurl", value.ToString());
        //}
        ///// <summary>
        ///// 显示歌词
        ///// </summary>
        //private bool ShowLrc
        //{
        //    get => bool.Parse(GetConfig("ShowLrc", "True"));
        //    set => SetConfig("ShowLrc", value.ToString());
        //}
        ///// <summary>
        ///// 是否将歌词时间偏移量保存在Lrc标签中
        ///// </summary>
        //public bool SaveLrcOffsetByTag
        //{
        //    get => bool.Parse(GetConfig("SaveLrcOffsetByTag", "False"));
        //    set => SetConfig("SaveLrcOffsetByTag", value.ToString());
        //}
        ///// <summary>
        ///// 是否在保存歌词时优先选择歌曲信息代替原歌词信息
        ///// </summary>
        //public bool PreferMusicInfo
        //{
        //    get => bool.Parse(GetConfig("PreferMusicInfo", "False"));
        //    set => SetConfig("PreferMusicInfo", value.ToString());
        //}
        ///// <summary>
        ///// 是否显示歌词动画
        ///// </summary>
        //public bool LrcAnimation
        //{
        //    get => bool.Parse(GetConfig("LrcAnimation", "True"));
        //    set => SetConfig("LrcAnimation", value.ToString());
        //}
        ///// <summary>
        ///// 歌词默认偏移量
        ///// </summary>
        //public double LrcDefautOffset
        //{
        //    get => double.Parse(GetConfig("LrcDefautOffset", "0"));
        //    set => SetConfig("LrcDefautOffset", value.ToString());
        //}
        ///// <summary>
        ///// 每秒钟检测次数
        ///// </summary>
        //public double UpdateSpeed
        //{
        //    get => double.Parse(GetConfig("UpdateFps", "30"));
        //    set
        //    {
        //        SetConfig("UpdateFps", value.ToString());
        //        mainTimer.Interval = TimeSpan.FromMilliseconds(1000 / value);
        //    }
        //}
        ///// <summary>
        ///// 动画帧率
        ///// </summary>
        public int AnimationFps
        {
            get => Setting.AnimationFps;
            set
            {
                if (value == -1)
                {
                    Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = AnimationFps });
                }
                else
                {
                    Setting.AnimationFps = value;
                }
            }
        }
        //}
        ///// <summary>
        ///// 使用列表框歌词栏来代替StackPanel
        ///// </summary>
        //public bool UseListBoxLrcInsteadOfStackPanel
        //{
        //    get => bool.Parse(GetConfig("UseListBoxLrcInsteadOfStackPanel", "False"));
        //    set => SetConfig("UseListBoxLrcInsteadOfStackPanel", value.ToString());

        //}
        ///// <summary>
        ///// 是否手动收缩了歌曲列表
        ///// </summary>
        //public bool ShrinkMusicListManually
        //{
        //    get => bool.Parse(GetConfig("ShrinkMusicListManually", "False"));
        //    set => SetConfig("ShrinkMusicListManually", value.ToString());

        //}
        //public bool ShowFloatLyric
        //{
        //    get => bool.Parse(GetConfig("ShowFloatLyric", "True"));
        //    set => SetConfig("ShowFloatLyric", value.ToString());

        //}
        #endregion

        #region 定时器
        /// <summary>
        /// 主定时器
        /// </summary>
        DispatcherTimer mainTimer;

        #endregion

        #region 初始化和配置

        /// <summary>
        /// 更新主题颜色
        /// </summary>
        public void UpdateColor(SolidColorBrush color)
        {
            //var color = colorPicker.CurrentColor;
            Resources["backgroundBrushColor"] = color;
            WpfControls.DarkerBrushConverter.GetDarkerColor(color, out SolidColorBrush darker1, out SolidColorBrush darker2, out SolidColorBrush darker3, out SolidColorBrush darker4);
            //Resources["darker1BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.9f, color.Color.ScG * 0.9f, color.Color.ScB * 0.9f));
            //Resources["darker2BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.8f, color.Color.ScG * 0.8f, color.Color.ScB * 0.8f));
            //Resources["darker3BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.7f, color.Color.ScG * 0.7f, color.Color.ScB * 0.7f));
            //Resources["darker4BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.6f, color.Color.ScG * 0.6f, color.Color.ScB * 0.6f));
            Resources["darker1BrushColor"] = darker1;
            Resources["darker2BrushColor"] = darker2;
            Resources["darker3BrushColor"] = darker3;
            Resources["darker4BrushColor"] = darker4;
            Resources["backgroundColor"] = color.Color;
            Resources["backgroundTransparentColor"] = Color.FromArgb(0, color.Color.R, color.Color.G, color.Color.B);

            Resources["foregroundBrushColor"] = new SolidColorBrush(Colors.Black);

        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            Current = this;
            InitializeComponent();
            DefautDialogOwner = this;

            windowHandle = new WindowInteropHelper(this).Handle;

            if (Setting.MaxWindow)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;

                Top = Setting.Top;
                Left = Setting.Left;
                Width = Setting.Width;
                Height = Setting.Height;

            }

            Un4seen.Bass.AddOn.Fx.BassFx.LoadMe();

            if (!Bass.BASS_Init(-1, Setting.SampleRate/*无设置界面*/, BASSInit.BASS_DEVICE_DEFAULT, new WindowInteropHelper(this).Handle))
            {
                ShowError("无法初始化音乐引擎：" + Environment.NewLine + Bass.BASS_ErrorGetCode());
                Application.Current.Shutdown();
            }
            WindowHelper.RepairWindowBehavior(this);

            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });

            // colorPicker.CurrentColor = new BrushConverter().ConvertFrom(set.BackgroundColor) as SolidColorBrush;
            UpdateColor(new BrushConverter().ConvertFrom(Setting.BackgroundColor) as SolidColorBrush);

            InitialiazeField();
            header.HeaderTextMaxWidth = SystemParameters.WorkArea.Width - 200;

            InitializeAnimation();

            ReloadFloatLrc();

            InitializeTray();

        }


        /// <summary>
        /// 初始化定时器事件
        /// </summary>
        private void InitialiazeField()
        {
            mainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / Setting.UpdateSpeed) };
            mainTimer.Tick += UpdateTick;
            //playTimer.Tick += (p1, p2) =>
            //{

            //    if (Volumn >= sldVolumn.Value)
            //    {
            //        playTimer.Stop();
            //        return;
            //    }
            //    Volumn += 0.05;
            //};
            //pauseTimer.Tick += (p1, p2) =>
            //{

            //    if (Volumn <= 0.05)
            //    {
            //        Bass.BASS_ChannelPause(stream);
            //        pauseTimer.Stop();
            //        if (closing)
            //        {
            //            Application.Current.Shutdown();
            //        }
            //        return;
            //    }
            //    Volumn -= 0.05;
            //};

            StringBuilder str = new StringBuilder();
            foreach (var i in supportExtension)
            {
                str.Append($"*{i}|");
            }
            str.Remove(str.Length - 1, 1);
            supportExtensionWithSplit = str.ToString();
        }
        /// <summary>
        /// 窗体载入事件，获取音乐列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowLoadedEventHandler(object sender, RoutedEventArgs e)
        {

            RegistGolbalHotKey();

            if (argPath != null && File.Exists(argPath))
            {
                PlayNew(await AddMusic(argPath), true);
            }
            else
            {

                string tempPath = Setting.LastMusic;
                if (File.Exists(tempPath) && MusicDatas.Where(p => p.Path.Equals(tempPath)).Count() != 0)
                {
                    PlayNew(await AddMusic(tempPath), false);
                }
            }



            if (!Setting.ShowLrc)
            {
                Setting.ShowLrc = false;
                grdLyricArea.Visibility = Visibility.Collapsed;
                // set.AutoFurl = false;
                grdMain.ColumnDefinitions[2].Width = new GridLength(0);
                grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                Width = grdMain.ColumnDefinitions[0].ActualWidth + 32;

            }


            //set. = double.Parse(GetConfig("NormalLrcFontSize", normalLrcFontSize.ToString()));
            //highlightLrcFontSize = double.Parse(GetConfig("HighlightLrcFontSize", highlightLrcFontSize.ToString()));
            //textLrcFontSize = double.Parse(GetConfig("TextLrcFontSize", textLrcFontSize.ToString()));
            //sldVolumn.Value = double.Parse(GetConfig("Volumn", "1"));
            Topmost = Setting.Topmost;
            //if (set.ShrinkMusicListManually)
            //{
            //    SleepThenDo(1000, (p1, p2) => BtnListSwitcherClickEventHandler(null, null));
            //}

        }




        //public void ShowTrayMessage(string message, int ms = 2000)
        //{
        //    trayIcon.BalloonTipText = message;
        //    trayIcon.ShowBalloonTip(ms);
        //}
        /// <summary>
        /// 托盘图标
        /// </summary>
        private void InitializeTray()
        {
            trayIcon = new WpfCodes.Program.TrayIcon(Properties.Resources.icon, "EasyMusic");

            trayIcon.MouseLeftClick += (p1, p2) =>
            {
                if (Visibility == Visibility.Hidden)
                {
                    Show();
                    Topmost = true;
                    Topmost = Setting.Topmost;
                    Activate();
                }
                else
                {
                    Visibility = Visibility.Hidden;
                }
            };

            trayIcon.AddContextMenu("悬浮歌词开关", () =>
            {
                Setting.ShowFloatLyric = !Setting.ShowFloatLyric;
                floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                floatLyric.Update(lrc.CurrentIndex);
            });
            trayIcon.AddContextMenu("退出", () => CloseWindow(true));

            if (Setting.TrayMode != 0)
            {
                trayIcon.Show();
            }
        }

        private bool readyToExit = false;

        public void CloseWindow(bool exitApp = false)
        {
            readyToExit = exitApp;
            Close();
        }

        public void OnStatusChanged(ControlStatus status)
        {
            switch (status)
            {
                case ControlStatus.Play:
                    tbiPlay.Visibility = Visibility.Collapsed;
                    tbiPause.Visibility = Visibility.Visible;
                    if (!mainTimer.IsEnabled)
                    {
                        mainTimer.Start();
                    }
                    break;
                case ControlStatus.Pause:
                    tbiPause.Visibility = Visibility.Collapsed;
                    tbiPlay.Visibility = Visibility.Visible;
                    // if (playTimer.IsEnabled)
                    // {
                    //     playTimer.Stop();
                    // }
                    //pauseTimer.Start();
                    break;
            }
            controlBar.OnStatusChanged(status);
        }

        /// <summary>
        /// 初始化歌词
        /// </summary>
        /// <param name="musicLength"></param>
        public void InitialiazeLrc()
        {
            try
            {
                lrc = null;
                floatLyric.SetFontEffect();

                FileInfo file = new FileInfo(Music.FilePath);
                file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
                if (file.Exists)//判断是否存在歌词文件
                {
                    lyricArea.CurrentLyricType = LyricType.LrcFormat;
                    lrc = new LyricInfo(file.FullName);//获取歌词信息
                    lrc.Offset /= 1000.0;
                   
                        lyricArea.AddLrcs();
                    
                    //lbxLrc.Add(lrc.LrcContent);
                    //lbxLrc.ChangeFontSize(normalLrcFontSize);
                   
                    // ReloadFloatLrc();
                    floatLyric.Reload(lrc.LrcContent.Values.ToList());
                }
                else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
                {
                    lyricArea.LoadTextFormatLyric(File.ReadAllText(file.FullName, EncodingType.GetType(file.FullName)));

                   
                    lyricArea.CurrentLyricType = LyricType.TextFormat;

                }
                else
                {
                    lyricArea.CurrentLyricType = LyricType.None;
                    if (Setting.ShowFloatLyric)
                    {
                        floatLyric.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("初始化歌词失败！", ex);
            }
        }

        public void InitializeNewMusic()
        {
            lvwMusic.SelectAndScroll(Music.Info);//选中列表中的歌曲
            InitialiazeLrc();
            header.HeaderText = Title = Music.Name + " - EasyMusic";
            header.AlbumImageSource = Music.AlbumImage;
        }
        #endregion
        #region 快捷键
        /// <summary>
        /// 执行前进快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyFowardEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Music.Position += 4;
        }
        /// <summary>
        /// 执行后退快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyBackEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            MusicControlHelper.Music.Position -= 4;
        }
        /// <summary>
        /// 执行播放暂停快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HotKeyPlayAndPauseEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Music.PlayOrPause();
        }
        /// <summary>
        /// 执行下一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyNextEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            MusicControlHelper.PlayListNext();
        }
        /// <summary>
        /// 执行上一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyLastEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            PlayLast();

        }


        HotKey hotKey;

        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegistGolbalHotKey()
        {

            if (hotKey != null)
            {
                hotKey.Dispose();
            }
            hotKey = new HotKey();
            Dictionary<string, HotKey.HotKeyInfo> hotKeys = null;

            try
            {
                string json = File.ReadAllText(Setting.ConfigPath + "\\HotKeyConfig.json");

                hotKeys = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, HotKey.HotKeyInfo>>(json);
            }
            catch
            {
                hotKeys = new Dictionary<string, HotKey.HotKeyInfo>()
                {
                    {"下一曲",new HotKey.HotKeyInfo(Key.Right,ModifierKeys.Control) },
                    {"上一曲",new HotKey.HotKeyInfo(Key.Left,ModifierKeys.Control) },
                    {"音量加",new HotKey.HotKeyInfo(Key.Up,ModifierKeys.Control) },
                    {"音量减",new HotKey.HotKeyInfo(Key.Down,ModifierKeys.Control) },
                    {"播放暂停",new HotKey.HotKeyInfo(Key.OemQuestion,ModifierKeys.Control) },
                    {"悬浮歌词",new HotKey.HotKeyInfo(Key.OemPeriod,ModifierKeys.Control) },
                    {"收放列表",new HotKey.HotKeyInfo(Key.OemComma,ModifierKeys.Control) },
                };
            }

            string error = "";

            hotKey.KeyPressed += (p1, p2) =>
            {
                if (hotKeys.ContainsValue(p2.HotKey))
                {
                    string command = hotKeys.First(p => p.Value.Equals(p2.HotKey)).Key;
                    switch (command)
                    {
                        case "下一曲":
                            PlayNext();
                            break;
                        case "上一曲":
                            PlayLast();
                            break;
                        case "音量加":
                            Volumn += 0.05;
                            break;
                        case "音量减":
                            Volumn -= 0.05;
                            break;
                        case "播放暂停":
                            Music.PlayOrPause();
                            break;
                        case "悬浮歌词":
                            BtnListSwitcherClickEventHandler(null, null);
                            break;
                        case "收放列表":
                            OpenOrCloseFloatLrc();
                            break;
                    }

                }
            };

            foreach (var key in hotKeys)
            {
                try
                {
                    hotKey.Register(key.Value);
                }
                catch
                {
                    error += key.Key + "（" + key.Value.ToString() + "）";
                }
            }

            if (error != "")
            {
                trayIcon.ShowMessage("以下热键无法注册，可能已被占用：" + error.TrimEnd('、'));
            }
        }
        #endregion

        #region 语音识别

        //private void RegistSpeechRecognition()
        //{
        //    return;
        //    SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(System.Globalization.CultureInfo.CurrentCulture);

        //    //----------------
        //    //初始化命令词
        //    Choices conmmonds = new Choices();
        //    //添加命令词
        //    conmmonds.Add(new string[] { "播放", "暂停", "轻一点", "响一点", "多轻一点", "多响一点" });
        //    //初始化命令词管理
        //    GrammarBuilder gBuilder = new GrammarBuilder();
        //    //将命令词添加到管理中
        //    gBuilder.Append(conmmonds);
        //    //实例化命令词管理
        //    Grammar grammar = new Grammar(gBuilder);
        //    //-----------------

        //    //创建并加载听写语法(添加命令词汇识别的比较精准)
        //    recognizer.LoadGrammar(grammar);
        //    //为语音识别事件添加处理程序。
        //    recognizer.SpeechRecognized += RecognizerSpeechRecognizedEventHandler;
        //    //将输入配置到语音识别器。
        //    recognizer.SetInputToDefaultAudioDevice();
        //    //启动异步，连续语音识别。
        //    recognizer.RecognizeAsync(RecognizeMode.Multiple);

        //    sy.SelectVoiceByHints(VoiceGender.Neutral);


        //}
        //SpeechSynthesizer sy = new SpeechSynthesizer();
        //private void RecognizerSpeechRecognizedEventHandler(object sender, SpeechRecognizedEventArgs e)
        //{
        //    switch (e.Result.Text)
        //    {
        //        case "暂停":
        //            if (btnPause.Visibility == Visibility.Visible)
        //            {
        //                BtnPauseClickEventHandler(null, null);
        //            }
        //            break;
        //        case "播放":
        //            if (btnPlay.Visibility == Visibility.Visible)
        //            {
        //                BtnPlayClickEventHandler(null, null);
        //            }
        //            break;
        //        case "轻一点":
        //            sldVolumn.Value -= 0.05;
        //            sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
        //            break;
        //        case "响一点":
        //            sldVolumn.Value += 0.05;
        //            sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
        //            break;
        //        case "多轻一点":
        //            sldVolumn.Value -= 0.2;
        //            sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
        //            break;
        //        case "多响一点":
        //            sldVolumn.Value += 0.2;
        //            sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
        //            break;
        //    }
        //}

        #endregion


        #region 鼠标滚轮
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            //    if (mouseInLrcArea && (stkLrc.Visibility == Visibility.Visible || lbxLrc.Visibility == Visibility.Visible))
            //    {
            //        if (e.Delta > 0)
            //        {
            //            offset -= 1d / 4;
            //        }
            //        else
            //        {
            //            offset += 1d / 4;
            //        }
            //        ShowInfo("当前歌词偏移量：" + (offset > 0 ? "+" : "") + Math.Round(offset, 2).ToString() + "秒");
            //    }
            //    else 
            if (lyricArea.IsMouseOver && lyricArea.CurrentLyricType==LyricType.LrcFormat)
            {
                if (e.Delta > 0)
                {
                    lrc.Offset -= 1d / 4;
                }
                else
                {
                    lrc.Offset += 1d / 4;
                }
                lyricArea. ShowMessage("当前歌词偏移量：" + (lrc.Offset > 0 ? "+" : "") + Math.Round(lrc.Offset, 2).ToString() + "秒");
            }
            else if (!lvwMusic.IsMouseOver)
            {
                Volumn += e.Delta > 0 ? 0.05 : -0.05;
            }
        }


        #endregion

        #region 列表相关

        /// <summary>
        /// 单击收放列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListSwitcherClickEventHandler(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation aniMargin = new ThicknessAnimation
            {
                Duration = new Duration(Setting.AnimationDuration),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }

            };
            DoubleAnimation aniOpacity = new DoubleAnimation
            {
                Duration = new Duration(Setting.AnimationDuration),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }

            };
            Storyboard.SetTarget(aniMargin, lvwMusic);
            Storyboard.SetTarget(aniOpacity, lvwMusic);

            Storyboard.SetTargetProperty(aniMargin, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(OpacityProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(aniMargin);
            story.Children.Add(aniOpacity);

            if (lvwMusic.Margin.Left < 0)
            {
                aniMargin.To = new Thickness(0, 0, 0, 0);
                aniOpacity.To = 1;
                // set.ShrinkMusicListManually = false;
            }
            else
            {
                aniMargin.To = new Thickness(-lvwMusic.ActualWidth, 0, 0, 0);
                aniOpacity.To = 0;
                // set.ShrinkMusicListManually = true;
            }
            story.Begin();

        }
        /// <summary>
        /// 窗体大小改变事件，自动收放列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
          

        }
        /// <summary>
        /// 将文件拖到列表上方事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowDragEnterEventHandler(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

        }
        /// <summary>
        /// 将文件拖到窗体上释放事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowDropEventHandler(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files == null)
            {
                return;
            }
            if (files.Length == 1 && File.Exists(files[0]))
            {
                int currentCount = MusicCount;
                string extension = new FileInfo(files[0]).Extension;
                if (supportExtension.Contains(extension))
                {
                    await AddMusic(files[0]);
                    if (MusicCount > currentCount + 1)
                    {
                        PlayNew(MusicDatas[currentCount + 1]);
                    }
                }
            }
            else
            {
                List<string> musics = new List<string>();
                foreach (var i in files)
                {
                    FileInfo file = new FileInfo(i);
                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        foreach (var j in EnumerateFiles(i, supportExtensionWithSplit, SearchOption.AllDirectories))
                        {
                            if (!musics.Contains(j))
                            {
                                musics.Add(j);
                            }
                        }
                    }
                    else
                    {
                        string extension = file.Extension;
                        if (supportExtension.Contains(extension))
                        {
                            if (!musics.Contains(i))
                            {
                                musics.Add(i);
                            }
                        }
                    }
                }
                AddMusic(musics.ToArray());
            }
        }
        #endregion

        #region 列表与歌词选项

        public void ExpandLyricsArea()
        {
            Setting.ShowLrc = true;
            grdLyricArea.Visibility = Visibility.Visible;
            grdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            grdMain.ColumnDefinitions[0].Width = GridLength.Auto;

            NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);
        }
        public void ShrinkLyricsArea()
        {
            Setting.ShowLrc = false;
            grdLyricArea.Visibility = Visibility.Collapsed;
            //set.AutoFurl = false;
            NewDoubleAnimation(this, WidthProperty, grdMain.ColumnDefinitions[0].ActualWidth + 32, 0.5, 0.3, (p3, p4) =>
            {
                grdMain.ColumnDefinitions[2].Width = new GridLength(0);

                grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            });
        }
        /// <summary>
        /// 打开关闭悬浮歌词
        /// </summary>
        public void OpenOrCloseFloatLrc()
        {
            Setting.ShowFloatLyric = !Setting.ShowFloatLyric;
            floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (lrc == null)
            {
                floatLyric.Update(-1);
            }
            else
            {

                floatLyric.Update(lrc.CurrentIndex);
            }
        }
        /// <summary>
        /// 执行清空列表后的清理工作
        /// </summary>
        public void AfterClearList()
        {
            Music = null;
            lyricArea.HideAll();
            if (Setting.ShowFloatLyric)
            {
                floatLyric.Clear();
            }
            Title = "EasyMusic";
            header.HeaderText = "EasyMusic";
            header.AlbumImageSource = null;
            controlBar.OnStatusChanged(ControlStatus.Pause);
        }
        /// <summary>
        /// 重新加载悬浮歌词
        /// </summary>
        private void ReloadFloatLrc()
        {
            if (floatLyric != null)
            {
                floatLyric.Close(true);
            }

            floatLyric = new FloatLyrics()
            {
                Top = Setting.FloatLyricsTop,
                Left = Setting.FloatLyricsLeft,
                Height = Setting.FloatLyricsHeight,
                Width = Setting.FloatLyricsWidth,
            };
            if (Setting.ShowFloatLyric)
            {
                floatLyric.Show();
            }
        }
        #endregion



        #region 任务栏按钮
        /// <summary>
        /// 单击任务栏上的播放按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThumbButtonClick(object sender, EventArgs e)
        {
            if (Music != null)
            {
                switch ((sender as ThumbButtonInfo).Description)

                {
                    case "下一曲":
                        PlayNext();
                        break;
                    case "上一曲":
                        PlayLast();
                        break;
                    case "播放":
                        Music.Play();
                        tbiPlay.Visibility = Visibility.Collapsed;
                        tbiPause.Visibility = Visibility.Visible;
                        break;
                    case "暂停":

                        Music.Pause();
                        tbiPause.Visibility = Visibility.Collapsed;
                        tbiPlay.Visibility = Visibility.Visible;

                        break;
                }
            }
        }


        #endregion

        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcTime = 0;
        /// <summary>
        /// 歌词故事板
        /// </summary>
        Storyboard storyLrc = new Storyboard();
        /// <summary>
        /// 歌词动画
        /// </summary>
        ThicknessAnimation aniLrc = new ThicknessAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.8)), DecelerationRatio = 0.5 };

        /// <summary>
        /// 初始化歌词动画
        /// </summary>
        private void InitializeAnimation()
        {
            if (Setting.LrcAnimation)
            {
                //Storyboard.SetTargetName(aniLrc, stkLrc.Name);
                Storyboard.SetTargetProperty(aniLrc, new PropertyPath(MarginProperty));
                storyLrc.Children.Add(aniLrc);
            }
            AnimationFps = -1;
        }
        /// <summary>
        /// 定时更新各项数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTick(object sender, EventArgs e)
        {
            if (Music == null || Music.Status != BASSActive.BASS_ACTIVE_PLAYING)
            {
                mainTimer.Stop();
                return;
            }
            if (!controlBar.IsManuallyChangingPosition)
            {
                double position = Music.Position;
                controlBar.UpdatePosition(position);

                UpdatePosition(position);
                if (Music.IsEnd)
                {
                    PlayNext();
                }
            }
        }
        /// <summary>
        /// 更新当前时间的歌词
        /// </summary>
        public void UpdatePosition(double position)
        {
            controlBar.UpdatePosition(position);
            if (lrc == null || lrc.LrcContent.Count == 0)
            {
                return;
            }
            bool changed = false;//是否
            if (position == 0 && lrc.CurrentIndex != 0)//如果还没播放并且没有更新位置
            {
                changed = true;
                lrc.CurrentIndex = 0;
            }
            else
            {
                for (int i = 0; i < lrc.LrcContent.Count - 1; i++)//从第一个循环到最后一个歌词时间
                {
                    if (lrc.LrcContent.Keys.ElementAt(i + 1) > position + lrc.Offset + Setting.LrcDefautOffset)//如果下一条歌词的时间比当前时间要后面（因为增序判断所以这一条歌词时间肯定小于的）
                    {
                        if (lrc.CurrentIndex != i)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            lrc.CurrentIndex = i;
                        }
                        break;
                    }
                    else if (i == lrc.LrcContent.Count - 2 && lrc.LrcContent.Keys.ElementAt(i + 1) < position + lrc.Offset + Setting.LrcDefautOffset)
                    {
                        if (lrc.CurrentIndex != i + 1)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            lrc.CurrentIndex = i + 1;
                        }
                        break;
                    }
                }
            }

            if (changed)
            {
                lyricArea.Update();
                if (Setting.ShowFloatLyric)
                {
                    floatLyric.Update(lrc.CurrentIndex);
                }
            }



        }

        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenHistory.RecordEnd();
            SaveListToFile(lvwMusic.lastMusicListBtn.Text, false);
            Setting.CycleMode = (int)MusicControlHelper.CycleMode;
            if (Music != null)
            {
                Setting.LastMusic = Music.FilePath;
            }
            Setting.LastMusicList = lvwMusic.lastMusicListBtn.Text;
            Setting.Top = Top;
            Setting.Left = Left;
            Setting.Height = Height;
            Setting.Width = Width;
            Setting.MaxWindow = (WindowState == WindowState.Maximized);

            if (Setting.ShowFloatLyric)
            {
                floatLyric.Close(true);
            }

            Setting.Save();
            if (readyToExit)
            {
                if (Music != null)
                {
                    Music.Dispose();
                }
                App.Current.Shutdown();
            }
            else
            {
                Hide();
            }
        }
        /// <summary>
        /// 窗体状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinMainStateChangedEventHandler(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(4);
            }
        }


    }

}
