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
        private enum CycleMode {SingleCycle,ListCycle, Shuffle}


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
        /// <summary>
        /// 音乐播放句柄
        /// </summary>
        int stream = 0;
        /// <summary>
        /// 托盘图标
        /// </summary>
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        /// <summary>
        /// 支持的格式
        /// </summary>
        private string[] supportExtension = { ".mp3", ".MP3", ".wav", ".WAV" };
        /// <summary>
        /// 支持的格式，过滤器格式
        /// </summary>
        private string supportExtensionWithSplit;


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
        private bool LoadingSpinner
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
        Properties.Settings set = new Properties.Settings();
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
        /// 音乐信息，与列表绑定
        /// </summary>
        public ObservableCollection<MusicInfo> musicInfo;
        /// <summary>
        /// 是否产生了不可挽救错误
        /// </summary>
        bool error = false;
        /// <summary>
        /// 歌单二进制文件名
        /// </summary>
        string MusicListName = "EasyMusicList.bin";
        /// <summary>
        /// 程序目录
        /// </summary>
        string programDirectory = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;

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


            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, new WindowInteropHelper(this).Handle))
            {
                ShowAlert("无法初始化音乐引擎。");
                error = true;
                Application.Current.Shutdown();
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

            floatLyric = new FloatLyrics(set)
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
            if (File.Exists(programDirectory + "\\" + MusicListName))
            {
                try
                {
                    musicInfo = DeserializeObject(File.ReadAllBytes(programDirectory + "\\" + MusicListName)) as ObservableCollection<MusicInfo>;
                }
                catch
                {
                    ShowAlert("歌单存档已损坏，将重置歌单。");
                    musicInfo = new ObservableCollection<MusicInfo>();
                }
            }
            else
            {
                musicInfo = new ObservableCollection<MusicInfo>();
            }
            lvw.DataContext = musicInfo;



            if (path == null)
            {
                string tempPath = set.LastMusic;
                if (File.Exists(tempPath))
                {
                    PlayNew(AddNewMusic(tempPath), false);

                }
            }
            else
            {
                PlayNew(AddNewMusic(path), true);

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
            if(set.ShowFloatLyric)
            {
                floatLyric.Show();
            }
        }
        /// <summary>
        /// 增加一组歌曲到列表中
        /// </summary>
        /// <param name="musics"></param>
        /// <param name="firstLoad"></param>
        /// <returns></returns>
        private void AddNewMusics(string[] musics)
        {
            taskBar.ProgressValue = 0;
            taskBar.ProgressState = TaskbarItemProgressState.Normal;
            LoadingSpinner = true;

            //if (cfa.AppSettings.Settings["MusicList"] != null)
            //{
               int n = 1;
                foreach (var i in musics)
                {
                    AddNewMusic(i);
                    taskBar.ProgressValue = 1.0 * (n++) / musics.Length;
                }
            //}
            taskBar.ProgressState = TaskbarItemProgressState.None;
            LoadingSpinner = false;

        }
        /// <summary>
        /// 增加新的歌曲到列表中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private MusicInfo AddNewMusic(string path)
        {

            try
            {
                foreach (var i in musicInfo)
                {
                    if (path == i.Path)
                    {
                        return i;
                    }
                }
                bool info = GetMusicInfo(path, out string name, out string singer, out string length, out string album);
                musicInfo.Add(new MusicInfo
                {
                    Enable = info,
                    MusicName = name,
                    Singer = singer,
                    Length = length.StartsWith("00:") ? length.Remove(0, 3) : length,
                    Path = path,
                    Album = album,
                });
                DoEvents();
                return musicInfo[musicInfo.Count - 1];
            }
            catch
            {
                return null;
            }

        }
        /// <summary>
        /// 获取音乐的信息
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="name">音乐的标题，若没有则返回无扩展名的文件名</param>
        /// <param name="singer">歌手</param>
        /// <param name="length">时长</param>
        private bool GetMusicInfo(string path, out string name, out string singer, out string length, out string album)
        {
            if (!File.Exists(path))
            {
                name = path;
                singer = "";
                length = "";
                album = "";
                return false;
            }
            ShellClass sh = new ShellClass();
            Folder dir = sh.NameSpace(Path.GetDirectoryName(path));
            FolderItem item = dir.ParseName(Path.GetFileName(path));
            name = dir.GetDetailsOf(item, 21);
            if (name == "")
            {
                name = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
            }
            singer = dir.GetDetailsOf(item, 13);
            length = dir.GetDetailsOf(item, 27);
            album = dir.GetDetailsOf(item, 14);
            return true;
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
            File.WriteAllBytes(programDirectory + "\\" + MusicListName, SerializeObject(musicInfo));
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
            set.Topmost = Topmost;
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

            if(set.ShowFloatLyric)
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
        private void Tray()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                BalloonTipText = "设置界面在任务栏托盘",
                Text = "EasyMusic",
                Icon = Properties.Resources.icon,
                Visible = true,
            };
            if (!File.Exists(MusicListName))
            {
                notifyIcon.ShowBalloonTip(2000);
            }
            notifyIcon.MouseClick += (p1, p2) =>
            {
                if (p2.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (Visibility == Visibility.Hidden)
                    {
                        Show();
                        Topmost = true;
                        Topmost = false;
                        Activate();
                    }
                    else
                    {
                        Visibility = Visibility.Hidden;
                    }
                }
                else if(p2.Button==System.Windows.Forms.MouseButtons.Right)
                {
                    TrayMenu(p1);
                }
            };
        }

        private void TrayMenu(object sender)
        {
            MenuItem menuFloat = new MenuItem() { Header = (set.ShowFloatLyric ? "√" : "×") + "悬浮歌词" };
            menuFloat.PreviewMouseLeftButtonUp += (p1, p2) =>
            {
                set.ShowFloatLyric = !set.ShowFloatLyric;
                floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                floatLyric.Update(currentLrcIndex);
            };
          
            menuFloat.PreviewMouseRightButtonDown += (p1, p2) =>ShowFloatLyricsMenu();
            MenuItem menuExit = new MenuItem() { Header = "退出" };
            menuExit.Click += (p1, p2) => Close();
            ContextMenu menu = new ContextMenu()
            {
                IsOpen = true,
                PlacementTarget =this,
                Items =
                {
                   menuFloat,
                   menuExit
                }
            };
        }
    
        #endregion


        #region 播放控制
        /// <summary>
        /// 当前音乐在列表中的索引
        /// </summary>
        public int currentMusicIndex = 0;
        /// <summary>
        /// 历史记录
        /// </summary>
        private List<MusicInfo> history = new List<MusicInfo>();
        /// <summary>
        /// 当前播放历史索引
        /// </summary>
        int currentHistoryIndex = -1;
        /// <summary>
        /// 歌曲时长
        /// </summary>
        double musicLength;

        /// <summary>
        /// 初始化新的歌曲
        /// </summary>
        private void InitialiazeMusic()
        {
            Stop();//停止正在播放的歌曲
            try
            {
                stream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);//获取歌曲句柄
                Volumn = sldVolumn.Value;
                txtMusicName.Text = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
                Title = txtMusicName.Text + " - EasyMusic";//将窗体标题改为歌曲名
                string[] length = musicInfo[currentMusicIndex].Length.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                musicLength = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                sldProcess.Maximum = musicLength;
                InitialiazeLrc();
            }
            catch(Exception ex)
            {
                ShowAlert("初始化失败!"+Environment.NewLine+ex.ToString());
                return;
            }
            ReadMusicSourceInfo(musicInfo[currentMusicIndex].Path);

            mainTimer.Tick += UpdateTick;
            mainTimer.Start();

        }
        /// <summary>
        /// 初始化歌词
        /// </summary>
        /// <param name="musicLength"></param>
        private void InitialiazeLrc()
        {
            lrcLineSumToIndex.Clear();
            lrcTime.Clear();//清空歌词时间
            lrcContent.Clear();//清除歌词内容
            currentLrcIndex = -1;//删除歌词索引
            stkLrc.Children.Clear();//清空歌词表
            lbxLrc.Clear();

            FileInfo file = new FileInfo(path);
            file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
            if (file.Exists)//判断是否存在歌词文件
            {
                grdLrc.Visibility = Visibility.Visible;
                txtLrc.Visibility = Visibility.Hidden;
                if(set.UseListBoxLrcInsteadOfStackPanel)
                {
                    lbxLrc.Visibility = Visibility.Visible;
                    stkLrc.Visibility = Visibility.Hidden;

                }
                else
                {
                    stkLrc.Visibility = Visibility.Visible;
                    lbxLrc.Visibility = Visibility.Hidden;
                }
                lrc = new Lyric(file.FullName);//获取歌词信息
                if (!double.TryParse(lrc.Offset, out offset))
                {
                    offset = 0;
                }
                offset /= 1000.0;
                int index = 0;//用于赋值Tag
                    foreach (var i in lrc.LrcContent)
                {
                    //if (i.Key > musicLength)//如果歌词文件有误，长度超过了歌曲的长度，那么超过部分就不管了
                    //{
                    //    break;
                    //}
                    //lbxLrc.Add(i.Value,index.ToString(),(p1,p2) =>
                    //{
                    //    var position = lrcTime[int.Parse(((p1 as FrameworkElement).Tag).ToString())] - offset - LrcDefautOffset;
                    //    Bass.BASS_ChannelSetPosition(stream, position > 0 ? position : 0);
                    //    });

                    lrcContent.Add(i.Value);
                    var tbk = new TextBlock()
                    {
                        Name = "tbk"+index.ToString(),
                        FontSize = set.NormalLrcFontSize,
                        Text = i.Value,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = index++,//标签用于定位
                        Cursor = Cursors.Hand,
                        TextAlignment = TextAlignment.Center,
                        FocusVisualStyle = null,
                    };
                    tbk.MouseLeftButtonUp += (p1, p2) =>
                    {
                        //单击歌词跳转到当前歌词
                        Bass.BASS_ChannelSetPosition(stream, (lrcTime[(int)tbk.Tag] - offset - set.LrcDefautOffset) > 0 ? lrcTime[(int)tbk.Tag] - offset - set.LrcDefautOffset : 0);
                    };
                    if(set.UseListBoxLrcInsteadOfStackPanel)
                    {
                        lbxLrc.Add(tbk);
                    }
                    else
                    {
                       stkLrc.Children.Add(tbk);
                       //stkLrc.
                    }
                    lrcTime.Add(i.Key);
                }
                //lbxLrc.Add(lrc.LrcContent);
                //lbxLrc.ChangeFontSize(normalLrcFontSize);
                foreach (var i in lrc.LineIndex)
                {
                    lrcLineSumToIndex.Add(i.Value);
                }
                floatLyric.ReLoadLrc(lrcContent);
            }
            else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
            {

                txtLrc.Text = File.ReadAllText(file.FullName, EncodingType.GetType(file.FullName));
                for (int i = 0; i < txtLrc.LineCount; i++)
                {
                    lrcContent.Add(txtLrc.GetLineText(i));
                }
                txtLrc.FontSize = set.TextLrcFontSize;
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Visible;
                stkLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;

            }
            else
            {
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                stkLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;
                if(set.ShowFloatLyric)
                {
                    floatLyric.Clear();
                }
            }
        }
        /// <summary>
        /// 根据不同的播放循环模式播放下一首
        /// </summary>
        private void PlayNext()
        {
            if (currentHistoryIndex < history.Count - 1)
            {
                history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex - 1);
            }

            switch (CurrentCycleMode)
            {
                case CycleMode.ListCycle:
                    PlayListNext();
                    break;
                case CycleMode.Shuffle:
                    int index;
                    while ((index =GetRandomNumber(0,musicInfo.Count)) == currentMusicIndex)
                        ;
                    PlayNew(index);
                    break;
                case CycleMode.SingleCycle:
                    PlayNew(currentMusicIndex);
                    break;
            }
        }
        /// <summary>
        /// 播放列表中的下一首歌
        /// </summary>
        private void PlayListNext()
        {
            PlayNew(currentMusicIndex == musicInfo.Count - 1 ? 0 : currentMusicIndex + 1);
        }
        /// <summary>
        /// （暂停后）播放
        /// </summary>
        /// <returns></returns>
        private void Play()
        {
            tbiPlay.Visibility = Visibility.Collapsed;
            tbiPause.Visibility = Visibility.Visible;
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
            if (pauseTimer.IsEnabled)
            {
                pauseTimer.Stop();
            }
            playTimer.Start();
            Bass.BASS_ChannelPlay(stream, false);

        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <returns></returns>
        private bool PlayNew(bool playAtOnce = true)
        {
            currentMusicIndex = lvw.SelectedIndex;
            return PlayNew(currentMusicIndex, playAtOnce);
        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="index">指定列表中的歌曲索引</param>
        /// <returns></returns>
        private bool PlayNew(int index, bool playAtOnce = true)
        {
            if (!File.Exists(musicInfo[index].Path))
            {
                if (ShowAlert("文件不存在！是否从列表中删除？", MessageBoxButton.YesNo))
                {
                    musicInfo.RemoveAt(index);
                }
                return false;
            }
            currentMusicIndex = index;//指定当前的索引
            path = musicInfo[currentMusicIndex].Path;//获取歌曲地址
            lvw.SelectedIndex = index;//选中列表中的歌曲
            lvw.ScrollIntoView(lvw.SelectedItem);
            if (currentHistoryIndex == history.Count - 1)
            {
                if (currentHistoryIndex == -1)
                {
                    history.Add(musicInfo[currentMusicIndex]);//加入历史记录
                    currentHistoryIndex++;
                }
                else
                if (history[currentHistoryIndex] != musicInfo[currentMusicIndex])
                {
                    history.Add(musicInfo[currentMusicIndex]);//加入历史记录
                    currentHistoryIndex++;
                }
            }
            //Debug.WriteLine(currentHistoryIndex);
            InitialiazeMusic();//初始化歌曲
            if (playAtOnce)
            {
                Play();
            }
            if (WindowState == WindowState.Normal)
            {
                Width += 0.01;
                Width -= 0.01;
            }
            return true;

        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="music">指定歌曲信息实例</param>
        /// <returns></returns>
        private bool PlayNew(MusicInfo music, bool playAtOnce = true)
        {
            int index = musicInfo.IndexOf(music);
            if (index < 0 && index >= musicInfo.Count)
            {
                return false;
            }
            PlayNew(index, playAtOnce);
            return true;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        private void Pause()
        {

            tbiPause.Visibility = Visibility.Collapsed;
            tbiPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
            btnPlay.Visibility = Visibility.Visible;
            if (playTimer.IsEnabled)
            {
                playTimer.Stop();
            }
            pauseTimer.Start();
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        private bool Stop()
        {
            return Bass.BASS_StreamFree(stream);
        }
        #endregion

        #region 定时更新
        /// <summary>
        /// 歌词列表
        /// </summary>
        List<double> lrcTime = new List<double>();
        /// <summary>
        /// 歌词内容
        /// </summary>
        List<string> lrcContent = new List<string>();
        /// <summary>
        /// 到某一条歌词一共有多少行
        /// </summary>
        List<int> lrcLineSumToIndex = new List<int>();
        /// <summary>
        /// 歌词对象
        /// </summary>
        Lyric lrc;
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcIndex = 0;
        /// <summary>
        /// 歌词时间偏移量
        /// </summary>
        double offset;
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
            if (set.LrcAnimation)
            {
                Storyboard.SetTargetName(aniLrc, stkLrc.Name);
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
            if (stream == 0)
            {
                mainTimer.Stop();
                return;
            }
            if (!changingPosition)
            {
                double currentPosition = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
                sldProcess.Value = currentPosition;
                if (Bass.BASS_ChannelGetPosition(stream) == Bass.BASS_ChannelGetLength(stream))
                {
                    //如果一首歌放完了
                    PlayNext();
                }
                UpdatePosition();
            }
        }
        /// <summary>
        /// 更新当前时间的歌词
        /// </summary>
        private void UpdatePosition()
        {
            if (lrcTime.Count == 0)
            {
                return;
            }
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));//获取当前播放的位置
            bool changed = false;//是否
            if (position == 0 && currentLrcIndex != 0)//如果还没播放并且没有更新位置
            {
                changed = true;
                currentLrcIndex = 0;
            }
            else
            {
                for (int i = 0; i < lrcTime.Count - 1; i++)//从第一个循环到最后一个歌词时间
                {
                    if (lrcTime[i + 1] > position + offset + set.LrcDefautOffset)//如果下一条歌词的时间比当前时间要后面（因为增序判断所以这一条歌词时间肯定小于的）
                    {
                        if (currentLrcIndex != i)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            currentLrcIndex = i;
                        }
                        break;
                    }
                    else if (i == lrcTime.Count - 2 && lrcTime[i + 1] < position + offset + set.LrcDefautOffset)
                    {
                        if (currentLrcIndex != i + 1)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            currentLrcIndex = i + 1;
                        }
                        break;
                    }
                }
            }

            if (changed)
            {

                if(set.UseListBoxLrcInsteadOfStackPanel)
                {
                  lbxLrc.RefreshFontSize(currentLrcIndex,set);
                    lbxLrc.ScrollTo(currentLrcIndex,lrcLineSumToIndex,set.NormalLrcFontSize);
                }
                else
                {
                    foreach (var i in stkLrc.Children)
                    {
                        //首先把所有的歌词都改为正常大小
                        (i as TextBlock).FontSize = set.NormalLrcFontSize;
                    }
                    (stkLrc.Children[currentLrcIndex] as TextBlock).FontSize = set.HighlightLrcFontSize;//当前歌词改为高亮
                    StackPanelLrcAnimition(currentLrcIndex);//歌词转变动画
                }

                if (set.ShowFloatLyric)
                {
                    floatLyric.Update(currentLrcIndex);
                }
            }

            

        }
        /// <summary>
        /// 歌词转变动画
        /// </summary>
        /// <param name="lrcIndex"></param>
        private void StackPanelLrcAnimition(int lrcIndex)
        {
            double top = 0.5 * ActualHeight - lrcLineSumToIndex[lrcIndex]/*第一行到当前行的总行数*/ * set.NormalLrcFontSize * FontFamily.LineSpacing/*歌词数量乘每行字的高度*/ - set.HighlightLrcFontSize;// 0.5 * ActualHeight - stkLrcHeight * lrcIndex / (stkLrc.Children.Count - 1)-highlightFontSize ;


            //Storyboard storyboard = new Storyboard();
            //TranslateTransform translateTransform = new TranslateTransform(0, top);
            ////ScaleTransform scale = new ScaleTransform(1.0, 1.0, 1, 1);
            //stkLrc.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            //TransformGroup myTransGroup = new TransformGroup();
            //myTransGroup.Children.Add(translateTransform);
            //stkLrc.RenderTransform = myTransGroup;

            //DoubleAnimation growAnimation = new DoubleAnimation();
            //growAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            ////growAnimation.From = 1;
            //growAnimation.To = 1.1;
            //storyboard.Children.Add(growAnimation);

            //DependencyProperty[] propertyChain = new DependencyProperty[]
            //{
            //Button.RenderTransformProperty,
            //TransformGroup.ChildrenProperty,
            //TranslateTransform.YProperty
            //};
            //string thePath = "(0).(1)[0].(2)";
            //PropertyPath myPropertyPath = new PropertyPath(thePath, propertyChain);
            //Storyboard.SetTargetProperty(growAnimation, myPropertyPath);
            //Storyboard.SetTarget(growAnimation, stkLrc);

            //storyboard.Begin();









            ////DoubleAnimationUsingKeyFrames ani = new DoubleAnimationUsingKeyFrames();
            ////LinearDoubleKeyFrame first = new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0));

            ////LinearDoubleKeyFrame second = new LinearDoubleKeyFrame(10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)));
            ////ani.KeyFrames.Add(first);
            ////ani.KeyFrames.Add(second);
            ////stkLrc.RenderTransform .BeginAnimation(RenderTransform., ani);

            //return;


            //Canvas.SetTop(stkLrc, top);
            // NewDoubleAnimation(stkLrc, Canvas.LeftProperty, top, 0.8, 0.5);
            //return;

            //DoubleAnimation ani = new DoubleAnimation()
            //{
            //    Duration = TimeSpan.FromMilliseconds(500),
            //    To = top,
            //};
            ////  Storyboard.SetTargetProperty(aniLrc, new PropertyPath("(ListView.RenderTransform).(TranslateTransform.Y)"));
            //Storyboard.SetTargetProperty(ani, new PropertyPath("(StackPanel.RenderTransform).(TranslateTransform.X)"));
            //Storyboard.SetTarget(ani, stkLrc);
            //Storyboard st = new Storyboard();
            //st.Children.Add(ani);
            //st.Begin(stkLrc);
            //return;

            if (set.LrcAnimation)
            {
                storyLrc.Stop(stkLrc);
                aniLrc.To = new Thickness(0, top, 0, 0);
                storyLrc.Begin(stkLrc);
            }
            else
            {
                stkLrc.Margin = new Thickness(0, top, 0, 0);
            }
        }
        #endregion


        #region 播放控制事件
        /// <summary>
        /// 单击播放按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (stream == 0)
            {
                if (musicInfo.Count != 0)
                {
                    PlayNew(0);
                }
            }
            Play();
        }
        /// <summary>
        /// 单击暂停按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPauseClickEventHandler(object sender, RoutedEventArgs e)
        {

            Pause();
        }
        /// <summary>
        /// 单击上一首按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLastClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (history.Count == 0)
            {
                PlayNew(currentMusicIndex == 0 ? musicInfo.Count - 1 : currentMusicIndex - 1);
            }
            else
            {
                currentHistoryIndex--;
                PlayNew(currentHistoryIndex == -1 ? history[currentHistoryIndex = history.Count - 1] : history[currentHistoryIndex]);
            }
        }
        /// <summary>
        /// 单击下一首按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (currentHistoryIndex == history.Count - 1)
            {
                if (CurrentCycleMode == CycleMode.SingleCycle)
                {
                    PlayListNext();
                }
                else
                {
                    PlayNext();
                }
            }
            else
            {
                PlayNew(history[++currentHistoryIndex]);
            }
        }
        /// <summary>
        /// 单击循环模式按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCycleModeClickEventHandler(object sender, RoutedEventArgs e)
        {
            //将三个按钮的事件放到了一起
            ((sender as FrameworkElement).Parent as ControlButton).Visibility = Visibility.Hidden;
            switch (((sender as FrameworkElement).Parent as ControlButton).Name)
            {
                case "btnListCycle":
                    btnShuffle.Visibility = Visibility.Visible;
                    break;
                case "btnShuffle":
                    btnSingleCycle.Visibility = Visibility.Visible;
                    break;
                case "btnSingleCycle":
                    btnListCycle.Visibility = Visibility.Visible;
                    break;
                default:
                    ShowAlert("黑人问号");
                    break;
            }

        }
        /// <summary>
        /// 单击打开文件按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpenFileClickEventHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog()
            {
                Title = "请选择音乐文件。",
                Filter = "MP3文件(*.mp3)|*.mp3|WAVE文件(*.wav)|*.wav|所有文件(*.*) | *.*",
                Multiselect = false
            };
            if (opd.ShowDialog() == true && opd.FileNames != null)
            {
                MusicInfo temp = AddNewMusic(opd.FileName);
                if (temp != null)
                {
                    PlayNew(temp);
                }

            }
        }
        #endregion



        #region 列表相关
  /// <summary>
        /// 将文件拖到列表上方事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwDragEnterEventHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            lvw.Drop += (p1, p2) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (files == null)
                {
                    return;
                }
                List<string> musics = new List<string>();
                foreach (var i in files)
                {
                    FileInfo file = new FileInfo(i);
                    if (file.Attributes .HasFlag( FileAttributes.Directory))
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
                AddNewMusics(musics.ToArray());
            };
        }
        /// <summary>
        /// 双击列表项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwItemPreviewMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (currentHistoryIndex < history.Count - 1)
            {
                history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex - 1);
            }
            currentMusicIndex = lvw.SelectedIndex;
            PlayNew();
        }
        /// <summary>
        /// 在列表项上按下按钮事件，包括打开、删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwItemPreviewKeyDownEventHandler(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Enter:
                    LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                    break;
                case Key.Delete:
                    musicInfo.RemoveAt(lvw.SelectedIndex);
                    break;
            }

        }
        /// <summary>
        /// 单击收放列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListSwitcherClickEventHandler(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation aniMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
            };
            DoubleAnimation aniOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
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
                set.ShrinkMusicListManually = false;
            }
            else
            {
                aniMargin.To = new Thickness(-lvw.ActualWidth, 0, 0, 0);
                aniOpacity.To = 0;
                set.ShrinkMusicListManually = true;
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
            lbxLrc.RefreshPlaceholder(grdLrcArea.ActualHeight / 2, set.HighlightLrcFontSize);

            if (!set.AutoFurl || !set.ShowLrc || set.ShrinkMusicListManually)
            {
                return;
            }
            double marginLeft = grdList.Margin.Left;

            if (ActualWidth < 500 && marginLeft == 0)
            {
                BtnListSwitcherClickEventHandler(null, null);
            }
            else if (ActualWidth >= 500 && marginLeft == -lvw.ActualWidth)
            {
                BtnListSwitcherClickEventHandler(null, null);
            }

        }
        /// <summary>
        /// 将文件拖到列表上方事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAndPlayMusicDragEnterEventHandler(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            (sender as UIElement).Drop += (p1, p2) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (files == null || files.Length > 1 )
                {
                    return;
                }
                int currentCount = musicInfo.Count;
                string extension = new FileInfo(files[0]).Extension;
                if (supportExtension.Contains(extension))
                {
                    AddNewMusic(files[0]);
                    if (musicInfo.Count > currentCount)
                    {
                        PlayNew(currentCount);
                    }
                }


            };
        }
        #endregion



        #region 列表与歌词选项
        /// <summary>
        /// 单击列表选项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = sender as UIElement,

                IsOpen = true,
                // Style=Resources["ctmStyle"] as Style
            };


            MenuItem menuOpenFile = new MenuItem() { Header = "文件" };
            menuOpenFile.Click += (p1, p2) =>
            {
                OpenFileDialog opd = new OpenFileDialog()
                {
                    Title = "请选择音乐文件。",
                    Filter = "MP3文件(*.mp3)|*.mp3|WAVE文件(*.wav)|*.wav|所有文件(*.*) | *.*",
                    Multiselect = true
                };
                if (opd.ShowDialog() == true && opd.FileNames != null)
                {
                    List<string> musics = new List<string>();
                    foreach (var i in opd.FileNames)
                    {
                        musics.Add(i);
                    }
                    if (musics.Count >= 1)
                    {
                        AddNewMusics(musics.ToArray());
                    }
                }
            };
            MenuItem menuOpenFolder = new MenuItem() { Header = "文件夹" };
            menuOpenFolder.Click += (p1, p2) =>
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。"
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> musics = new List<string>();

                    foreach (var i in EnumerateFiles(fbd.SelectedPath, supportExtensionWithSplit, SearchOption.TopDirectoryOnly))
                    {
                        musics.Add(i);
                    }

                    if (musics.Count >= 1)
                    {
                        AddNewMusics(musics.ToArray());
                    }
                }
            };

            MenuItem menuOpenAllFolder = new MenuItem() { Header = "文件夹及子文件夹" };
            menuOpenAllFolder.Click += (p1, p2) =>
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。",
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> musics = new List<string>();

                    foreach (var i in EnumerateFiles(fbd.SelectedPath, supportExtensionWithSplit, SearchOption.AllDirectories))
                    {
                        musics.Add(i);
                    }

                    if (musics.Count >= 1)
                    {
                        AddNewMusics(musics.ToArray());
                    }
                }
            };

            MenuItem menuOpenMusicFolder = new MenuItem() { Header = "打开所在文件夹" };
            menuOpenMusicFolder.Click += (p1, p2) =>
  Process.Start("Explorer.exe", @"/select," + musicInfo[lvw.SelectedIndex].Path);

            MenuItem menuShowMusicInfo = new MenuItem() { Header = "显示音乐信息" };
            menuShowMusicInfo.Click += (p1, p2) =>
            {
                MusicInfo music = musicInfo[lvw.SelectedIndex];
                FileInfo fileInfo = new FileInfo(music.Path);
                string l = Environment.NewLine;
                string info = fileInfo.Name + l
                + music.Path + l
                + Math.Round(fileInfo.Length / 1024d) + "KB" + l
                + music.MusicName + l
                + music.Length + l
                + music.Singer + l
                + music.Album;
                WinMusicInfo winMusicInfo = new WinMusicInfo() {Title = fileInfo.Name + "-音乐信息"};
                winMusicInfo.txt.Text = info;
                winMusicInfo.ShowDialog();
            };
            MenuItem menuPlayNext = new MenuItem() { Header = "下一首播放" };
            menuPlayNext.Click += (p1, p2) =>
            {
                if (currentHistoryIndex < history.Count - 1)
                {
                    history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex - 1);
                }
                history.Add(lvw.SelectedItem as MusicInfo);
            };
            MenuItem menuDelete = new MenuItem() { Header = "删除选中项" };
            menuDelete.Click += (p1, p2) =>
            {
                int needDeleteIndex = lvw.SelectedIndex;
                if (currentMusicIndex == needDeleteIndex)
                {
                    PlayListNext();
                }
                musicInfo.RemoveAt(needDeleteIndex);
                if (musicInfo.Count == 0)
                {
                    AfterClearList();
                }
            };

            MenuItem menuClear = new MenuItem() { Header = "清空列表", };
            menuClear.Click += (p1, p2) =>
            {
                musicInfo.Clear();
                AfterClearList();
            };
            MenuItem menuClearExceptCurrent = new MenuItem() { Header = "删除其他", };
            menuClearExceptCurrent.Click += (p1, p2) =>
            {
                int index = lvw.SelectedIndex;
                for (int i = 0; i < index; i++)
                {
                    musicInfo.RemoveAt(0);
                }
                int count = musicInfo.Count;
                for (int i = 1; i < count; i++)
                {
                    musicInfo.RemoveAt(1);
                }
            };
            void AfterClearList()
            {
                currentMusicIndex = -1;
                stkLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;
                if(set.ShowFloatLyric)
                {
                    floatLyric.Clear(); 
                }
                Title = "EasyMusic";
                txtMusicName.Text = "";
                btnPlay.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Hidden;
                path = "";
                Bass.BASS_ChannelStop(stream);
                stream = 0;
            }

            MenuItem menuAutoFurl = new MenuItem() { Header = (set.AutoFurl ? "√" : "×") + "自动收放列表" };
            menuAutoFurl.Click += (p1, p2) =>
            {
                set.AutoFurl = !set.AutoFurl;
                WindowSizeChangedEventHandler(null, null);
            };
            MenuItem menuShowLrc = new MenuItem() { Header = "显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                set.ShowLrc = true;
                grdLrcArea.Visibility = Visibility.Visible;
                //MaxWidth = double.PositiveInfinity;
                //MinWidth = 0;
                grdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                grdMain.ColumnDefinitions[0].Width = GridLength.Auto;

                NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);

            };

            menu.Items.Add(menuOpenFile);
            menu.Items.Add(menuOpenFolder);
            menu.Items.Add(menuOpenAllFolder);
            menu.Items.Add(NewSeparatorLine);
            //menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);

            if (lvw.SelectedIndex != -1)
            {
                menu.Items.Add(menuOpenMusicFolder);
                menu.Items.Add(menuShowMusicInfo);
                menu.Items.Add(menuPlayNext);
                menu.Items.Add(NewSeparatorLine);

                //menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);
            }

            if (musicInfo.Count > 0)
            {
                if (lvw.SelectedIndex != -1)
                {
                    menu.Items.Add(menuDelete);
                    menu.Items.Add(menuClearExceptCurrent);
                }
                menu.Items.Add(menuClear);
                menu.Items.Add(NewSeparatorLine);
            }
            if (set.ShowLrc)
            {
                menu.Items.Add(menuAutoFurl);
            }
            else
            {
                menu.Items.Add(menuShowLrc);
            }
        }
        /// <summary>
        /// 在列表项上单击鼠标右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItemMouseRightButtonUpEvetnHandler(object sender, MouseButtonEventArgs e)
        {
            if (lvw.SelectedIndex != -1)
            {
                BtnListOptionClickEventHanlder(sender, null);

            }
        }
        /// <summary>
        /// 在列表项上按下按钮事件（Enter、Del）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItemKeyUpEventHandler(object sender, KeyEventArgs e)
        {
            if (lvw.SelectedIndex != 1)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                        break;
                    case Key.Delete:
                        musicInfo.RemoveAt(lvw.SelectedIndex);
                        break;
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
            StackPanel menuNormalFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="正常歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.NormalLrcFontSize.ToString()},
           }
            };
            StackPanel menuHighlightFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="当前歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.HighlightLrcFontSize.ToString()},
           }
            };
            StackPanel menuTextFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="文本歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.TextLrcFontSize.ToString()},
           }
            };


            MenuItem menuShowLrc = new MenuItem() { Header = "不显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                set.ShowLrc = false;
                grdLrcArea.Visibility = Visibility.Collapsed;
                set.AutoFurl = false;
                NewDoubleAnimation(this, WidthProperty, grdMain.ColumnDefinitions[0].ActualWidth + 32, 0.5, 0.3, (p3, p4) =>
                {
                    grdMain.ColumnDefinitions[2].Width = new GridLength(0);

                    grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                });
            };


            MenuItem menuCopyLrc = new MenuItem() { Header = "复制歌词" };
            menuCopyLrc.Click += (p1, p2) =>
            {
                if (lrcContent.Count != 0)
                {
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < lrcContent.Count - 1; i++)
                    {
                        str.Append(lrcContent[i] + Environment.NewLine);
                    }
                    str.Append(lrcContent[lrcContent.Count - 1]);
                    Clipboard.SetText(str.ToString());
                }
            };
            MenuItem menuSave = new MenuItem() { Header = "保存歌词" };
            menuSave.Click += (p1, p2) => SaveLrc(false);
            MenuItem menuSaveAs = new MenuItem() { Header = "另存为歌词" };
            menuSaveAs.Click += (p1, p2) => SaveLrc(true);

            MenuItem menuSearchInNetEase = new MenuItem() { Header = "在网易云中搜索" };
            menuSearchInNetEase.Click += (p1, p2) => Process.Start($"https://music.163.com/#/search/m/?s={musicInfo[currentMusicIndex].MusicName}");

            MenuItem menuReload = new MenuItem() { Header = "重载歌词" };
            menuReload.Click += (p1, p2) => InitialiazeLrc();

           
            MenuItem menuFloat = new MenuItem() { Header = (set.ShowFloatLyric ? "√" : "×") + "悬浮歌词" };
            menuFloat.PreviewMouseLeftButtonUp += (p1, p2) =>
            {
                set.ShowFloatLyric = !set.ShowFloatLyric;
                floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                floatLyric.Update(currentLrcIndex);
            };
            menuFloat.PreviewMouseRightButtonDown += (p1, p2) => 
            ShowFloatLyricsMenu();


            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,
                Placement=PlacementMode.Top,
                IsOpen = true
            };
            

            menu.Items.Add(menuShowLrc);
            menu.Items.Add(menuReload);
            menu.Items.Add(menuFloat);
            if (lrcContent.Count != 0 &&(lbxLrc.Visibility==Visibility.Visible || stkLrc.Visibility == Visibility.Visible))
            {
                menu.Items.Add(NewSeparatorLine);
                menu.Items.Add(menuCopyLrc);
                menu.Items.Add(menuSave);
                menu.Items.Add(menuSaveAs);
                
            }
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuSearchInNetEase);
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuNormalFontSizeSetting);
            menu.Items.Add(menuHighlightFontSizeSetting);
            menu.Items.Add(menuTextFontSizeSetting);

            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += (p1, p2) =>
            {
                for (int i = 0; i < 3; i++)
                {
                    string text = ((menu.Items[menu.Items.Count - 4 + i] as StackPanel).Children[1] as TextBox).Text;
                    if (double.TryParse(text, out double newValue))
                    {
                        switch (i)
                        {
                            case 0:
                                set.NormalLrcFontSize = newValue;
                                break;
                            case 1:
                                set.HighlightLrcFontSize = newValue;
                                break;
                            case 2:
                                set.TextLrcFontSize = newValue;
                                txtLrc.FontSize = set.TextLrcFontSize;
                                break;
                        }
                    }
                }
            };

            menu.Items.Add(menuOK);
        }
        /// <summary>
        /// 保存歌词
        /// </summary>
        /// <param name="saveAs"></param>
        private void SaveLrc(bool saveAs)
        {
            StringBuilder str = new StringBuilder();
            if (set.PreferMusicInfo)
            {
                if (musicInfo[currentMusicIndex].MusicName != "")
                {
                    str.Append("[ti:" + musicInfo[currentMusicIndex].MusicName + "]" + Environment.NewLine);
                }
                if (musicInfo[currentMusicIndex].Singer != "")
                {
                    str.Append("[ar:" + musicInfo[currentMusicIndex].Singer + "]" + Environment.NewLine);
                }
                if (musicInfo[currentMusicIndex].Album != "")
                {
                    str.Append("[al:" + musicInfo[currentMusicIndex].Album + "]" + Environment.NewLine);
                }
            }
            else
            {
                if (lrc.Title != "")
                {
                    str.Append("[ti:" + lrc.Title + "]" + Environment.NewLine);
                }
                if (lrc.Artist != "")
                {
                    str.Append("[ar:" + lrc.Artist + "]" + Environment.NewLine);
                }
                if (lrc.Album != "")
                {
                    str.Append("[al:" + lrc.Album + "]" + Environment.NewLine);
                }
                if (lrc.LrcBy != "")
                {
                    str.Append("[by:" + lrc.LrcBy + "]" + Environment.NewLine);
                }
            }
            if (set.SaveLrcOffsetByTag && offset != 0)
            {
                str.Append("[offset:" + (int)Math.Round(offset * 1000) + "]" + Environment.NewLine);
            }
            List<double> lrcTime = new List<double>();
            foreach (var i in this.lrcTime)
            {
                lrcTime.Add((set.SaveLrcOffsetByTag) ? i : i + offset);
            }

            FileInfo file = new FileInfo(path);
            for (int i = 0; i < lrcTime.Count; i++)
            {
                double time = lrcTime[i];
                string word = lrcContent[i];
                int intMinute = (int)time / 60;
                string minute = string.Format("{0:00}", intMinute);
                string second = string.Format("{0:00.00}", time - 60 * intMinute);
                foreach (var j in word.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    str.Append("[" + minute + ":" + second + "]" + j + Environment.NewLine);
                }

            }
            if (saveAs)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    AddExtension = true,
                    InitialDirectory = file.DirectoryName,
                    Title = "请选择目标文件夹",
                    Filter = "歌词文件（*.lrc）|*.lrc",
                    FileName = file.Name.Replace(file.Extension, ".lrc"),
                };
                if (sfd.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, str.ToString());
                        ShowInfo("歌词保存成功");
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("无法保存文件：" + Environment.NewLine + ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(path.Replace(file.Extension, "") + ".lrc", str.ToString());
                    ShowInfo("歌词保存成功");
                }
                catch (Exception ex)
                {
                    ShowAlert("无法保存文件：" + Environment.NewLine + ex.Message);
                }
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
        private void ShowInfo(string info)
        {
            tbkOffset.Text = info;
            tbkOffset.Opacity = 1;
            infoWaitTimer?.Stop();
            infoWaitTimer = SleepThenDo(1000, (p1, p2) => NewDoubleAnimation(tbkOffset, OpacityProperty, 0, 0.5, 0, (p3, p4) => tbkOffset.Opacity = 0, true));
        }
        /// <summary>
        /// 显示悬浮菜单的菜单
        /// </summary>
        private void ShowFloatLyricsMenu()
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,
                
                IsOpen = true,
            };



            StackPanel menuNormalFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="正常歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsNormalFontSize.ToString()},
           }
            };
            StackPanel menuHighlightFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="当前歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsHighlightFontSize.ToString()},
           }
            };
            MenuItem menuAdjust = new MenuItem() { Header = "调整位置和大小" };
            menuAdjust.Click += (p1, p2) => floatLyric.Adjuest = true;
            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += (p1, p2) =>
            {
                for (int i = 0; i < 2; i++)
                {
                    string text = ((menu.Items[menu.Items.Count - 3 + i] as StackPanel).Children[1] as TextBox).Text;
                    if (double.TryParse(text, out double newValue))
                    {
                        switch (i)
                        {
                            case 0:
                                set.FloatLyricsNormalFontSize = newValue;
                                break;
                            case 1:
                                set.FloatLyricsHighlightFontSize = newValue;
                                break;
                        }
                    }
                }
            };
            menu.Items.Add(menuAdjust);
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuNormalFontSizeSetting);
            menu.Items.Add(menuHighlightFontSizeSetting);
            menu.Items.Add(menuOK);





        }
        #endregion


        #region 进度条与音量条
        /// <summary>
        /// 是否正在拖动进度条
        /// </summary>
        bool changingPosition = false;

        /// <summary>
        /// 进度条鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProgressPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("down");
            var pt = e.GetPosition(sldProcess);
            double newPosition = (pt.X / sldProcess.ActualWidth) * sldProcess.Maximum;
            changingPosition = true;
            if (Math.Abs(sldProcess.Value - newPosition) >= 5)//不然按Thumb也会跳
            {
                sldProcess.Value = newPosition;
            }
        }
        /// <summary>
        /// 进度条值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProcessValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double position = sldProcess.Value;
            if (changingPosition)
            {
                Bass.BASS_ChannelSetPosition(stream, position);
                //Debug.WriteLine("change");
                UpdatePosition();
            }
            TimeSpan time = TimeSpan.FromSeconds(position);
            tbkCurrentPosition.Text = $"{string.Format("{0:00}", (int)time.TotalMinutes)}:{string.Format("{0:00}", time.Seconds)}";

        }
        /// <summary>
        /// 进度条鼠标松开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProcessPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("up");
            changingPosition = false;
        }
        /// <summary>
        /// 拖动或点击音量滑杆事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldVolumnValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volumn = sldVolumn.Value;
            btnDevice.Opacity = 0.2 + 0.6 * sldVolumn.Value;
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
            //BtnNextClickEventHandler(null, null);
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) + 4;
            if (position > musicLength)
            {
                position = musicLength;
            }
            Bass.BASS_ChannelSetPosition(stream, position);
        }
        /// <summary>
        /// 执行后退快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyBackEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) - 4;
            if (position < 0)
            {
                position = 0;
            }
            Bass.BASS_ChannelSetPosition(stream, position);
        }
        /// <summary>
        /// 执行播放暂停快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyPlayAndPauseEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (btnPause.Visibility == Visibility.Visible)
            {
                BtnPauseClickEventHandler(null, null);
            }
            else
            {
                BtnPlayClickEventHandler(null, null);
            }
        }
        /// <summary>
        /// 执行下一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyNextEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            BtnNextClickEventHandler(null, null);
        }
        /// <summary>
        /// 执行上一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyLastEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            BtnLastClickEventHandler(null, null);
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
                HotKeyPlayAndPauseEventHandler(null, null);
            }
        }
        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegistGolbalHotKey()
        {
            HotKey next = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Right);
            HotKey last = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Left);
            next.OnHotKey += () => BtnNextClickEventHandler(null, null);
            last.OnHotKey += () => BtnLastClickEventHandler(null, null);
            HotKey up = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Up);
            HotKey down = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Down);
            up.OnHotKey += () => sldVolumn.Value += 0.05;
            down.OnHotKey += () => sldVolumn.Value -= 0.05;
            HotKey playAndPause = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.OemQuestion);
            playAndPause.OnHotKey += () => HotKeyPlayAndPauseEventHandler(null, null);

        }
        #endregion

        #region 音频硬件
        /// <summary>
        /// 单击切换播放设备按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeviceSwitchClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (Bass.BASS_GetDeviceCount() - 2 <= 0)
            {
                return;
            }
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = sender as UIElement,
                IsOpen = true
            };
            var devices = Bass.BASS_GetDeviceInfos();
            for (int i = 1; i < devices.Length; i++)
            {
                Bass.BASS_Init(i, 44100, BASSInit.BASS_DEVICE_DEFAULT, new WindowInteropHelper(this).Handle);
            }
            int n = -1;
            foreach (var i in devices)
            {
                n++;
                if (n == 0 || n == Bass.BASS_ChannelGetDevice(stream))
                {
                    continue;
                }
                MenuItem menuItem = new MenuItem() { Header = i.name, Tag = n };
                menuItem.Click += MenuDeviceClickEventHandler;
                menu.Items.Add(menuItem);
            }
        }
        /// <summary>
        /// 单击播放设备菜单项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDeviceClickEventHandler(object sender, RoutedEventArgs e)
        {
            int device = int.Parse((sender as MenuItem).Tag.ToString());

            if (!Bass.BASS_ChannelSetDevice(stream, device)
            || !Bass.BASS_SetDevice(device))
            {

                ShowAlert(Bass.BASS_ErrorGetCode().ToString());
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
            foreach (var i in musicInfo)
            {
                index++;

                if (IsInfoMatch(value, i))
                {
                    ComboBoxItem cbbItem = new ComboBoxItem() { Content = i.MusicName, Tag = index };
                    cbbItem.PreviewMouseLeftButtonUp += (p1, p2) =>
                    {
                        PlayNew((int)cbbItem.Tag, false);
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
                int index = -1;
                foreach (var i in musicInfo)
                {
                    index++;
                    if (IsInfoMatch(value, i))
                    {
                        PlayNew(index, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    }
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
            string musicName = (info.MusicName + new FileInfo(info.Path).Name).ToLower();
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
        private void UpdateColor()
        {
            var color = colorPicker.CurrentColor;
            Resources["backgroundBrushColor"] = color;
            Resources["darker1BrushColor"] = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.9f, color.Color.ScG * 0.9f, color.Color.ScB * 0.9f));
            Resources["darker2BrushColor"] = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.8f, color.Color.ScG * 0.8f, color.Color.ScB * 0.8f));
            Resources["darker3BrushColor"] = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.7f, color.Color.ScG * 0.7f, color.Color.ScB * 0.7f));
            Resources["darker4BrushColor"] = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.6f, color.Color.ScG * 0.6f, color.Color.ScB * 0.6f));

            Resources["backgroundColor"] = color.Color;
            Resources["backgroundTransparentColor"] = System.Windows.Media.Color.FromArgb(0, color.Color.R, color.Color.G, color.Color.B);

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
                (mainContextMenu.Items[0] as MenuItem).Header = Topmost ? "√" : "×"+"置顶";
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
            };
            StackPanel menuColor = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock{Text="背景"},
                    colorPicker,
                },
            };
            colorPicker.ChooseComplete((p1, p2) => mainContextMenu.IsOpen = false);

            MenuItem menuSettings = new MenuItem()
            {
                Header = "设置"
            };
            menuSettings.Click += (p1, p2) =>
            {
                new WinSettings(set).ShowDialog();
            };
            MenuItem menuHelp = new MenuItem()
            {
                Header = "帮助"
            };
            menuHelp.Click += (p1, p2) =>
            {
                new WinHelp().ShowDialog();
            };
            MenuItem menuAbout = new MenuItem()
            {
                Header = "关于"
            };
            menuAbout.Click += (p1, p2) =>
            {
                new WinAbout().ShowDialog();
            };
            mainContextMenu = new ContextMenu()
            {
                PlacementTarget = btnSettings,
                IsOpen = true,
                Items = { menuTop, menuColor, menuSettings, menuHelp, menuAbout },
            };
            mainContextMenu.Closed += (p1, p2) => UpdateColor();
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
            if (set.TrayMode == 1)
            {
                NewDoubleAnimation(this, TopProperty, SystemParameters.FullPrimaryScreenHeight, 0.2, 0, (p1, p2) =>
                {
                    Top = reservedTop;
                        Hide();
                }, true);
            }
            else
            {
                Close();
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
            NewDoubleAnimation(this, TopProperty, SystemParameters.FullPrimaryScreenHeight, 0.2, 0, (p1, p2) =>
              {
                  Top = reservedTop;
                  if (set.TrayMode == 2)
                  {
                      Hide();
                  }
                  else
                  {
                      WindowState = WindowState.Minimized;
                  }
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
                WinAlbumPicture win = new WinAlbumPicture(this);
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
        /// 读取Mp3信息
        /// </summary>
        /// <param name="path"></param>
        private void ReadMusicSourceInfo(string path)//copy
        {
            string[] tags = new string[6];
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            string mp3ID = "";
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }
                ReadFrame();
            }
            void ReadFrame()//copy
            {
                while (size > 0)
                {
                    //读取标签帧头的10个字节
                    fs.Read(buffer, 0, 10);
                    size -= 10;
                    //得到标签帧ID
                    string FramID = Encoding.Default.GetString(buffer, 0, 4);
                    //计算标签帧大小，第一个字节代表帧的编码方式
                    int frmSize = 0;

                    frmSize = buffer[4] * 0x1000000 + buffer[5] * 0x10000 + buffer[6] * 0x100 + buffer[7];
                    if (frmSize == 0)
                    {
                        //就说明真的没有信息了
                        break;
                    }
                    //bFrame 用来保存帧的信息
                    byte[] bFrame = new byte[frmSize];
                    fs.Read(bFrame, 0, frmSize);
                    size -= frmSize;
                    string str = GetFrameInfoByEcoding(bFrame, bFrame[0], frmSize - 1);
                    imgAlbum.Source = null;
                    imgAlbum.Visibility = Visibility.Collapsed;
                    if (FramID.CompareTo("APIC") == 0)
                    {
                        try
                        {
                            int i = 0;
                            while (true)
                            {
                                if (255 == bFrame[i] && 216 == bFrame[i + 1])
                                {
                                    break;
                                }
                                i++;
                            }
                            byte[] imge = new byte[frmSize - i];
                            fs.Seek(-frmSize + i, SeekOrigin.Current);
                            fs.Read(imge, 0, imge.Length);
                            MemoryStream ms = new MemoryStream(imge);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                            string imgPath = Path.GetTempFileName();
                            FileStream save = new FileStream(imgPath, FileMode.Create);
                            img.Save(save, System.Drawing.Imaging.ImageFormat.Jpeg);
                            save.Close();
                            imgAlbum.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imgPath));
                            imgAlbum.Visibility = Visibility.Visible;
                        }
                        catch
                        {
                        }
                    }
                }

            }
            string GetFrameInfoByEcoding(byte[] b, byte conde, int length)//copy
            {
                string str = "";
                switch (conde)
                {
                    case 0:
                        str = Encoding.GetEncoding("ISO-8859-1").GetString(b, 1, length);
                        break;
                    case 1:
                        str = Encoding.GetEncoding("UTF-16LE").GetString(b, 1, length);
                        break;
                    case 2:
                        str = Encoding.GetEncoding("UTF-16BE").GetString(b, 1, length);
                        break;
                    case 3:
                        str = Encoding.UTF8.GetString(b, 1, length);
                        break;
                }
                return str;
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
        #endregion

        #region 鼠标滚轮
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            if (mouseInLrcArea &&( stkLrc.Visibility == Visibility.Visible || lbxLrc.Visibility==Visibility.Visible))
            {
                if (e.Delta > 0)
                {
                    offset -= 1d / 4;
                }
                else
                {
                    offset += 1d / 4;
                }
                ShowInfo("当前歌词偏移量：" + (offset > 0 ? "+" : "") + Math.Round(offset, 2).ToString() + "秒");
            }
            else if (!(mouseInList || mouseInLrcArea))
            {
                if (e.Delta > 0)
                {
                    sldVolumn.Value += 0.05;
                }
                else
                {
                    sldVolumn.Value -= 0.05;
                }
            }
        }
        /// <summary>
        /// 鼠标是否在歌词区域
        /// </summary>
        bool mouseInLrcArea = false;
        /// <summary>
        /// 鼠标进入歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            mouseInLrcArea = true;
        }
        /// <summary>
        /// 鼠标离开歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            mouseInLrcArea = false;
        }
        /// <summary>
        /// 鼠标是否在列表上
        /// </summary>
        bool mouseInList = false;
        /// <summary>
        /// 鼠标进入列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            mouseInList = true;
        }
        /// <summary>
        /// 鼠标离开列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            mouseInList = false;
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
            BtnPlayClickEventHandler(null, null);
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
            BtnPauseClickEventHandler(null, null);
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
            BtnLastClickEventHandler(null, null);
        }
        /// <summary>
        /// 单击任务栏上的下一曲按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbiNextClickEventHandler(object sender, EventArgs e)
        {
            BtnNextClickEventHandler(null, null);
        }
        #endregion


    }

}
