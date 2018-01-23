using EasyMuisc.UserControls;
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

namespace EasyMuisc.Windows
{
    /// <summary>
    /// FloatLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class FloatLyrics : Window
    {
        Properties.Settings set;

        public FloatLyrics(Properties.Settings set)
        {

            InitializeComponent();
            this.set = set;
            tbkLeft.set = set;
            tbkRight.set = set;
            tbkLeft.TextAlignment = TextAlignment.Left;
            tbkRight.TextAlignment = TextAlignment.Right;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
            });
            

            //sbdOpacity.Children.Add(aniOpacity);
        }



        private bool adjuesting;
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
            }
            get => adjuesting;
        }
        List<string> lrc;

        public void ReLoadLrc(List<string> lrc)
        {
            this.lrc = new List<string>(lrc);
            if (lrc.Count > 0)
            {
                tbkLeft.Text = lrc[0];
                tbkRight.Text = lrc[1];
            }
        }
        int currentIndex = 0;

        private RradualChangedTextBlock GetTextBlock(int index)
        {
            return (stk.Children[index] as RradualChangedTextBlock);
        }
        public void Update(int index)
        {
            ChangeLrc(index);
            currentIndex = index;
        }
        private void ChangeLrc(int index)
        {
            if (index < lrc.Count)
            {
                GetTextBlock((index+1) % 2).ToMinor(lrc[index + 1]);
            }
            else
            {
                GetTextBlock((index + 1) % 2).ToMinor("");

            }
            GetTextBlock(index % 2).ToMajor(lrc[index]);
        }


        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!mouseOverButton)
            {
                DragMove();
            }
            //base.OnPreviewMouseLeftButtonDown(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            set.FloatLyricsTop =Top;
            set.FloatLyricsLeft =Left;
            set.FloatLyricsHeight =Height;
            set.FloatLyricsWidth =Width;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Adjuest = false;

        }
        bool mouseOverButton = false;
        private void btnOk_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseOverButton = true;
        }

        private void btnOk_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseOverButton = false;
        }
        double lastHeight;
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
           
            if(!adjuesting)
            {
                lastHeight = ActualHeight;
                Height = Tools.OtherTools.ScreenHight - Top + ActualHeight;
                ThicknessAnimation ani = new ThicknessAnimation(new Thickness(0, Tools.OtherTools.ScreenHight - Top,0,0), TimeSpan.FromSeconds(0.3));
                stk.BeginAnimation(MarginProperty, ani);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            return;
            if (!adjuesting)
            {
                Height = lastHeight;
                   ThicknessAnimation ani = new ThicknessAnimation(new Thickness(0), TimeSpan.FromSeconds(0.3));
                stk.BeginAnimation(MarginProperty, ani);

            }
        }

        private void winFloatLyrics_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("safdas");
        }

    }
}
