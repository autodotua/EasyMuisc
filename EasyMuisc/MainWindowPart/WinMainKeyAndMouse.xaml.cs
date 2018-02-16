using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Un4seen.Bass;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Text.RegularExpressions;
using EasyMuisc.Windows;
using static EasyMuisc.Tools.Tools;
using EasyMuisc.Tools;
using static EasyMuisc.ShareStaticResources;

namespace EasyMuisc
{
    public partial class MainWindow : Window
    {


        #region 快捷键
        /// <summary>
        /// 执行前进快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyFowardEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            //BtnNextClickEventHandler(null, null);
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) + 4;
            if (position > musicLength)
            {
                position = musicLength;
            }
            Bass.BASS_ChannelSetPosition(stream, position);
        }
        /// <summary>
        /// 执行后退快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyBackEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) - 4;
            if (position < 0)
            {
                position = 0;
            }
            Bass.BASS_ChannelSetPosition(stream, position);
        }
        /// <summary>
        /// 执行播放暂停快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyPlayAndPauseEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (btnPause.Visibility == Visibility.Visible)
            {
                BtnPauseClickEventHandler(null, null);
            }
            else
            {
                BtnPlayClickEventHandler(null, null);
            }
        }
        /// <summary>
        /// 执行下一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyNextEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            BtnNextClickEventHandler(null, null);
        }
        /// <summary>
        /// 执行上一曲快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotKeyLastEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            BtnLastClickEventHandler(null, null);
        }
        /// <summary>
        /// 在文本歌词和列表按空格同样有效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtLrcAndLvwPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HotKeyPlayAndPauseEventHandler(null, null);
            }
        }
        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegistGolbalHotKey()
        {
            HotKey next = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Right);
            HotKey last = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Left);
            next.OnHotKey += () => BtnNextClickEventHandler(null, null);
            last.OnHotKey += () => BtnLastClickEventHandler(null, null);
            HotKey up = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Up);
            HotKey down = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Down);
            up.OnHotKey += () => sldVolumn.Value += 0.05;
            down.OnHotKey += () => sldVolumn.Value -= 0.05;
            HotKey playAndPause = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.OemQuestion);
            playAndPause.OnHotKey += () => HotKeyPlayAndPauseEventHandler(null, null);

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
            if (mouseInLrcArea && (stkLrc.Visibility == Visibility.Visible || lbxLrc.Visibility == Visibility.Visible))
            {
                if (e.Delta > 0)
                {
                    offset -= 1d / 4;
                }
                else
                {
                    offset += 1d / 4;
                }
                ShowInfo("当前歌词偏移量：" + (offset > 0 ? "+" : "") + Math.Round(offset, 2).ToString() + "秒");
            }
            else if (!(mouseInList || mouseInLrcArea))
            {
                if (e.Delta > 0)
                {
                    sldVolumn.Value += 0.05;
                }
                else
                {
                    sldVolumn.Value -= 0.05;
                }
            }
        }
        /// <summary>
        /// 鼠标是否在歌词区域
        /// </summary>
        bool mouseInLrcArea = false;
        /// <summary>
        /// 鼠标进入歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            mouseInLrcArea = true;
        }
        /// <summary>
        /// 鼠标离开歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            mouseInLrcArea = false;
        }
        /// <summary>
        /// 鼠标是否在列表上
        /// </summary>
        bool mouseInList = false;
        /// <summary>
        /// 鼠标进入列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            mouseInList = true;
        }
        /// <summary>
        /// 鼠标离开列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            mouseInList = false;
        }
        #endregion

    }
}
