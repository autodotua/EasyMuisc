using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace EasyMuisc
{
    public static class Tools
    {
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message"></param>
        public static bool ShowAlert(string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            if (button == MessageBoxButton.YesNo)
            {
                if (MessageBox.Show(message, "错误", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    return true;
                }
                return false;
            }
            else
            {
                MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }
        /// <summary>
        /// 菜单分隔栏
        /// </summary>
        public static System.Windows.Shapes.Line SeparatorLine
        {
            get
            {
                return new System.Windows.Shapes.Line()
                {
                    X1 = 0,
                    X2 = 140,
                    Y1 = 0,
                    Y2 = 0,
                    Stroke = System.Windows.Media.Brushes.LightGray,
                };
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
        public static Storyboard NewDoubleAnimation(FrameworkElement obj, DependencyProperty property, double to, double duration, double decelerationRatio = 0, EventHandler completed = null, bool stopAfterComplete = false)
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
            Debug.WriteLine(Timeline.GetDesiredFrameRate(story));

            story.Children.Add(ani);
            if (completed != null)
            {
                story.Completed += completed;
            }
            story.Begin(obj);
            return story;
        }
        /// <summary>
        /// 支持多个过滤器的枚举
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static string[] EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string i in searchPatterns)
            {
                files.AddRange(Directory.EnumerateFiles(path, i, searchOption));
            }
            return files.ToArray();
        }

    }
}
