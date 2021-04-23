using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static EasyMusic.GlobalDatas;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// LrcListView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricList : UserControl
    {
        //ScrollViewer scroll;
        public LyricList()
        {
            InitializeComponent();
            DataContext = this;
        }

        //public LrcListView(ObservableCollection<string> lrcs)
        //{
        //    lbx.ItemsSource = Lrcs;
        //}
        private List<double> sumHeights = new List<double>() { 0 };

        private List<double> heights = new List<double>();

        public void Add(TextBlock tbk)
        {
            var item = new ListBoxItem() { Content = tbk, Foreground = Foreground, FontWeight = FontWeight };
            item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double height = item.DesiredSize.Height;
            sumHeights.Add(sumHeights[sumHeights.Count - 1] + height);
            heights.Add(height);
            lbx.Items.Add(item);
        }

        //public void ChangeFontSize(double size)
        //{
        //    lbx.FontSize = size;
        //}
        public void Clear()
        {
            lbx.Items.Clear();
            sumHeights.Clear();
            sumHeights.Add(0);
            heights.Clear();
        }

        public void RefreshPlaceholder(double height, double highLightFontSize)
        {
            Resources["topHalfHeight"] = height - highLightFontSize / 2;
            Resources["bottomHalfHeight"] = height - highLightFontSize;
        }

        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            // e.Handled = true;
        }

        public void ScrollTo(int index, List<int> indexArray)
        {
            //double fontSize = Setting.NormalLrcFontSize;
            //int lines = (index == 0 ? 0 : (indexArray[index - 1] - 1));
            //double height = lines * fontSize * FontFamily.LineSpacing * FontFamily.Baseline;// (1 + 1.8 / fontSize);//瞎几把乱写的公式

            double height = sumHeights[index] + 0.5 * heights[index];
            DoubleAnimation ani = new DoubleAnimation(-height, Setting.AnimationDuration) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(ani, lbx);
            Storyboard.SetTargetProperty(ani, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard storyToSmall = new Storyboard() { Children = { ani } };
            storyToSmall.Begin();
        }

        private DoubleAnimation aniFontSize = new DoubleAnimation
        {
            Duration = Setting.AnimationDuration,// new Duration(TimeSpan.FromSeconds(0.8)),//动画时间1秒
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut },
            //DecelerationRatio = 0.5,
        };

        public void RefreshFontSize(int index)
        {
            //return;

            for (int i = 0; i < lbx.Items.Count; i++)
            {
                var txt = ((lbx.Items[i] as ListBoxItem).Content as TextBlock);
                if (i == index)
                {
                    //    //txt.FontSize = highlight;
                    //Tools.NewDoubleAnimation(
                    //    txt,
                    //    FontSizeProperty,
                    //    highlight,
                    //    0.8,0.5
                    //    );
                    aniFontSize.To = Setting.HighlightLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                    //BeginStoryboard(story);
                }
                else if (txt.FontSize != Setting.NormalLrcFontSize)
                {
                    //txt.FontSize = normal;
                    aniFontSize.To = Setting.NormalLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                }
            }
        }
    }
}