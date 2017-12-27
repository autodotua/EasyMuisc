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
using System.Collections.Generic;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Drawing;
using System.Windows.Data;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Security.Permissions;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyMuisc
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 单曲信息
        /// </summary>
        public class MusicInfo
        {
            public string MusicName { get; internal set; }
            public string Singer { get; internal set; }
            public string Length { get; internal set; }
            public string Album { get; internal set; }
            public string Path { get; internal set; }
            public bool Enable { get; internal set; }
        }
        #region 枚举、属性、字段
        /// <summary>
        /// 循环模式
        /// </summary>
        private enum CycleMode
        {
            SingleCycle,
            ListCycle,
            Shuffle,
        }
        #region 模板
        public static class DispatcherHelper
        {
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                try
                {
                    DispatcherFrame frame = new DispatcherFrame();
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
                    Dispatcher.PushFrame(frame);
                }
                catch (InvalidOperationException) { }
            }
            private static object ExitFrames(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message"></param>
        public bool ShowAlert(string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            if (button == MessageBoxButton.YesNo)
            {
                if (MessageBox.Show(message, "错误", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    return true;
                }
                return false;
            }
            else
            {
                MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }
        System.Windows.Shapes.Line SeparatorLine = new System.Windows.Shapes.Line()
        {
            X1 = 0,
            X2 = 100,
            Y1 = 0,
            Y2 = 0,
            Stroke = System.Windows.Media.Brushes.LightGray,
        };
        /// <summary>
        /// 创建并启动一个新的浮点数据类型动画
        /// </summary>
        /// <param name="obj">动画主体</param>
        /// <param name="property">更改的属性</param>
        /// <param name="to">目标值</param>
        /// <param name="duration">时间</param>
        /// <param name="decelerationRatio">减缓时间</param>
        /// <param name="completed">完成以后的事件</param>
        /// <returns></returns>
        private Storyboard NewDoubleAnimation(FrameworkElement obj, DependencyProperty property, double to, double duration, double decelerationRatio, EventHandler completed = null)
        {

            DoubleAnimation ani = new DoubleAnimation
            {
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),//动画时间1秒
                DecelerationRatio = decelerationRatio
            };
            Storyboard.SetTargetName(ani, obj.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(property));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            if (completed != null)
            {
                story.Completed += completed;
            }
            story.Begin(obj);
            return story;
        }
        #endregion
        /// <summary>
        /// 是否产生了不可挽救错误
        /// </summary>
        bool error = false;
        /// <summary>
        /// 每秒钟检测次数
        /// </summary>
        const double fps = 20;
        /// <summary>
        /// 音乐文件路径
        /// </summary>
        public string path;
        /// <summary>
        /// 音乐播放句柄
        /// </summary>
        int stream = 0;
        /// <summary>
        /// 配置文件
        /// </summary>
        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        /// <summary>
        /// 分隔符
        /// </summary>
        const string split = "#Split#";
        /// <summary>
        /// 音乐信息，与列表绑定
        /// </summary>
        public ObservableCollection<MusicInfo> musicInfo;
        /// <summary>
        /// 当前音乐在列表中的索引
        /// </summary>
        public int currentMusicIndex = 0;
        /// <summary>
        /// 是否正在拖动进度条
        /// </summary>
        bool changingPosition = false;
        /// <summary>
        /// 歌词列表
        /// </summary>
        List<double> lrcTime = new List<double>();
        /// <summary>
        /// 歌词内容
        /// </summary>
        List<string> lrcContent = new List<string>();
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcIndex = 0;
        /// <summary>
        /// 歌词字体大小
        /// </summary>
        double normalLrcFontSize = 18;
        /// <summary>
        /// 当前歌词字体大小
        /// </summary>
        double highlightLrcFontSize = 36;
        /// <summary>
        /// txt格式的歌词的字体大小
        /// </summary>
        double textLrcFontSize = 28;
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
        /// 历史记录
        /// </summary>
        private List<MusicInfo> history = new List<MusicInfo>();
        /// <summary>
        /// 当前播放历史索引
        /// </summary>
        int currentHistoryIndex = -1;
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
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, (float)value);
            }
            get
            {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref value);
                return value;
            }
        }
        /// <summary>
        /// 自动收放列表
        /// </summary>
        private bool AutoFurl
        {
            get
            {
                return GetConfig("autoFurl", "True") == "True" ? true : false;
            }
            set
            {
                SetConfig("autoFurl", value.ToString());
            }
        }
        /// <summary>
        /// 显示歌词
        /// </summary>
        private bool ShowLrc
        {
            get
            {
                return GetConfig("showLrc", "True") == "True" ? true : false;
            }
            set
            {
                SetConfig("showLrc", value.ToString());
            }
        }
        /// <summary>
        /// 是否正在关闭
        /// </summary>
        bool closing = false;
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
        DispatcherTimer mainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / fps) };
        #endregion



        #region 初始化和配置
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("bass.dll"))
            {
                File.WriteAllBytes("bass.dll", Properties.Resources.bass);
            }

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, new WindowInteropHelper(this).Handle))
            {
                ShowAlert("无法初始化音乐引擎。");
                error = true;
                Application.Current.Shutdown();
            }

            musicInfo = new ObservableCollection<MusicInfo>();
            lvw.DataContext = musicInfo;
            InitialiazeTimer();


        }
        /// <summary>
        /// 初始化定时器事件
        /// </summary>
        private void InitialiazeTimer()
        {
            playTimer.Tick += delegate
            {

                if (Volumn >= sldVolumn.Value)
                {
                    playTimer.Stop();
                    return;
                }
                Volumn += 0.05;
            };
            pauseTimer.Tick += delegate
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
        }
        /// <summary>
        /// 窗体载入事件，获取音乐列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoadedEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                GetMusicListFromConfig();
                Cursor = Cursors.Arrow;
                switch (GetConfig("CycleMode", "1"))
                {
                    case "1":
                        btnListCycle.Visibility = Visibility.Visible;
                        break;
                    case "2":
                        btnShuffle.Visibility = Visibility.Visible;
                        break;
                    case "3":
                        btnSingleCycle.Visibility = Visibility.Visible;
                        break;
                }
                normalLrcFontSize = double.Parse(GetConfig("normalLrcFontSize", normalLrcFontSize.ToString()));
                highlightLrcFontSize = double.Parse(GetConfig("highlightLrcFontSize", highlightLrcFontSize.ToString()));
                textLrcFontSize = double.Parse(GetConfig("textLrcFontSize", textLrcFontSize.ToString()));



            }
            catch
            {
                if (ShowAlert("配置文件被篡改，是否重置？", MessageBoxButton.YesNo))
                {
                    cfa.AppSettings.Settings.Clear();
                }
            }
        }
        /// <summary>
        /// 从配置文件读取音乐列表
        /// </summary>
        /// <returns></returns>
        private bool GetMusicListFromConfig()
        {
            try
            {
                Thread t = new Thread(() =>
                {
                    if (cfa.AppSettings.Settings["MusicList"] != null)
                    {
                        string[] musics = cfa.AppSettings.Settings["MusicList"].Value.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var i in musics)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                AddNewMusic(i);
                            }));
                        }

                    }
                    if (path == null)
                    {
                        string tempPath = GetConfig("lastMusic", "");
                        if (File.Exists(tempPath))
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                PlayNew(AddNewMusic(tempPath), false);
                            }));
                        }
                    }
                });
                t.Start();
                return true;
            }
            catch
            {
                return false;
            }
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
                musicInfo.Add(new MusicInfo
                {
                    Enable = GetMusicInfo(path, out string name, out string singer, out string length, out string album) ? true : false,
                    MusicName = name,
                    Singer = singer,
                    Length = length.StartsWith("00:") ? length.Remove(0, 3) : length,
                    Path = path,
                    Album = album,
                });
                DispatcherHelper.DoEvents();
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
                name = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
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
        /// 保存音乐列表到配置文件中
        /// </summary>
        /// <returns></returns>
        private bool SaveMusicListFromConfig()
        {
            try
            {
                StringBuilder str = new StringBuilder();
                foreach (var i in musicInfo)
                {
                    str.Append(i.Path + split);
                }
                SetConfig("MusicList", str.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 设置配置项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool SetConfig(string key, string value)
        {
            if (cfa.AppSettings.Settings[key] != null)
            {
                cfa.AppSettings.Settings[key].Value = value;
            }
            else
            {
                cfa.AppSettings.Settings.Add(key, value);
            }
            return true;
        }
        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string GetConfig(string key, string defaultValue = "null")
        {
            if (cfa.AppSettings.Settings[key] != null)
            {
                return cfa.AppSettings.Settings[key].Value;
            }
            else
            {
                return defaultValue;
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
            SaveMusicListFromConfig();
            switch (CurrentCycleMode)
            {
                case CycleMode.ListCycle:
                    SetConfig("CycleMode", "1");
                    break;
                case CycleMode.Shuffle:
                    SetConfig("CycleMode", "2");
                    break;
                case CycleMode.SingleCycle:
                    SetConfig("CycleMode", "3");
                    break;
            }
            SetConfig("normalLrcFontSize", normalLrcFontSize.ToString());
            SetConfig("highlightLrcFontSize", highlightLrcFontSize.ToString());
            SetConfig("textLrcFontSize", textLrcFontSize.ToString());
            SetConfig("lastMusic", path);
            //Bass.BASS_Stop();
            cfa.Save();
            e.Cancel = true;
            closing = true;
            Hide();
            pauseTimer.Start();
        }
        #endregion
        #region 音乐相关
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
                Title = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "") + " - EasyMusic";//将窗体标题改为歌曲名
                string[] length = musicInfo[currentMusicIndex].Length.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                double musicLength = length.Length == 2 ?//如果不到一个小时
                    int.Parse(length[0]) * 60 + int.Parse(length[1]) ://得到秒钟
                     int.Parse(length[0]) * 3600 + int.Parse(length[1]) * 60 + int.Parse(length[2]);
                sldProcess.Maximum = musicLength;
                InitialiazeLrc(musicLength);
            }
            catch
            {
                ShowAlert("初始化失败!");
                return;
            }
            ReadMp3(musicInfo[currentMusicIndex].Path);

            mainTimer.Tick += Update;
            mainTimer.Start();

        }
        /// <summary>
        /// 初始化歌词
        /// </summary>
        /// <param name="musicLength"></param>
        private void InitialiazeLrc(double musicLength)
        {
            FileInfo file = new FileInfo(path);
            file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
            if (file.Exists)//判断是否存在歌词文件
            {
                grdLrc.Visibility = Visibility.Visible;
                txtLrc.Visibility = Visibility.Hidden;
                stkLrc.Visibility = Visibility.Visible;
                var lrc = new Lrc(file.FullName).LrcContent;//获取歌词信息
                int index = 0;//用于赋值Tag
                foreach (var i in lrc)
                {
                    if (i.Key > musicLength)//如果歌词文件有误，长度超过了歌曲的长度，那么超过部分就不管了
                    {
                        break;
                    }
                    lrcContent.Add(i.Value);
                    var tbk = new TextBlock()
                    {
                        FontSize = normalLrcFontSize,
                        Text = i.Value,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = index++,//标签用于定位
                        Cursor = Cursors.Hand,
                    };
                    tbk.MouseLeftButtonUp += delegate
                    {
                        //单击歌词跳转到当前歌词
                        Bass.BASS_ChannelSetPosition(stream, lrcTime[(int)tbk.Tag]);
                    };
                    stkLrc.Children.Add(tbk);
                    lrcTime.Add(i.Key);

                }
            }
            else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
            {

                txtLrc.Text = File.ReadAllText(file.FullName, EncodingType.GetType(file.FullName));
                for (int i = 0; i < txtLrc.LineCount; i++)
                {
                    lrcContent.Add(txtLrc.GetLineText(i));
                }
                txtLrc.FontSize = textLrcFontSize;
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Visible;
                stkLrc.Visibility = Visibility.Hidden;
            }
            else
            {
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                stkLrc.Visibility = Visibility.Hidden;
            }
        }
        /// <summary>
        /// 定时更新各项数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update(object sender, EventArgs e)
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
                UpdatePositionOfLrcPanel();
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
                    Random r = new Random();
                    int index;
                    while ((index = r.Next(musicInfo.Count)) == currentMusicIndex)
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
        /// 更新当前时间的歌词
        /// </summary>
        private void UpdatePositionOfLrcPanel()
        {
            if (lrcTime.Count == 0)
            {
                return;
            }
            double position = Bass.BASS_ChannelBytes2Seconds(stream,
                Bass.BASS_ChannelGetPosition(stream));//获取当前播放的位置
            bool changed = false;//是否
            if (position == 0)
            {
                changed = true;
                currentLrcIndex = 0;
            }
            else
            {
                for (int i = 0; i < lrcTime.Count; i++)//从第一个循环到倒数第二个歌词时间
                {
                    if (lrcTime[i] < position)//如果当前的播放时间夹在两个歌词时间之间
                    {
                        if (currentLrcIndex != i)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            currentLrcIndex = i;
                        }
                    }
                }
            }

            if (changed)
            {
                foreach (var i in stkLrc.Children)
                {
                    //首先把所有的歌词都改为正常大小
                    (i as TextBlock).FontSize = normalLrcFontSize;
                }
            (stkLrc.Children[currentLrcIndex] as TextBlock).FontSize = highlightLrcFontSize;//当前歌词改为高亮
                LrcAnimition(currentLrcIndex);//歌词转变动画
            }
        }
        /// <summary>
        /// 歌词转变动画
        /// </summary>
        /// <param name="lrcIndex"></param>
        private void LrcAnimition(int lrcIndex)
        {
            double top = 0.5 * ActualHeight - lrcIndex * normalLrcFontSize * FontFamily.LineSpacing/*歌词数量乘每行字的高度*/- normalLrcFontSize - highlightLrcFontSize;// 0.5 * ActualHeight - stkLrcHeight * lrcIndex / (stkLrc.Children.Count - 1)-highlightFontSize ;
            ThicknessAnimation ani = new ThicknessAnimation
            {
                To = new Thickness(0, top, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),//动画时间1秒
                DecelerationRatio = 0.5
            };
            Storyboard.SetTargetName(ani, stkLrc.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(MarginProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            story.Begin(stkLrc);

        }
        /// <summary>
        /// （暂停后）播放
        /// </summary>
        /// <returns></returns>
        private void Play()
        {

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
            currentLrcIndex = 0;//删除歌词索引
            lrcTime.Clear();//清空歌词时间
            lrcContent.Clear();//清除歌词内容
            stkLrc.Children.Clear();//清空歌词表
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
        #region 播放控制
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
            (sender as Button).Visibility = Visibility.Hidden;
            switch ((sender as Button).Name)
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

            lvw.Drop += delegate
              {
                  string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                  if (files == null)
                  {
                      return;
                  }
                  foreach (var i in files)
                  {
                      string extension = new FileInfo(i).Extension;
                      if (extension == ".mp3" || extension == ".wav")
                      {
                          AddNewMusic(i);
                      }
                  }

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
            }
            else
            {
                aniMargin.To = new Thickness(-lvw.ActualWidth, 0, 0, 0);
                aniOpacity.To = 0;
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
            if (!AutoFurl || !ShowLrc)
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

            (sender as UIElement).Drop += delegate
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (files == null)
                {
                    return;
                }
                int currentCount = musicInfo.Count;
                foreach (var i in files)
                {
                    string extension = new FileInfo(i).Extension;
                    if (extension == ".mp3" || extension == ".wav")
                    {
                        AddNewMusic(i);
                    }
                }
                if (musicInfo.Count > currentCount)
                {
                    PlayNew(currentCount);
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
                PlacementTarget = btnListOption,
                Placement = PlacementMode.Top,
                IsOpen = true,
                // Style=Resources["ctmStyle"] as Style
            };


            MenuItem menuOpenFile = new MenuItem() { Header = "文件" };
            menuOpenFile.Click += delegate
            {
                OpenFileDialog opd = new OpenFileDialog()
                {
                    Title = "请选择音乐文件。",
                    Filter = "MP3文件(*.mp3)|*.mp3|WAVE文件(*.wav)|*.wav|所有文件(*.*) | *.*",
                    Multiselect = true
                };
                if (opd.ShowDialog() == true && opd.FileNames != null)
                {
                    foreach (var i in opd.FileNames)
                    {
                        AddNewMusic(i);
                    }
                }
            };
            MenuItem menuOpenFolder = new MenuItem() { Header = "文件夹" };
            menuOpenFolder.Click += delegate
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。"
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var extension in new string[] { "*.mp3", "*.wav" })
                    {
                        foreach (var i in Directory.EnumerateFiles(fbd.SelectedPath, extension, SearchOption.TopDirectoryOnly))
                        {
                            AddNewMusic(i);
                        }
                    }
                }
            };

            MenuItem menuOpenAllFolder = new MenuItem() { Header = "文件夹及子文件夹" };
            menuOpenAllFolder.Click += delegate
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。",
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var extension in new string[] { "*.mp3", "*.wav" })
                    {
                        foreach (var i in Directory.EnumerateFiles(fbd.SelectedPath, extension, SearchOption.AllDirectories))
                        {
                            AddNewMusic(i);
                        }
                    }
                }
            };

            MenuItem menuOpenMusicFolder = new MenuItem() { Header = "打开所在文件夹" };
            menuOpenMusicFolder.Click += delegate
              {
                  System.Diagnostics.Process.Start("Explorer.exe", @"/select," + musicInfo[lvw.SelectedIndex].Path);
              };

            MenuItem menuShowMusicInfo = new MenuItem() { Header = "显示音乐信息" };
            menuShowMusicInfo.Click += delegate
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
                  + music.Album
                  ;
                  WinMusicInfo winMusicInfo = new WinMusicInfo()
                  {
                      Title = fileInfo.Name + "-音乐信息",
                  };
                  winMusicInfo.txt.Text = info;
                  winMusicInfo.ShowDialog();
              };
            MenuItem menuDelete = new MenuItem() { Header = "删除选中项" };
            menuDelete.Click += delegate
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
            menuClear.Click += delegate
            {
                musicInfo.Clear();
                AfterClearList();
            };
            void AfterClearList()
            {
                currentMusicIndex = -1;
                stkLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                Title = "EasyMusic";
                btnPlay.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Hidden;
                Bass.BASS_ChannelStop(stream);
                stream = 0;
            }

            MenuItem menuAutoFurl = new MenuItem() { Header = (AutoFurl ? "√" : "×") + "自动收放列表" };
            menuAutoFurl.Click += delegate
            {
                AutoFurl = !AutoFurl;
                WindowSizeChangedEventHandler(null, null);
            };
            MenuItem menuShowLrc = new MenuItem() { Header = "显示歌词" };
            menuShowLrc.Click += delegate
            {
                ShowLrc = true;
                grdLrcArea.Visibility = Visibility.Visible;
                MaxWidth = double.PositiveInfinity;
                MinWidth = 0;
                NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);
            };

            menu.Items.Add(menuOpenFile);
            menu.Items.Add(menuOpenFolder);
            menu.Items.Add(menuOpenAllFolder);
            menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);

            if (lvw.SelectedIndex != -1)
            {
                menu.Items.Add(menuOpenMusicFolder);
                menu.Items.Add(menuShowMusicInfo);
                menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);
            }

            if (musicInfo.Count > 0)
            {
                if (lvw.SelectedIndex != -1)
                {
                    menu.Items.Add(menuDelete);
                }
                menu.Items.Add(menuClear);
                menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);
            }
            if (ShowLrc)
            {
                menu.Items.Add(menuAutoFurl);
            }
            else
            {
                menu.Items.Add(menuShowLrc);
            }
        }
        /// <summary>
        ///  选项按钮鼠标进入动画事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnimitionMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            NewDoubleAnimation(sender as Button, OpacityProperty, 0.8, 0.5, 0.3);
        }
        /// <summary>
        /// 选项按钮鼠标离开动画事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnimitionMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            NewDoubleAnimation(sender as Button, OpacityProperty, 0.2, 0.5, 0.3);
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
                Items =
                {
                     new StackPanel()
            {
                Orientation=Orientation.Horizontal,
                Children =
                {
                    new TextBlock(){Text="正常歌词字体大小："},
                    new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=normalLrcFontSize.ToString()},
                }
            },
                     new StackPanel()
            {
                Orientation=Orientation.Horizontal,
                Children =
                {
                    new TextBlock(){Text="当前歌词字体大小："},
                    new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=highlightLrcFontSize.ToString()},
                }
            },
                     new StackPanel()
            {
                Orientation=Orientation.Horizontal,
                Children =
                {
                    new TextBlock(){Text="文本歌词字体大小："},
                    new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=textLrcFontSize.ToString()},
                }
            },
                 },
                IsOpen = true
            };

            MenuItem menuCopyLrc = new MenuItem()
            {
                Header = "复制歌词"
            };
            menuCopyLrc.Click += delegate
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
            if (lrcContent.Count != 0)
            {
                menu.Items.Insert(0, menuCopyLrc);
            }

            MenuItem menuShowLrc = new MenuItem() { Header = "不显示歌词" };
            menuShowLrc.Click += delegate
              {
                  ShowLrc = false;
                  grdLrcArea.Visibility = Visibility.Collapsed;
                  AutoFurl = false;
                  NewDoubleAnimation(this, WidthProperty, grdList.ActualWidth + 24, 0.5, 0.3, delegate
                   {
                       MaxWidth = MinWidth = grdList.ActualWidth + 24;
                   });

              };
            menu.Items.Insert(0, menuShowLrc);


            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += delegate
            {
                for (int i = 0; i < 3; i++)
                {
                    string text = ((menu.Items[i] as StackPanel).Children[1] as TextBox).Text;
                    if (double.TryParse(text, out double newValue))
                    {
                        switch (i)
                        {
                            case 0:
                                normalLrcFontSize = newValue;
                                break;
                            case 1:
                                highlightLrcFontSize = newValue;
                                break;
                            case 2:
                                textLrcFontSize = newValue;
                                txtLrc.FontSize = textLrcFontSize;
                                break;
                        }
                    }
                }
            };
            menu.Items.Add(menuOK);
        }
        #endregion
        #region 进度条与音量条
        /// <summary>
        /// 进度条鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProgressPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(sldProcess);
            sldProcess.Value = (pt.X / sldProcess.ActualWidth) * sldProcess.Maximum;
            changingPosition = true;
        }
        /// <summary>
        /// 进度条值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProcessValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changingPosition)
            {
                Bass.BASS_ChannelSetPosition(stream, sldProcess.Value);
            }
        }
        /// <summary>
        /// 进度条鼠标松开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProcessPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            changingPosition = false;
        }
        /// <summary>
        /// 滚进度条鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldProcessMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (changingPosition)
            {
                //如果正在拖动进度条，那么更新歌词
                UpdatePositionOfLrcPanel();
            }
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
        #endregion
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
                Placement = PlacementMode.Top,
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
        /// <summary>
        /// 读取Mp3信息
        /// </summary>
        /// <param name="path"></param>
        private void ReadMp3(string path)//copy
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
                        }
                        catch
                        {
                            imgAlbum.Source = null;
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
        /// 单击专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            WinAlbumPicture win = new WinAlbumPicture(this);
            win.img.Source = imgAlbum.Source;
            win.ShowDialog();
        }


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
                    cbbItem.PreviewMouseLeftButtonUp += (o, e2) =>
                    {
                        PlayNew((int)cbbItem.Tag, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    };

                    cbbSearch.Items.Add(cbbItem);
                    cbbSearch.DropDownOpened += (o, e2) =>
                      {

                          txt.SelectionLength = 0;
                          txt.SelectionStart = (txt.Text.Length);
                      };
                   cbbSearch.IsDropDownOpen = true;

                }
            }

        }

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
                    if (IsInfoMatch(value,i))
                    {
                        PlayNew(index, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    }
                }
            }
        }

        private bool IsInfoMatch(string str,MusicInfo info)
        {
            str = str.ToLower();
            string musicName = (info.MusicName + new FileInfo(info.Path).Name).ToLower();
            if(musicName.Contains(str))
            {
                return true;
            }
            if (info.Singer.ToLower().Contains(str))
            {
                return true;
            }
            if(info.Album.ToLower().Contains(str))
            {
                return true;
            }

            if(ConvertChToPinYin(musicName).ToLower().Contains(str))
            {
                return true;
            }
            if (ConvertChToPinYin(info.Singer).ToLower().Contains(str))
            {
                return true;
            }

            var namePinYinTitle = new string(ConvertChToPinYin(musicName).Select(x => (x >= 'A' && x <= 'Z') ? x : ' ').Select(x => x).ToArray()).Replace(" ","");
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

        //定义拼音区编码数组
        private int[] wordCode = new int[]
            {
                -20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,
                -20032,-20026,-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,
                -19756,-19751,-19746,-19741,-19739,-19728,-19725,-19715,-19540,-19531,-19525,-19515,
                -19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263,-19261,-19249,
                -19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,
                -19003,-18996,-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,
                -18731,-18722,-18710,-18697,-18696,-18526,-18518,-18501,-18490,-18478,-18463,-18448,
                -18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, -18181,-18012,
                -17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,
                -17733,-17730,-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,
                -17468,-17454,-17433,-17427,-17417,-17202,-17185,-16983,-16970,-16942,-16915,-16733,
                -16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459,-16452,-16448,
                -16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,
                -16212,-16205,-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,
                -15933,-15920,-15915,-15903,-15889,-15878,-15707,-15701,-15681,-15667,-15661,-15659,
                -15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416,-15408,-15394,
                -15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,
                -15149,-15144,-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,
                -14941,-14937,-14933,-14930,-14929,-14928,-14926,-14922,-14921,-14914,-14908,-14902,
                -14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668,-14663,-14654,
                -14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,
                -14170,-14159,-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,
                -14109,-14099,-14097,-14094,-14092,-14090,-14087,-14083,-13917,-13914,-13910,-13907,
                -13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658,-13611,-13601,
                -13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,
                -13340,-13329,-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,
                -13068,-13063,-13060,-12888,-12875,-12871,-12860,-12858,-12852,-12849,-12838,-12831,
                -12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346,-12320,-12300,
                -12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,
                -11781,-11604,-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,
                -11055,-11052,-11045,-11041,-11038,-11024,-11020,-11019,-11018,-11014,-10838,-10832,
                -10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331,-10329,-10328,
                -10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254
            };
        //定义拼音数组
        private string[] pinYin = new string[]
            {
                "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
                "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Cai","Can",
                "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
                "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
                "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
                "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
                "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
                "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
                "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
                "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
                "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
                "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
                "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
                "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
                "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
                "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
                "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
                "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
                "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
                "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
                "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
                "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
                "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
                "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
                "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
                "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
                "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
                "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
                "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
                "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
                "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
                "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
                "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
           };
        //建立一个convertCh方法用于将汉字转换成全拼的拼音，其中，参数代表汉字字符串，此方法的返回值是转换后的拼音字符串

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
    }

}
