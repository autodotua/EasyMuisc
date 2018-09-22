using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;
using static Un4seen.Bass.Bass;

namespace EasyMusic.UserControls
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
        /// 刷新界面
        /// </summary>
        public void Load()
        {
            stkDevices.Children.Clear();
            if (BASS_GetDeviceCount() - 2 <= 0)
            {
                return;
            }

            var devices = BASS_GetDeviceInfos();

            int n = -1;
            foreach (var device in devices)
            {
                if (++n == 0)
                {
                    continue;
                }
                if (Device == n)
                {
                    continue;
                }

                Button btn = new Button
                {
                    Content = device.name,
                    Style = Resources["btnStyleNormal"] as Style,
                };
                int value = n;
                btn.Click += (p1, p2) =>
                {
                    Device = value;
                    BtnCloseEventHandler(null, null);
                };
                stkDevices.Children.Add(btn);
            }
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pitch"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tempo"));
            Pitch = Pitch;
            Tempo = Tempo;

        }
        private void ScrollViewerPreviewMouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {

            var scv = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scv.LineLeft();
            }
            else
            {
                scv.LineRight();
            }
        }

        ///// <summary>
        ///// 单击关闭按钮事件
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        private void BtnCloseEventHandler(object sender, RoutedEventArgs e)
        {
            (Parent as Popup).IsOpen = false;
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

    }
}
