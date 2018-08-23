using System;
using System.Windows;
using System.Windows.Input;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using EasyMusic.Info;
using EasyMusic.Enum;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// MusicControlBar.xaml 的交互逻辑
    /// </summary>
    public partial class MusicControlBar : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public MusicControlBar()
        {
            InitializeComponent();
            //DataContext = this;
        }

        public CycleMode CycleModeButtonVisibility =>
            MusicControlHelper.CycleMode;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(params string[] properties)
        {
            foreach (var item in properties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
            }
        }
        public bool IsManuallyChangingPosition => sldProcess.IsMouseOver && System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;
        public double SliderMaxBinding
        {
            get
            {
                if (Music != null)
                {
                    return Music.Length;
                }
                return 0;
            }
        }

        public double SliderVolumnBinding
        {
            get => Math.Sqrt(Setting.Volumn);
            set => Volumn = value * value;
        }

        /// <summary>
        /// 单击播放按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayClickEventHandler(object sender, RoutedEventArgs e)
        {
            PlayOrPlayNew();

        }
        /// <summary>
        /// 单击暂停按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPauseClickEventHandler(object sender, RoutedEventArgs e)
        {
            Music.Pause();
        }
        /// <summary>
        /// 单击上一首按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLastClickEventHandler(object sender, RoutedEventArgs e)
        {
            PlayLast();
        }
        /// <summary>
        /// 单击下一首按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextClickEventHandler(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }
        /// <summary>
        /// 单击循环模式按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCycleModeClickEventHandler(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine((sender as FrameworkElement).Name);
            //将三个按钮的事件放到了一起
            switch ((sender as FrameworkElement).Name)
            {
                case "btnListCycle":
                    MusicControlHelper.CycleMode = Enum.CycleMode.SingleCycle;
                    break;
                case "btnSingleCycle":
                    MusicControlHelper.CycleMode = Enum.CycleMode.Shuffle;
                    break;
                case "btnShuffle":
                    MusicControlHelper.CycleMode = Enum.CycleMode.ListCycle;
                    break;
                default:
                    ShowError("黑人问号");
                    break;
            }
            Notify("CycleModeButtonVisibility");

        }
        /// <summary>
        /// 单击打开文件按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnOpenFileClickEventHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog()
            {
                Title = "请选择音乐文件。",
                Filter = "MP3文件(*.mp3)|*.mp3|WAVE文件(*.wav)|*.wav|所有文件(*.*) | *.*",
                Multiselect = false
            };
            if (opd.ShowDialog() == true && opd.FileNames != null)
            {
                MusicInfo temp = await AddMusic(opd.FileName);
                if (temp != null)
                {
                    PlayNew(temp);
                }

            }
        }

        public void UpdatePosition(double position)
        {
            Notify("SliderPositionBinding", "PostionText");
        }

        /// <summary>
        /// 单击切换播放设备按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeviceSwitchClickEventHandler(object sender, RoutedEventArgs e)
        {
            Popup ppp = FindResource("ppp") as Popup;
            ppp.PlacementTarget = sender as FrameworkElement;
            ppp.IsOpen = true;
            (ppp.Child as MusicFxPopupContent).Load();

        }
        ControlStatus lastStatus;
        public void OnStatusChanged(ControlStatus status)
        {
            if (status != lastStatus)
            {
                switch (status)
                {
                    case ControlStatus.Play:
                        ShowStatusChangeAnimation(status);
                        break;
                    case ControlStatus.Pause:
                        ShowStatusChangeAnimation(status);
                        break;
                }
            }
            if (status != ControlStatus.Initialized)
            {
                lastStatus = status;
            }
            Notify("SliderMaxBinding");
            if (CurrentHistoryIndex == 0)
            {
                btnLast.IsEnabled = false;
            }
            else if (!btnLast.IsEnabled)
            {
                btnLast.IsEnabled = true;
            }
        }
        private void ShowStatusChangeAnimation(ControlStatus status)
        {
            ControlButton btn1 = null;
            ControlButton btn2 = null;
            double angle = 0;
            if (status == ControlStatus.Play)
            {
                btn1 = btnPlay;
                btn2 = btnPause;
                angle = 360;
            }
            else if (status == ControlStatus.Pause)
            {
                btn1 = btnPause;
                btn2 = btnPlay;
                angle = -360;
            }

            btn1.IsEnabled = btn2.IsEnabled = false;
            btn2.Opacity = 0;
            btn2.Visibility = Visibility.Visible;

            DoubleAnimation aniBtn1Rotate = new DoubleAnimation(angle, Setting.VolumnChangeTime);
            DoubleAnimation aniBtn2Rotate = new DoubleAnimation(angle, Setting.VolumnChangeTime);
            DoubleAnimation aniBtn1Opacity = new DoubleAnimation(0, Setting.VolumnChangeTime);
            DoubleAnimation aniBtn2Opacity = new DoubleAnimation(1, Setting.VolumnChangeTime);

            aniBtn1Rotate.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            aniBtn2Rotate.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard.SetTarget(aniBtn1Rotate, btn1);
            Storyboard.SetTarget(aniBtn2Rotate, btn2);
            Storyboard.SetTarget(aniBtn1Opacity, btn1);
            Storyboard.SetTarget(aniBtn2Opacity, btn2);

            Storyboard.SetTargetProperty(aniBtn1Rotate, new PropertyPath("RenderTransform.Angle"));
            Storyboard.SetTargetProperty(aniBtn2Rotate, new PropertyPath("RenderTransform.Angle"));
            Storyboard.SetTargetProperty(aniBtn1Opacity, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(aniBtn2Opacity, new PropertyPath(OpacityProperty));

            Storyboard storyboard = new Storyboard() { FillBehavior = FillBehavior.Stop, Children = { aniBtn1Opacity, aniBtn2Opacity, aniBtn1Rotate, aniBtn2Rotate } };
            storyboard.Completed += (p1, p2) =>
              {
                  btn1.Visibility = Visibility.Hidden;
                  btn2.Visibility = Visibility.Visible;
                  btn1.IsEnabled = btn2.IsEnabled = true;
                  btn1.Opacity = btn2.Opacity = 1;
                  (btn1.RenderTransform as RotateTransform).Angle = 0;
                  (btn2.RenderTransform as RotateTransform).Angle = 0;
              };
            storyboard.Begin();
        }

        public double SliderPositionBinding
        {
            get
            {
                if (Music != null)
                {
                    return Music.Position;
                }
                return 0;
            }
            set
            {
                if (Music != null)
                {
                    Music.Position = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PostionText"));

                }
            }
        }

        public string PostionText
        {
            get
            {
                var time = TimeSpan.FromSeconds(SliderPositionBinding);
                return $"{string.Format("{0:00}", (int)time.TotalMinutes)}:{string.Format("{0:00}", time.Seconds)}";
            }
        }

        private void sldProcess_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // if (sldProcess.IsMouseCaptured)
            // {
            //     double position = sldProcess.Value;
            //     if (IsManuallyChangingPosition)
            //     {
            //         Music.Position = position;
            //         MainWindow.Current.UpdateLyric(position);
            //     }
            // }
        }

        private void BarLoaded(object sender, RoutedEventArgs e)
        {
            CheckListEmpty();
        }
    }
}
