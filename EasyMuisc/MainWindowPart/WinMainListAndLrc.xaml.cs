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
using static EasyMuisc.MusicHelper;

namespace EasyMuisc
{
    public partial class MainWindow : Window
    {

        #region 列表相关

        /// <summary>
        /// 单击收放列表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListSwitcherClickEventHandler(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation aniMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
            };
            DoubleAnimation aniOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),//动画时间1秒
                DecelerationRatio = 0.3
            };
            Storyboard.SetTargetName(aniMargin, grdList.Name);
            Storyboard.SetTargetProperty(aniMargin, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(OpacityProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(aniMargin);
            story.Children.Add(aniOpacity);

            if (grdList.Margin.Left < 0)
            {
                aniMargin.To = new Thickness(0, 0, 0, 0);
                aniOpacity.To = 1;
                set.ShrinkMusicListManually = false;
            }
            else
            {
                aniMargin.To = new Thickness(-musicList.ActualWidth, 0, 0, 0);
                aniOpacity.To = 0;
                set.ShrinkMusicListManually = true;
            }
            story.Begin(grdList);

        }
        /// <summary>
        /// 窗体大小改变事件，自动收放列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            lbxLrc.RefreshPlaceholder(grdLrcArea.ActualHeight / 2, set.HighlightLrcFontSize);

            if (!set.AutoFurl || !set.ShowLrc || set.ShrinkMusicListManually)
            {
                return;
            }
            double marginLeft = grdList.Margin.Left;

            if (ActualWidth < 500 && marginLeft == 0)
            {
                BtnListSwitcherClickEventHandler(null, null);
            }
            else if (ActualWidth >= 500 && marginLeft == -musicList.ActualWidth)
            {
                BtnListSwitcherClickEventHandler(null, null);
            }

        }
        /// <summary>
        /// 将文件拖到列表上方事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowDragEnterEventHandler(object sender, DragEventArgs e)
        {
     
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            
        }

        private void WindowDropEventHandler(object sender, DragEventArgs e)
        {


            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files == null)
            {
                return;
            }
            if (files.Length == 1)
            {
                int currentCount = MusicCount;
                string extension = new FileInfo(files[0]).Extension;
                if (supportExtension.Contains(extension))
                {
                    AddMusic(files[0]);
                    if (MusicCount > currentCount)
                    {
                        PlayNew(currentCount);
                    }
                }
            }
            else
            {
                List<string> musics = new List<string>();
                foreach (var i in files)
                {
                    FileInfo file = new FileInfo(i);
                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        foreach (var j in EnumerateFiles(i, supportExtensionWithSplit, SearchOption.AllDirectories))
                        {
                            if (!musics.Contains(j))
                            {
                                musics.Add(j);
                            }
                        }
                    }
                    else
                    {
                        string extension = file.Extension;
                        if (supportExtension.Contains(extension))
                        {
                            if (!musics.Contains(i))
                            {
                                musics.Add(i);
                            }
                        }
                    }
                }
                AddMusic(musics.ToArray());
            }
        }
        #endregion



        #region 列表与歌词选项
        /// <summary>
        /// 单击列表选项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnListOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = sender as UIElement,

                IsOpen = true,
                // Style=Resources["ctmStyle"] as Style
            };


            MenuItem menuOpenFile = new MenuItem() { Header = "文件" };
            menuOpenFile.Click += (p1, p2) =>
            {
                OpenFileDialog opd = new OpenFileDialog()
                {
                    Title = "请选择音乐文件。",
                    Filter = "MP3文件(*.mp3)|*.mp3|WAVE文件(*.wav)|*.wav|所有文件(*.*) | *.*",
                    Multiselect = true
                };
                if (opd.ShowDialog() == true && opd.FileNames != null)
                {
                    List<string> musics = new List<string>();
                    foreach (var i in opd.FileNames)
                    {
                        musics.Add(i);
                    }
                    if (musics.Count >= 1)
                    {
                        AddMusic(musics.ToArray());
                    }
                }
            };
            MenuItem menuOpenFolder = new MenuItem() { Header = "文件夹" };
            menuOpenFolder.Click += (p1, p2) =>
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。"
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> musics = new List<string>();

                    foreach (var i in EnumerateFiles(fbd.SelectedPath, supportExtensionWithSplit, SearchOption.TopDirectoryOnly))
                    {
                        musics.Add(i);
                    }

                    if (musics.Count >= 1)
                    {
                        AddMusic(musics.ToArray());
                    }
                }
            };

            MenuItem menuOpenAllFolder = new MenuItem() { Header = "文件夹及子文件夹" };
            menuOpenAllFolder.Click += (p1, p2) =>
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "请选择包含音乐文件的文件夹。",
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> musics = new List<string>();

                    foreach (var i in EnumerateFiles(fbd.SelectedPath, supportExtensionWithSplit, SearchOption.AllDirectories))
                    {
                        musics.Add(i);
                    }

                    if (musics.Count >= 1)
                    {
                        AddMusic(musics.ToArray());
                    }
                }
            };

            MenuItem menuOpenMusicFolder = new MenuItem() { Header = "打开所在文件夹" };
            menuOpenMusicFolder.Click += (p1, p2) =>
  Process.Start("Explorer.exe", @"/select," + GetMusic(musicList.SelectedIndex).Path);

            MenuItem menuShowMusicInfo = new MenuItem() { Header = "显示音乐信息" };
            menuShowMusicInfo.Click += (p1, p2) =>
            {
                MusicInfo music = GetMusic(musicList.SelectedIndex);
                FileInfo fileInfo = new FileInfo(music.Path);
                string l = Environment.NewLine;
                string info = fileInfo.Name + l
                + music.Path + l
                + Math.Round(fileInfo.Length / 1024d) + "KB" + l
                + music.MusicName + l
                + music.Length + l
                + music.Singer + l
                + music.Album;
                WinMusicInfo winMusicInfo = new WinMusicInfo() { Title = fileInfo.Name + "-音乐信息" };
                winMusicInfo.txt.Text = info;
                winMusicInfo.ShowDialog();
            };
            MenuItem menuPlayNext = new MenuItem() { Header = "下一首播放" };
            menuPlayNext.Click += (p1, p2) =>
            {
                if (CurrentHistoryIndex < HistoryCount - 1)
                {
                    RemoveHistory(CurrentHistoryIndex + 1, HistoryCount - CurrentHistoryIndex - 1);
                }
               AddHistory(musicList.SelectedItem);
            };
            MenuItem menuDelete = new MenuItem() { Header = "删除选中项" };
            menuDelete.Click += (p1, p2) =>
            {
                int needDeleteIndex = musicList.SelectedIndex;

                if (CurrentMusicIndex == needDeleteIndex)
                {
                    PlayListNext();
                }
                RemoveMusic(needDeleteIndex);
                if (MusicCount== 0)
                {
                    AfterClearList();
                }
            };

            MenuItem menuClear = new MenuItem() { Header = "清空列表", };
            menuClear.Click += (p1, p2) =>
            {
                RemoveMusic();
                AfterClearList();
            };
            MenuItem menuClearExceptCurrent = new MenuItem() { Header = "删除其他", };
            menuClearExceptCurrent.Click += (p1, p2) =>
            {
                int index = musicList.SelectedIndex;
                for (int i = 0; i < index; i++)
                {
                    RemoveMusic(0);
                }
                int count = MusicCount;
                for (int i = 1; i < count; i++)
                {
                    RemoveMusic(1);
                }
            };
            void AfterClearList()
            {
                SetCurrent(-1);
                stkLrc.Visibility = Visibility.Hidden;
                txtLrc.Visibility = Visibility.Hidden;
                lbxLrc.Visibility = Visibility.Hidden;
                if (set.ShowFloatLyric)
                {
                    floatLyric.Clear();
                }
                Title = "EasyMusic";
                txtMusicName.Text = "";
                btnPlay.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Hidden;
                path = "";
                Bass.BASS_ChannelStop(stream);
                stream = 0;
            }

            MenuItem menuAutoFurl = new MenuItem() { Header = (set.AutoFurl ? "√" : "×") + "自动收放列表" };
            menuAutoFurl.Click += (p1, p2) =>
            {
                set.AutoFurl = !set.AutoFurl;
                WindowSizeChangedEventHandler(null, null);
            };
            MenuItem menuShowLrc = new MenuItem() { Header = "显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                set.ShowLrc = true;
                grdLrcArea.Visibility = Visibility.Visible;
                //MaxWidth = double.PositiveInfinity;
                //MinWidth = 0;
                grdMain.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                grdMain.ColumnDefinitions[0].Width = GridLength.Auto;

                NewDoubleAnimation(this, WidthProperty, 1000, 0.5, 0.3);

            };

            menu.Items.Add(menuOpenFile);
            menu.Items.Add(menuOpenFolder);
            menu.Items.Add(menuOpenAllFolder);
            menu.Items.Add(NewSeparatorLine);
            //menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);

            if (musicList.SelectedIndex != -1)
            {
                menu.Items.Add(menuOpenMusicFolder);
                menu.Items.Add(menuShowMusicInfo);
                menu.Items.Add(menuPlayNext);
                menu.Items.Add(NewSeparatorLine);

                //menu.Items.Add(System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(SeparatorLine)) as System.Windows.Shapes.Line);
            }

            if (MusicCount> 0)
            {
                if (musicList.SelectedIndex != -1)
                {
                    menu.Items.Add(menuDelete);
                    menu.Items.Add(menuClearExceptCurrent);
                }
                menu.Items.Add(menuClear);
                menu.Items.Add(NewSeparatorLine);
            }
            if (set.ShowLrc)
            {
                menu.Items.Add(menuAutoFurl);
            }
            else
            {
                menu.Items.Add(menuShowLrc);
            }
        }

        /// <summary>
        /// 单击歌词选项按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLrcOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            StackPanel menuNormalFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="正常歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.NormalLrcFontSize.ToString()},
           }
            };
            StackPanel menuHighlightFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="当前歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.HighlightLrcFontSize.ToString()},
           }
            };
            StackPanel menuTextFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="文本歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.TextLrcFontSize.ToString()},
           }
            };


            MenuItem menuShowLrc = new MenuItem() { Header = "不显示歌词" };
            menuShowLrc.Click += (p1, p2) =>
            {
                set.ShowLrc = false;
                grdLrcArea.Visibility = Visibility.Collapsed;
                set.AutoFurl = false;
                NewDoubleAnimation(this, WidthProperty, grdMain.ColumnDefinitions[0].ActualWidth + 32, 0.5, 0.3, (p3, p4) =>
                {
                    grdMain.ColumnDefinitions[2].Width = new GridLength(0);

                    grdMain.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                });
            };


            MenuItem menuCopyLrc = new MenuItem() { Header = "复制歌词" };
            menuCopyLrc.Click += (p1, p2) =>
            {
                if (lrcContent.Count != 0)
                {
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < lrcContent.Count - 1; i++)
                    {
                        str.Append(lrcContent[i] + Environment.NewLine);
                    }
                    str.Append(lrcContent[lrcContent.Count - 1]);
                    Clipboard.SetText(str.ToString());
                }
            };
            MenuItem menuSave = new MenuItem() { Header = "保存歌词" };
            menuSave.Click += (p1, p2) => SaveLrc(false);
            MenuItem menuSaveAs = new MenuItem() { Header = "另存为歌词" };
            menuSaveAs.Click += (p1, p2) => SaveLrc(true);

            MenuItem menuSearchInNetEase = new MenuItem() { Header = "在网易云中搜索" };
            menuSearchInNetEase.Click += (p1, p2) => Process.Start($"https://music.163.com/#/search/m/?s={CurrentMusic.MusicName}");

            MenuItem menuReload = new MenuItem() { Header = "重载歌词" };
            menuReload.Click += (p1, p2) => InitialiazeLrc();


            MenuItem menuFloat = new MenuItem() { Header = (set.ShowFloatLyric ? "√" : "×") + "悬浮歌词" };
            menuFloat.PreviewMouseLeftButtonUp += (p1, p2) =>
            {
                set.ShowFloatLyric = !set.ShowFloatLyric;
                floatLyric.Visibility = floatLyric.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                floatLyric.Update(currentLrcIndex);
            };
            menuFloat.PreviewMouseRightButtonDown += (p1, p2) =>
            ShowFloatLyricsMenu();


            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,
                Placement = PlacementMode.Top,
                IsOpen = true
            };


            menu.Items.Add(menuShowLrc);
            menu.Items.Add(menuReload);
            menu.Items.Add(menuFloat);
            if (lrcContent.Count != 0 && (lbxLrc.Visibility == Visibility.Visible || stkLrc.Visibility == Visibility.Visible))
            {
                menu.Items.Add(NewSeparatorLine);
                menu.Items.Add(menuCopyLrc);
                menu.Items.Add(menuSave);
                menu.Items.Add(menuSaveAs);

            }
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuSearchInNetEase);
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuNormalFontSizeSetting);
            menu.Items.Add(menuHighlightFontSizeSetting);
            menu.Items.Add(menuTextFontSizeSetting);

            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += (p1, p2) =>
            {
                for (int i = 0; i < 3; i++)
                {
                    string text = ((menu.Items[menu.Items.Count - 4 + i] as StackPanel).Children[1] as TextBox).Text;
                    if (double.TryParse(text, out double newValue))
                    {
                        switch (i)
                        {
                            case 0:
                                set.NormalLrcFontSize = newValue;
                                break;
                            case 1:
                                set.HighlightLrcFontSize = newValue;
                                break;
                            case 2:
                                set.TextLrcFontSize = newValue;
                                txtLrc.FontSize = set.TextLrcFontSize;
                                break;
                        }
                    }
                }
            };

            menu.Items.Add(menuOK);
        }
        /// <summary>
        /// 保存歌词
        /// </summary>
        /// <param name="saveAs"></param>
        private void SaveLrc(bool saveAs)
        {
            StringBuilder str = new StringBuilder();
            if (set.PreferMusicInfo)
            {
                if (CurrentMusic.MusicName != "")
                {
                    str.Append("[ti:" + CurrentMusic.MusicName + "]" + Environment.NewLine);
                }
                if (CurrentMusic.Singer != "")
                {
                    str.Append("[ar:" + CurrentMusic.Singer + "]" + Environment.NewLine);
                }
                if (CurrentMusic.Album != "")
                {
                    str.Append("[al:" + CurrentMusic.Album + "]" + Environment.NewLine);
                }
            }
            else
            {
                if (lrc.Title != "")
                {
                    str.Append("[ti:" + lrc.Title + "]" + Environment.NewLine);
                }
                if (lrc.Artist != "")
                {
                    str.Append("[ar:" + lrc.Artist + "]" + Environment.NewLine);
                }
                if (lrc.Album != "")
                {
                    str.Append("[al:" + lrc.Album + "]" + Environment.NewLine);
                }
                if (lrc.LrcBy != "")
                {
                    str.Append("[by:" + lrc.LrcBy + "]" + Environment.NewLine);
                }
            }
            if (set.SaveLrcOffsetByTag && offset != 0)
            {
                str.Append("[offset:" + (int)Math.Round(offset * 1000) + "]" + Environment.NewLine);
            }
            List<double> lrcTime = new List<double>();
            foreach (var i in this.lrcTime)
            {
                lrcTime.Add((set.SaveLrcOffsetByTag) ? i : i + offset);
            }

            FileInfo file = new FileInfo(path);
            for (int i = 0; i < lrcTime.Count; i++)
            {
                double time = lrcTime[i];
                string word = lrcContent[i];
                int intMinute = (int)time / 60;
                string minute = string.Format("{0:00}", intMinute);
                string second = string.Format("{0:00.00}", time - 60 * intMinute);
                foreach (var j in word.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    str.Append("[" + minute + ":" + second + "]" + j + Environment.NewLine);
                }

            }
            if (saveAs)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    AddExtension = true,
                    InitialDirectory = file.DirectoryName,
                    Title = "请选择目标文件夹",
                    Filter = "歌词文件（*.lrc）|*.lrc",
                    FileName = file.Name.Replace(file.Extension, ".lrc"),
                };
                if (sfd.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, str.ToString());
                        ShowInfo("歌词保存成功");
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("无法保存文件：" + Environment.NewLine + ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(path.Replace(file.Extension, "") + ".lrc", str.ToString());
                    ShowInfo("歌词保存成功");
                }
                catch (Exception ex)
                {
                    ShowAlert("无法保存文件：" + Environment.NewLine + ex.Message);
                }
            }
        }
        /// <summary>
        /// 用于暂时保存计时器
        /// </summary>
        DispatcherTimer infoWaitTimer;
        /// <summary>
        /// 显示不重要的信息
        /// </summary>
        /// <param name="info"></param>
        private void ShowInfo(string info)
        {
            tbkOffset.Text = info;
            tbkOffset.Opacity = 1;
            infoWaitTimer?.Stop();
            infoWaitTimer = SleepThenDo(1000, (p1, p2) => NewDoubleAnimation(tbkOffset, OpacityProperty, 0, 0.5, 0, (p3, p4) => tbkOffset.Opacity = 0, true));
        }
        /// <summary>
        /// 显示悬浮菜单的菜单
        /// </summary>
        private void ShowFloatLyricsMenu()
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,

                IsOpen = true,
            };



            StackPanel menuNormalFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="正常歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsNormalFontSize.ToString()},
           }
            };
            StackPanel menuHighlightFontSizeSetting = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
           {
               new TextBlock(){Text="当前歌词字体大小："},
               new TextBox(){Style=Resources["txtStyle"] as Style, Width=36,Text=set.FloatLyricsHighlightFontSize.ToString()},
           }
            };
            MenuItem menuAdjust = new MenuItem() { Header = "调整位置和大小" };
            menuAdjust.Click += (p1, p2) => floatLyric.Adjuest = true;
            MenuItem menuOK = new MenuItem()
            {
                Header = "确定",
            };
            menuOK.Click += (p1, p2) =>
            {
                for (int i = 0; i < 2; i++)
                {
                    string text = ((menu.Items[menu.Items.Count - 3 + i] as StackPanel).Children[1] as TextBox).Text;
                    if (double.TryParse(text, out double newValue))
                    {
                        switch (i)
                        {
                            case 0:
                                set.FloatLyricsNormalFontSize = newValue;
                                break;
                            case 1:
                                set.FloatLyricsHighlightFontSize = newValue;
                                break;
                        }
                    }
                }
            };
            menu.Items.Add(menuAdjust);
            menu.Items.Add(NewSeparatorLine);
            menu.Items.Add(menuNormalFontSizeSetting);
            menu.Items.Add(menuHighlightFontSizeSetting);
            menu.Items.Add(menuOK);





        }
        #endregion

        #region 歌曲搜索
        /// <summary>
        /// 搜索框内容改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxTextBoxTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            string value = txt.Text;
            if (value == "")
            {
                cbbSearch.IsDropDownOpen = false;
                return;
            }
            cbbSearch.Items.Clear();
            int index = -1;
            foreach (var i in musicDatas)
            {
                index++;

                if (IsInfoMatch(value, i))
                {
                    ComboBoxItem cbbItem = new ComboBoxItem() { Content = i.MusicName, Tag = index };
                    cbbItem.PreviewMouseLeftButtonUp += (p1, p2) =>
                    {
                        PlayNew((int)cbbItem.Tag, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    };

                    cbbSearch.Items.Add(cbbItem);
                    cbbSearch.DropDownOpened += (p1, p2) =>
                    {

                        txt.SelectionLength = 0;
                        txt.SelectionStart = (txt.Text.Length);
                    };
                    cbbSearch.IsDropDownOpen = true;

                }
            }

        }
        /// <summary>
        /// 搜索框按下按键事件（回车键）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxTextBoxKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            string value = (sender as TextBox).Text;
            if (value == "")
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                int index = -1;
                foreach (var i in musicDatas)
                {
                    index++;
                    if (IsInfoMatch(value, i))
                    {
                        PlayNew(index, false);
                        cbbSearch.IsDropDownOpen = false;
                        cbbSearch.Text = "";
                    }
                }
            }
        }
        /// <summary>
        /// 信息匹配判断
        /// </summary>
        /// <param name="str"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsInfoMatch(string str, MusicInfo info)
        {
            str = str.ToLower();
            string musicName = (info.MusicName + new FileInfo(info.Path).Name).ToLower();
            if (musicName.Contains(str))
            {
                return true;
            }
            if (info.Singer.ToLower().Contains(str))
            {
                return true;
            }
            if (info.Album.ToLower().Contains(str))
            {
                return true;
            }

            if (ConvertChToPinYin(musicName).ToLower().Contains(str))
            {
                return true;
            }
            if (ConvertChToPinYin(info.Singer).ToLower().Contains(str))
            {
                return true;
            }

            var namePinYinTitle = new string(ConvertChToPinYin(musicName).Select(x => (x >= 'A' && x <= 'Z') ? x : ' ').Select(x => x).ToArray()).Replace(" ", "");
            if (namePinYinTitle.ToLower().Contains(str))
            {
                return true;
            }

            var singerPinYinTitle = new string(ConvertChToPinYin(info.Singer).Select(x => (x >= 'A' && x <= 'Z') ? x : ' ').Select(x => x).ToArray()).Replace(" ", "");
            if (singerPinYinTitle.ToLower().Contains(str))
            {
                return true;
            }

            // Debug.WriteLine(PYTitle);
            return false;
        }
        /// <summary>
        /// 拼音区编码数组
        /// </summary>
        private int[] wordCode = new int[] { -20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032, -20026, -20002, -19990, -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728, -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, -19003, -18996, -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211, -18201, -18184, -18183, -18181, -18012, -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730, -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, -16465, -16459, -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915, -15903, -15889, -15878, -15707, -15701, -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416, -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, -14929, -14928, -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353, -14345, -14170, -14159, -14151, -14149, -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090, -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, -13340, -13329, -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585, -12556, -12359, -12346, -12320, -12300, -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604, -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533, -10519, -10331, -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256, -10254 };
        /// <summary>
        /// 拼音数组
        /// </summary>
        private string[] pinYin = new string[] { "A", "Ai", "An", "Ang", "Ao", "Ba", "Bai", "Ban", "Bang", "Bao", "Bei", "Ben", "Beng", "Bi", "Bian", "Biao", "Bie", "Bin", "Bing", "Bo", "Bu", "Ba", "Cai", "Can", "Cang", "Cao", "Ce", "Ceng", "Cha", "Chai", "Chan", "Chang", "Chao", "Che", "Chen", "Cheng", "Chi", "Chong", "Chou", "Chu", "Chuai", "Chuan", "Chuang", "Chui", "Chun", "Chuo", "Ci", "Cong", "Cou", "Cu", "Cuan", "Cui", "Cun", "Cuo", "Da", "Dai", "Dan", "Dang", "Dao", "De", "Deng", "Di", "Dian", "Diao", "Die", "Ding", "Diu", "Dong", "Dou", "Du", "Duan", "Dui", "Dun", "Duo", "E", "En", "Er", "Fa", "Fan", "Fang", "Fei", "Fen", "Feng", "Fo", "Fou", "Fu", "Ga", "Gai", "Gan", "Gang", "Gao", "Ge", "Gei", "Gen", "Geng", "Gong", "Gou", "Gu", "Gua", "Guai", "Guan", "Guang", "Gui", "Gun", "Guo", "Ha", "Hai", "Han", "Hang", "Hao", "He", "Hei", "Hen", "Heng", "Hong", "Hou", "Hu", "Hua", "Huai", "Huan", "Huang", "Hui", "Hun", "Huo", "Ji", "Jia", "Jian", "Jiang", "Jiao", "Jie", "Jin", "Jing", "Jiong", "Jiu", "Ju", "Juan", "Jue", "Jun", "Ka", "Kai", "Kan", "Kang", "Kao", "Ke", "Ken", "Keng", "Kong", "Kou", "Ku", "Kua", "Kuai", "Kuan", "Kuang", "Kui", "Kun", "Kuo", "La", "Lai", "Lan", "Lang", "Lao", "Le", "Lei", "Leng", "Li", "Lia", "Lian", "Liang", "Liao", "Lie", "Lin", "Ling", "Liu", "Long", "Lou", "Lu", "Lv", "Luan", "Lue", "Lun", "Luo", "Ma", "Mai", "Man", "Mang", "Mao", "Me", "Mei", "Men", "Meng", "Mi", "Mian", "Miao", "Mie", "Min", "Ming", "Miu", "Mo", "Mou", "Mu", "Na", "Nai", "Nan", "Nang", "Nao", "Ne", "Nei", "Nen", "Neng", "Ni", "Nian", "Niang", "Niao", "Nie", "Nin", "Ning", "Niu", "Nong", "Nu", "Nv", "Nuan", "Nue", "Nuo", "O", "Ou", "Pa", "Pai", "Pan", "Pang", "Pao", "Pei", "Pen", "Peng", "Pi", "Pian", "Piao", "Pie", "Pin", "Ping", "Po", "Pu", "Qi", "Qia", "Qian", "Qiang", "Qiao", "Qie", "Qin", "Qing", "Qiong", "Qiu", "Qu", "Quan", "Que", "Qun", "Ran", "Rang", "Rao", "Re", "Ren", "Reng", "Ri", "Rong", "Rou", "Ru", "Ruan", "Rui", "Run", "Ruo", "Sa", "Sai", "San", "Sang", "Sao", "Se", "Sen", "Seng", "Sha", "Shai", "Shan", "Shang", "Shao", "She", "Shen", "Sheng", "Shi", "Shou", "Shu", "Shua", "Shuai", "Shuan", "Shuang", "Shui", "Shun", "Shuo", "Si", "Song", "Sou", "Su", "Suan", "Sui", "Sun", "Suo", "Ta", "Tai", "Tan", "Tang", "Tao", "Te", "Teng", "Ti", "Tian", "Tiao", "Tie", "Ting", "Tong", "Tou", "Tu", "Tuan", "Tui", "Tun", "Tuo", "Wa", "Wai", "Wan", "Wang", "Wei", "Wen", "Weng", "Wo", "Wu", "Xi", "Xia", "Xian", "Xiang", "Xiao", "Xie", "Xin", "Xing", "Xiong", "Xiu", "Xu", "Xuan", "Xue", "Xun", "Ya", "Yan", "Yang", "Yao", "Ye", "Yi", "Yin", "Ying", "Yo", "Yong", "You", "Yu", "Yuan", "Yue", "Yun", "Za", "Zai", "Zan", "Zang", "Zao", "Ze", "Zei", "Zen", "Zeng", "Zha", "Zhai", "Zhan", "Zhang", "Zhao", "Zhe", "Zhen", "Zheng", "Zhi", "Zhong", "Zhou", "Zhu", "Zhua", "Zhuai", "Zhuan", "Zhuang", "Zhui", "Zhun", "Zhuo", "Zi", "Zong", "Zou", "Zu", "Zuan", "Zui", "Zun", "Zuo" };
        /// <summary>
        /// 汉字转换成全拼的拼音
        /// </summary>
        /// <param name="Chstr">汉字字符串</param>
        /// <returns>转换后的拼音字符串</returns>
        public string ConvertChToPinYin(string Chstr)
        {
            Regex reg = new Regex("^[\u4e00-\u9fa5]$");//验证是否输入汉字
            byte[] arr = new byte[2];
            string pystr = "";
            int asc = 0, M1 = 0, M2 = 0;
            char[] mChar = Chstr.ToCharArray();//获取汉字对应的字符数组
            for (int j = 0; j < mChar.Length; j++)
            {
                //如果输入的是汉字
                if (reg.IsMatch(mChar[j].ToString()))
                {
                    arr = Encoding.Default.GetBytes(mChar[j].ToString());
                    M1 = arr[0];
                    M2 = arr[1];
                    asc = M1 * 256 + M2 - 65536;
                    if (asc > 0 && asc < 160)
                    {
                        pystr += mChar[j];
                    }
                    else
                    {
                        switch (asc)
                        {
                            case -9254:
                                pystr += "Zhen";
                                break;
                            case -8985:
                                pystr += "Qian";
                                break;
                            case -5463:
                                pystr += "Jia";
                                break;
                            case -8274:
                                pystr += "Ge";
                                break;
                            case -5448:
                                pystr += "Ga";
                                break;
                            case -5447:
                                pystr += "La";
                                break;
                            case -4649:
                                pystr += "Chen";
                                break;
                            case -5436:
                                pystr += "Mao";
                                break;
                            case -5213:
                                pystr += "Mao";
                                break;
                            case -3597:
                                pystr += "Die";
                                break;
                            case -5659:
                                pystr += "Tian";
                                break;
                            default:
                                for (int i = (wordCode.Length - 1); i >= 0; i--)
                                {
                                    if (wordCode[i] <= asc)//判断汉字的拼音区编码是否在指定范围内
                                    {
                                        pystr += pinYin[i];//如果不超出范围则获取对应的拼音
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }
                else//如果不是汉字
                {
                    pystr += mChar[j].ToString();//如果不是汉字则返回
                }
            }
            return pystr;//返回获取到的汉字拼音
        }
        #endregion
    }
}
