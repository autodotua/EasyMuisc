using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Un4seen.Bass;
using static EasyMusic.Tool.Tools;
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

namespace EasyMusic.UserControl
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
        public bool IsManuallyChangingPosition => sldProcess.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed;
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
            get => Setting.Volumn;
            set
            {
                if (Music != null)
                {
                    Volumn = value;
                }
            }
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
                    MusicControlHelper.CycleMode = CycleMode.SingleCycle;
                    break;
                case "btnSingleCycle":
                    MusicControlHelper.CycleMode = CycleMode.Shuffle;
                    break;
                case "btnShuffle":
                    MusicControlHelper.CycleMode = CycleMode.ListCycle;
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

        public void OnStatusChanged(ControlStatus status)
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
            if (sldProcess.IsMouseCaptured)
            {
                double position = sldProcess.Value;
                if (IsManuallyChangingPosition)
                {
                    Music.Position = position;
                    //Debug.WriteLine("change");
                    //MainWindow.Current.UpdatePosition(position);
                }
            }
        }


    }
}
