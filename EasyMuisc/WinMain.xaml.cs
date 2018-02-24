using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Shell32;
using Un4seen.Bass;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Windows.Media;
using System.Security.Permissions;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shell;
using CustomWPFColorPicker;
using EasyMuisc.Tools;
using EasyMuisc.Windows;
using EasyMuisc.UserControls;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.ShareStaticResources;
using static EasyMuisc.MusicHelper;

namespace EasyMuisc
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// 循环模式
        /// </summary>
        private enum CycleMode { SingleCycle, ListCycle, Shuffle }


        #region 字段
        /// <summary>
        /// 颜色选择器
        /// </summary>
        ColorPickerControlView colorPicker = new ColorPickerControlView();
        /// <summary>
        /// 
        /// </summary>
        FloatLyrics floatLyric;
        /// <summary>
        /// 音乐文件路径
        /// </summary>
        public string path;




        #endregion


        #region 属性
        /// <summary>
        /// 获取当前的播放循环模式
        /// </summary>
        private CycleMode CurrentCycleMode
        {
            get
            {
                if (btnListCycle.Visibility == Visibility.Visible)
                {
                    return CycleMode.ListCycle;
                }
                if (btnShuffle.Visibility == Visibility.Visible)
                {
                    return CycleMode.Shuffle;
                }
                return CycleMode.SingleCycle;
            }
        }
        /// <summary>
        /// 内部音量
        /// </summary>
        private double Volumn
        {
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                if (value < 0)
                {
                    value = 0;
                }
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Math.Pow(value, 2));

            }
            get
            {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref value);
                return Math.Sqrt(value);
            }
        }
        /// <summary>
        /// 是否显示加载动画
        /// </summary>
        public bool LoadingSpinner
        {
            set
            {
                if (value)
                {
                    grdLoading.Visibility = Visibility.Visible;
                    NewDoubleAnimation(grdLoading, OpacityProperty, 0.5, 0.5, 0);

                }
                else
                {
                    NewDoubleAnimation(grdLoading, OpacityProperty, 0, 0.5, 0, (p1, p2) => grdLoading.Visibility = Visibility.Hidden);
                }
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
            get => set.AnimationFps;
            set
            {
                if (value == -1)
                {
                    Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = AnimationFps });
                }
                else
                {
                    set.AnimationFps = value;
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
        /// 继续播放渐响定时器
        /// </summary>
        DispatcherTimer playTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000 / 60) };
        /// <summary>
        /// 暂停播放渐隐定时器
        /// </summary>
        DispatcherTimer pauseTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000 / 60) };
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



        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            //if (!File.Exists("bass.dll"))
            //{
            //    File.WriteAllBytes("bass.dll", Properties.Resources.bass);
            //}
            InitializeComponent();

            windowHandle = new WindowInteropHelper(this).Handle;

            if (set.MaxWindow)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;

                Top = set.Top;
                Left = set.Left;
                Width = set.Width;
                Height = set.Height;

            }

            Un4seen.Bass.AddOn.Fx.BassFx.LoadMe();

            if (!Bass.BASS_Init(-1, set.SampleRate/*无设置界面*/, BASSInit.BASS_DEVICE_DEFAULT, new WindowInteropHelper(this).Handle))
            {
                ShowAlert("无法初始化音乐引擎，可能是采样率不支持。");
                error = true;
                //Application.Current.Shutdown();
            }
            WindowHelper.RepairWindowBehavior(this);

            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });

            colorPicker.CurrentColor = new BrushConverter().ConvertFrom(set.BackgroundColor) as SolidColorBrush;
            UpdateColor();

            InitialiazeField();
            txtMusicName.MaxWidth = SystemParameters.WorkArea.Width - 200;

            InitializeAnimation();

            floatLyric = new FloatLyrics()
            {
                Top = set.FloatLyricsTop,
                Left = set.FloatLyricsLeft,
                Height = set.FloatLyricsHeight,
                Width = set.FloatLyricsWidth,
            };

            Tray();

        }
        /// <summary>
        /// 初始化定时器事件
        /// </summary>
        private void InitialiazeField()
        {
            mainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / set.UpdateSpeed) };
            playTimer.Tick += (p1, p2) =>
            {

                if (Volumn >= sldVolumn.Value)
                {
                    playTimer.Stop();
                    return;
                }
                Volumn += 0.05;
            };
            pauseTimer.Tick += (p1, p2) =>
            {

                if (Volumn <= 0.05)
                {
                    Bass.BASS_ChannelPause(stream);
                    pauseTimer.Stop();
                    if (closing)
                    {
                        Application.Current.Shutdown();
                    }
                    return;
                }
                Volumn -= 0.05;
            };

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
        private void WindowLoadedEventHandler(object sender, RoutedEventArgs e)
        {

            RegistGolbalHotKey();
     



            if (path == null)
            {
                string tempPath = set.LastMusic;
                if (File.Exists(tempPath) && musicDatas.Where(p=>p.Path.Equals(tempPath)).Count()!=0)
                {
                    PlayNew(AddMusic(tempPath), false);

                }
            }
            else
            {
                PlayNew(AddMusic(path), true);

            }



            if (!set.ShowLrc)
            {
                set.ShowLrc = false;
                grdLrcArea.Visibility = Visibility.Collapsed;
                set.AutoFurl = false;
                grdMain.ColumnDefinitions[2].Width = new GridLength(0);
                grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                Width = grdMain.ColumnDefinitions[0].ActualWidth + 32;

            }


            switch (set.CycleMode)
            {
                case 1:
                    btnListCycle.Visibility = Visibility.Visible;
                    break;
                case 2:
                    btnShuffle.Visibility = Visibility.Visible;
                    break;
                case 3:
                    btnSingleCycle.Visibility = Visibility.Visible;
                    break;
            }
            //set. = double.Parse(GetConfig("NormalLrcFontSize", normalLrcFontSize.ToString()));
            //highlightLrcFontSize = double.Parse(GetConfig("HighlightLrcFontSize", highlightLrcFontSize.ToString()));
            //textLrcFontSize = double.Parse(GetConfig("TextLrcFontSize", textLrcFontSize.ToString()));
            //sldVolumn.Value = double.Parse(GetConfig("Volumn", "1"));
            Volumn = set.Volumn;
            sldVolumn.Value = set.Volumn;
            Topmost = set.Topmost;
            if (set.ShrinkMusicListManually)
            {
                SleepThenDo(1000, (p1, p2) => BtnListSwitcherClickEventHandler(null, null));
            }
            if (set.ShowFloatLyric)
            {
                floatLyric.Show();
            }
        }

     

        /// <summary>
        /// 窗体关闭事件，保存配置项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (error)
            {
                Bass.BASS_Stop();
                return;
            }
            //SaveMusicListFromConfig();
            MusicHelper.SaveListToFile();
            switch (CurrentCycleMode)
            {
                case CycleMode.ListCycle:
                    set.CycleMode = 1;
                    break;
                case CycleMode.Shuffle:
                    set.CycleMode = 2;
                    break;
                case CycleMode.SingleCycle:
                    set.CycleMode = 3;
                    break;
            }
            //SetConfig("NormalLrcFontSize", normalLrcFontSize.ToString());
            //SetConfig("HighlightLrcFontSize", highlightLrcFontSize.ToString());
            //SetConfig("TextLrcFontSize", textLrcFontSize.ToString());
            //SetConfig("LastMusic", path);
            set.LastMusic = path;
            //SetConfig("Volumn", sldVolumn.Value.ToString());
            set.Volumn = sldVolumn.Value;
            //SetConfig("AlwaysOnTop", Topmost.ToString());
            // set.Topmost = Topmost;
            // SetConfig("BackgroundColor", colorPicker.CurrentColor.ToString());
            set.BackgroundColor = colorPicker.CurrentColor.ToString();
            //SetConfig("Top", Top.ToString());
            // SetConfig("Left", Left.ToString());
            // SetConfig("Width", ActualWidth.ToString());
            //SetConfig("Height", ActualHeight.ToString());
            //SetConfig("MaxWindow", (WindowState == WindowState.Maximized) ? "True" : "False");
            set.Top = Top;
            set.Left = Left;
            set.Height = Height;
            set.Width = Width;
            set.MaxWindow = (WindowState == WindowState.Maximized);

            if (set.ShowFloatLyric)
            {
                floatLyric.Close();
            }

            //cfa.Save();
            set.Save();
            e.Cancel = true;
            closing = true;
            Hide();
            pauseTimer.Start();
        }
        /// <summary>
        /// 托盘图标
        /// </summary>
        private void Tray()
        {
            trayIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                BalloonTipText = "设置界面在任务栏托盘",
                Text = "EasyMusic",
                Icon = Properties.Resources.icon,
                Visible = true,
            };
            //if (!File.Exists(MusicListName))
            //{
            //    trayIcon.ShowBalloonTip(2000);
            //}
            trayIcon.MouseClick += (p1, p2) =>
            {
                if (p2.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (Visibility == Visibility.Hidden)
                    {
                        Show();
                        Topmost = true;
                        Topmost = set.Topmost;
                        Activate();
                    }
                    else
                    {
                        Visibility = Visibility.Hidden;
                    }
                }
                else if (p2.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    TrayMenu(p1);
                }
            };
        }
        /// <summary>
        /// 托盘图标菜单
        /// </summary>
        /// <param name="sender"></param>
        private void TrayMenu(object sender)
        {
            MenuItem menuFloat = new MenuItem() { Header = (set.ShowFloatLyric ? "关闭" : "打开") + "悬浮歌词" };
            menuFloat.PreviewMouseLeftButtonUp += (p1, p2) =>
            {
                set.ShowFloatLyric = !set.ShowFloatLyric;
                floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                floatLyric.Update(currentLrcIndex);
            };

            menuFloat.PreviewMouseRightButtonDown += (p1, p2) => ShowFloatLyricsMenu();
            MenuItem menuExit = new MenuItem() { Header = "退出" };
            menuExit.Click += (p1, p2) => Close();
            ContextMenu menu = new ContextMenu()
            {
                IsOpen = true,
                PlacementTarget = this,
                Items =
                {
                   menuFloat,
                   menuExit
                }
            };
        }





        #endregion
        
    }

}
