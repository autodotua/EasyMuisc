using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using EasyMusic.Windows;
using static EasyMusic.GlobalDatas;
using static FzLib.Control.Dialog.DialogBox;
using EasyMusic.Helper;
using System.Security.Principal;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using FzLib.Windows;
using FzLib.Program;

namespace EasyMusic.UserControls
{
    /// <summary>
    /// Header.xaml 的交互逻辑
    /// </summary>
    public partial class Header : UserControl, INotifyPropertyChanged
    {
        public Header()
        {
            InitializeComponent();
        }

        #region 标题栏
        /// <summary>
        /// 主菜单
        /// </summary>
        ContextMenu mainContextMenu;
        /// <summary>
        /// 窗体上边界
        /// </summary>
        double reservedTop;
        /// <summary>
        /// 单击菜单按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSettingsClickEventHandler(object sender, RoutedEventArgs e)
        {

            if (mainContextMenu != null)
            {
                (mainContextMenu.Items[0] as MenuItem).Header = MainWindow.Current.Topmost ? "取消置顶" : "置顶";
                (mainContextMenu.Items[1] as MenuItem).Header = FileFormatAssociation.IsAssociated(".mp3", Properties.Resources.AppName) ? "取关格式" : "关联格式";
                mainContextMenu.IsOpen = true;
                return;
            }
            MenuItem menuTop = new MenuItem()
            {
                Header = MainWindow.Current.Topmost ? "取消置顶" : "置顶"
            };
            menuTop.Click += (p1, p2) =>
            {
                MainWindow.Current.Topmost = !MainWindow.Current.Topmost;
                Setting.Topmost = MainWindow.Current.Topmost;
            };
            MenuItem menuFileAssociation = new MenuItem() { Header = FileFormatAssociation.IsAssociated(".mp3", Properties.Resources.AppName) ? "取关格式": "关联格式" };
            menuFileAssociation.Click += MenuFileAssociationClick;
            MenuItem menuListenHistory = new MenuItem() { Header = "聆听历史" };
            menuListenHistory.Click += (p1, p2) =>
            {
                WinListenHistory win = new WinListenHistory();
                win.Owner = MainWindow.Current;
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
                new WinSettings() { Owner = MainWindow.Current }.ShowDialog();
            };
            MenuItem menuHelp = new MenuItem()
            {
                Header = "帮助"
            };
            menuHelp.Click += (p1, p2) =>
            {
                new WinHelp() { Owner = MainWindow.Current }.ShowDialog();
            };
            MenuItem menuAbout = new MenuItem()
            {
                Header = "关于"
            };
            menuAbout.Click += (p1, p2) =>
            {
                new WinAbout() { Owner = MainWindow.Current }.ShowDialog();
            };
            mainContextMenu = new ContextMenu()
            {
                PlacementTarget = btnSettings,
                Placement = PlacementMode.Bottom,
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

        private void MenuFileAssociationClick(object sender, RoutedEventArgs e)
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            //if (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            //{
            //    FileFotmatAssociation.Associate(".mp3", EasyMusic.Properties.Resources.AppName, "mp3 文件",FzLib.Program.Information.ProgramDirectoryPath + "\\icon.ico", Process.GetCurrentProcess().MainModule.FileName);
            //    ShowPrompt("成功");
            //}
            //else
            //{
            //    ShowError("需要管理员权限，请用管理员权限打开此程序。");
            //}
            if (FileFormatAssociation.IsAssociated(".mp3", Properties.Resources.AppName))
            {
                FileFormatAssociation.DeleteAssociation(".mp3", Properties.Resources.AppName);
            }
            else
            {
                FileFormatAssociation.SetAssociation(".mp3", Properties.Resources.AppName, "mp3 文件", Information.ProgramDirectoryPath + "\\music.png");
            }
        }
        double mouseDownY = 1000;
        /// <summary>
        /// 鼠标左键在标题栏上按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderPreviewMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Current.WindowState == WindowState.Maximized)
            {
                mouseDownY = e.MouseDevice.GetPosition(sender as Button).Y;
            }
            else
            {
                MainWindow.Current.DragMove();
            }
        }
        /// <summary>
        /// 双击标题栏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Current.WindowState = (MainWindow.Current.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            e.Handled = true;
        }

        /// <summary>
        /// 单击关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseClickEventHandler(object sender, RoutedEventArgs e)
        {
            //if (Setting.TrayMode == 1)
            //{
            //    NewDoubleAnimation(this, OpacityProperty, 0, 0.1, 0, (p1, p2) => { MainWindow.Current.Hide(); }, true);
            //}
            //else
            //{
                MainWindow.Current.Close();
           // }
        }
        /// <summary>
        /// 单击最大化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMaxmizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.WindowState = (MainWindow.Current.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }


        /// <summary>
        /// 单击最小化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinimizeClickEventHandler(object sender, RoutedEventArgs e)
        {
            reservedTop = MainWindow.Current.Top;
            NewDoubleAnimation(MainWindow.Current, OpacityProperty, 0, 0.1, 0, (p1, p2) =>
            {
                //if (Setting.TrayMode == 2)
                //{
                //    MainWindow.Current.Hide();
                //}
                //else
                //{
                    MainWindow.Current.WindowState = WindowState.Minimized;
                //}
                // Opacity = 1;
            }, true);
        }
        /// <summary>
        /// 鼠标是否在专辑图上按下了
        /// </summary>
        bool imgAlbumMousePress = false;
        private string headerText = "EasuMusic";
        private ImageSource albumImageSource;

        public event PropertyChangedEventHandler PropertyChanged;

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
        public string HeaderText
        {
            get => headerText;
            set
            {
                headerText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeaderText"));
            }
        }
        public double HeaderTextMaxWidth { get; set; }

        public ImageSource AlbumImageSource
        {
            get => albumImageSource;
            set
            {
                albumImageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumImageSource"));
            }
        }
        #endregion




        double dpi;
        bool isMoving = false;
        Point rawPoint;
        double rawTop;
        double rawLeft;
        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            Point mousePosition = FzLib.Device.Mouse.Position;
            if (isMoving && btn.IsMouseOver)
            {
                MainWindow.Current.Top = rawTop + (mousePosition.Y - rawPoint.Y) /dpi;
                MainWindow.Current.Left = rawLeft + (mousePosition.X - rawPoint.X) / dpi;
               // Debug.WriteLine(MainWindow.Current.Top + "        " + mousePosition.Y + "       " + rawPoint.Y + "         " + rawTop + "       " + PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.M11);
            }


            if (MainWindow.Current.WindowState == WindowState.Maximized && btn.IsMouseCaptured && e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                if (mousePosition.Y - mouseDownY > 8)
                {
                    MainWindow.Current.Top = 0;
                    MainWindow.Current.WindowState = WindowState.Normal;
                    rawPoint = FzLib.Device.Mouse.Position;
                    rawTop = MainWindow.Current.Top;
                    rawLeft = MainWindow.Current.Left;
                   dpi = VisualTreeHelper.GetDpi(MainWindow.Current).DpiScaleX;
                    isMoving = true;
                }
            }
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isMoving = false;
        }
    }
}
