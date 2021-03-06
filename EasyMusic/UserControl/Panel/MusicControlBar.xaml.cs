﻿using EasyMusic.Enum;
using EasyMusic.Helper;
using EasyMusic.Info;
using FzLib.UI.Dialog;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;
using static EasyMusic.Helper.MusicListHelper;

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
            ppp = FindResource("ppp") as Popup;
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

        public bool IsManuallyChangingPosition => sldProgress.IsMouseOver && System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;

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
            PlayNext(true);
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
                    throw new Exception();
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "请选择音乐文件",
                Multiselect = false
            };
            dialog.Filters.Add(new CommonFileDialogFilter("支持的格式", GetExtensionFilter()));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileNames != null)
            {
                MusicInfo temp = await AddMusic(dialog.FileName);
                if (temp != null)
                {
                    PlayNew(temp);
                }
            }
        }

        public void UpdatePosition()
        {
            Notify(nameof(SliderPositionBinding), nameof(PositionText));
        }

        private Popup ppp;

        /// <summary>
        /// 单击切换播放设备按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeviceSwitchClickEventHandler(object sender, RoutedEventArgs e)
        {
            ppp.PlacementTarget = sender as FrameworkElement;
            ppp.IsOpen = true;
            ReLoadFx();
        }

        public void ReLoadFx()
        {
            (ppp.Child as MusicFxPopupContent).Load();
        }

        private ControlStatus lastStatus;

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PositionText"));
                }
            }
        }

        public string PositionText
        {
            get
            {
                var time = TimeSpan.FromSeconds(SliderPositionBinding);
                return $"{string.Format("{0:00}", (int)time.TotalMinutes)}:{string.Format("{0:00}", time.Seconds)}";
            }
        }

        private void sldProgress_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // if (sldProgress.IsMouseCaptured)
            // {
            //     double position = sldProgress.Value;
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