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
using static EasyMuisc.ShareStaticResources;

namespace EasyMuisc.UserControls
{
    /// <summary>
    /// RradualChangedTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class RradualChangedTextBlock : UserControl
    {
        public RradualChangedTextBlock()
        {
            InitializeComponent();

            DoubleAnimation ani1 = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
                FillBehavior=FillBehavior.Stop
            };
            DoubleAnimation ani2 = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut } ,               
                FillBehavior = FillBehavior.Stop

            };
            Storyboard.SetTargetName(ani1, tbk1.Name);
            Storyboard.SetTargetName(ani2, tbk2.Name);
            Storyboard.SetTargetProperty(ani1, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(ani2, new PropertyPath(OpacityProperty));
            story.Children.Add(ani1);
            story.Children.Add(ani2);
            story.Completed += (p1, p2) =>
              {
                  //story.Stop();
                  tbk1.Opacity = 1;
                  tbk2.Opacity = 0;

              };



        }
        
        public string Text { get => tbk1.Text; set => tbk1.Text = value; }

        public void ToMinor(string text)
        {
            FontSizeAnimation
                  (
                  tbk1,
                  set.FloatLyricsHighlightFontSize,
                  set.FloatLyricsNormalFontSize
                  );
            FontSizeAnimation
             (
             tbk2,
             set.FloatLyricsHighlightFontSize,
             set.FloatLyricsNormalFontSize
             );
            ChangeText(text);

        }
        
        public void ChangeText(string text)
        {
            tbk1.Opacity = 0;
            tbk2.Text = string.Copy(tbk1.Text);
            tbk1.Text = text;
            tbk2.Opacity = 1;
            story.Begin(this);
        }

        Storyboard story = new Storyboard();
        
        public void ToMajor(string text)
        {
            tbk1.Text = text;
            ToMajor();
        }
        public void ToMajor()
        {
            FontSizeAnimation
                  (
                  tbk1,
                               set.FloatLyricsNormalFontSize,

                  set.FloatLyricsHighlightFontSize
                   );
        }

            public void FontSizeAnimation(TextBlock obj, double from,double to)
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
            tbk1.FontSize = set.FloatLyricsNormalFontSize;

        }

        public TextAlignment TextAlignment
        {
            set => tbk1.TextAlignment = tbk2.TextAlignment = value;
        }
    }
}
