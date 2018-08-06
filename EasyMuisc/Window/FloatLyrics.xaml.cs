using EasyMusic.UserControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using static EasyMusic.GlobalDatas;
using static WpfCodes.WindowsApi.WindowMode;

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
           // tbkLeft.TextAlignment = TextAlignment.Left;
           // tbkRight.TextAlignment = TextAlignment.Right;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });


            //sbdOpacity.Children.Add(aniOpacity);
        }
        //#region WindowsAPI
        //public const int WS_EX_TRANSPARENT = 0x00000020;
        //public const int GWL_EXSTYLE = (-20);
        //[DllImport("user32.dll")]
        //public static extern int GetWindowLong(IntPtr hwnd, int index);
        //[DllImport("user32.dll")]
        //public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        //int extendedStyle;
        //#endregion
        ///// <summary>
        ///// 设置鼠标穿透
        ///// </summary>
        //private void SetToMouseThrough()
        //{

        //    // Get this window's handle
        //    IntPtr hwnd = new WindowInteropHelper(this).Handle;

        //    // Change the extended window style to include WS_EX_TRANSPARENT
        //    extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        //    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        //}
        ///// <summary>
        ///// 取消鼠标穿透
        ///// </summary>
        //private void SetToNoMouseThrough()
        //{

        //    // Get this window's handle
        //    IntPtr hwnd = new WindowInteropHelper(this).Handle;

        //    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
        //}
        WpfCodes.WindowsApi.WindowMode windowMode;
        /// <summary>
        /// 在加载时设置鼠标穿透
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //SetToMouseThrough();
            windowMode = new WpfCodes.WindowsApi.WindowMode(this);
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
        List<string> lrc;
        /// <summary>
        /// 加载歌词
        /// </summary>
        /// <param name="lrc"></param>
        public void Reload(List<string> lrc)
        {
            CurrentIndex = 0;
            this.lrc = lrc.ToList();
            //lrc.Add("\t");
            //if (lrc.Count > 0)
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
                Update(0);
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
            if(index==-1)
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
            if(lrc[index].Replace(" ","")=="")
            {
                return;
            }
            while (index < lrc.Count - 1 && lrc[index + 1].Replace(" ", "") == "")
            {
                index++;
                //if (index >= lrc.Count - 1)
                //{
                //    return;
                //}
            }
            if(index >= lrc.Count - 1)
            {
                GetTextBlock(1-CurrentIndex).ToMajor(lrc[oldIndex]);
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
            GetTextBlock( CurrentIndex).ToMinor(lrc[index + 1]);
            //}
            //else
            //{
            //    GetTextBlock(1-CurrentIndex).ToMinor("");
            //}
            GetTextBlock(1-CurrentIndex).ToMajor(lrc[oldIndex]);
            CurrentIndex = 1 - CurrentIndex;
            //currentIndex = index;
        }


        private void WindowPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!mouseOverButton)
            {
                DragMove();
            }
            //base.OnPreviewMouseLeftButtonDown(e);
        }

        public void Close(bool mustTrue)
        {
            if(mustTrue)
            {
                closing = true;
            }
            Close();
        }
        private bool closing = false;
        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Setting.FloatLyricsTop = Top;
            Setting.FloatLyricsLeft = Left;
            Setting.FloatLyricsHeight = Height;
            Setting.FloatLyricsWidth = Width;

            if(!closing)
            {
                e.Cancel = true;
            }
        }

        private void BtnOkClickEventHandler(object sender, RoutedEventArgs e)
        {
            Adjuest = false;

        }
        bool mouseOverButton = false;
        private void BtnOkMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            mouseOverButton = true;
        }

        private void BtnOkMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            mouseOverButton = false;
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
