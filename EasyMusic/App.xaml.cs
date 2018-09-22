using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
                "Microsoft.WindowsAPICodePack.ExtendedLinguisticServices.dll",
                "Microsoft.WindowsAPICodePack.Sensors.dll",
                "Microsoft.WindowsAPICodePack.Shell.dll",
                "Microsoft.WindowsAPICodePack.ShellExtensions.dll",
                "Newtonsoft.Json.dll",
                "System.Windows.Interactivity.dll",
                "WpfCodes.dll",
                "WpfControls.dll",
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

        private async void Application_Startup(object sender, StartupEventArgs e)
        {

            CheckFiles();

            new WpfCodes.Program.Exception(true);

            if (e.Args.Length == 0 || e.Args[0] != "restart")
            {
                if (WpfCodes.Program.Startup.HaveAnotherInstance("EasyMusic"))
                {
                    bool opened = false;
                    WpfCodes.Program.Startup.CheckAnotherInstanceAndOpenWindow<MainWindow>("EasyMusic", this).ContinueWith(p => opened = true);
                    await Task.Delay(1000);
                    if (!opened)
                    {
                        WpfControls.Dialog.DialogHelper.ShowError("已存在另一实例且无法打开，请手动打开已存在的实例");
                    }
                    Environment.Exit(0);
                }
            }
            else
            {
                WpfCodes.Program.Startup.HaveAnotherInstance("EasyMusic");
            }

            if (e.Args.Length != 0)
            {
                argPath = e.Args[0];
            }

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        //private void CollectExceptions() => new WpfCodes.Program.Exception().UnhandledException += (p1, p2) =>
        //{
        //    try
        //    {
        //        Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("程序发生了未捕获的错误，类型" + p2.Source.ToString(), p2.Exception));

        //        File.AppendAllText("UnhandledException.log", Environment.NewLine + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + p2.Exception.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("错误信息无法写入", ex));
        //    }
        //    finally
        //    {
        //        Environment.Exit(-1);
        //    }
        //};

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Helper.MusicControlHelper.Music != null)
            {
                Setting.LastMusic = Helper.MusicControlHelper.Music.FilePath;
            }
            Setting.Save();
            trayIcon.Dispose();
        }


    }

}
