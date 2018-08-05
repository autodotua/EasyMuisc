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
using static EasyMusic.GlobalDatas;
using EasyMusic.Tools;
using static EasyMusic.Helper.MusicListHelper;
using static WpfControls.Dialog.DialogHelper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using WpfControls.Dialog;
using System.Windows.Media;
using EasyMusic.Helper;
using WpfCodes.Basic;
using static EasyMusic.Helper.MusicControlHelper;

namespace EasyMusic
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 到某一条歌词一共有多少行
        /// </summary>
        List<int> lrcLineSumToIndex = new List<int>();
        /// <summary>
        /// 歌词对象
        /// </summary>
        LyricInfo lrc;
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcTime = 0;
        /// <summary>
        /// 歌词故事板
        /// </summary>
        Storyboard storyLrc = new Storyboard();
        /// <summary>
        /// 歌词动画
        /// </summary>
        ThicknessAnimation aniLrc = new ThicknessAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.8)), DecelerationRatio = 0.5 };

        /// <summary>
        /// 初始化歌词动画
        /// </summary>
        private void InitializeAnimation()
        {
            if (Setting.LrcAnimation)
            {
                //Storyboard.SetTargetName(aniLrc, stkLrc.Name);
                Storyboard.SetTargetProperty(aniLrc, new PropertyPath(MarginProperty));
                storyLrc.Children.Add(aniLrc);
            }
            AnimationFps = -1;
        }
        /// <summary>
        /// 定时更新各项数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTick(object sender, EventArgs e)
        {
            if (Music.Status!=BASSActive.BASS_ACTIVE_PLAYING)
            {
                mainTimer.Stop();
                return;
            }
            if (!controlBar.isManuallyChangingPosition)
            {
                double position = Music.Position;
                controlBar.UpdatePosition(position);

                UpdatePosition(position);
                if (Music.Status != BASSActive.BASS_ACTIVE_STOPPED)
                {
                    PlayNext();
                }
            }
        }
        /// <summary>
        /// 更新当前时间的歌词
        /// </summary>
        public void UpdatePosition(double position)
        {
            if (lrc.LrcContent.Count == 0)
            {
                return;
            }
            bool changed = false;//是否
            if (position == 0 && lrc.CurrentIndex != 0)//如果还没播放并且没有更新位置
            {
                changed = true;
                lrc.CurrentIndex = 0;
            }
            else
            {
                for (int i = 0; i < lrc.LrcContent.Count - 1; i++)//从第一个循环到最后一个歌词时间
                {
                    if (lrc.LrcContent.Keys.ElementAt(i+1) > position + lrc.Offset + Setting.LrcDefautOffset)//如果下一条歌词的时间比当前时间要后面（因为增序判断所以这一条歌词时间肯定小于的）
                    {
                        if (lrc.CurrentIndex != i)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            lrc.CurrentIndex = i;
                        }
                        break;
                    }
                    else if (i == lrc.LrcContent.Count - 2 && lrc.LrcContent.Keys.ElementAt(i+1) < position + lrc.Offset + Setting.LrcDefautOffset)
                    {
                        if (lrc.CurrentIndex != i + 1)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            lrc.CurrentIndex = i + 1;
                        }
                        break;
                    }
                }
            }

            if (changed)
            {
                lbxLrc.RefreshFontSize(lrc.CurrentIndex);
                lbxLrc.ScrollTo(lrc.CurrentIndex, lrcLineSumToIndex, Setting.NormalLrcFontSize);
                if (Setting.ShowFloatLyric)
                {
                    floatLyric.Update(lrc.CurrentIndex);
                }
            }



        }
    }
}
