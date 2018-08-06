using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static EasyMusic.GlobalDatas;

namespace EasyMusic.UserControl
{
    /// <summary>
    /// RradualChangedTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class RradualChangedTextBlock : System.Windows.Controls.UserControl
    {
        dynamic text1;
        dynamic text2;
        public RradualChangedTextBlock()
        {
            InitializeComponent();
            SetEffect();
            DoubleAnimation ani1 = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            DoubleAnimation ani2 = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop

            };
            Storyboard.SetTargetName(ani1, text1.Name);
            Storyboard.SetTargetName(ani2, text2.Name);
            Storyboard.SetTargetProperty(ani1, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(ani2, new PropertyPath(OpacityProperty));
            story.Children.Add(ani1);
            story.Children.Add(ani2);
            story.Completed += (p1, p2) =>
            {
                //story.Stop();
                text1.Opacity = 1;
                text2.Opacity = 0;

            };


        }

        public void SetEffect()
        {
            if (Setting.FloatLyricsFontEffect == 0)
            {
                text1 = lbl1;
                text2 = lbl2;
                gLabel.Visibility = Visibility.Visible;
                gTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                text1 = tbk1;
                text2 = tbk2;
                gLabel.Visibility = Visibility.Collapsed;
                gTextBlock.Visibility = Visibility.Visible;
            }

            SolidColorBrush borderBrush = new BrushConverter().ConvertFrom(Setting.FloatLyricsBorderColor) as SolidColorBrush;
            Resources["borderBrush"] = borderBrush;
            Resources["borderColor"] = borderBrush.Color;
            Resources["thickness"] = Setting.FloatLyricsThickness;
            Resources["blurRadius"] = Setting.FloatLyricsBlurRadius;
            Resources["fontBrush"] = new BrushConverter().ConvertFrom(Setting.FloatLyricsFontColor) as SolidColorBrush;
            Resources["bold"] = Setting.FloatLyricsFontBold ? FontWeights.Bold : FontWeights.Normal;
            try
            {
                Resources["font"] = new FontFamily(Setting.FloatLyricsFont);
            }
            catch
            {
                trayIcon.ShowMessage("选取的悬浮歌词字体无效");
            }
        }

        public string Text { get => text1.Text; set => text1.Text = value; }

        public void ToMinor(string text)
        {
            FontSizeAnimation
                  (
                  text1,
                  Setting.FloatLyricsHighlightFontSize,
                  Setting.FloatLyricsNormalFontSize
                  );
            FontSizeAnimation
             (
             text2,
             Setting.FloatLyricsHighlightFontSize,
             Setting.FloatLyricsNormalFontSize
             );
            ChangeText(text);

        }

        public void ChangeText(string text)
        {
            text1.Opacity = 0;
            text2.Text = string.Copy(text1.Text);
            text1.Text = text;
            text2.Opacity = 1;
            story.Begin(this);
        }

        Storyboard story = new Storyboard();

        public void ToMajor(string text)
        {
            text1.Text = text;
            ToMajor();
        }
        public void ToMajor()
        {
            FontSizeAnimation
                  (
                  text1,
                  Setting.FloatLyricsNormalFontSize,
                  Setting.FloatLyricsHighlightFontSize
                   );
        }

        public void FontSizeAnimation(dynamic obj, double from, double to)
        {
            DoubleAnimation ani = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };


            Storyboard.SetTargetName(ani, obj.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(FontSizeProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            story.Begin(obj);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            text1.FontSize = Setting.FloatLyricsNormalFontSize;

        }

        public TextAlignment TextAlignment
        {
            set
            {
                if (text1 is TextBlock)
                {
                    text1.TextAlignment = text2.TextAlignment = value;
                }
                else
                {
                    text1.HorizontalContentAlignment = text2.HorizontalContentAlignment = text1.HorizontalAlignment = text2.HorizontalAlignment =
                        value == TextAlignment.Left ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                }
            }
        }
    }
}
