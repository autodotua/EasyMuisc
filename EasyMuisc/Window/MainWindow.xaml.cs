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
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Text.RegularExpressions;
using EasyMusic.Windows;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;
using WpfControls.Dialog;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using EasyMusic.Info;
using EasyMusic.UserControl;
using System.Threading.Tasks;
using WpfCodes.WindowsApi;

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
        /// 获取当前的播放循环模式
        /// </summary>

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
        /// 是否正在关闭
        /// </summary>
        bool closing = false;
        /// <summary>
        /// 分隔符
        /// </summary>
        const string split = "#Split#";

        /// <summary>
        /// 是否产生了不可挽救错误
        /// </summary>
        bool error = false;

        public bool IsProcessing
        {
            set
            {

            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            Current = this;

            //if (!File.Exists("bass.dll"))
            //{
            //    File.WriteAllBytes("bass.dll", Properties.Resources.bass);
            //}
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
                error = true;
                //Application.Current.Shutdown();
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
            txtMusicName.MaxWidth = SystemParameters.WorkArea.Width - 200;

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
                grdLrcArea.Visibility = Visibility.Collapsed;
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

        private void CloseWindow(bool exitApp=false)
        {
            listenHistory.RecordEnd();
            if (error)
            {
                Bass.BASS_Stop();
                return;
            }
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
            closing = true;
            if(exitApp)
            {
                if(Music!=null)
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
                FontFamily font = new FontFamily(Setting.LyricsFont);
                if (font == null)
                {
                    trayIcon.ShowMessage("主界面字体应用失败，请重新设置");
                }
                else
                {
                    lbxLrc.FontFamily = font;
                }
                lbxLrc.Foreground = new BrushConverter().ConvertFrom(Setting.LyricsFontColor) as SolidColorBrush;
                lbxLrc.FontWeight = Setting.LyricsFontBold ? FontWeights.Bold : FontWeights.Normal;

                lrcLineSumToIndex.Clear();

                // stkLrc.Children.Clear();//清空歌词表
                lbxLrc.Clear();

                FileInfo file = new FileInfo(Music.FilePath);
                file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
                if (file.Exists)//判断是否存在歌词文件
                {
                    grdLrc.Visibility = Visibility.Visible;
                    txtLrc.Visibility = Visibility.Hidden;
                    //if (set.UseListBoxLrcInsteadOfStackPanel)
                    //{
                    lbxLrc.Visibility = Visibility.Visible;
                    lrc = new LyricInfo(file.FullName);//获取歌词信息
                    lrc.Offset /= 1000.0;
                    int index = 0;//用于赋值Tag
                    foreach (var i in lrc.LrcContent)
                    {
                        var tbk = new TextBlock()
                        {
                            Name = "tbk" + index.ToString(),
                            FontSize = Setting.NormalLrcFontSize,
                            Text = i.Value,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Tag = index++,//标签用于定位
                            Cursor = Cursors.Hand,
                            TextAlignment = TextAlignment.Center,
                            FocusVisualStyle = null,
                        };
                        tbk.MouseLeftButtonUp += (p1, p2) => Music.Position = (lrc.LrcContent.ElementAt((int)tbk.Tag).Key - lrc.Offset - Setting.LrcDefautOffset);

                        lbxLrc.Add(tbk);
                    }
                    //lbxLrc.Add(lrc.LrcContent);
                    //lbxLrc.ChangeFontSize(normalLrcFontSize);
                    foreach (var i in lrc.LineIndex)
                    {
                        lrcLineSumToIndex.Add(i.Value);
                    }
                    // ReloadFloatLrc();
                    floatLyric.Reload(lrc.LrcContent.Values.ToList());
                }
                else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
                {

                    txtLrc.Text = File.ReadAllText(file.FullName, EncodingType.GetType(file.FullName));

                    txtLrc.FontSize = Setting.TextLrcFontSize;
                    grdLrc.Visibility = Visibility.Hidden;
                    txtLrc.Visibility = Visibility.Visible;
                    //stkLrc.Visibility = Visibility.Hidden;
                    lbxLrc.Visibility = Visibility.Hidden;

                }
                else
                {
                    grdLrc.Visibility = Visibility.Hidden;
                    txtLrc.Visibility = Visibility.Hidden;
                    //stkLrc.Visibility = Visibility.Hidden;
                    lbxLrc.Visibility = Visibility.Hidden;
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
            txtMusicName.Text=Title = Music.Name + " - EasyMusic";
            imgAlbum.Source = Music.AlbumImage;
        }
        #endregion
        #region 标题栏
        /// <summary>
        /// 主菜单
        /// </summary>
        ContextMenu mainContextMenu;
        /// <summary>
        /// 鼠标是否正在标题栏上且按下
        /// </summary>
        private bool headerMouseDowning = false;
        /// <summary>
        /// 窗体上边界
        /// </summary>
        double reservedTop;
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
        /// 单击菜单按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSettingsClickEventHandler(object sender, RoutedEventArgs e)
        {

            if (mainContextMenu != null)
            {
                (mainContextMenu.Items[0] as MenuItem).Header = Topmost ? "取消置顶" : "置顶";
                mainContextMenu.IsOpen = true;
                return;
            }
            MenuItem menuTop = new MenuItem()
            {
                Header = Topmost ? "取消置顶" : "置顶"
            };
            menuTop.Click += (p1, p2) =>
            {
                Topmost = !Topmost;
                Setting.Topmost = Topmost;
            };
            MenuItem menuFileAssociation = new MenuItem() { Header = "注册格式" };
            menuFileAssociation.Click += MenuFileAssociationClickEventHandler;
            MenuItem menuListenHistory = new MenuItem() { Header = "聆听历史" };
            menuListenHistory.Click += (p1, p2) =>
            {
                WinListenHistory win = new WinListenHistory();
                win.Owner = this;
                win.Show();
            };

            //StackPanel menuColor = new StackPanel()
            //{
            //    Orientation = Orientation.Horizontal,
            //    Children =
            //    {
            //        new TextBlock{Text="背景"},
            //        colorPicker,
            //    },
            //};
            //colorPicker.ChooseComplete((p1, p2) => mainContextMenu.IsOpen = false);

            MenuItem menuSettings = new MenuItem()
            {
                Header = "设置"
            };
            menuSettings.Click += (p1, p2) =>
            {
                new WinSettings() { Owner = this }.ShowDialog();
            };
            MenuItem menuHelp = new MenuItem()
            {
                Header = "帮助"
            };
            menuHelp.Click += (p1, p2) =>
            {
                new WinHelp() { Owner = this }.ShowDialog();
            };
            MenuItem menuAbout = new MenuItem()
            {
                Header = "关于"
            };
            menuAbout.Click += (p1, p2) =>
            {
                new WinAbout() { Owner = this }.ShowDialog();
            };
            mainContextMenu = new ContextMenu()
            {
                PlacementTarget = btnSettings,
                Placement = PlacementMode.Bottom,
                IsOpen = true,
            };
            mainContextMenu.Items.Add(menuTop);
            mainContextMenu.Items.Add(menuFileAssociation);
            if (Setting.RecordListenHistory)
            {
                mainContextMenu.Items.Add(menuListenHistory);
            }
            mainContextMenu.Items.Add(menuSettings);
            mainContextMenu.Items.Add(menuHelp);
            mainContextMenu.Items.Add(menuAbout);

            //mainContextMenu.Closed += (p1, p2) => UpdateColor();
        }

        private void MenuFileAssociationClickEventHandler(object sender, RoutedEventArgs e)
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            if (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                FileAssociationHelper.Associate(".mp3", "EasyMusic", "mp3 文件", AppDomain.CurrentDomain.BaseDirectory + "icon.ico", Process.GetCurrentProcess().MainModule.FileName);
                ShowPrompt("成功");
            }
            else
            {
                ShowError("需要管理员权限，请用管理员权限打开此程序。");
            }
        }

        /// <summary>
        /// 鼠标左键在标题栏上按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// 双击标题栏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }
        /// <summary>
        /// 鼠标在标题栏上按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            headerMouseDowning = true;
        }
        /// <summary>
        /// 鼠标在标题栏上抬起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            headerMouseDowning = false;
        }
        /// <summary>
        /// 单击关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (Setting.TrayMode == 1)
            {
                NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0, (p1, p2) => { Hide(); }, true);
            }
            else
            {
                CloseWindow(true);
            }
        }
        /// <summary>
        /// 单击最大化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMaxmizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }


        /// <summary>
        /// 单击最小化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinimizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            reservedTop = Top;
            NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0, (p1, p2) =>
            {
                if (Setting.TrayMode == 2)
                {
                    Hide();
                }
                else
                {
                    WindowState = WindowState.Minimized;
                }
                // Opacity = 1;
            }, true);
        }
        /// <summary>
        /// 鼠标是否在专辑图上按下了
        /// </summary>
        bool imgAlbumMousePress = false;
        /// <summary>
        /// 单击专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (imgAlbumMousePress)
            {
                WinAlbumPicture win = new WinAlbumPicture();
                win.img.Source = imgAlbum.Source;
                win.ShowDialog();
                imgAlbumMousePress = false;
            }
        }
        /// <summary>
        /// 鼠标按下专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            //不知什么原因，鼠标在Header上按下会出发DragMove然后触发了鼠标在专辑图上抬起事件，所以写此事件确保鼠标确实是在专辑图上
            imgAlbumMousePress = true;
        }
        /// <summary>
        /// 鼠标离开专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            imgAlbumMousePress = false;
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
        #endregion
        #region 快捷键
        /// <summary>
        /// 执行前进快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyFowardEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            MusicControlHelper.Music.Position += 4;
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
        /// <summary>
        /// 在文本歌词和列表按空格同样有效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtLrcAndLvwPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                MainWindow.Current.HotKeyPlayAndPauseEventHandler(null, null);
            }
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

            //try
            //{
            //    hotKey.Register(Key.Right, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "（下一曲）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Left, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ←（上一曲）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Up, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ↑（音量加）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Down, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ↓（音量减）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.OemQuestion, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl /（播放暂停）、";
            //}
            //try
            //{
            //    hotKey.Register(Key.OemComma, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ,（收放列表）、";
            //}
            //try
            //{
            //    hotKey.Register(Key.OemPeriod, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl .（开关悬浮歌词）、";
            //}
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
            if (lbxLrc.IsMouseOver && lbxLrc.Visibility == Visibility.Visible)
            {
                if (e.Delta > 0)
                {
                    lrc.Offset -= 1d / 4;
                }
                else
                {
                    lrc.Offset += 1d / 4;
                }
                ShowInfo("当前歌词偏移量：" + (lrc.Offset > 0 ? "+" : "") + Math.Round(lrc.Offset, 2).ToString() + "秒");
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
            Storyboard.SetTargetName(aniMargin, grdList.Name);
            Storyboard.SetTargetProperty(aniMargin, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(OpacityProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(aniMargin);
            story.Children.Add(aniOpacity);

            if (grdList.Margin.Left < 0)
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
            story.Begin(grdList);

        }
        /// <summary>
        /// 窗体大小改变事件，自动收放列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            lbxLrc.RefreshPlaceholder(grdLrcArea.ActualHeight / 2, Setting.HighlightLrcFontSize);

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
        /// <summary>
        /// 单击列表选项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnListOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = sender as UIElement,

                Placement = PlacementMode.Top,
                IsOpen = true,
                // Style=Resources["ctmStyle"] as Style
            };

            MenuItem menuOpenFile = new MenuItem() { Header = "文件" };
            menuOpenFile.Click += (p1, p2) => FileHelper.ImportMusicsFiles();
            MenuItem menuOpenFolder = new MenuItem() { Header = "文件夹" };
            menuOpenFolder.Click += (p1, p2) => FileHelper.ImportMusicsFolder(false);

            MenuItem menuOpenAllFolder = new MenuItem() { Header = "文件夹及子文件夹" };
            menuOpenAllFolder.Click += (p1, p2) => FileHelper.ImportMusicsFolder(true);

            MenuItem menuAdd = new MenuItem() { Header = "添加到歌单", Items = { menuOpenFile, menuOpenFolder, menuOpenAllFolder } };

            MenuItem menuDeleteSelected = new MenuItem() { Header = "删除选中项" };
            menuDeleteSelected.Click += (p1, p2) =>
            {
                foreach (var item in lvwMusic.SelectedMusics.ToArray())
                {
                    MusicDatas.Remove(item);
                }
            };
            MenuItem menuClear = new MenuItem() { Header = "清空列表", };
            menuClear.Click += (p1, p2) => ClearMusics();
            //MenuItem menuClearExceptCurrent = new MenuItem() { Header = "删除其他", };
            //menuClearExceptCurrent.Click += (p1, p2) =>
            //{
            //    var seletedItems = lvwMusic.SelectedMusics.ToArray();

            //};
            MenuItem menuDelete = new MenuItem() { Header = "删除" };
            if (lvwMusic.SelectedMusic != null)
            {
                menuDelete.Items.Add(menuDeleteSelected);
                //menuDelete.Items.Add(menuClearExceptCurrent);
            }
            menuDelete.Items.Add(menuClear);

            MenuItem menuRandom = new MenuItem() { Header = "随机排序" };
            menuRandom.Click += (p1, p2) => RandomizeList();

            MenuItem menuOpenMusicFolder = new MenuItem() { Header = "打开所在文件夹" };
            menuOpenMusicFolder.Click += (p1, p2) => Process.Start("Explorer.exe", @"/select," + Music.Info.Path);

            MenuItem menuShowMusicInfo = new MenuItem() { Header = "显示音乐信息" };
            menuShowMusicInfo.Click += (p1, p2) => new WinMusicInfo(Music.Info) { Owner = this }.ShowDialog();
            MenuItem menuPlayNext = new MenuItem() { Header = "下一首播放" };
            menuPlayNext.Click += (p1, p2) =>
            {
                if (CurrentHistoryIndex < HistoryCount - 1)
                {
                    RemoveHistory(CurrentHistoryIndex + 1, HistoryCount - CurrentHistoryIndex - 1);
                }
                AddHistory(lvwMusic.SelectedMusic, false);
            };



            MenuItem menuImport = new MenuItem() { Header = "导入歌单" };
            menuImport.Click += (p1, p2) => FileHelper.ImportMusicList();

            MenuItem menuExport = new MenuItem() { Header = "导出歌单" };
            menuExport.Click += (p1, p2) => FileHelper.ExportMusicList();


            MenuItem menuRefreshList = new MenuItem() { Header = "刷新列表" };
            menuRefreshList.Click += MenuRefreshListClickEventHandler;

            MenuItem menuShowLrc = new MenuItem() { Header = "显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                Setting.ShowLrc = true;
                grdLrcArea.Visibility = Visibility.Visible;
                grdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                grdMain.ColumnDefinitions[0].Width = GridLength.Auto;

                NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);

            };
            menu.Items.Add(menuAdd);
            if (MusicCount > 0)
            {
                menu.Items.Add(menuDelete);
            }
            menu.Items.Add(new SeparatorLine());
            if (lvwMusic.SelectedMusic != null)
            {
                menu.Items.Add(menuOpenMusicFolder);
                menu.Items.Add(menuShowMusicInfo);
                menu.Items.Add(menuPlayNext);
                menu.Items.Add(new SeparatorLine());
            }

            if (MusicCount > 0)
            {
                menu.Items.Add(menuRandom);
                menu.Items.Add(menuRefreshList);
            }

            menu.Items.Add(menuImport);
            menu.Items.Add(menuExport);
            if (!Setting.ShowLrc)
            {
                menu.Items.Add(menuShowLrc);
            }
        }
        /// <summary>
        /// 单击刷新列表按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuRefreshListClickEventHandler(object sender, RoutedEventArgs e)
        {
            string[] paths = MusicDatas.Select(p => p.Path).ToArray();


            List<string> exists = new List<string>(paths.Length);
            List<string> notExists = new List<string>(paths.Length);
            foreach (var i in paths)
            {
                if (File.Exists(i))
                {
                    exists.Add(i);
                }
                else
                {
                    notExists.Add(i);
                }
            }

            if (notExists.Count == 0)
            {
                if (ShowMessage("检测完成，没有发现不存在的文件，是否刷新？", DialogType.Information, new string[] { "是", "否" }) == 1)
                {
                    ClearMusics();
                    AfterClearList();

                    AddMusic(paths);
                }
            }
            else
            {
                if (ShowMessage($"检测完成，发现{notExists.Count}个不存在的文件，是否删除并刷新？", DialogType.Information, new string[] { "是", "否" }) == 1)
                {
                    ClearMusics();
                    AfterClearList();
                    AddMusic(exists.ToArray());
                }
            }
        }
        /// <summary>
        /// 单击歌词选项按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLrcOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,
                Placement = PlacementMode.Top,
                IsOpen = true
            };


            MenuItem menuShowLrc = new MenuItem() { Header = "不显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                Setting.ShowLrc = false;
                grdLrcArea.Visibility = Visibility.Collapsed;
                //set.AutoFurl = false;
                NewDoubleAnimation(this, WidthProperty, grdMain.ColumnDefinitions[0].ActualWidth + 32, 0.5, 0.3, (p3, p4) =>
                {
                    grdMain.ColumnDefinitions[2].Width = new GridLength(0);

                    grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                });
            };


            MenuItem menuCopyLrc = new MenuItem() { Header = "复制歌词" };
            menuCopyLrc.Click += (p1, p2) => lrc.CopyLyrics();
            MenuItem menuSave = new MenuItem() { Header = "保存歌词" };
            menuSave.Click += (p1, p2) => lrc.SaveLrc(false);
            MenuItem menuSaveAs = new MenuItem() { Header = "另存为歌词" };
            menuSaveAs.Click += (p1, p2) => lrc.SaveLrc(true);

            MenuItem menuSearchInNetEase = new MenuItem() { Header = "在网易云中搜索" };
            menuSearchInNetEase.Click += (p1, p2) => Process.Start($"https://music.163.com/#/search/m/?s={Music.Info.Name}");

            MenuItem menuReload = new MenuItem() { Header = "重载歌词" };
            menuReload.Click += (p1, p2) => InitialiazeLrc();

            MenuItem menuEdit = new MenuItem() { Header = (lbxLrc.Visibility == Visibility.Visible || txtLrc.Visibility == Visibility.Visible) ? "编辑歌词" : "新建歌词" };
            menuEdit.Click += (p1, p2) =>
            {
                FileInfo file = new FileInfo(Music.FilePath);
                if (lbxLrc.Visibility == Visibility.Visible)
                {
                    new WinLrcEdit(file.FullName.TrimEnd(file.Extension.ToCharArray()) + ".lrc") { Owner = this }.Show();
                }
                else if (txtLrc.Visibility == Visibility.Visible)
                {
                    new WinLrcEdit(file.FullName.TrimEnd(file.Extension.ToCharArray()) + ".txt") { Owner = this }.Show();
                }
                else
                {
                    int index = ShowMessage("Lrc歌词文件还是Txt文本文件？", DialogType.Information, new string[] { "Lrc文件", "Txt文件", "取消" });
                    if (index == 0)
                    {
                        new WinLrcEdit(file.FullName.TrimEnd(file.Extension.ToCharArray()) + ".lrc") { Owner = this }.Show();
                    }
                    else if (index == 1)
                    {
                        new WinLrcEdit(file.FullName.TrimEnd(file.Extension.ToCharArray()) + ".txt") { Owner = this }.Show();
                    }
                }
            };
            MenuItem menuOpenSetting = new MenuItem() { Header = "主界面歌词设置" };
            menuOpenSetting.Click += (p1, p2) => new WinSettings(1) { Owner = this }.ShowDialog();


            MenuItem menuFloat = new MenuItem() { Header = "悬浮歌词" };




            // StackPanel menuFloatNormalFontSizeSetting = new StackPanel()
            // {
            //     Orientation = Orientation.Horizontal,
            //     Children =
            //{
            //    new TextBlock(){Text="正常歌词字体大小："},
            //    new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsNormalFontSize.ToString()},
            //}
            // };
            // StackPanel menuFloatHighlightFontSizeSetting = new StackPanel()
            // {
            //     Orientation = Orientation.Horizontal,
            //     Children =
            //{
            //    new TextBlock(){Text="当前歌词字体大小："},
            //    new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsHighlightFontSize.ToString()},
            //}
            // };
            MenuItem menuFloatSwitch = new MenuItem() { Header = (Setting.ShowFloatLyric ? "关闭" : "打开") };
            menuFloatSwitch.Click += (p1, p2) => OpenOrCloseFloatLrc();
            MenuItem menuFloatAdjust = new MenuItem() { Header = "调整位置和大小" };
            menuFloatAdjust.Click += (p1, p2) => floatLyric.Adjuest = true;
            //MenuItem menuFloatOK = new MenuItem()
            //{
            //    Header = "确定",
            //};

            MenuItem menuFloatOpenSetting = new MenuItem() { Header = "悬浮歌词设置" };
            menuFloatOpenSetting.Click += (p1, p2) => new WinSettings(2) { Owner = this }.ShowDialog();

            //menuFloatOK.Click += (p1, p2) =>
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        string text = ((menuFloat.Items[menuFloat.Items.Count - 3 + i] as StackPanel).Children[1] as TextBox).Text;
            //        if (double.TryParse(text, out double newValue))
            //        {
            //            switch (i)
            //            {
            //                case 0:
            //                    set.FloatLyricsNormalFontSize = newValue;
            //                    break;
            //                case 1:
            //                    set.FloatLyricsHighlightFontSize = newValue;
            //                    break;
            //            }
            //        }
            //    }
            //};
            menuFloat.Items.Add(menuFloatSwitch);
            menuFloat.Items.Add(menuFloatAdjust);
            menuFloat.Items.Add(menuFloatOpenSetting);
            //menuFloat.Items.Add(new SeparatorLine());
            //menuFloat.Items.Add(menuFloatNormalFontSizeSetting);
            //menuFloat.Items.Add(menuFloatHighlightFontSizeSetting);
            //menuFloat.Items.Add(menuFloatOK);










            menu.Items.Add(menuShowLrc);
            menu.Items.Add(menuReload);
            menu.Items.Add(menuFloat);
            menu.Items.Add(new SeparatorLine());
            menu.Items.Add(menuEdit);
            //if (lrcContent.Count != 0 && (lbxLrc.Visibility == Visibility.Visible || stkLrc.Visibility == Visibility.Visible))
            if (lrc == null && lbxLrc.Visibility == Visibility.Visible)
            {

                menu.Items.Add(menuCopyLrc);
                menu.Items.Add(menuSave);
                menu.Items.Add(menuSaveAs);

            }
            menu.Items.Add(new SeparatorLine());
            menu.Items.Add(menuSearchInNetEase);
            menu.Items.Add(menuOpenSetting);

        }
        /// <summary>
        /// 打开关闭悬浮歌词
        /// </summary>
        private void OpenOrCloseFloatLrc()
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
        /// 用于暂时保存计时器
        /// </summary>
        DispatcherTimer infoWaitTimer;
        /// <summary>
        /// 显示不重要的信息
        /// </summary>
        /// <param name="info"></param>
        private async void ShowInfo(string info)
        {
            tbkOffset.Text = info;
            tbkOffset.Opacity = 1;
            infoWaitTimer?.Stop();
            await Task.Delay(1000);
            NewDoubleAnimation(tbkOffset, OpacityProperty, 0, 0.5, 0, (p3, p4) => tbkOffset.Opacity = 0, true);
        }
        ///// <summary>
        ///// 显示悬浮菜单的菜单
        ///// </summary>
        //private void ShowFloatLyricsMenu()
        //{



        //    StackPanel menuNormalFontSizeSetting = new StackPanel()
        //    {
        //        Orientation = Orientation.Horizontal,
        //        Children =
        //   {
        //       new TextBlock(){Text="正常歌词字体大小："},
        //       new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsNormalFontSize.ToString()},
        //   }
        //    };
        //    StackPanel menuHighlightFontSizeSetting = new StackPanel()
        //    {
        //        Orientation = Orientation.Horizontal,
        //        Children =
        //   {
        //       new TextBlock(){Text="当前歌词字体大小："},
        //       new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsHighlightFontSize.ToString()},
        //   }
        //    };
        //    MenuItem menuAdjust = new MenuItem() { Header = "调整位置和大小" };
        //    menuAdjust.Click += (p1, p2) => floatLyric.Adjuest = true;
        //    MenuItem menuOK = new MenuItem()
        //    {
        //        Header = "确定",
        //    };
        //    menuOK.Click += (p1, p2) =>
        //    {
        //        for (int i = 0; i < 2; i++)
        //        {
        //            string text = ((menu.Items[menu.Items.Count - 3 + i] as StackPanel).Children[1] as TextBox).Text;
        //            if (double.TryParse(text, out double newValue))
        //            {
        //                switch (i)
        //                {
        //                    case 0:
        //                        set.FloatLyricsNormalFontSize = newValue;
        //                        break;
        //                    case 1:
        //                        set.FloatLyricsHighlightFontSize = newValue;
        //                        break;
        //                }
        //            }
        //        }
        //    };
        //    menu.Items.Add(menuAdjust);
        //    menu.Items.Add(new SeparatorLine());
        //    menu.Items.Add(menuNormalFontSizeSetting);
        //    menu.Items.Add(menuHighlightFontSizeSetting);
        //    menu.Items.Add(menuOK);





        //}
        /// <summary>
        /// 执行清空列表后的清理工作
        /// </summary>
        public void AfterClearList()
        {
            Music = null;
            // stkLrc.Visibility = Visibility.Hidden;
            txtLrc.Visibility = Visibility.Hidden;
            lbxLrc.Visibility = Visibility.Hidden;
            if (Setting.ShowFloatLyric)
            {
                floatLyric.Clear();
            }
            Title = "EasyMusic";
            txtMusicName.Text = "";
            imgAlbum.Source = null;
            imgAlbum.Visibility = Visibility.Collapsed;
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

        #region 歌曲搜索
        /// <summary>
        /// 搜索框内容改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxTextBoxTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            string value = txt.Text;
            if (value == "")
            {
                cbbSearch.IsDropDownOpen = false;
                return;
            }
            cbbSearch.Items.Clear();
            int index = -1;
            foreach (var i in MusicDatas)
            {
                index++;

                if (IsInfoMatch(value, i))
                {
                    ComboBoxItem cbbItem = new ComboBoxItem() { Content = i.Name, Tag = i };
                    cbbItem.PreviewMouseLeftButtonUp += (p1, p2) =>
                    {
                        PlayNew((MusicInfo)cbbItem.Tag, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    };

                    cbbSearch.Items.Add(cbbItem);
                    cbbSearch.DropDownOpened += (p1, p2) =>
                    {

                        txt.SelectionLength = 0;
                        txt.SelectionStart = (txt.Text.Length);
                    };
                    cbbSearch.IsDropDownOpen = true;

                }
            }

        }
        /// <summary>
        /// 搜索框按下按键事件（回车键）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxTextBoxKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            string value = (sender as TextBox).Text;
            if (value == "")
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                MusicInfo info = MusicDatas.FirstOrDefault(p => IsInfoMatch(value, p));
                if (info != null)
                {
                    PlayNew(info, false);
                    cbbSearch.IsDropDownOpen = false;
                    cbbSearch.Text = "";
                }

            }
        }
        /// <summary>
        /// 信息匹配判断
        /// </summary>
        /// <param name="str"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsInfoMatch(string str, MusicInfo info)
        {
            str = str.ToLower();
            string musicName = (info.Name + new FileInfo(info.Path).Name).ToLower();
            if (musicName.Contains(str))
            {
                return true;
            }
            if (info.Singer.ToLower().Contains(str))
            {
                return true;
            }
            if (info.Album.ToLower().Contains(str))
            {
                return true;
            }

            if (ConvertChToPinYin(musicName).ToLower().Contains(str))
            {
                return true;
            }
            if (ConvertChToPinYin(info.Singer).ToLower().Contains(str))
            {
                return true;
            }

            var namePinYinTitle = new string(ConvertChToPinYin(musicName).Select(x => (x >= 'A' && x <= 'Z') ? x : ' ').Select(x => x).ToArray()).Replace(" ", "");
            if (namePinYinTitle.ToLower().Contains(str))
            {
                return true;
            }

            var singerPinYinTitle = new string(ConvertChToPinYin(info.Singer).Select(x => (x >= 'A' && x <= 'Z') ? x : ' ').Select(x => x).ToArray()).Replace(" ", "");
            if (singerPinYinTitle.ToLower().Contains(str))
            {
                return true;
            }

            // Debug.WriteLine(PYTitle);
            return false;
        }
        /// <summary>
        /// 拼音区编码数组
        /// </summary>
        private int[] wordCode = new int[] { -20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032, -20026, -20002, -19990, -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728, -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, -19003, -18996, -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211, -18201, -18184, -18183, -18181, -18012, -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730, -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, -16465, -16459, -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915, -15903, -15889, -15878, -15707, -15701, -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416, -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, -14929, -14928, -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353, -14345, -14170, -14159, -14151, -14149, -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090, -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, -13340, -13329, -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585, -12556, -12359, -12346, -12320, -12300, -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604, -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533, -10519, -10331, -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256, -10254 };
        /// <summary>
        /// 拼音数组
        /// </summary>
        private string[] pinYin = new string[] { "A", "Ai", "An", "Ang", "Ao", "Ba", "Bai", "Ban", "Bang", "Bao", "Bei", "Ben", "Beng", "Bi", "Bian", "Biao", "Bie", "Bin", "Bing", "Bo", "Bu", "Ba", "Cai", "Can", "Cang", "Cao", "Ce", "Ceng", "Cha", "Chai", "Chan", "Chang", "Chao", "Che", "Chen", "Cheng", "Chi", "Chong", "Chou", "Chu", "Chuai", "Chuan", "Chuang", "Chui", "Chun", "Chuo", "Ci", "Cong", "Cou", "Cu", "Cuan", "Cui", "Cun", "Cuo", "Da", "Dai", "Dan", "Dang", "Dao", "De", "Deng", "Di", "Dian", "Diao", "Die", "Ding", "Diu", "Dong", "Dou", "Du", "Duan", "Dui", "Dun", "Duo", "E", "En", "Er", "Fa", "Fan", "Fang", "Fei", "Fen", "Feng", "Fo", "Fou", "Fu", "Ga", "Gai", "Gan", "Gang", "Gao", "Ge", "Gei", "Gen", "Geng", "Gong", "Gou", "Gu", "Gua", "Guai", "Guan", "Guang", "Gui", "Gun", "Guo", "Ha", "Hai", "Han", "Hang", "Hao", "He", "Hei", "Hen", "Heng", "Hong", "Hou", "Hu", "Hua", "Huai", "Huan", "Huang", "Hui", "Hun", "Huo", "Ji", "Jia", "Jian", "Jiang", "Jiao", "Jie", "Jin", "Jing", "Jiong", "Jiu", "Ju", "Juan", "Jue", "Jun", "Ka", "Kai", "Kan", "Kang", "Kao", "Ke", "Ken", "Keng", "Kong", "Kou", "Ku", "Kua", "Kuai", "Kuan", "Kuang", "Kui", "Kun", "Kuo", "La", "Lai", "Lan", "Lang", "Lao", "Le", "Lei", "Leng", "Li", "Lia", "Lian", "Liang", "Liao", "Lie", "Lin", "Ling", "Liu", "Long", "Lou", "Lu", "Lv", "Luan", "Lue", "Lun", "Luo", "Ma", "Mai", "Man", "Mang", "Mao", "Me", "Mei", "Men", "Meng", "Mi", "Mian", "Miao", "Mie", "Min", "Ming", "Miu", "Mo", "Mou", "Mu", "Na", "Nai", "Nan", "Nang", "Nao", "Ne", "Nei", "Nen", "Neng", "Ni", "Nian", "Niang", "Niao", "Nie", "Nin", "Ning", "Niu", "Nong", "Nu", "Nv", "Nuan", "Nue", "Nuo", "O", "Ou", "Pa", "Pai", "Pan", "Pang", "Pao", "Pei", "Pen", "Peng", "Pi", "Pian", "Piao", "Pie", "Pin", "Ping", "Po", "Pu", "Qi", "Qia", "Qian", "Qiang", "Qiao", "Qie", "Qin", "Qing", "Qiong", "Qiu", "Qu", "Quan", "Que", "Qun", "Ran", "Rang", "Rao", "Re", "Ren", "Reng", "Ri", "Rong", "Rou", "Ru", "Ruan", "Rui", "Run", "Ruo", "Sa", "Sai", "San", "Sang", "Sao", "Se", "Sen", "Seng", "Sha", "Shai", "Shan", "Shang", "Shao", "She", "Shen", "Sheng", "Shi", "Shou", "Shu", "Shua", "Shuai", "Shuan", "Shuang", "Shui", "Shun", "Shuo", "Si", "Song", "Sou", "Su", "Suan", "Sui", "Sun", "Suo", "Ta", "Tai", "Tan", "Tang", "Tao", "Te", "Teng", "Ti", "Tian", "Tiao", "Tie", "Ting", "Tong", "Tou", "Tu", "Tuan", "Tui", "Tun", "Tuo", "Wa", "Wai", "Wan", "Wang", "Wei", "Wen", "Weng", "Wo", "Wu", "Xi", "Xia", "Xian", "Xiang", "Xiao", "Xie", "Xin", "Xing", "Xiong", "Xiu", "Xu", "Xuan", "Xue", "Xun", "Ya", "Yan", "Yang", "Yao", "Ye", "Yi", "Yin", "Ying", "Yo", "Yong", "You", "Yu", "Yuan", "Yue", "Yun", "Za", "Zai", "Zan", "Zang", "Zao", "Ze", "Zei", "Zen", "Zeng", "Zha", "Zhai", "Zhan", "Zhang", "Zhao", "Zhe", "Zhen", "Zheng", "Zhi", "Zhong", "Zhou", "Zhu", "Zhua", "Zhuai", "Zhuan", "Zhuang", "Zhui", "Zhun", "Zhuo", "Zi", "Zong", "Zou", "Zu", "Zuan", "Zui", "Zun", "Zuo" };
        /// <summary>
        /// 汉字转换成全拼的拼音
        /// </summary>
        /// <param name="Chstr">汉字字符串</param>
        /// <returns>转换后的拼音字符串</returns>
        public string ConvertChToPinYin(string Chstr)
        {
            Regex reg = new Regex("^[\u4e00-\u9fa5]$");//验证是否输入汉字
            byte[] arr = new byte[2];
            string pystr = "";
            int asc = 0, M1 = 0, M2 = 0;
            char[] mChar = Chstr.ToCharArray();//获取汉字对应的字符数组
            for (int j = 0; j < mChar.Length; j++)
            {
                //如果输入的是汉字
                if (reg.IsMatch(mChar[j].ToString()))
                {
                    arr = Encoding.Default.GetBytes(mChar[j].ToString());
                    M1 = arr[0];
                    M2 = arr[1];
                    asc = M1 * 256 + M2 - 65536;
                    if (asc > 0 && asc < 160)
                    {
                        pystr += mChar[j];
                    }
                    else
                    {
                        switch (asc)
                        {
                            case -9254:
                                pystr += "Zhen";
                                break;
                            case -8985:
                                pystr += "Qian";
                                break;
                            case -5463:
                                pystr += "Jia";
                                break;
                            case -8274:
                                pystr += "Ge";
                                break;
                            case -5448:
                                pystr += "Ga";
                                break;
                            case -5447:
                                pystr += "La";
                                break;
                            case -4649:
                                pystr += "Chen";
                                break;
                            case -5436:
                                pystr += "Mao";
                                break;
                            case -5213:
                                pystr += "Mao";
                                break;
                            case -3597:
                                pystr += "Die";
                                break;
                            case -5659:
                                pystr += "Tian";
                                break;
                            default:
                                for (int i = (wordCode.Length - 1); i >= 0; i--)
                                {
                                    if (wordCode[i] <= asc)//判断汉字的拼音区编码是否在指定范围内
                                    {
                                        pystr += pinYin[i];//如果不超出范围则获取对应的拼音
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }
                else//如果不是汉字
                {
                    pystr += mChar[j].ToString();//如果不是汉字则返回
                }
            }
            return pystr;//返回获取到的汉字拼音
        }
        #endregion

        #region 任务栏按钮
        /// <summary>
        /// 单击任务栏上的播放按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiPlayClickEventHandler(object sender, EventArgs e)
        {
            Music.Play();
            tbiPlay.Visibility = Visibility.Collapsed;
            tbiPause.Visibility = Visibility.Visible;

        }
        /// <summary>
        /// 单击任务栏上的暂停按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiPauseClickEventHandler(object sender, EventArgs e)
        {
            Music.Pause();
            tbiPause.Visibility = Visibility.Collapsed;
            tbiPlay.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 单击任务栏上的上一曲按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiLastClickEventHandler(object sender, EventArgs e)
        {
            PlayLast();
        }
        /// <summary>
        /// 单击任务栏上的下一曲按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiNextClickEventHandler(object sender, EventArgs e)
        {
            PlayNext();
        }

        #endregion

        /// <summary>
        /// 到某一条歌词一共有多少行
        /// </summary>
        List<int> lrcLineSumToIndex = new List<int>();
        /// <summary>
        /// 歌词对象
        /// </summary>
        LyricInfo lrc;
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
                lbxLrc.RefreshFontSize(lrc.CurrentIndex);
                lbxLrc.ScrollTo(lrc.CurrentIndex, lrcLineSumToIndex, Setting.NormalLrcFontSize);
                if (Setting.ShowFloatLyric)
                {
                    floatLyric.Update(lrc.CurrentIndex);
                }
            }



        }
    }

}
