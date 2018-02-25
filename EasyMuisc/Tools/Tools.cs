using EasyMuisc.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Security.Cryptography;

namespace EasyMuisc.Tools
{
    public static class Tools
    {
        ///// <summary>
        ///// 显示错误信息
        ///// </summary>
        ///// <param name="message"></param>
        //public static bool ShowAlert(string message, MessageBoxButton button = MessageBoxButton.OK)
        //{
        //    if (button == MessageBoxButton.YesNo)
        //    {
        //        if (MessageBox.Show(message, "错误", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    else
        //    {
        //        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    return true;
        //}
        /// <summary>
        /// 菜单分隔栏
        /// </summary>
        public static UserControls.SeparatorLine NewSeparatorLine => new UserControls.SeparatorLine();
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
        public static Storyboard NewDoubleAnimation(FrameworkElement obj, DependencyProperty property, double to, double duration, double decelerationRatio = 0, EventHandler completed = null, bool stopAfterComplete = false, EasingFunctionBase easingFunction = null)
        {
            DoubleAnimation ani = new DoubleAnimation
            {
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),//动画时间1秒
                DecelerationRatio = decelerationRatio,
                FillBehavior = stopAfterComplete ? FillBehavior.Stop : FillBehavior.HoldEnd,
                EasingFunction = easingFunction,
            };
            

            Storyboard.SetTargetName(ani, obj.Name);
            Storyboard.SetTargetProperty(ani, new PropertyPath(property));
            Storyboard story = new Storyboard();
            //Debug.WriteLine(Timeline.GetDesiredFrameRate(story));

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
        /// <summary>
        /// 延时
        /// </summary>
        /// <param name="millisecond">延时时间</param>
        /// <param name="tick">延时结束后的事件</param>
        public static DispatcherTimer SleepThenDo(double millisecond, EventHandler tick)
        {
            DispatcherTimer waitTimer = new DispatcherTimer();
            waitTimer.Interval = new TimeSpan(10000 * (long)millisecond);
            waitTimer.Tick += tick;
            waitTimer.Tick += (p1, p2) => waitTimer.Stop();
            waitTimer.Start();
            return waitTimer;
        }
        /// <summary>
        /// 处理事件
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            try
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback((p1) => { ((DispatcherFrame)frame).Continue = false; return null; }), frame);
                Dispatcher.PushFrame(frame);
            }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// 把对象序列化为字节数组
        /// </summary>
        public static byte[] SerializeObject(object obj)
        {
            if (obj == null)
                return null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, bytes.Length);
            ms.Close();
            return bytes;
        }

        /// <summary>
        /// 把字节数组反序列化成对象
        /// </summary>
        public static object DeserializeObject(byte[] bytes)
        {
            object obj = null;
            if (bytes == null)
                return obj;
            MemoryStream ms = new MemoryStream(bytes)
            {
                Position = 0
            };
            BinaryFormatter formatter = new BinaryFormatter();
            obj = formatter.Deserialize(ms);
            ms.Close();
            return obj;
        }

        public static double ScreenHight => SystemParameters.PrimaryScreenHeight;
        public static double ScreenWidth => SystemParameters.PrimaryScreenWidth;
        public static int GetRandomNumber(int from, int smallerThan)
        {
            RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
            byte[] b = new byte[8];
            r.GetBytes(b);
            return (b[0] + b[1] + b[2] + b[3] + b[4] + b[5] + b[6] + b[7]) % (smallerThan - from) + from;
        }

    }
}
