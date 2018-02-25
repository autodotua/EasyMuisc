﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static EasyMuisc.ShareStaticResources;
using Un4seen.Bass;
using System.Diagnostics;
using System.Windows.Interop;
using static EasyMuisc.Tools.Tools;
using static Dialog.DialogHelper;
namespace EasyMuisc.UserControls
{
    /// <summary>
    /// MusicFxPopupContent.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFxPopupContent : UserControl
    {
        public MusicFxPopupContent()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 刷新界面（因为这个东西不是对话框，创建出来以后就不会变了
        /// </summary>
        public void Refresh()
        {
            sldPitch.Value = Pitch;
            sldTempo.Value = Tempo;

            stkDevices.Children.Clear();


            if (Bass.BASS_GetDeviceCount() - 2 <= 0)
            {
                return;
            }

            var devices = Bass.BASS_GetDeviceInfos();
            for (int i = 1; i < devices.Length; i++)
            {
                Bass.BASS_Init(i, set.SampleRate, BASSInit.BASS_DEVICE_DEFAULT, windowHandle);
            }
            int n = -1;
            foreach (var i in devices)
            {
                n++;
                if (n == 0 || n == Bass.BASS_ChannelGetDevice(stream))
                {
                    Bass.BASS_SetDevice(n);
                    continue;
                }
                Button btn = new Button
                {
                    Content = i.name,
                    Tag = n,
                };
                btn.Click += BtnDeviceClickEventHandler;
                stkDevices.Children.Add(btn);
            }
        }
        /// <summary>
        /// 单击设备选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeviceClickEventHandler(object sender, RoutedEventArgs e)
        {
            int device = int.Parse((sender as Button).Tag.ToString());

            if (!Bass.BASS_ChannelSetDevice(stream, device)
            || !Bass.BASS_SetDevice(device))
            {
                ShowError(Bass.BASS_ErrorGetCode().ToString());
            }
            BtnCloseEventHandler(null, null);
        }
        /// <summary>
        /// 单击关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseEventHandler(object sender, RoutedEventArgs e)
        {
            (Parent as Popup).IsOpen = false;
        }
        /// <summary>
        /// 音调条改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldPitchValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Pitch = (int)sldPitch.Value;
            if ((int)sldPitch.Value == 0)
            {
                txtPitch.Text = "±0";
            }
            if ((int)sldPitch.Value > 0)
            {
                txtPitch.Text = "+" + (int)sldPitch.Value;
            }
            else
            {
                txtPitch.Text = ((int)sldPitch.Value).ToString();
            }
        }
        /// <summary>
        /// 速度条改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldTempoValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Tempo = (int)sldTempo.Value;
            if ((int)sldTempo.Value == 0)
            {
                txtTempo.Text = "0%";
            }
            if ((int)sldTempo.Value > 0)
            {
                txtTempo.Text = "+" + (int)sldTempo.Value + "%";
            }
            else
            {
                txtTempo.Text = (int)sldTempo.Value + "%";
            }
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
    }
}
