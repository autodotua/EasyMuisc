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

namespace EasyMuisc
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class MusicInfo
        {
            public string MusicName { get; internal set; }
            public string Singer { get; internal set; }
            public string Length { get; internal set; }
            public string Path { get; internal set; }
            public bool Enable { get; internal set; }


        }
        private enum CycleMode
        {
            SingleCycle,
            ListCycle,
            Shuffle,
        }
        double fps = 20;
        /// <summary>
        /// 音乐文件路径
        /// </summary>
        string path = "";
        /// <summary>
        /// 音乐播放句柄
        /// </summary>
        int stream = int.MinValue;
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
        int currentMusicIndex = 0;
        /// <summary>
        /// 是否正在拖动进度条
        /// </summary>
        bool changingPosition = false;
        /// <summary>
        /// 歌词列表
        /// </summary>
        List<double> lrcTime = new List<double>();
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcIndex = -1;
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
        int currentHistoryIndex = -1;
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
        DispatcherTimer playTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1000 / 60),
        };
        DispatcherTimer pauseTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1000 / 60),
        };
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //if(!File.Exists("bass.dll"))
            //{
            //    File.WriteAllBytes("bass.dll", Properties.Resources.bass);
            //}
            try
            {
                if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, new WindowInteropHelper(this).Handle))
                {

                    ShowAlert("无法初始化音乐引擎。");
                    Application.Current.Shutdown();

                }
            }
            catch
            {
                ShowAlert("无法初始化音乐引擎。");
                Application.Current.Shutdown();
            }
            musicInfo = new ObservableCollection<MusicInfo>();
            lvw.DataContext = musicInfo;

            playTimer.Tick += delegate
            {

                if (Volumn >= 1)
                {
                    playTimer.Stop();
                    return;
                }
                Volumn += 0.02;
            };
            pauseTimer.Tick += delegate
            {

                if (Volumn <= 0.02)
                {
                    Bass.BASS_ChannelPause(stream);
                    pauseTimer.Stop();
                    return;
                }
                Volumn -= 0.02;
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
               if( ShowAlert("配置文件被篡改，是否重置？", MessageBoxButton.YesNo))
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
                if (cfa.AppSettings.Settings["MusicList"] != null)
                {
                    string[] musics = cfa.AppSettings.Settings["MusicList"].Value.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var i in musics)
                    {

                        AddNewMusic(i);

                    }
                }
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
        private void AddNewMusic(string path)
        {
            Thread t = new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (var i in musicInfo)
                        {
                            if (path == i.Path)
                            {
                                return;
                            }
                        }
                        musicInfo.Add(new MusicInfo
                        {
                            Enable = GetMusicInfo(path, out string name, out string singer, out string length) ? true : false,
                            MusicName = name,
                            Singer = singer,
                            Length = length.StartsWith("00:") ? length.Remove(0, 3) : length,
                            Path = path,
                        });
                    }));

            });
            t.Start();
        }
        /// <summary>
        /// 获取音乐的信息
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="name">音乐的标题，若没有则返回无扩展名的文件名</param>
        /// <param name="singer">歌手</param>
        /// <param name="length">时长</param>
        private bool GetMusicInfo(string path, out string name, out string singer, out string length)
        {
            if (!File.Exists(path))
            {
                name = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
                singer = "";
                length = "";
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
        /// 单击播放按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (stream == int.MinValue)
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
                PlayNext();
            }
            else
            {
                PlayNew(history[++currentHistoryIndex]);
            }
        }
        /// <summary>
        /// 初始化新的歌曲
        /// </summary>
        private void InitialiazeMusic()
        {
            Stop();//停止正在播放的歌曲

            try
            {
                stream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);//获取歌曲句柄
                Title = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");//将窗体标题改为歌曲名
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
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / fps)
            };
            timer.Tick += Update;
            timer.Start();

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
                var lrc = new Lrc(file.FullName).LrcWord;//获取歌词信息
                int index = 0;//用于赋值Tag
                foreach (var i in lrc)
                {
                    if (i.Key > musicLength)//如果歌词文件有误，长度超过了歌曲的长度，那么超过部分就不管了
                    {
                        break;
                    }
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
                txtLrc.FontSize = textLrcFontSize;
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Visible;
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
                history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex);
            }

            switch (CurrentCycleMode)
            {
                case CycleMode.ListCycle:
                    PlayNew(currentMusicIndex == musicInfo.Count - 1 ? 0 : currentMusicIndex + 1);
                    break;
                case CycleMode.Shuffle:
                    Random r = new Random();
                    PlayNew(r.Next(musicInfo.Count));
                    break;
                case CycleMode.SingleCycle:
                    PlayNew(currentMusicIndex);
                    break;
            }
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
                for (int i = 0; i < lrcTime.Count ; i++)//从第一个循环到倒数第二个歌词时间
                {
                    if (lrcTime[i] < position )//如果当前的播放时间夹在两个歌词时间之间
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
            //svwLrc.ScrollToVerticalOffset();
            //DoubleAnimation ani = new DoubleAnimation
            //{
            //    To =lrcIndex * normalFontSize * FontFamily.LineSpacing + ActualHeight / 2
            //    Duration = new Duration(TimeSpan.FromSeconds(1)),//动画时间1秒
            //    DecelerationRatio = 0.5
            //};
            //Storyboard.SetTargetName(ani, svwLrc.Name);
            //Storyboard.SetTargetProperty(ani, new PropertyPath(ScrollViewer.));
            //Storyboard story = new Storyboard();
            //story.Children.Add(ani);
            //story.Begin(stkLrc);

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
        private bool PlayNew()
        {
            currentMusicIndex = lvw.SelectedIndex;
            return PlayNew(currentMusicIndex);
        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="index">指定列表中的歌曲索引</param>
        /// <returns></returns>
        private bool PlayNew(int index)
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
            currentLrcIndex = -1;//删除歌词索引
            lrcTime.Clear();//清空歌词时间
            stkLrc.Children.Clear();//清空歌词表
            lvw.SelectedIndex = index;//选中列表中的歌曲
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
            Volumn = 1;
            Play();
            return true;

        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="music">指定歌曲信息实例</param>
        /// <returns></returns>
        private bool PlayNew(MusicInfo music)
        {
            int index = musicInfo.IndexOf(music);
            if (index < 0 && index >= musicInfo.Count)
            {
                return false;
            }
            PlayNew(index);
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
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message"></param>

        private bool ShowAlert(string message, MessageBoxButton button = MessageBoxButton.OK)
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
                history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex);
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
        /// 窗体关闭事件，保存配置项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            Bass.BASS_Stop();
            cfa.Save();
        }
        /// <summary>
        /// 滚动条鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScbPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(sldProcess);
            sldProcess.Value = (pt.X / sldProcess.ActualWidth) * sldProcess.Maximum;
            changingPosition = true;
        }
        /// <summary>
        /// 滚动条值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changingPosition)
            {
                Bass.BASS_ChannelSetPosition(stream, sldProcess.Value);
            }
        }
        /// <summary>
        /// 滚动条鼠标松开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            changingPosition = false;
        }
        /// <summary>
        /// 滚动条鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (changingPosition)
            {
                //如果正在拖动进度条，那么更新歌词
                UpdatePositionOfLrcPanel();
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
                AddNewMusic(opd.FileName);
            }
        }

        private void BtnListOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnListOption,
                Placement = PlacementMode.Top,
                IsOpen = true
            };
            MenuItem openFile = new MenuItem()
            {
                Header = "文件",
                //Icon = new System.Windows.Controls.Image()
                //{
                //   Source= new BitmapImage(new Uri("Images\\icon.png", UriKind.RelativeOrAbsolute))
                //},
                //new System.Windows.Controls.Image() { Source = new BitmapImage(new Uri("Images\\add.png", UriKind.Relative)) },
            };
            openFile.Click += delegate
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
            MenuItem openFolder = new MenuItem()
            {
                Header = "文件夹",
            };
            openFolder.Click += delegate
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
            MenuItem openAllFolder = new MenuItem()
            {
                Header = "文件夹及子文件夹",
            };
            openAllFolder.Click += delegate
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
            MenuItem delete = new MenuItem()
            {
                Header = "删除选中项"
            };
            delete.Click += delegate
              {
                  musicInfo.RemoveAt(lvw.SelectedIndex);
              };
            MenuItem clear = new MenuItem()
            {
                Header = "清空列表",
            };
            clear.Click += delegate
              {
                  musicInfo.Clear();
              };
            menu.Items.Add(openFile);
            menu.Items.Add(openFolder);
            menu.Items.Add(openAllFolder);
            if (lvw.SelectedIndex != -1)
            {
                menu.Items.Add(delete);

            }
            menu.Items.Add(clear);
        }

        private void BtnAnimitionMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            DoubleAnimation ani = new DoubleAnimation
            {
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
            };
            Storyboard.SetTargetName(ani, btn.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(OpacityProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            story.Begin(btn);
        }

        private void BtnAnimitionMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            DoubleAnimation ani = new DoubleAnimation
            {
                To = 0.2,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
            };
            Storyboard.SetTargetName(ani, btn.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(OpacityProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            story.Begin(btn);
        }

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
            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += delegate
              {
                  for(int i=0;i<3;i++)
                  {
                      string text = ((menu.Items[i] as StackPanel).Children[1] as TextBox).Text;
                      if (double.TryParse(text, out double newValue))
                      {
                          switch(i)
                          {
                              case 0:normalLrcFontSize = newValue;break;
                              case 1: highlightLrcFontSize = newValue; break;
                              case 2: textLrcFontSize = newValue;txtLrc.FontSize = textLrcFontSize; break;
                          }
                      }
                  }
              };
            menu.Items.Add(menuOK);
        }
        private void btnListSwitcher_Click(object sender, RoutedEventArgs e)
        {
                ThicknessAnimation ani = new ThicknessAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                    DecelerationRatio = 0.3
                };
                Storyboard.SetTargetName(ani, grdList.Name);
                Storyboard.SetTargetProperty(ani, new PropertyPath(MarginProperty));
                Storyboard story = new Storyboard();
                
                story.Children.Add(ani);
                if (grdList.Margin.Left < 0)
                {
                    ani.To = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    ani.To = new Thickness(-lvw.ActualWidth, 0, 0, 0);
                }
                story.Begin(grdList);
            

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            double marginLeft = grdList.Margin.Left;
              
                if (ActualWidth < 500 && marginLeft==0)
                {
                    btnListSwitcher_Click(null, null);
                }
                else if (ActualWidth >= 500 && marginLeft==-lvw.ActualWidth)
                {
                    btnListSwitcher_Click(null, null);
                }
            }
        }
    
}
