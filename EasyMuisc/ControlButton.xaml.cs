using System;
using System.Collections.Generic;
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

namespace EasyMuisc
{
    /// <summary>
    /// ControlButton.xaml 的交互逻辑
    /// </summary>
    public partial class ControlButton : UserControl
    {
        public ControlButton()
        {
            InitializeComponent();
        }
        public string PathData
        {
            set
            {
                Resources["pathG"] = Geometry.Parse(value);
            }
        }

        double normalOpacity = 0.2;

        double mouseOverOpacity = 0.6;

        bool opacityAnimation = false;

        public bool OpacityAnimation {
            get=>opacityAnimation;
                set
            {
                btn.Opacity = NormalOpacity;
                opacityAnimation = value;
            }
        }
        public double PathThickness
        {
            set
            {
                Resources["pathThickness"] = value;
            }
        }

        public double NormalSize
        {
            set
            {
                Resources["viewWidth"] = value;
            }
        }
        public double MouseOverSize
        {
            set
            {
                Resources["tri1Width"] = value;
            }
        }
        public double PressedSize
        {
            set
            {
                Resources["tri2Width"] = value;
            }
        }

        public double NormalOpacity { get => normalOpacity; set => normalOpacity = value; }
        public double MouseOverOpacity { get => mouseOverOpacity; set => mouseOverOpacity = value; }

        public event RoutedEventHandler Click
        {
            add
            {
                btn.AddHandler(Button.ClickEvent, value);
            }
            remove
            {
                btn.RemoveHandler(Button.ClickEvent, value);
            }
        }


        /// <summary>
        /// 创建并启动一个新的浮点数据类型动画
        /// </summary>
        /// <param name="obj">动画主体</param>
        /// <param name="property">更改的属性</param>
        /// <param name="to">目标值</param>
        /// <param name="duration">时间</param>
        /// <param name="decelerationRatio">减缓时间</param>
        /// <param name="completed">完成以后的事件</param>
        /// <returns></returns>
        private Storyboard NewDoubleAnimation(FrameworkElement obj, DependencyProperty property, double to, double duration, double decelerationRatio = 0, EventHandler completed = null, bool stopAfterComplete = false)
        {

            DoubleAnimation ani = new DoubleAnimation
            {
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),//动画时间1秒
                DecelerationRatio = decelerationRatio,
                FillBehavior = stopAfterComplete ? FillBehavior.Stop : FillBehavior.HoldEnd,
            };
            Storyboard.SetTargetName(ani, obj.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(property));
            Storyboard story = new Storyboard();
            story.Children.Add(ani);
            if (completed != null)
            {
                story.Completed += completed;
            }
            story.Begin(obj);
            return story;
        }
        /// <summary>
        /// 鼠标进入动画事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnimitionMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            if (OpacityAnimation)
            {
                NewDoubleAnimation(sender as Button, OpacityProperty, mouseOverOpacity, 0.5, 0.3);
            }
        }
        /// <summary>
        /// 鼠标离开动画事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnimitionMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            if (OpacityAnimation)
            {
                NewDoubleAnimation(sender as Button, OpacityProperty, normalOpacity, 0.5, 0.3);
            }
        }
    }
}
