using EasyMusic.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using static EasyMusic.GlobalDatas;

namespace EasyMusic.Windows
{
    /// <summary>
    /// FloatLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class FloatLyrics : Window
    {
        public FloatLyrics()
        {
            InitializeComponent();
            Top = Setting.FloatLyricsTop;
            Left = Setting.FloatLyricsLeft;
            Height = Setting.FloatLyricsHeight;
            Width = Setting.FloatLyricsWidth;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });

            //sbdOpacity.Children.Add(aniOpacity);
        }

        private FzLib.Windows.WindowStyle windowMode;

        /// <summary>
        /// 在加载时设置鼠标穿透
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //SetToMouseThrough();
            windowMode = new FzLib.Windows.WindowStyle(this);
            windowMode.SetToMouseThrough();
        }

        /// <summary>
        /// 是否正在调整歌词位置、大小
        /// </summary>
        private bool adjuesting;

        /// <summary>
        /// 调整歌词位置、大小
        /// </summary>
        public bool Adjuest
        {
            set
            {
                adjuesting = value;
                Background = new SolidColorBrush(value ? Colors.Gray : Colors.Transparent);
                BorderThickness = new Thickness((value ? 4 : 0));
                ResizeMode = value ? ResizeMode.CanResize : ResizeMode.NoResize;
                IsHitTestVisible = value;
                btnOk.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (value)
                {
                    windowMode.SetToNormal();
                }
                else
                {
                    windowMode.SetToMouseThrough();
                }
            }
            get => adjuesting;
        }

        /// <summary>
        /// 歌词列表
        /// </summary>
        private string[] lrc;

        /// <summary>
        /// 加载歌词
        /// </summary>
        /// <param name="lrc"></param>
        public void Reload(IEnumerable<string> lrc, int position = 0)
        {
            CurrentIndex = 0;
            if (Setting.ShowOneLineInFloatLyric)
            {
                this.lrc = lrc.Select(p => p.Contains(Environment.NewLine) ? p.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0] : p).ToArray();
            }
            else
            {
                this.lrc = lrc.ToArray();
            }
            //lrc.Add("\t");
            //if (lrc.Count > 0))
            //{
            //    tbkLeft.Text = lrc[0];
            //    //  Update(0);
            //    if(lrc.Count>1)
            //    {
            //        tbkRight.Text = lrc[1];
            //    }
            //}

            try
            {
                Update(position);
            }
            catch
            { }
        }

        /// <summary>
        /// 根据索引计算应该使用哪一个文本框
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private RradualChangedTextBlock GetTextBlock(int index)
        {
            return (stk.Children[index] as RradualChangedTextBlock);
        }

        private int CurrentIndex = 0;

        /// <summary>
        /// 通知改变当前歌词
        /// </summary>
        /// <param name="index"></param>
        public void Update(int index)
        {
            if (index == -1)
            {
                tbkLeft.Text = tbkRight.Text = "";
                return;
            }

            int oldIndex = index;
            //if (index == lrc.Count - 1)
            //{
            //    GetTextBlock( CurrentIndex).ToMinor("");
            //    return;
            //}
            if (lrc[index].Replace(" ", "") == "")
            {
                return;
            }
            while (index < lrc.Length - 1 && lrc[index + 1].Replace(" ", "") == "")
            {
                index++;
                //if (index >= lrc.Count - 1)
                //{
                //    return;
                //}
            }
            if (index >= lrc.Length - 1)
            {
                GetTextBlock(1 - CurrentIndex).ToMajor(lrc[oldIndex]);
                GetTextBlock(CurrentIndex).ToMinor("");
                return;
            }
            if (index < 0 || lrc == null)
            {
                Clear();
                return;
            }
            //if (index < lrc.Count - 1)
            //{
            GetTextBlock(CurrentIndex).ToMinor(lrc[index + 1]);
            //}
            //else
            //{
            //    GetTextBlock(1-CurrentIndex).ToMinor("");
            //}
            GetTextBlock(1 - CurrentIndex).ToMajor(lrc[oldIndex]);
            CurrentIndex = 1 - CurrentIndex;
            //currentIndex = index;
        }

        private void WindowPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!btnOk.IsMouseOver)
            {
                DragMove();
            }
            //base.OnPreviewMouseLeftButtonDown(e);
        }

        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Setting.FloatLyricsTop = Top;
            Setting.FloatLyricsLeft = Left;
            Setting.FloatLyricsHeight = Height;
            Setting.FloatLyricsWidth = Width;
        }

        private void BtnOkClickEventHandler(object sender, RoutedEventArgs e)
        {
            Adjuest = false;
        }

        public void Clear()
        {
            GetTextBlock(0).Text = GetTextBlock(1).Text = "";
        }

        public void SetFontEffect()
        {
            tbkLeft.SetEffect();
            tbkRight.SetEffect();
        }
    }
}