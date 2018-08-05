using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using EasyMusic.Windows;
using static EasyMusic.Tools.Tools;
using static EasyMusic.GlobalDatas;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;
using static WpfControls.Dialog.DialogHelper;

namespace EasyMusic
{
    public partial class MainWindow : Window
    {

        #region 标题栏
        /// <summary>
        /// 主菜单
        /// </summary>
        ContextMenu mainContextMenu;
        /// <summary>
        /// 鼠标是否正在标题栏上且按下
        /// </summary>
        private bool headerMouseDowning = false;
        /// <summary>
        /// 窗体上边界
        /// </summary>
        double reservedTop;
        /// <summary>
        /// 更新主题颜色
        /// </summary>
        public void UpdateColor(SolidColorBrush color)
        {
            //var color = colorPicker.CurrentColor;
            Resources["backgroundBrushColor"] = color;
            WpfControls.DarkerBrushConverter.GetDarkerColor(color, out SolidColorBrush darker1, out SolidColorBrush darker2, out SolidColorBrush darker3, out SolidColorBrush darker4);
            //Resources["darker1BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.9f, color.Color.ScG * 0.9f, color.Color.ScB * 0.9f));
            //Resources["darker2BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.8f, color.Color.ScG * 0.8f, color.Color.ScB * 0.8f));
            //Resources["darker3BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.7f, color.Color.ScG * 0.7f, color.Color.ScB * 0.7f));
            //Resources["darker4BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.6f, color.Color.ScG * 0.6f, color.Color.ScB * 0.6f));
            Resources["darker1BrushColor"] = darker1;
            Resources["darker2BrushColor"] = darker2;
            Resources["darker3BrushColor"] = darker3;
            Resources["darker4BrushColor"] = darker4;
            Resources["backgroundColor"] = color.Color;
            Resources["backgroundTransparentColor"] = Color.FromArgb(0, color.Color.R, color.Color.G, color.Color.B);

            Resources["foregroundBrushColor"] = new SolidColorBrush(Colors.Black);

        }
        /// <summary>
        /// 单击菜单按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSettingsClickEventHandler(object sender, RoutedEventArgs e)
        {

            if (mainContextMenu != null)
            {
                (mainContextMenu.Items[0] as MenuItem).Header = Topmost ? "取消置顶" : "置顶";
                mainContextMenu.IsOpen = true;
                return;
            }
            MenuItem menuTop = new MenuItem()
            {
                Header = Topmost ? "取消置顶" : "置顶"
            };
            menuTop.Click += (p1, p2) =>
            {
                Topmost = !Topmost;
                Setting.Topmost = Topmost;
            };
            MenuItem menuFileAssociation = new MenuItem() { Header = "注册格式" };
            menuFileAssociation.Click += MenuFileAssociationClickEventHandler;
            MenuItem menuListenHistory = new MenuItem() { Header = "聆听历史" };
            menuListenHistory.Click += (p1, p2) =>
            {
                WinListenHistory win = new WinListenHistory();
                win.Owner = this;
                win.Show();
            };

            //StackPanel menuColor = new StackPanel()
            //{
            //    Orientation = Orientation.Horizontal,
            //    Children =
            //    {
            //        new TextBlock{Text="背景"},
            //        colorPicker,
            //    },
            //};
            //colorPicker.ChooseComplete((p1, p2) => mainContextMenu.IsOpen = false);

            MenuItem menuSettings = new MenuItem()
            {
                Header = "设置"
            };
            menuSettings.Click += (p1, p2) =>
            {
                new WinSettings() { Owner = this }.ShowDialog();
            };
            MenuItem menuHelp = new MenuItem()
            {
                Header = "帮助"
            };
            menuHelp.Click += (p1, p2) =>
            {
                new WinHelp() { Owner = this }.ShowDialog();
            };
            MenuItem menuAbout = new MenuItem()
            {
                Header = "关于"
            };
            menuAbout.Click += (p1, p2) =>
            {
                new WinAbout() { Owner = this }.ShowDialog();
            };
            mainContextMenu = new ContextMenu()
            {
                PlacementTarget = btnSettings,
                IsOpen = true,
            };
            mainContextMenu.Items.Add(menuTop);
            mainContextMenu.Items.Add(menuFileAssociation);
            if (Setting.RecordListenHistory)
            {
                mainContextMenu.Items.Add(menuListenHistory);
            }
            mainContextMenu.Items.Add(menuSettings);
            mainContextMenu.Items.Add(menuHelp);
            mainContextMenu.Items.Add(menuAbout);

            //mainContextMenu.Closed += (p1, p2) => UpdateColor();
        }

        private void MenuFileAssociationClickEventHandler(object sender, RoutedEventArgs e)
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            if (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                FileAssociation.Associate(".mp3", "EasyMusic", "mp3 文件", AppDomain.CurrentDomain.BaseDirectory + "icon.ico", Process.GetCurrentProcess().MainModule.FileName);
                ShowPrompt("成功");
            }
            else
            {
                ShowError("需要管理员权限，请用管理员权限打开此程序。");
            }
        }

        /// <summary>
        /// 鼠标左键在标题栏上按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// 双击标题栏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }
        /// <summary>
        /// 鼠标在标题栏上按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            headerMouseDowning = true;
        }
        /// <summary>
        /// 鼠标在标题栏上抬起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            headerMouseDowning = false;
        }
        /// <summary>
        /// 单击关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (Setting.TrayMode == 1)
            {
                //NewDoubleAnimation(this, TopProperty, SystemParameters.FullPrimaryScreenHeight, 0.2, 0, (p1, p2) =>
                //{
                //    Top = reservedTop;
                //    Hide();
                //}, true);
                NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0, (p1, p2) => { Hide(); }, true);
            }
            else
            {
                CloseWindow();
            }
        }
        /// <summary>
        /// 单击最大化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMaxmizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }


        /// <summary>
        /// 单击最小化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinimizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            reservedTop = Top;
            NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0, (p1, p2) =>
            {
                if (Setting.TrayMode == 2)
                {
                    Hide();
                }
                else
                {
                    WindowState = WindowState.Minimized;
                }
               // Opacity = 1;
            }, true);
        }
        /// <summary>
        /// 鼠标是否在专辑图上按下了
        /// </summary>
        bool imgAlbumMousePress = false;
        /// <summary>
        /// 单击专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumPreviewMouseUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (imgAlbumMousePress)
            {
                WinAlbumPicture win = new WinAlbumPicture();
                win.img.Source = imgAlbum.Source;
                win.ShowDialog();
                imgAlbumMousePress = false;
            }
        }
        /// <summary>
        /// 鼠标按下专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumPreviewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            //不知什么原因，鼠标在Header上按下会出发DragMove然后触发了鼠标在专辑图上抬起事件，所以写此事件确保鼠标确实是在专辑图上
            imgAlbumMousePress = true;
        }
        /// <summary>
        /// 鼠标离开专辑图事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgAlbumMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            imgAlbumMousePress = false;
        }
        /// <summary>
        /// 窗体状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinMainStateChangedEventHandler(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(4);
            }
        }
        #endregion
    }


    public class FileAssociation
    {
        // Associate file extension with progID, description, icon and application
        public static void Associate(string extension,
               string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("",
                                    ToShortPathName(application) + " \"%1\"");
                }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,
            [Out] StringBuilder lpszShortPath, uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }
    }
}
