using EasyMusic.Helper;
using FzLib.Program.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static EasyMusic.GlobalDatas;

namespace EasyMusic
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void CheckFiles()
        {
            string[] neededFiles = new string[] {
                "Bass.Net.dll",
                "ControlzEx.dll",
                "CsvHelper.dll",
                "Interop.Shell32.dll",
                "MahApps.Metro.dll",
                "Microsoft.WindowsAPICodePack.dll",
                "Microsoft.WindowsAPICodePack.Shell.dll",
                "Newtonsoft.Json.dll",
                "System.Windows.Interactivity.dll",
                "FzStandardLib.dll",
                "FzWpfControlLib.dll",
                "FzWpfLib.dll",
                "music.png",
                "bass.dll",
                "bass_fx.dll"};

            foreach (var file in neededFiles)
            {
                if (!File.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\" + file))
                {
                    MessageBox.Show("缺少依赖文件" + file, "打开EasyMusic失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                    break;
                }
            }
        }

        private SingleInstance single;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            CheckFiles();

            UnhandledException.RegistAll();
            single = new SingleInstance(EasyMusic.Properties.Resources.AppName);
            if (e.Args.Length > 0 && single.ExistAnotherInstance)
            {
                await PipeHelper.Send("play " + e.Args[0]);
                Environment.Exit(0);
            }
            else
            {
                if (e.Args.Length != 0)
                {
                    argPath = e.Args[0];
                }
                await CheckInstance();
                PipeHelper.RegistClinet();
            }
            UpdateColor();
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private async Task CheckInstance()
        {
            bool ok = false;
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            //Task.Delay(1000).ContinueWith(p =>
            //{
            //    if (!ok)
            //    {
            //        Dispatcher.Invoke(() => DialogHelper.ShowError("存在另一实例，但无法唤醒"));
            //        Environment.Exit(-1);
            //    }
            //}
            //);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            if (await single.CheckAndOpenWindow<MainWindow>(this))
            {
                Environment.Exit(-1);
            }
            else
            {
                ok = true;
            }
        }

        //public async  Task<bool> CheckAnotherInstanceAndOpenWindow<T>(Application app) where T : Window, new()
        //{
        //    string programName = EasyMusic.Properties.Resources.AppName;
        //    if (single.ExistAnotherInstance)
        //    {
        //        SinglePipe.Server pipe = new SinglePipe.Server(programName + "Mutex");
        //        await pipe.SendMessageAsync("OpenWindow");
        //        //await pipe.StopClinetAsync();
        //        pipe.Dispose();
        //        app.Shutdown();
        //        return true;
        //    }
        //    else
        //    {
        //        SinglePipe.Clinet pipe = new SinglePipe.Clinet(programName + "Mutex");
        //        pipe.Start();
        //        pipe.GotMessage += (p1, p2) =>
        //        {
        //            if (p2.Message == "OpenWindow")
        //            {
        //                app.Dispatcher.Invoke(() =>
        //                {
        //                    if (app.MainWindow == null)
        //                    {
        //                        app.MainWindow = new T();
        //                    }
        //                    try
        //                    {
        //                        app.MainWindow.Show();
        //                    }
        //                    catch (InvalidOperationException)
        //                    {
        //                        app.MainWindow = new T();
        //                        app.MainWindow.Show();
        //                    }
        //                    if (app.MainWindow.Visibility != Visibility.Visible)
        //                    {
        //                        app.MainWindow.Visibility = Visibility.Visible;
        //                    }
        //                    if (app.MainWindow.WindowState == WindowState.Minimized)
        //                    {
        //                        app.MainWindow.WindowState = WindowState.Normal;
        //                    }
        //                    app.MainWindow.Activate();
        //                    //pipe.Dispose();
        //                    //pipe.Start();
        //                    //SetForegroundWindow(new WindowInteropHelper(app.MainWindow).Handle);
        //                });
        //            }
        //        };
        //        return false;
        //    }
        //}

        /// <summary>
        /// 更新主题颜色
        /// </summary>
        public void UpdateColor()
        {
            var color = Setting.BackgroundColor;
            Resources["backgroundBrushColor"] = color;
            FzLib.Control.DarkerBrushConverter.GetDarkerColor(color, out SolidColorBrush darker1, out SolidColorBrush darker2, out SolidColorBrush darker3, out SolidColorBrush darker4);
            Resources["darker1BrushColor"] = darker1;
            Resources["darker2BrushColor"] = darker2;
            Resources["darker3BrushColor"] = darker3;
            Resources["darker4BrushColor"] = darker4;
            Resources["backgroundColor"] = color.Color;
            Resources["veryDarkColor"] = darker4.Color;
            Resources["backgroundTransparentColor"] = Color.FromArgb(0, color.Color.R, color.Color.G, color.Color.B);

            Resources["foregroundBrushColor"] = new SolidColorBrush(Colors.Black);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Helper.MusicControlHelper.Music != null)
            {
                Setting.LastMusic = Helper.MusicControlHelper.Music.FilePath;
            }
            Setting.Save();
            trayIcon?.Dispose();
        }
    }
}