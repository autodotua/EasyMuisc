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
using EasyMusic.Windows;
using static EasyMusic.Tools.Tools;
using EasyMusic.Tools;
using static EasyMusic.GlobalDatas;
using static WpfControls.Dialog.DialogHelper;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using WpfCodes.WindowsApi;

namespace EasyMusic
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

        HotKey hotKey;

        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegistGolbalHotKey()
        {

            if(hotKey!=null)
            {
                hotKey.Dispose();
            }
           hotKey = new HotKey();
            Dictionary<string, HotKey.HotKeyInfo> hotKeys=null;

            try
            {
                string json = File.ReadAllText(Setting.ConfigPath + "\\HotKeyConfig.json");

                hotKeys= Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, HotKey.HotKeyInfo>>(json);
            }
            catch
            {
                hotKeys = new Dictionary<string, HotKey.HotKeyInfo>()
                {
                    {"下一曲",new HotKey.HotKeyInfo(Key.Right,ModifierKeys.Control) },
                    {"上一曲",new HotKey.HotKeyInfo(Key.Left,ModifierKeys.Control) },
                    {"音量加",new HotKey.HotKeyInfo(Key.Up,ModifierKeys.Control) },
                    {"音量减",new HotKey.HotKeyInfo(Key.Down,ModifierKeys.Control) },
                    {"播放暂停",new HotKey.HotKeyInfo(Key.OemQuestion,ModifierKeys.Control) },
                    {"悬浮歌词",new HotKey.HotKeyInfo(Key.OemPeriod,ModifierKeys.Control) },
                    {"收放列表",new HotKey.HotKeyInfo(Key.OemComma,ModifierKeys.Control) },
                };
            }

            string error = "";

            hotKey.KeyPressed += (p1, p2) =>
              {
                  if(hotKeys.ContainsValue(p2.HotKey))
                  {
                      string command = hotKeys.First(p => p.Value.Equals(p2.HotKey)).Key;
                      switch (command)
                      {
                          case "下一曲":
                              BtnNextClickEventHandler(null, null);
                              break;
                          case "上一曲":
                              BtnLastClickEventHandler(null, null);
                              break;
                          case "音量加":
                              sldVolumn.Value += 0.05;
                              break;
                          case "音量减":
                              sldVolumn.Value -= 0.05;
                              break;
                          case "播放暂停":
                              HotKeyPlayAndPauseEventHandler(null, null);
                              break;
                          case "悬浮歌词":
                              BtnListSwitcherClickEventHandler(null, null);
                              break;
                          case "收放列表":
                              OpenOrCloseFloatLrc();
                              break;
                      }

                  }
              };

            foreach (var key in hotKeys)
            {
                try
                {
                    hotKey.Register(key.Value);
                }
                catch
                {
                    error += key.Key + "（"+key.Value.ToString()+"）";
                }
            }

            //try
            //{
            //    hotKey.Register(Key.Right, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "（下一曲）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Left, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ←（上一曲）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Up, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ↑（音量加）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.Down, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ↓（音量减）、";

            //}
            //try
            //{
            //    hotKey.Register(Key.OemQuestion, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl /（播放暂停）、";
            //}
            //try
            //{
            //    hotKey.Register(Key.OemComma, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl ,（收放列表）、";
            //}
            //try
            //{
            //    hotKey.Register(Key.OemPeriod, ModifierKeys.Control);
            //}
            //catch
            //{
            //    error += "Ctrl .（开关悬浮歌词）、";
            //}
            if (error != "")
            {
                trayIcon.ShowMessage("以下热键无法注册，可能已被占用：" + error.TrimEnd('、'));
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
