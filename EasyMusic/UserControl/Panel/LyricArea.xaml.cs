using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Un4seen.Bass;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Linq;
using System.Windows.Shell;
using EasyMusic.Windows;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicListHelper;
using static FzLib.Control.Dialog.DialogHelper;
using static EasyMusic.Helper.MusicControlHelper;
using EasyMusic.Helper;
using FzLib.Basic;
using EasyMusic.Info;
using System.Security.Principal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using FzLib.Control.Dialog;
using EasyMusic.UserControls;
using System.Threading.Tasks;

using EasyMusic.Enum;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// LyricArea.xaml 的交互逻辑
    /// </summary>
    public partial class LyricArea : UserControl
    {

        /// <summary>
        /// 到某一条歌词一共有多少行
        /// </summary>
        List<int> lrcLineSumToIndex = new List<int>();
        private LyricType currentLyricType = LyricType.None;
        public LyricType CurrentLyricType
        {
            get => currentLyricType;
            set
            {
                switch (value)
                {
                    case LyricType.None:
                        grdLrc.Visibility = Visibility.Hidden;
                        txtLrc.Visibility = Visibility.Hidden;
                        lbxLrc.Visibility = Visibility.Hidden;
                        break;
                    case LyricType.LrcFormat:
                        grdLrc.Visibility = Visibility.Visible;
                        txtLrc.Visibility = Visibility.Hidden;
                        lbxLrc.Visibility = Visibility.Visible;
                        break;
                    case LyricType.TextFormat:
                        grdLrc.Visibility = Visibility.Hidden;
                        txtLrc.Visibility = Visibility.Visible;
                        lbxLrc.Visibility = Visibility.Hidden;
                        txtLrc.FontSize = Setting.TextLrcFontSize;
                      
                        break;
                }
                FontFamily font = new FontFamily(Setting.LyricsFont);
                if (font == null)
                {
                    trayIcon.ShowMessage("主界面字体应用失败，请重新设置");
                }
                else
                {
                    lbxLrc.FontFamily = font;
                }
                lbxLrc.Foreground =Setting.LyricsFontColor;
                lbxLrc.FontWeight = Setting.LyricsFontBold ? FontWeights.Bold : FontWeights.Normal;

                lrcLineSumToIndex.Clear();

                lbxLrc.Clear();
                currentLyricType = value;
            }
        }

        public LyricArea()
        {
            InitializeComponent();
        }

        public void AddLrcs()
        {
            //lbxLrc.Clear(); lrcLineSumToIndex.Clear();

            int index = 0;//用于赋值Tag
            foreach (var i in lrc.LrcContent)
            {
                var tbk = new TextBlock()
                {
                    Name = "tbk" + index.ToString(),
                    FontSize = Setting.NormalLrcFontSize,
                    Text = i.Value,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Tag = index++,//标签用于定位
                    Cursor = Cursors.Hand,
                    TextAlignment = TextAlignment.Center,
                    FocusVisualStyle = null,
                };
                tbk.MouseLeftButtonUp += (p1, p2) =>
                {
                    var position = (lrc.LrcContent.ElementAt((int)tbk.Tag).Key - lrc.Offset - Setting.LrcDefautOffset);
                    Music.Position = position;
                };

                lbxLrc.Add(tbk);
            }
            foreach (var i in lrc.LineIndex)
            {
                lrcLineSumToIndex.Add(i.Value);
            }

        }

        public void LoadTextFormatLyric(string text)
        {
            txtLrc.Text = text;
        }



        private int showingMessageCount = 0;
        /// <summary>
        /// 显示不重要的信息
        /// </summary>
        /// <param name="info"></param>
        public async void ShowMessage(string info)
        {
            showingMessageCount++;
            tbkOffset.Text = info;
            tbkOffset.Opacity = 1;
            await Task.Delay(1000);
            if (showingMessageCount == 1)
            {
                NewDoubleAnimation(tbkOffset, OpacityProperty, 0, 0.5, 0, (p3, p4) => tbkOffset.Opacity = 0, true);
            }
            showingMessageCount--;
        }
        /// <summary>
        /// 单击歌词选项按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLrcOptionClickEventHanlder(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget = btnLrcOption,
                Placement = PlacementMode.Top,
                IsOpen = true
            };


            MenuItem menuShowLrc = new MenuItem() { Header = "不显示歌词" };
            menuShowLrc.Click += (p1, p2) => MainWindow.Current.ShrinkLyricsArea();


            MenuItem menuCopyLrc = new MenuItem() { Header = "复制歌词" };
            menuCopyLrc.Click += (p1, p2) => lrc.CopyLyrics();
            MenuItem menuSave = new MenuItem() { Header = "保存歌词" };
            menuSave.Click += (p1, p2) => lrc.SaveLrc(false);
            MenuItem menuSaveAs = new MenuItem() { Header = "另存为歌词" };
            menuSaveAs.Click += (p1, p2) => lrc.SaveLrc(true);

            MenuItem menuSearchInNetEase = new MenuItem() { Header = "在网易云中搜索" };
            menuSearchInNetEase.Click += (p1, p2) => Process.Start($"https://music.163.com/#/search/m/?s={Music.Info.Name}");

            MenuItem menuReload = new MenuItem() { Header = "重载歌词" };
            menuReload.Click += (p1, p2) => MainWindow.Current.InitializeLrc();

            MenuItem menuEdit = new MenuItem() { Header = (lbxLrc.Visibility == Visibility.Visible || txtLrc.Visibility == Visibility.Visible) ? "编辑歌词" : "新建歌词" };
            menuEdit.Click += (p1, p2) =>
            {
                FileInfo file = new FileInfo(Music.FilePath);
                if (lbxLrc.Visibility == Visibility.Visible)
                {
                    WinLrcEdit.Edit(MainWindow.Current, file.FullName.RemoveEnd(file.Extension) + ".lrc");
                }
                else if (txtLrc.Visibility == Visibility.Visible)
                {
                    WinLrcEdit.Edit(MainWindow.Current, file.FullName.RemoveEnd(file.Extension) + ".txt");
                }
                else
                {
                    int index = DialogHelper.ShowMessage("Lrc歌词文件还是Txt文本文件？", DialogType.Information, new string[] { "Lrc文件", "Txt文件", "取消" });
                    if (index == 0)
                    {
                    WinLrcEdit.Edit(MainWindow.Current, file.FullName.RemoveEnd(file.Extension) + ".lrc");
                    }
                    else if (index == 1)
                    {
                        WinLrcEdit.Edit(MainWindow.Current, file.FullName.RemoveEnd(file.Extension) + ".txt");
                    }
                }
            };

            MenuItem menuShow = new MenuItem() {Header="展示歌词" };
            menuShow.Click += (p1, p2) =>
            {

                    WinLrcEdit.ShowOnly(MainWindow.Current,lrc.LrcContent.Values);
           
            };

            MenuItem menuOpenSetting = new MenuItem() { Header = "主界面歌词设置" };
            menuOpenSetting.Click += (p1, p2) => new WinSettings(1) { Owner = MainWindow.Current }.ShowDialog();


            MenuItem menuFloat = new MenuItem() { Header = "悬浮歌词" };

            MenuItem menuFloatSwitch = new MenuItem() { Header = (Setting.ShowFloatLyric ? "关闭" : "打开") };
            menuFloatSwitch.Click += (p1, p2) => MainWindow.Current.OpenOrCloseFloatLrc();
            MenuItem menuFloatAdjust = new MenuItem() { Header = "调整位置和大小" };
            menuFloatAdjust.Click += (p1, p2) => MainWindow.Current.FloatLyric.Adjuest = true;
            MenuItem menuFloatOpenSetting = new MenuItem() { Header = "悬浮歌词设置" };
            menuFloatOpenSetting.Click += (p1, p2) => new WinSettings(2) { Owner = MainWindow.Current }.ShowDialog();

            menuFloat.Items.Add(menuFloatSwitch);
            if (Setting.ShowFloatLyric)
            {
                menuFloat.Items.Add(menuFloatAdjust);
            }
            menuFloat.Items.Add(menuFloatOpenSetting);



            menu.Items.Add(menuShowLrc);
            menu.Items.Add(menuReload);
            menu.Items.Add(menuFloat);
            menu.Items.Add(new SeparatorLine());
            menu.Items.Add(menuEdit);
            //if (lrcContent.Count != 0 && (lbxLrc.Visibility == Visibility.Visible || stkLrc.Visibility == Visibility.Visible))
            if (lrc != null && CurrentLyricType == LyricType.LrcFormat)
            {
                menu.Items.Add(menuShow);
                menu.Items.Add(menuCopyLrc);
                menu.Items.Add(menuSave);
                menu.Items.Add(menuSaveAs);

            }
            menu.Items.Add(new SeparatorLine());
            menu.Items.Add(menuSearchInNetEase);
            menu.Items.Add(menuOpenSetting);

        }
        public void Update()
        {
            lbxLrc.RefreshFontSize(lrc.CurrentIndex);
            lbxLrc.ScrollTo(lrc.CurrentIndex, lrcLineSumToIndex, Setting.NormalLrcFontSize);

        }

        public void HideAll()
        {
            txtLrc.Visibility = Visibility.Hidden;
            lbxLrc.Visibility = Visibility.Hidden;

        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lbxLrc.RefreshPlaceholder(grdLrcArea.ActualHeight / 2, Setting.HighlightLrcFontSize);
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CurrentLyricType == LyricType.LrcFormat)
            {
                lrc.Offset += e.Delta / 600.0;
                ShowMessage("当前歌词偏移量：" + (lrc.Offset > 0 ? "+" : "") + Math.Round(lrc.Offset, 2).ToString() + "秒");
            }
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && CurrentLyricType == LyricType.LrcFormat)
            {
                lrc.Offset = 0;
                ShowMessage("恢复歌词偏移量");
            }
        }
    }
}
