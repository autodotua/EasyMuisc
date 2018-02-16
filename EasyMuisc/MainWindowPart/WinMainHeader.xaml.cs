using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using EasyMuisc.Windows;
using static EasyMuisc.Tools.Tools;
using static EasyMuisc.ShareStaticResources;
using System.Windows.Media;
using System.Windows.Shell;

namespace EasyMuisc
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
        private void UpdateColor()
        {
            var color = colorPicker.CurrentColor;
            Resources["backgroundBrushColor"] = color;
            Resources["darker1BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.9f, color.Color.ScG * 0.9f, color.Color.ScB * 0.9f));
            Resources["darker2BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.8f, color.Color.ScG * 0.8f, color.Color.ScB * 0.8f));
            Resources["darker3BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.7f, color.Color.ScG * 0.7f, color.Color.ScB * 0.7f));
            Resources["darker4BrushColor"] = new SolidColorBrush(Color.FromScRgb(color.Color.ScA, color.Color.ScR * 0.6f, color.Color.ScG * 0.6f, color.Color.ScB * 0.6f));

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
                set.Topmost = Topmost;
            };
            StackPanel menuColor = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock{Text="背景"},
                    colorPicker,
                },
            };
            colorPicker.ChooseComplete((p1, p2) => mainContextMenu.IsOpen = false);

            MenuItem menuSettings = new MenuItem()
            {
                Header = "设置"
            };
            menuSettings.Click += (p1, p2) =>
            {
                new WinSettings().ShowDialog();
            };
            MenuItem menuHelp = new MenuItem()
            {
                Header = "帮助"
            };
            menuHelp.Click += (p1, p2) =>
            {
                new WinHelp().ShowDialog();
            };
            MenuItem menuAbout = new MenuItem()
            {
                Header = "关于"
            };
            menuAbout.Click += (p1, p2) =>
            {
                new WinAbout().ShowDialog();
            };
            mainContextMenu = new ContextMenu()
            {
                PlacementTarget = btnSettings,
                IsOpen = true,
                Items = { menuTop, menuColor, menuSettings, menuHelp, menuAbout },
            };
            mainContextMenu.Closed += (p1, p2) => UpdateColor();
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
            if (set.TrayMode == 1)
            {
                NewDoubleAnimation(this, TopProperty, SystemParameters.FullPrimaryScreenHeight, 0.2, 0, (p1, p2) =>
                {
                    Top = reservedTop;
                    Hide();
                }, true);
            }
            else
            {
                Close();
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
            NewDoubleAnimation(this, TopProperty, SystemParameters.FullPrimaryScreenHeight, 0.2, 0, (p1, p2) =>
            {
                Top = reservedTop;
                if (set.TrayMode == 2)
                {
                    Hide();
                }
                else
                {
                    WindowState = WindowState.Minimized;
                }
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
                WinAlbumPicture win = new WinAlbumPicture(this);
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
        /// 读取Mp3信息
        /// </summary>
        /// <param name="path"></param>
        private void ReadMusicSourceInfo(string path)//copy
        {
            string[] tags = new string[6];
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            string mp3ID = "";
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }
                ReadFrame();
            }
            void ReadFrame()//copy
            {
                while (size > 0)
                {
                    //读取标签帧头的10个字节
                    fs.Read(buffer, 0, 10);
                    size -= 10;
                    //得到标签帧ID
                    string FramID = Encoding.Default.GetString(buffer, 0, 4);
                    //计算标签帧大小，第一个字节代表帧的编码方式
                    int frmSize = 0;

                    frmSize = buffer[4] * 0x1000000 + buffer[5] * 0x10000 + buffer[6] * 0x100 + buffer[7];
                    if (frmSize == 0)
                    {
                        //就说明真的没有信息了
                        break;
                    }
                    //bFrame 用来保存帧的信息
                    byte[] bFrame = new byte[frmSize];
                    fs.Read(bFrame, 0, frmSize);
                    size -= frmSize;
                    string str = GetFrameInfoByEcoding(bFrame, bFrame[0], frmSize - 1);
                    imgAlbum.Source = null;
                    imgAlbum.Visibility = Visibility.Collapsed;
                    if (FramID.CompareTo("APIC") == 0)
                    {
                        try
                        {
                            int i = 0;
                            while (true)
                            {
                                if (255 == bFrame[i] && 216 == bFrame[i + 1])
                                {
                                    break;
                                }
                                i++;
                            }
                            byte[] imge = new byte[frmSize - i];
                            fs.Seek(-frmSize + i, SeekOrigin.Current);
                            fs.Read(imge, 0, imge.Length);
                            MemoryStream ms = new MemoryStream(imge);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                            string imgPath = Path.GetTempFileName();
                            FileStream save = new FileStream(imgPath, FileMode.Create);
                            img.Save(save, System.Drawing.Imaging.ImageFormat.Jpeg);
                            save.Close();
                            imgAlbum.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imgPath));
                            imgAlbum.Visibility = Visibility.Visible;
                        }
                        catch
                        {
                        }
                    }
                }

            }
            string GetFrameInfoByEcoding(byte[] b, byte conde, int length)//copy
            {
                string str = "";
                switch (conde)
                {
                    case 0:
                        str = Encoding.GetEncoding("ISO-8859-1").GetString(b, 1, length);
                        break;
                    case 1:
                        str = Encoding.GetEncoding("UTF-16LE").GetString(b, 1, length);
                        break;
                    case 2:
                        str = Encoding.GetEncoding("UTF-16BE").GetString(b, 1, length);
                        break;
                    case 3:
                        str = Encoding.UTF8.GetString(b, 1, length);
                        break;
                }
                return str;
            }
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
}
