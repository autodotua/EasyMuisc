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

namespace EasyMusic.UserControls
{
    /// <summary>
    /// ControlButton.xaml 的交互逻辑
    /// </summary>
    public partial class ControlButton : UserControl
    {
        public ControlButton()
        {
            InitializeComponent();
           // btn.Opacity = NormalOpacity;
        }
        /// <summary>
        /// 矢量形状路径信息
        /// </summary>
        public string PathData { get; set; }

        /// <summary>
        /// 透明度动画
        /// </summary>
        bool opacityAnimation = false;
        /// <summary>
        /// 透明度动画
        /// </summary>
        public bool OpacityAnimation
        {
            get => opacityAnimation;
            set
            {
                btn.Opacity = NormalOpacity;
                opacityAnimation = value;
            }
        }
        public double DisableOpacity { get; set; } = 0.5;
        public bool EnableDisableOpacity { get; set; } = true;
        /// <summary>
        /// 路径边框宽度
        /// </summary>
        public double PathThickness { get; set; }
        /// <summary>
        /// 正常宽度
        /// </summary>
        public double NormalSize { get; set; } = 22;
        /// <summary>
        /// 鼠标在上方的宽度
        /// </summary>
        public double MouseOverSize { get; set; } = 19;
        /// <summary>
        /// 鼠标点击时的宽度
        /// </summary>
        public double PressedSize { get; set; } = 16;
        /// <summary>
        /// 正常时的透明度
        /// </summary>
        public double NormalOpacity { get; set; } = 0.8;

        /// <summary>
        /// 鼠标在上方时的透明度
        /// </summary>
        public double MouseOverOpacity { get; set; } = 1;

        /// <summary>
        /// 单击事件
        /// </summary>
        public event RoutedEventHandler Click;
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
                NewDoubleAnimation(sender as Button, OpacityProperty, MouseOverOpacity, 0.5, 0.3);
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
                NewDoubleAnimation(sender as Button, OpacityProperty, NormalOpacity, 0.5, 0.3);
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
