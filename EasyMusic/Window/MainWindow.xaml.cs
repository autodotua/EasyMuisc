using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Un4seen.Bass;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Linq;
using System.Windows.Shell;
using EasyMusic.Windows;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static FzLib.Control.Dialog.DialogHelper;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using FzLib.Basic;
using EasyMusic.Info;
using System.Collections.Generic;
using System.Threading.Tasks;

using EasyMusic.Enum;
using System.ComponentModel;
using System.Windows.Controls;

namespace EasyMusic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        #region 属性
        /// <summary>
        /// 当前窗体实例
        /// </summary>
        public static MainWindow Current { get; private set; }
        //private FloatLyrics floatLyric = new FloatLyrics();
        /// <summary>
        /// 悬浮歌词
        /// </summary>
        public FloatLyrics FloatLyric { get; set; }= new FloatLyrics();
        public IntPtr Handle { get; private set; }
        /// <summary>
        /// 是否显示加载动画
        /// </summary>
        public bool LoadingSpinner
        {
            set
            {
                loading.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
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

        /// <summary>
        /// 主定时器
        /// </summary>
        DispatcherTimer UiTimer { get; set; }

        #endregion

        #region 初始化和配置



        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            Current = this;
            InitializeComponent();
            DefaultDialogOwner = this;


            if (Setting.MaxWindow)
            {
                WindowState = WindowState.Maximized;
                grdMain.Margin = new Thickness(8);
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;

                Top = Setting.Top;
                Left = Setting.Left;
                Width = Setting.Width;
                Height = Setting.Height;

            }
            Handle = new WindowInteropHelper(this).EnsureHandle();
            if (!Un4seen.Bass.AddOn.Fx.BassFx.LoadMe() || !Bass.BASS_Init(-1, Setting.SampleRate/*无设置界面*/, BASSInit.BASS_DEVICE_DEFAULT, Handle))
            {
                ShowError("无法初始化音乐引擎：" + Environment.NewLine + Bass.BASS_ErrorGetCode());
                Application.Current.Shutdown();
            }

            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });

            //UpdateColor(Setting.BackgroundColor);

            header.HeaderTextMaxWidth = SystemParameters.WorkArea.Width - 200;


           //Initialized+=(p1,p2) =>
           //WindowHelper.RepairWindowBehavior(this); 
        }


        /// <summary>
        /// 初始化定时器事件
        /// </summary>
        private void InitializeField()
        {
            UiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / Setting.UpdateSpeed) };
            UiTimer.Tick += UpdateTick;
            StringBuilder str = new StringBuilder();
            foreach (var i in supportExtension)
            {
                str.Append($"*{i}|");
            }
            str.Remove(str.Length - 1, 1);
            supportExtensionWithSplit = str.ToString();
            if (Setting.ShowFloatLyric)
            {
                FloatLyric.Show();
            }
        }
        /// <summary>
        /// 窗体载入事件，获取音乐列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowLoadedEventHandler(object sender, RoutedEventArgs e)
        {
            InitializeTray();

            HotKeyHelper.RegistGolbalHotKey();

            InitializeField();
            InitializeAnimation();



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

            Topmost = Setting.Topmost;

        }


        public string Cpu { get; private set; } = "       ";
        private bool enableCpu = false;


        /// <summary>
        /// 托盘图标
        /// </summary>
        private void InitializeTray()
        {
            trayIcon = new FzLib.Program.Notify.TrayIcon(Properties.Resources.icon, Properties.Resources.AppName);

            trayIcon.MouseLeftClick += (p1, p2) =>
            {
                if (Visibility != Visibility.Visible)
                {
                    Show();
                    Activate();
                }
                else
                {
                    Hide();
                }
            };

            //trayIcon.AddContextMenu("悬浮歌词开关", () =>
            //{
            //    Setting.ShowFloatLyric = !Setting.ShowFloatLyric;
            //    FloatLyric.Visibility = FloatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            //    FloatLyric.Update(lrc.CurrentIndex);
            //});
            trayIcon.AddContextMenuItem("退出", Close);

            if (Setting.ShowTray)
            {
                trayIcon.Show();
            }
        }

  

        #endregion

        #region 窗体相关

        public bool SkipSavingSettings { get; set; } = false;
        bool appShutingDown = false;
        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0);
            if (SkipSavingSettings)
            {
                return;
            }
            listenHistory.RecordEnd();

            HotKeyHelper.SaveHotKeys();
            SaveListToFile(lvwMusic.lastMusicListBtn.Text, false);
            Setting.CycleMode = MusicControlHelper.CycleMode;
      
            Setting.LastMusicList = lvwMusic.lastMusicListBtn.Text;
            Setting.Top = Top;
            Setting.Left = Left;
            Setting.Height = Height;
            Setting.Width = Width;
            Setting.MaxWindow = (WindowState == WindowState.Maximized);

            if (Setting.ShowFloatLyric)
            {
                FloatLyric.Close();
            }
            Setting.Save();
            //if (Setting.TrayMode==0 || Setting.TrayMode == 3 || appShutingDown)
            //{
                if (Music != null)
                {
                    if (Music.Status == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                        Music.Pause();
                        await Task.Delay(Setting.VolumnChangeTime);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                    Music.Dispose();
                }
                App.Current.Shutdown();
            //}
            //else
            //{
            //    await Task.Delay(100);
            //    Hide();
            //}
        }
        /// <summary>
        /// 窗体状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinMainStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
               BorderThickness = new Thickness();
                grdMain.Margin = new Thickness(8);
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                BorderThickness = new Thickness(16);
                grdMain.Margin = new Thickness(4);
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(16);
            }
        }
        #endregion

        #region 音乐相关
        public void OnStatusChanged(ControlStatus status)
        {
            switch (status)
            {
                case ControlStatus.Play:
                    tbiPlay.Visibility = Visibility.Collapsed;
                    tbiPause.Visibility = Visibility.Visible;
                    if (!UiTimer.IsEnabled)
                    {
                        UiTimer.Start();
                    }
                    break;
                case ControlStatus.Pause:
                    tbiPause.Visibility = Visibility.Collapsed;
                    tbiPlay.Visibility = Visibility.Visible;
                    break;
            }
            controlBar.OnStatusChanged(status);
        }

        /// <summary>
        /// 初始化歌词
        /// </summary>
        /// <param name="musicLength"></param>
        public void InitializeLrc()
        {
            try
            {
                lrc = null;
                if (Setting.ShowFloatLyric)
                {
                    FloatLyric.SetFontEffect();
                }
                FileInfo file = new FileInfo(Music.FilePath);
                file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
                if (file.Exists)//判断是否存在歌词文件
                {
                    lyricArea.CurrentLyricType = LyricType.LrcFormat;
                    lrc = new LyricInfo(file.FullName);//获取歌词信息
                    lrc.Offset /= 1000.0;

                    lyricArea.AddLrcs();
                    if (Setting.ShowFloatLyric)
                    {
                        FloatLyric.Reload(lrc.LrcContent.Values.ToList());
                        FloatLyric.Visibility = Visibility.Visible;
                    }
                }
                else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
                {
                    lyricArea.LoadTextFormatLyric(File.ReadAllText(file.FullName, FzLib.Basic.String.GetEncoding(file.FullName)));
                    lyricArea.CurrentLyricType = LyricType.TextFormat;
                    if (Setting.ShowFloatLyric)
                    {
                        FloatLyric.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    lyricArea.CurrentLyricType = LyricType.None;
                    if (Setting.ShowFloatLyric)
                    {
                        FloatLyric.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("初始化歌词失败！", ex);
            }
        }
        /// <summary>
        /// 在即将播放新歌曲时初始化界面
        /// </summary>
        public void InitializeNewMusic()
        {
            lvwMusic.SelectAndScroll(Music.Info);//选中列表中的歌曲
            InitializeLrc();
            controlBar.ReLoadFx();
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

        #endregion

        #region 鼠标滚轮
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            if (!lvwMusic.IsMouseOver && !lyricArea.IsMouseOver)
            {
                Volumn += e.Delta / 2000.0;
            }
        }

        #endregion

        #region 列表相关
        /// <summary>
        /// 执行清空列表后的清理工作
        /// </summary>
        public void AfterClearList()
        {
            Music = null;
            lyricArea.HideAll();
            if (Setting.ShowFloatLyric)
            {
                FloatLyric.Clear();
            }
            Title = EasyMusic.Properties.Resources.AppName;
            header.HeaderText = EasyMusic.Properties.Resources.AppName;
            header.AlbumImageSource = null;
            controlBar.OnStatusChanged(ControlStatus.Pause);
        }
        /// <summary>
        /// 单击收放列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListSwitcherClickEventHandler(object sender, RoutedEventArgs e)
        {
            ChangeMusicListVisibility();
        }

        public void ChangeMusicListVisibility()
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

        #region 歌词相关
        /// <summary>
        /// 展开歌词区域
        /// </summary>
        public void ExpandLyricsArea()
        {
            Setting.ShowLrc = true;
            grdLyricArea.Visibility = Visibility.Visible;
            grdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            grdMain.ColumnDefinitions[0].Width = GridLength.Auto;

            NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);
        }
        /// <summary>
        /// 收缩歌词区域
        /// </summary>
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
            if (Setting.ShowFloatLyric)
            {
                FloatLyric.Hide();
            }
            Setting.ShowFloatLyric = !Setting.ShowFloatLyric;
            if (Setting.ShowFloatLyric)
            {
               
                FloatLyric.Show();
                if (Music != null)
                {
                    //InitializeLrc(); 
                    FloatLyric.Reload(lrc.LrcContent.Values.ToList(), lrc.CurrentIndex);
                }
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

        #region 动画和定时器
        /// <summary>
        /// 歌词故事板
        /// </summary>
        Storyboard storyLrc = new Storyboard();
        /// <summary>
        /// 歌词动画
        /// </summary>
        ThicknessAnimation aniLrc = new ThicknessAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.8)), DecelerationRatio = 0.5 };

        public event PropertyChangedEventHandler PropertyChanged;

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
            if (enableCpu)
            {
                Cpu = Bass.BASS_GetCPU().ToString("0.00") + "%";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cpu"));
            }
            if (Music == null || Music.Status != BASSActive.BASS_ACTIVE_PLAYING)
            {
                UiTimer.Stop();
                return;
            }
            if (!controlBar.IsManuallyChangingPosition)
            {
                double position = Music.Position;
                controlBar.UpdatePosition(position);

                UpdateLyric(position);
                if (Music.IsEnd)
                {
                    PlayNext();
                }
            }
        }
        /// <summary>
        /// 更新当前时间的歌词
        /// </summary>
        public void UpdateLyric(double position)
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
                    FloatLyric.Update(lrc.CurrentIndex);
                }
            }
        }

        #endregion

        private void Label_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            enableCpu = !enableCpu;
            (sender as Label).Opacity = enableCpu ? 1 : 0;
        }
    }

}
