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
using static EasyMuisc.GlobalDatas;
using static WpfControls.Dialog.DialogHelper;
using System.Speech.Recognition;
using System.Speech.Synthesis;

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
        public void HotKeyPlayAndPauseEventHandler(object sender, ExecutedRoutedEventArgs e)
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
                WinMain.HotKeyPlayAndPauseEventHandler(null, null);
            }
        }
        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegistGolbalHotKey()
        {
            string error = "";
            try
            {
                HotKey next = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Right);
                next.OnHotKey += () => BtnNextClickEventHandler(null, null);
            }
            catch (Exception ex)
            {
                error += "Ctrl →（下一曲）、";

            }
            try
            {
                HotKey last = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Left);

                last.OnHotKey += () => BtnLastClickEventHandler(null, null);
            }
            catch (Exception ex)
            {
                error += "Ctrl ←（上一曲）、";

            }
            try
            {
                HotKey up = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Up);
                up.OnHotKey += () => sldVolumn.Value += 0.05;

            }
            catch (Exception ex)
            {
                error += "Ctrl ↑（音量加）、";

            }
            try
            {
                HotKey down = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Down);
                down.OnHotKey += () => sldVolumn.Value -= 0.05;
            }
            catch (Exception ex)
            {
                error += "Ctrl ↓（音量减）、";

            }
            try
            {
                HotKey playAndPause = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.OemQuestion);
                playAndPause.OnHotKey += () => HotKeyPlayAndPauseEventHandler(null, null);
            }
            catch (Exception ex)
            {
                error += "Ctrl /（播放暂停）、";
            }
            try
            {
                HotKey playAndPause = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.Oemcomma);
                playAndPause.OnHotKey += () => BtnListSwitcherClickEventHandler(null, null);
            }
            catch (Exception ex)
            {
                error += "Ctrl ,（收放列表）、";
            }
            try
            {
                HotKey playAndPause = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, System.Windows.Forms.Keys.OemPeriod);
                playAndPause.OnHotKey += () => OpenOrCloseFloatLrc();
            }
            catch (Exception ex)
            {
                error += "Ctrl .（开关悬浮歌词）、";
            }
            if (error != "")
            {
                trayIcon.ShowMessage("以下热键无法注册，可能已被占用：" + error.TrimEnd(new char[] { '、' }));
            }
        }
        #endregion

        #region 语音识别

        private void RegistSpeechRecognition()
        {
            return;
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(System.Globalization.CultureInfo.CurrentCulture);

            //----------------
            //初始化命令词
            Choices conmmonds = new Choices();
            //添加命令词
            conmmonds.Add(new string[] { "播放", "暂停", "轻一点", "响一点", "多轻一点", "多响一点" });
            //初始化命令词管理
            GrammarBuilder gBuilder = new GrammarBuilder();
            //将命令词添加到管理中
            gBuilder.Append(conmmonds);
            //实例化命令词管理
            Grammar grammar = new Grammar(gBuilder);
            //-----------------

            //创建并加载听写语法(添加命令词汇识别的比较精准)
            recognizer.LoadGrammar(grammar);
            //为语音识别事件添加处理程序。
            recognizer.SpeechRecognized += RecognizerSpeechRecognizedEventHandler;
            //将输入配置到语音识别器。
            recognizer.SetInputToDefaultAudioDevice();
            //启动异步，连续语音识别。
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            sy.SelectVoiceByHints(VoiceGender.Neutral);


        }
        SpeechSynthesizer sy = new SpeechSynthesizer();
        private void RecognizerSpeechRecognizedEventHandler(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "暂停":
                    if (btnPause.Visibility == Visibility.Visible)
                    {
                        BtnPauseClickEventHandler(null, null);
                    }
                    break;
                case "播放":
                    if (btnPlay.Visibility == Visibility.Visible)
                    {
                        BtnPlayClickEventHandler(null, null);
                    }
                    break;
                case "轻一点":
                    sldVolumn.Value -= 0.05;
                    sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
                    break;
                case "响一点":
                    sldVolumn.Value += 0.05;
                    sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
                    break;
                case "多轻一点":
                    sldVolumn.Value -= 0.2;
                    sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
                    break;
                case "多响一点":
                    sldVolumn.Value += 0.2;
                    sy.SpeakAsync("百分之" + (int)(sldVolumn.Value * 100));
                    break;
            }
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
            //    if (mouseInLrcArea && (stkLrc.Visibility == Visibility.Visible || lbxLrc.Visibility == Visibility.Visible))
            //    {
            //        if (e.Delta > 0)
            //        {
            //            offset -= 1d / 4;
            //        }
            //        else
            //        {
            //            offset += 1d / 4;
            //        }
            //        ShowInfo("当前歌词偏移量：" + (offset > 0 ? "+" : "") + Math.Round(offset, 2).ToString() + "秒");
            //    }
            //    else 
            if (mouseInLrcArea && lbxLrc.Visibility == Visibility.Visible)
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
            else if (!mouseInLrcArea && !mouseInMusicListArea)
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
        bool mouseInMusicListArea = false;
        /// <summary>
        /// 鼠标进入歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            if ((sender as FrameworkElement).Name == "grdList")
            {
                mouseInMusicListArea = true;
            }
            else
            {
                mouseInLrcArea = true;
            }
        }
        /// <summary>
        /// 鼠标离开歌词区域事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLrcAreaMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            if ((sender as FrameworkElement).Name == "grdList")
            {
                mouseInMusicListArea = false;
            }
            else
            {
                mouseInLrcArea = false;
            }
        }

        #endregion

    }
}
