using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;
using System.ComponentModel;

namespace EasyMusic.UserControl
{
    /// <summary>
    /// MusicFxPopupContent.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFxPopupContent : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public MusicFxPopupContent()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 刷新界面（因为这个东西不是对话框，创建出来以后就不会变了
        /// </summary>
        public void Load()
        {
            //    sldPitch.Value = Music.Pitch;
            //    sldTempo.Value = Music.Tempo;

            //    stkDevices.Children.Clear();


            //    if (Bass.BASS_GetDeviceCount() - 2 <= 0)
            //    {
            //        return;
            //    }

            //    var devices = Bass.BASS_GetDeviceInfos();
            //    for (int i = 1; i < devices.Length; i++)
            //    {
            //        Bass.BASS_Init(i, Setting.SampleRate, BASSInit.BASS_DEVICE_DEFAULT, windowHandle);
            //    }
            //    int n = -1;
            //    foreach (var i in devices)
            //    {
            //        n++;
            //        if (n == 0 ||n==Bass.BASS_GetDevice() || n == Bass.BASS_ChannelGetDevice(stream))
            //        {
            //            Bass.BASS_SetDevice(n);
            //            continue;
            //        }
            //        Button btn = new Button
            //        {
            //            Content = i.name,
            //            Tag = n,
            //            Style = Resources["btnStyleNormal"] as Style,
            //        };
            //        btn.Click += BtnDeviceClickEventHandler;
            //        stkDevices.Children.Add(btn);
            //    }
        }
        private void ScrollViewerPreviewMouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
        }
        //    var scv = sender as ScrollViewer;
        //    if (e.Delta > 0)
        //    {
        //        scv.LineLeft();
        //    }
        //    else
        //    {
        //        scv.LineRight();
        //    }
        //}
        ///// <summary>
        ///// 单击设备选择按钮
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void BtnDeviceClickEventHandler(object sender, RoutedEventArgs e)
        //{
        //    int device = int.Parse((sender as Button).Tag.ToString());

        //    if (Bass.BASS_ChannelSetDevice(stream, device)
        //    && Bass.BASS_SetDevice(device))
        //    {
        //        Bass.BASS_ChannelPause(stream);
        //        Bass.BASS_ChannelPlay(stream, false);
        //    }
        //    else
        //    {
        //        ShowError(Bass.BASS_ErrorGetCode().ToString());
        //    }
        //    BtnCloseEventHandler(null, null);
        //}
        ///// <summary>
        ///// 单击关闭按钮事件
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        private void BtnCloseEventHandler(object sender, RoutedEventArgs e)
        {
            (Parent as Popup).IsOpen = false;
        }
        ///// <summary>
        ///// 音调条改变事件
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void SldPitchValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    Pitch = (int)sldPitch.Value;
        //    if ((int)sldPitch.Value == 0)
        //    {
        //        txtPitch.Text = "±0";
        //    }
        //    if ((int)sldPitch.Value > 0)
        //    {
        //        txtPitch.Text = "+" + (int)sldPitch.Value;
        //    }
        //    else
        //    {
        //        txtPitch.Text = ((int)sldPitch.Value).ToString();
        //    }
        //}
        /// <summary>
        /// 速度条改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldTempoValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Tempo = (int)sldTempo.Value;
            //if ((int)sldTempo.Value == 0)
            //{
            //    txtTempo.Text = "0%";
            //}
            //if ((int)sldTempo.Value > 0)
            //{
            //    txtTempo.Text = "+" + (int)sldTempo.Value + "%";
            //}
            //else
            //{
            //    txtTempo.Text = (int)sldTempo.Value + "%";
            //}
        }
        /// <summary>
        /// 单击恢复初始化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRecoverClickEventHandler(object sender, RoutedEventArgs e)
        {
            sldPitch.Value = sldTempo.Value = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag as string)
            {
                case "1":
                    Pitch--;
                    break;
                case "2":
                    Pitch++;
                    break;
                case "3":
                    sldTempo.Value--;
                    break;
                case "4":
                    sldTempo.Value++;
                    break;
            }


        }
        public string PitchText { get; set; } = "±0";

        public int Pitch
        {
            get
            {
                if (Music == null)
                {
                    if (Setting.KeepMusicSettings)
                    {
                        return Setting.Pitch;
                    }
                    return 0;
                }
                return Music.Pitch;
            }
            set
            {
                if (Music != null)
                {
                    Music.Pitch = value;
                }
                PitchText = Setting.Pitch == 0 ? "±0" : ((Setting.Pitch > 0 ? "+" : "") + Setting.Pitch.ToString());

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PitchText"));
            }
        }

        public string TempoText { get; private set; } = "100%";

        public int Tempo
        {
            get
            {
                if (Music == null)
                {
                    if (Setting.KeepMusicSettings)
                    {
                        return Setting.Tempo;
                    }
                    return 0;
                }
                return Music.Tempo;
            }
            set
            {
                if (Music != null)
                {
                    Music.Tempo = value;
                }
                TempoText = (Setting.Tempo + 100).ToString() + "%";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempoText"));
            }
        }

        private void sldPitch_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            //   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pitch"));
            //
            //   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempoText"));
            //   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tempo"));

        }
    }
}
