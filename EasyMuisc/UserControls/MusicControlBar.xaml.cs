using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Un4seen.Bass;
using static EasyMusic.Tools.Tools;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// MusicControlBar.xaml 的交互逻辑
    /// </summary>
    public partial class MusicControlBar : UserControl
    {
        public MusicControlBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Visibility ListCycleVisibility => MusicControlHelper.CycleMode == CycleMode.ListCycle ? Visibility.Visible : Visibility.Hidden;
        public Visibility SingleCycleVisibility => MusicControlHelper.CycleMode == CycleMode.SingleCycle ? Visibility.Visible : Visibility.Hidden;
        public Visibility ShuffleVisibility => MusicControlHelper.CycleMode == CycleMode.Shuffle ? Visibility.Visible : Visibility.Hidden;


        #region 进度条与音量条

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
                MusicControlHelper.Music.Position = position;
                //Debug.WriteLine("change");
                MainWindow.Current.UpdatePosition();
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
        //private void SldVolumnValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    MainWindow.Current. Volumn = sldVolumn.Value;
        //    btnDevice.Opacity = 0.2 + 0.6 * sldVolumn.Value;
        //}
        #endregion

        public bool isManuallyChangingPosition => sldProcess.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed;

        #region 播放控制事件
        /// <summary>
        /// 单击播放按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayClickEventHandler(object sender, RoutedEventArgs e)
        {
            MusicControlHelper.PlayOrPlayNew();

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
                    ShowError("黑人问号");
                    break;
            }

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
        #endregion
        public void UpdatePosition(double position)
        {
            sldProcess.Value = position;
        }






        #region 音频硬件
        /// <summary>
        /// 单击切换播放设备按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeviceSwitchClickEventHandler(object sender, RoutedEventArgs e)
        {
            Popup ppp = Resources["pppMusicSettings"] as Popup;
            ppp.PlacementTarget = sender as FrameworkElement;
            ppp.Placement = PlacementMode.Bottom;
            ppp.IsOpen = true;
            (ppp.Child as MusicFxPopupContent).Load();

        }

        public void ResetControls(ControlStatus status)
        {
            switch (status)
            {
                case ControlStatus.Play:
                    btnPlay.Visibility = Visibility.Hidden;
                    btnPause.Visibility = Visibility.Visible;

                    break;
                case ControlStatus.Pause:
                    btnPause.Visibility = Visibility.Hidden;
                    btnPlay.Visibility = Visibility.Visible;
                    break;
            }

        }



        #endregion

    }
}
