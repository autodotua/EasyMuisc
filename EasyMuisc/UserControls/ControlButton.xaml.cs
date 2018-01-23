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

namespace EasyMuisc.UserControls
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
        /// <summary>
        /// 矢量形状路径信息
        /// </summary>
        public string PathData
        {
            set
            {
                Resources["pathG"] = Geometry.Parse(value);
            }
        }
        /// <summary>
        /// 正常情况下的透明度（需开启opacityAnimation）
        /// </summary>
        double normalOpacity = 0.2;
        /// <summary>
        /// 鼠标在上空情况下的透明度（需开启opacityAnimation）
        /// </summary>
        double mouseOverOpacity = 0.6;
        /// <summary>
        /// 透明度动画
        /// </summary>
        bool opacityAnimation = false;
        /// <summary>
        /// 透明度动画
        /// </summary>
        public bool OpacityAnimation {
            get=>opacityAnimation;
                set
            {
                btn.Opacity = NormalOpacity;
                opacityAnimation = value;
            }
        }
        /// <summary>
        /// 路径边框宽度
        /// </summary>
        public double PathThickness
        {
            set
            {
                Resources["pathThickness"] = value;
            }
        }
        /// <summary>
        /// 正常宽度
        /// </summary>
        public double NormalSize
        {
            set
            {
                Resources["viewWidth"] = value;
            }
        }
        /// <summary>
        /// 鼠标在上方的宽度
        /// </summary>
        public double MouseOverSize
        {
            set
            {
                Resources["tri1Width"] = value;
            }
        }
        /// <summary>
        /// 鼠标点击时的宽度
        /// </summary>
        public double PressedSize
        {
            set
            {
                Resources["tri2Width"] = value;
            }
        }
        /// <summary>
        /// 正常时的透明度
        /// </summary>
        public double NormalOpacity { get => normalOpacity; set => normalOpacity = value; }
        /// <summary>
        /// 鼠标在上方时的透明度
        /// </summary>
        public double MouseOverOpacity { get => mouseOverOpacity; set => mouseOverOpacity = value; }
        /// <summary>
        /// 单击事件
        /// </summary>
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
