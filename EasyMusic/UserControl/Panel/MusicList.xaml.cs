using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using System.IO;
using static FzLib.Control.Dialog.DialogHelper;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using FzLib.Control.Dialog;
using static EasyMusic.Helper.MusicControlHelper;
using System.ComponentModel;
using System.Collections.Generic;
using EasyMusic.Info;
using EasyMusic.Helper;
using EasyMusic.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// MusicList.xaml 的交互逻辑
    /// </summary>
    public partial class MusicList : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public UserControls.ToggleButton lastMusicListBtn;
        private static ObservableCollection<MusicInfo> musicListBinding;

        public event PropertyChangedEventHandler PropertyChanged;

        public MusicList()
        {
            InitializeComponent();
            DataContext = this;
            MusicListBinding = MusicDatas;
            if (!Directory.Exists(GetMusicListPath()))
            {
                try
                {
                    Directory.CreateDirectory(GetMusicListPath());
                }
                catch (Exception ex)
                {
                    ShowException("目录不存在且无法创建", ex);
                    return;
                }

            }
            if (Directory.EnumerateFiles(GetMusicListPath()).Count() == 0)
            {
                try
                {
                    using (File.Create(GetMusicListPath("默认"))) { }
                }
                catch (Exception ex)
                {
                    ShowException("不存在任何歌单且无法创建", ex);
                    return;
                }
            }
            foreach (var i in Directory.EnumerateFiles(GetMusicListPath()))
            {
                UserControls.ToggleButton button = null;
                FileInfo file = new FileInfo(i);
                button = new UserControls.ToggleButton() { Text = file.Name.Replace(file.Extension, "") };

                if (i == GetMusicListPath(Setting.LastMusicList))
                {
                    lastMusicListBtn = button;
                }
                stkMusiList.Children.Add(button);
                button.Select += BtnSelectEventHandler;
                button.PreviewMouseRightButtonDown += BtnPreviewMouseRightButtonDownEventHandler;

            }


            if (lastMusicListBtn == null)
            {
                // (stkMusiList.Children[1] as UserControls.ToggleButton).IsPressed = true;
                lastMusicListBtn = stkMusiList.Children[2] as ToggleButton;
            }
            lastMusicListBtn.RaiseClickEvent();


        }
        /// <summary>
        /// 歌单按钮右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreviewMouseRightButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {

            var btn = sender as UserControls.ToggleButton;

            MenuItem menuRename = new MenuItem() { Header = "重命名" };
            menuRename.Click += (p1, p2) =>
              {
                  if (!File.Exists(GetMusicListPath(btn.Text)))
                  {
                      if (ShowMessage("歌单文件不存在，是否直接从界面删除？", DialogType.Information, MessageBoxButton.YesNo) == 1)
                      {
                          RemoveButton();
                      }
                  }
                  else
                  {
                      if (GetInput("请输入目标名称（不含后缀名）：", out string name, FzLib.Control.DarkerBrushConverter.GetDarkerColor(lvw.Background as SolidColorBrush), btn.Text, @"^[^\/:*\?\”“\<>|,]+$"))
                      {
                          try
                          {
                              RenameMusicListFile(btn.Text, name, false);
                              //if(btn.Text==set.DefautMusicList)
                              //{
                              //    set.DefautMusicList = name;
                              //}
                              btn.Text = name;
                          }
                          catch (Exception ex)
                          {
                              ShowException("重命名歌单文件失败", ex);
                          }
                      }
                  }
              };

            MenuItem menuDelete = new MenuItem() { Header = "删除" };
            menuDelete.Click += (p1, p2) =>
              {
                  if (!File.Exists(GetMusicListPath(btn.Text)))
                  {
                      if (ShowMessage("歌单文件不存在，是否直接从界面删除？", DialogType.Warn, MessageBoxButton.YesNo) == 1)
                      {
                          RemoveButton();
                      }
                  }
                  else
                  {
                      try
                      {
                          DeleteMusicListFile(btn.Text, false);
                          RemoveButton();
                      }
                      catch (Exception ex)
                      {
                          ShowException("删除歌单文件失败", ex);
                      }
                  }
              };


            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = this,
                IsOpen = true,
            };
            menu.Items.Add(menuRename);
            if (stkMusiList.Children.Count > 2)
            {
                menu.Items.Add(menuDelete);
            }
            void RemoveButton()
            {
                if (btn.IsPressed)
                {
                    lastMusicListBtn = null;
                }
                string temp = btn.Text;
                stkMusiList.Children.Remove(btn);
                //if(temp==set.DefautMusicList)
                //{
                //    ShowPrompt("默认歌单被删除，请重新选择默认歌单。");
                //    new Windows.WinSettings().ShowDialog();
                //}
                (stkMusiList.Children[2] as ToggleButton).RaiseClickEvent();
            }
        }
        /// <summary>
        /// 歌单按钮选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectEventHandler(object sender, EventArgs e)
        {
            UserControls.ToggleButton btn = sender as UserControls.ToggleButton;
            foreach (var i in stkMusiList.Children)
            {
                if (!(i is UserControls.ToggleButton) || i == sender)
                {
                    continue;
                }
                (i as UserControls.ToggleButton).IsPressed = false;
            }
            if (lastMusicListBtn != null && lastMusicListBtn != btn && MusicDatas != null)
            {
                SaveListToFile(lastMusicListBtn.Text, false);
                lastMusicListBtn = btn;
            }

            ReadFileToList(btn.Text);

        }

        /// <summary>
        /// 双击列表项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LvwItemPreviewMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            PlayNew(SelectedMusic);
        }
        /// <summary>
        /// 在列表项上按下按钮事件，包括打开、删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvwItemPreviewKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (lvw.SelectedIndex == -1)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Enter:
                    LvwItemPreviewMouseDoubleClickEventHandler(null, null);
                    break;
                case Key.Delete:
                    RemoveAllSelection();
                    break;
            }

        }
        /// <summary>
        /// 在列表项上单击鼠标右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItemMouseRightButtonUpEvetnHandler(object sender, MouseButtonEventArgs e)
        {
            if (lvw.SelectedIndex != -1)
            {
                BtnListOptionClickEventHanlder(sender, null);

            }
        }
        /// <summary>
        /// 删除所有选中的项
        /// </summary>
        public void RemoveAllSelection()
        {
            MusicInfo[] list = lvw.SelectedItems.Cast<MusicInfo>().ToArray();
            foreach (var i in list)
            {
                RemoveMusic(i);
            }
        }
        public MusicInfo SelectedMusic { get; set; }
        public IEnumerable<MusicInfo> SelectedMusics => lvw.SelectedItems.Cast<MusicInfo>();

        public ObservableCollection<MusicInfo> MusicListBinding
        {
            get => musicListBinding;
            set
            {
                musicListBinding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MusicListBinding"));
            }
        }

        /// <summary>
        /// 定位到
        /// </summary>
        /// <param name="index"></param>
        public void SelectAndScroll(MusicInfo musicInfo)
        {
            lvw.SelectedItem = musicInfo;
            lvw.ScrollIntoView(lvw.SelectedItem);
        }
        /// <summary>
        /// 单击增加歌单按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (GetInput("请输入文件名：", out string name, FzLib.Control.DarkerBrushConverter.GetDarkerColor(lvw.Background as SolidColorBrush)))
            {
                var button = new UserControls.ToggleButton() { Text = new FileInfo(name).Name };

                button.Select += BtnSelectEventHandler;
                button.PreviewMouseRightButtonDown += BtnPreviewMouseRightButtonDownEventHandler;
                stkMusiList.Children.Add(button);
                try
                {
                    using (File.Create(GetMusicListPath(name)))
                    { }
                    button.RaiseClickEvent();
                    scv.ScrollToRightEnd();
                }
                catch (Exception ex)
                {
                    ShowException("创建歌单文件失败", ex);
                }
            }
        }
        /// <summary>
        /// 鼠标滚轮在歌单按钮上滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerPreviewMouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                scv.LineLeft();
            }
            else
            {
                scv.LineRight();
            }
        }

        private void BtnHistoryClickEventHandler(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (lvw.ItemsSource == MusicDatas)
            {
                lvw.ItemsSource = historyList;
                btn.Background = FzLib.Control.DarkerBrushConverter.GetDarkerColor(FzLib.Control.DarkerBrushConverter.GetDarkerColor(FzLib.Control.DarkerBrushConverter.GetDarkerColor(Setting.BackgroundColor)));

            }
            else
            {
                lvw.ItemsSource = MusicDatas;
                btn.Background = Setting.BackgroundColor;

            }
        }


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

                Placement = PlacementMode.Top,
                IsOpen = true,
                // Style=Resources["ctmStyle"] as Style
            };

            MenuItem menuOpenFile = new MenuItem() { Header = "文件" };
            menuOpenFile.Click += (p1, p2) => FileHelper.ImportMusicsFiles();
            MenuItem menuOpenFolder = new MenuItem() { Header = "文件夹" };
            menuOpenFolder.Click += (p1, p2) => FileHelper.ImportMusicsFolder(false);

            MenuItem menuOpenAllFolder = new MenuItem() { Header = "文件夹及子文件夹" };
            menuOpenAllFolder.Click += (p1, p2) => FileHelper.ImportMusicsFolder(true);

            MenuItem menuAdd = new MenuItem() { Header = "添加", Items = { menuOpenFile, menuOpenFolder, menuOpenAllFolder } };

            MenuItem menuDeleteSelected = new MenuItem() { Header = "删除选中项" };
            menuDeleteSelected.Click += (p1, p2) =>
            {
                foreach (var item in SelectedMusics.ToArray())
                {
                    MusicDatas.Remove(item);
                }
            };
            MenuItem menuClear = new MenuItem() { Header = "清空列表", };
            menuClear.Click += (p1, p2) => ClearMusics();
            //MenuItem menuClearExceptCurrent = new MenuItem() { Header = "删除其他", };
            //menuClearExceptCurrent.Click += (p1, p2) =>
            //{
            //    var seletedItems = SelectedMusics.ToArray();

            //};
            MenuItem menuDelete = new MenuItem() { Header = "删除" };
            if (SelectedMusic != null)
            {
                menuDelete.Items.Add(menuDeleteSelected);
                //menuDelete.Items.Add(menuClearExceptCurrent);
            }
            menuDelete.Items.Add(menuClear);

            MenuItem menuRandom = new MenuItem() { Header = "随机排序" };
            menuRandom.Click += (p1, p2) => RandomizeList();

            MenuItem menuOpenMusicFolder = new MenuItem() { Header = "打开所在文件夹" };
            menuOpenMusicFolder.Click += (p1, p2) => Process.Start("Explorer.exe", @"/select," + Music.Info.Path);

            MenuItem menuShowMusicInfo = new MenuItem() { Header = "显示音乐信息" };
            menuShowMusicInfo.Click += (p1, p2) => new WinMusicInfo(Music.Info) { Owner = MainWindow.Current }.ShowDialog();
            MenuItem menuPlayNext = new MenuItem() { Header = "下一首播放" };
            menuPlayNext.Click += (p1, p2) =>
            {
                if (CurrentHistoryIndex < HistoryCount - 1)
                {
                    RemoveHistory(CurrentHistoryIndex + 1, HistoryCount - CurrentHistoryIndex - 1);
                }
                AddHistory(SelectedMusic, false);
            };



            MenuItem menuImport = new MenuItem() { Header = "导入歌单" };
            menuImport.Click += (p1, p2) => FileHelper.ImportMusicList();

            MenuItem menuExport = new MenuItem() { Header = "导出歌单" };
            menuExport.Click += (p1, p2) => FileHelper.ExportMusicList();


            MenuItem menuRefreshList = new MenuItem() { Header = "刷新列表" };
            menuRefreshList.Click += MenuRefreshListClickEventHandler;

            MenuItem menuShowLrc = new MenuItem() { Header = "显示歌词" };
            menuShowLrc.Click += (p1, p2) => MainWindow.Current.ExpandLyricsArea();
            menu.Items.Add(menuAdd);
            if (MusicCount > 0)
            {
                menu.Items.Add(menuDelete);
            }
            menu.Items.Add(new SeparatorLine());
            if (SelectedMusic != null)
            {
                menu.Items.Add(menuOpenMusicFolder);
                menu.Items.Add(menuShowMusicInfo);
                menu.Items.Add(menuPlayNext);
                menu.Items.Add(new SeparatorLine());
            }

            if (MusicCount > 0)
            {
                menu.Items.Add(menuRandom);
                menu.Items.Add(menuRefreshList);
            }

            menu.Items.Add(menuImport);
            menu.Items.Add(menuExport);
            if (!Setting.ShowLrc)
            {
                menu.Items.Add(menuShowLrc);
            }
        }
        /// <summary>
        /// 单击刷新列表按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuRefreshListClickEventHandler(object sender, RoutedEventArgs e)
        {
            string[] paths = MusicDatas.Select(p => p.Path).ToArray();


            List<string> exists = new List<string>(paths.Length);
            List<string> notExists = new List<string>(paths.Length);
            foreach (var i in paths)
            {
                if (File.Exists(i))
                {
                    exists.Add(i);
                }
                else
                {
                    notExists.Add(i);
                }
            }

            if (notExists.Count == 0)
            {
                if (ShowMessage("检测完成，没有发现不存在的文件，是否刷新？", DialogType.Information, new string[] { "是", "否" }) == 1)
                {
                    ClearMusics();
                  MainWindow.Current.  AfterClearList();
                    AddMusic(paths);
                }
            }
            else
            {
                if (ShowMessage($"检测完成，发现{notExists.Count}个不存在的文件，是否删除并刷新？", DialogType.Information, new string[] { "是", "否" }) == 1)
                {
                    ClearMusics();
                    MainWindow.Current.AfterClearList();
                    AddMusic(exists.ToArray());
                }
            }
        }


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
            foreach (var i in MusicDatas)
            {
                index++;

                if (IsInfoMatch(value, i))
                {
                    ComboBoxItem cbbItem = new ComboBoxItem() { Content = i.Name, Tag = i };
                    cbbItem.PreviewMouseLeftButtonUp += (p1, p2) =>
                    {
                        PlayNew((MusicInfo)cbbItem.Tag, false);
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
                MusicInfo info = MusicDatas.FirstOrDefault(p => IsInfoMatch(value, p));
                if (info != null)
                {
                    PlayNew(info, false);
                    cbbSearch.IsDropDownOpen = false;
                    cbbSearch.Text = "";
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
            string musicName = (info.Name + new FileInfo(info.Path).Name).ToLower();
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
