using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static EasyMusic.Tools.Tools;
using static EasyMusic.GlobalDatas;
using EasyMusic.Tools;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// LrcListView.xaml 的交互逻辑
    /// </summary>
    public partial class LrcListView : UserControl
    {
        //ScrollViewer scroll;
        public LrcListView()
        {
            InitializeComponent();
            DataContext = this;
        }

        //public LrcListView(ObservableCollection<string> lrcs)
        //{
        //    lbx.ItemsSource = Lrcs;
        //}

        public void Add(TextBlock txt)
        {
            var item = new ListBoxItem() { Content = txt, Foreground = Foreground, FontWeight = FontWeight };
            lbx.Items.Add(item);
        }

        //public void ChangeFontSize(double size)
        //{
        //    lbx.FontSize = size;
        //}
        public void Clear()
        {
            lbx.Items.Clear();
        }
        public void RefreshPlaceholder(double height, double highLightFontSize)
        {
            Resources["topHalfHeight"] = height - highLightFontSize / 2;
            Resources["bottomHalfHeight"] = height - highLightFontSize;

        }

        private void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        public void ScrollTo(int index, List<int> indexArray, double fontSize)
        {
            double height = (indexArray[index] - 1) * fontSize * FontFamily.LineSpacing * (1 + 1.8 / fontSize);//瞎几把乱写的公式

            DoubleAnimation ani = new DoubleAnimation(-height, Setting.AnimationDuration) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(ani, lbx);
            Storyboard.SetTargetProperty(ani, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard storyToSmall = new Storyboard() { Children = { ani } };
            storyToSmall.Begin();
        }



        DoubleAnimation aniFontSize = new DoubleAnimation
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



    