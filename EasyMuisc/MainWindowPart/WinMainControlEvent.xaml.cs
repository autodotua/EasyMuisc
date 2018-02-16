using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using EasyMuisc.Windows;
using static EasyMuisc.Tools.Tools;
using System.Windows.Media;
using System.Windows.Shell;
using EasyMuisc.UserControls;
using Microsoft.Win32;
using Un4seen.Bass;
using System.Windows.Controls.Primitives;
using static EasyMuisc.ShareStaticResources;

namespace EasyMuisc
{
    public partial class MainWindow : Window
    {

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
            ppp.Placement = PlacementMode.Center;
            ppp.IsOpen = true;
            (ppp.Child as MusicFxPopupContent).Refresh();

        }
        /// <summary>
        /// 单击播放设备菜单项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
      
        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup ppp = Resources["pppMusicSettings"] as Popup;
            ppp.IsOpen = false;
        }

        #endregion
    }
}
