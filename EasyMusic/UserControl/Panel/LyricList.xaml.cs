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
            double height = sumHeights[index] + 0.5 * heights[index];
            DoubleAnimation ani = new DoubleAnimation(-height, Setting.AnimationDuration) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(ani, lbx);
            Storyboard.SetTargetProperty(ani, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard storyToSmall = new Storyboard() { Children = { ani } };
            storyToSmall.Begin();
        }

        private DoubleAnimation aniFontSize = new DoubleAnimation
        {
            Duration = Setting.AnimationDuration,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut },
        };

        private DoubleAnimation aniOpacity = new DoubleAnimation
        {
            Duration = Setting.AnimationDuration,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut },
        };

        public void RefreshFontSize(int index)
        {
            for (int i = 0; i < lbx.Items.Count; i++)
            {
                var txt = ((lbx.Items[i] as ListBoxItem).Content as TextBlock);
                if (i == index)
                {
                    aniFontSize.To = Setting.HighlightLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                }
                else if (txt.FontSize != Setting.NormalLrcFontSize)
                {
                    aniFontSize.To = Setting.NormalLrcFontSize;
                    txt.BeginAnimation(TextBlock.FontSizeProperty, aniFontSize);
                }
            }
        }

        public void RefreshFontOpacity(int index)
        {
            for (int i = 0; i < lbx.Items.Count; i++)
            {
                var txt = ((lbx.Items[i] as ListBoxItem).Content as TextBlock);
                if (i == index)
                {
                    aniOpacity.To = 1;
                    txt.BeginAnimation(OpacityProperty, aniOpacity);
                    txt.FontWeight = FontWeights.Bold;
                }
                else if (txt.Opacity != Setting.NormalLrcOpacity)
                {
                    aniOpacity.To = Setting.NormalLrcOpacity;
                    txt.BeginAnimation(OpacityProperty, aniOpacity);
                    txt.FontWeight = FontWeights.Normal;
                }
            }
        }
    }
}