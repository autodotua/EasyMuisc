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
using static EasyMuisc.ShareStaticResources;
using EasyMuisc.Tools;

namespace EasyMuisc
{
    public partial class MainWindow : Window
    {
        #region 播放控制
        /// <summary>
        /// 当前音乐在列表中的索引
        /// </summary>
        public int currentMusicIndex = 0;
        /// <summary>
        /// 历史记录
        /// </summary>
        private List<MusicInfo> history = new List<MusicInfo>();
        /// <summary>
        /// 当前播放历史索引
        /// </summary>
        int currentHistoryIndex = -1;
        /// <summary>
        /// 歌曲时长
        /// </summary>
        double musicLength;

        /// <summary>
        /// 初始化新的歌曲
        /// </summary>
        private void InitialiazeMusic()
        {
            Stop();//停止正在播放的歌曲
            try
            {
                var tempStream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_STREAM_DECODE);//获取歌曲句柄
                                                                                                     // var decoder = Bass.BASS_StreamCreateFile(path, 0, 0,BASSFlag. BASS_STREAM_DECODE); // create a "decoding channel" from a file
                stream = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_TempoCreate(tempStream, BASSFlag.BASS_FX_FREESOURCE); // create a tempo stream for it
                                                                                                                    //Bass.  BASS_ChannelSetAttribute(tempostream, BASS_ATTRIB_TEMPO_PITCH, pitch); // set the pitch
                                                                                                                    //  BASS_ChannelPlay(tempostream, FALSE); // start playing
                if (set.MusicSettings)
                {
                    Pitch = set.Pitch;
                    Tempo = set.Tempo;
                }

                Volumn = sldVolumn.Value;
                txtMusicName.Text = new FileInfo(path).Name.Replace(new FileInfo(path).Extension, "");
                Title = txtMusicName.Text + " - EasyMusic";//将窗体标题改为歌曲名
                string[] length = musicInfo[currentMusicIndex].Length.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                musicLength = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                sldProcess.Maximum = musicLength;
                InitialiazeLrc();
            }
            catch (Exception ex)
            {
                ShowAlert("初始化失败!" + Environment.NewLine + ex.ToString());
                return;
            }
            ReadMusicSourceInfo(musicInfo[currentMusicIndex].Path);

            mainTimer.Tick += UpdateTick;
            mainTimer.Start();

        }
        /// <summary>
        /// 初始化歌词
        /// </summary>
        /// <param name="musicLength"></param>
        private void InitialiazeLrc()
        {
            lrcLineSumToIndex.Clear();
            lrcTime.Clear();//清空歌词时间
            lrcContent.Clear();//清除歌词内容
            currentLrcIndex = -1;//删除歌词索引
            stkLrc.Children.Clear();//清空歌词表
            lbxLrc.Clear();

            FileInfo file = new FileInfo(path);
            file = new FileInfo(file.FullName.Replace(file.Extension, ".lrc"));
            if (file.Exists)//判断是否存在歌词文件
            {
                grdLrc.Visibility = Visibility.Visible;
                txtLrc.Visibility = Visibility.Hidden;
                if (set.UseListBoxLrcInsteadOfStackPanel)
                {
                    lbxLrc.Visibility = Visibility.Visible;
                    stkLrc.Visibility = Visibility.Hidden;

                }
                else
                {
                    stkLrc.Visibility = Visibility.Visible;
                    lbxLrc.Visibility = Visibility.Hidden;
                }
                lrc = new Lyric(file.FullName);//获取歌词信息
                if (!double.TryParse(lrc.Offset, out offset))
                {
                    offset = 0;
                }
                offset /= 1000.0;
                int index = 0;//用于赋值Tag
                foreach (var i in lrc.LrcContent)
                {
                    //if (i.Key > musicLength)//如果歌词文件有误，长度超过了歌曲的长度，那么超过部分就不管了
                    //{
                    //    break;
                    //}
                    //lbxLrc.Add(i.Value,index.ToString(),(p1,p2) =>
                    //{
                    //    var position = lrcTime[int.Parse(((p1 as FrameworkElement).Tag).ToString())] - offset - LrcDefautOffset;
                    //    Bass.BASS_ChannelSetPosition(stream, position > 0 ? position : 0);
                    //    });

                    lrcContent.Add(i.Value);
                    var tbk = new TextBlock()
                    {
                        Name = "tbk" + index.ToString(),
                        FontSize = set.NormalLrcFontSize,
                        Text = i.Value,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = index++,//标签用于定位
                        Cursor = Cursors.Hand,
                        TextAlignment = TextAlignment.Center,
                        FocusVisualStyle = null,
                    };
                    tbk.MouseLeftButtonUp += (p1, p2) =>
                    {
                        //单击歌词跳转到当前歌词
                        Bass.BASS_ChannelSetPosition(stream, (lrcTime[(int)tbk.Tag] - offset - set.LrcDefautOffset) > 0 ? lrcTime[(int)tbk.Tag] - offset - set.LrcDefautOffset : 0);
                    };
                    if (set.UseListBoxLrcInsteadOfStackPanel)
                    {
                        lbxLrc.Add(tbk);
                    }
                    else
                    {
                        stkLrc.Children.Add(tbk);
                        //stkLrc.
                    }
                    lrcTime.Add(i.Key);
                }
                //lbxLrc.Add(lrc.LrcContent);
                //lbxLrc.ChangeFontSize(normalLrcFontSize);
                foreach (var i in lrc.LineIndex)
                {
                    lrcLineSumToIndex.Add(i.Value);
                }
                floatLyric.ReLoadLrc(lrcContent);
            }
            else if ((file = new FileInfo(file.FullName.Replace(file.Extension, ".txt"))).Exists)
            {

                txtLrc.Text = File.ReadAllText(file.FullName, EncodingType.GetType(file.FullName));
                for (int i = 0; i < txtLrc.LineCount; i++)
                {
                    lrcContent.Add(txtLrc.GetLineText(i));
                }
                txtLrc.FontSize = set.TextLrcFontSize;
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Visible;
                stkLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;

            }
            else
            {
                grdLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                stkLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;
                if (set.ShowFloatLyric)
                {
                    floatLyric.Clear();
                }
            }
        }
        /// <summary>
        /// 根据不同的播放循环模式播放下一首
        /// </summary>
        private void PlayNext()
        {
            if (currentHistoryIndex < history.Count - 1)
            {
                history.RemoveRange(currentHistoryIndex + 1, history.Count - currentHistoryIndex - 1);
            }

            switch (CurrentCycleMode)
            {
                case CycleMode.ListCycle:
                    PlayListNext();
                    break;
                case CycleMode.Shuffle:
                    int index;
                    while ((index = GetRandomNumber(0, musicInfo.Count)) == currentMusicIndex)
                        ;
                    PlayNew(index);
                    break;
                case CycleMode.SingleCycle:
                    PlayNew(currentMusicIndex);
                    break;
            }
        }
        /// <summary>
        /// 播放列表中的下一首歌
        /// </summary>
        private void PlayListNext()
        {
            PlayNew(currentMusicIndex == musicInfo.Count - 1 ? 0 : currentMusicIndex + 1);
        }
        /// <summary>
        /// （暂停后）播放
        /// </summary>
        /// <returns></returns>
        private void Play()
        {
            tbiPlay.Visibility = Visibility.Collapsed;
            tbiPause.Visibility = Visibility.Visible;
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
            if (pauseTimer.IsEnabled)
            {
                pauseTimer.Stop();
            }
            playTimer.Start();
        
            Bass.BASS_ChannelPlay(stream, false);


        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <returns></returns>
        private bool PlayNew(bool playAtOnce = true)
        {
            currentMusicIndex = lvw.SelectedIndex;
            return PlayNew(currentMusicIndex, playAtOnce);
        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="index">指定列表中的歌曲索引</param>
        /// <returns></returns>
        private bool PlayNew(int index, bool playAtOnce = true)
        {
            if (!File.Exists(musicInfo[index].Path))
            {
                if (ShowAlert("文件不存在！是否从列表中删除？", MessageBoxButton.YesNo))
                {
                    musicInfo.RemoveAt(index);
                }
                return false;
            }
            currentMusicIndex = index;//指定当前的索引
            path = musicInfo[currentMusicIndex].Path;//获取歌曲地址
            lvw.SelectedIndex = index;//选中列表中的歌曲
            lvw.ScrollIntoView(lvw.SelectedItem);
            if (currentHistoryIndex == history.Count - 1)
            {
                if (currentHistoryIndex == -1)
                {
                    history.Add(musicInfo[currentMusicIndex]);//加入历史记录
                    currentHistoryIndex++;
                }
                else
                if (history[currentHistoryIndex] != musicInfo[currentMusicIndex])
                {
                    history.Add(musicInfo[currentMusicIndex]);//加入历史记录
                    currentHistoryIndex++;
                }
            }
            //Debug.WriteLine(currentHistoryIndex);
            InitialiazeMusic();//初始化歌曲
          
            if (playAtOnce)
            {
                Play();
            }
            if (WindowState == WindowState.Normal)
            {
                Width += 0.01;
                Width -= 0.01;
            }
            return true;

        }
        /// <summary>
        /// 播放新的歌曲
        /// </summary>
        /// <param name="music">指定歌曲信息实例</param>
        /// <returns></returns>
        private bool PlayNew(MusicInfo music, bool playAtOnce = true)
        {
            int index = musicInfo.IndexOf(music);
            if (index < 0 && index >= musicInfo.Count)
            {
                return false;
            }
            PlayNew(index, playAtOnce);
            return true;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        private void Pause()
        {

            tbiPause.Visibility = Visibility.Collapsed;
            tbiPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
            btnPlay.Visibility = Visibility.Visible;
            if (playTimer.IsEnabled)
            {
                playTimer.Stop();
            }
            pauseTimer.Start();
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        private bool Stop()
        {
            return Bass.BASS_StreamFree(stream);
        }
        #endregion

        #region 定时更新
        /// <summary>
        /// 歌词列表
        /// </summary>
        List<double> lrcTime = new List<double>();
        /// <summary>
        /// 歌词内容
        /// </summary>
        List<string> lrcContent = new List<string>();
        /// <summary>
        /// 到某一条歌词一共有多少行
        /// </summary>
        List<int> lrcLineSumToIndex = new List<int>();
        /// <summary>
        /// 歌词对象
        /// </summary>
        Lyric lrc;
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        int currentLrcIndex = 0;
        /// <summary>
        /// 歌词时间偏移量
        /// </summary>
        double offset;
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
            if (set.LrcAnimation)
            {
                Storyboard.SetTargetName(aniLrc, stkLrc.Name);
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
            if (stream == 0)
            {
                mainTimer.Stop();
                return;
            }
            if (!changingPosition)
            {
                double currentPosition = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
                sldProcess.Value = currentPosition;
                if (Bass.BASS_ChannelGetPosition(stream) == Bass.BASS_ChannelGetLength(stream))
                {
                    //如果一首歌放完了
                    PlayNext();
                }
                UpdatePosition();
            }
        }
        /// <summary>
        /// 更新当前时间的歌词
        /// </summary>
        private void UpdatePosition()
        {
            if (lrcTime.Count == 0)
            {
                return;
            }
            double position = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));//获取当前播放的位置
            bool changed = false;//是否
            if (position == 0 && currentLrcIndex != 0)//如果还没播放并且没有更新位置
            {
                changed = true;
                currentLrcIndex = 0;
            }
            else
            {
                for (int i = 0; i < lrcTime.Count - 1; i++)//从第一个循环到最后一个歌词时间
                {
                    if (lrcTime[i + 1] > position + offset + set.LrcDefautOffset)//如果下一条歌词的时间比当前时间要后面（因为增序判断所以这一条歌词时间肯定小于的）
                    {
                        if (currentLrcIndex != i)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            currentLrcIndex = i;
                        }
                        break;
                    }
                    else if (i == lrcTime.Count - 2 && lrcTime[i + 1] < position + offset + set.LrcDefautOffset)
                    {
                        if (currentLrcIndex != i + 1)//如果上一次不是这一句歌词
                        {
                            changed = true;
                            currentLrcIndex = i + 1;
                        }
                        break;
                    }
                }
            }

            if (changed)
            {

                if (set.UseListBoxLrcInsteadOfStackPanel)
                {
                    lbxLrc.RefreshFontSize(currentLrcIndex);
                    lbxLrc.ScrollTo(currentLrcIndex, lrcLineSumToIndex, set.NormalLrcFontSize);
                }
                else
                {
                    foreach (var i in stkLrc.Children)
                    {
                        //首先把所有的歌词都改为正常大小
                        (i as TextBlock).FontSize = set.NormalLrcFontSize;
                    }
                    (stkLrc.Children[currentLrcIndex] as TextBlock).FontSize = set.HighlightLrcFontSize;//当前歌词改为高亮
                    StackPanelLrcAnimition(currentLrcIndex);//歌词转变动画
                }

                if (set.ShowFloatLyric)
                {
                    floatLyric.Update(currentLrcIndex);
                }
            }



        }
        /// <summary>
        /// 歌词转变动画
        /// </summary>
        /// <param name="lrcIndex"></param>
        private void StackPanelLrcAnimition(int lrcIndex)
        {
            double top = 0.5 * ActualHeight - lrcLineSumToIndex[lrcIndex]/*第一行到当前行的总行数*/ * set.NormalLrcFontSize * FontFamily.LineSpacing/*歌词数量乘每行字的高度*/ - set.HighlightLrcFontSize;// 0.5 * ActualHeight - stkLrcHeight * lrcIndex / (stkLrc.Children.Count - 1)-highlightFontSize ;


            //Storyboard storyboard = new Storyboard();
            //TranslateTransform translateTransform = new TranslateTransform(0, top);
            ////ScaleTransform scale = new ScaleTransform(1.0, 1.0, 1, 1);
            //stkLrc.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            //TransformGroup myTransGroup = new TransformGroup();
            //myTransGroup.Children.Add(translateTransform);
            //stkLrc.RenderTransform = myTransGroup;

            //DoubleAnimation growAnimation = new DoubleAnimation();
            //growAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            ////growAnimation.From = 1;
            //growAnimation.To = 1.1;
            //storyboard.Children.Add(growAnimation);

            //DependencyProperty[] propertyChain = new DependencyProperty[]
            //{
            //Button.RenderTransformProperty,
            //TransformGroup.ChildrenProperty,
            //TranslateTransform.YProperty
            //};
            //string thePath = "(0).(1)[0].(2)";
            //PropertyPath myPropertyPath = new PropertyPath(thePath, propertyChain);
            //Storyboard.SetTargetProperty(growAnimation, myPropertyPath);
            //Storyboard.SetTarget(growAnimation, stkLrc);

            //storyboard.Begin();









            ////DoubleAnimationUsingKeyFrames ani = new DoubleAnimationUsingKeyFrames();
            ////LinearDoubleKeyFrame first = new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0));

            ////LinearDoubleKeyFrame second = new LinearDoubleKeyFrame(10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)));
            ////ani.KeyFrames.Add(first);
            ////ani.KeyFrames.Add(second);
            ////stkLrc.RenderTransform .BeginAnimation(RenderTransform., ani);

            //return;


            //Canvas.SetTop(stkLrc, top);
            // NewDoubleAnimation(stkLrc, Canvas.LeftProperty, top, 0.8, 0.5);
            //return;

            //DoubleAnimation ani = new DoubleAnimation()
            //{
            //    Duration = TimeSpan.FromMilliseconds(500),
            //    To = top,
            //};
            ////  Storyboard.SetTargetProperty(aniLrc, new PropertyPath("(ListView.RenderTransform).(TranslateTransform.Y)"));
            //Storyboard.SetTargetProperty(ani, new PropertyPath("(StackPanel.RenderTransform).(TranslateTransform.X)"));
            //Storyboard.SetTarget(ani, stkLrc);
            //Storyboard st = new Storyboard();
            //st.Children.Add(ani);
            //st.Begin(stkLrc);
            //return;

            if (set.LrcAnimation)
            {
                storyLrc.Stop(stkLrc);
                aniLrc.To = new Thickness(0, top, 0, 0);
                storyLrc.Begin(stkLrc);
            }
            else
            {
                stkLrc.Margin = new Thickness(0, top, 0, 0);
            }
        }
        #endregion
    }
}
