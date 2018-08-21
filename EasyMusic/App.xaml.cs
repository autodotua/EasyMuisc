using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
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
                if (!File.Exists(file))
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
#if !DEBUG
            CollectExceptions();
#endif
            if (e.Args.Length == 0 || e.Args[0] != "restart")
            {
                if (await WpfCodes.Program.Startup.CheckAnotherInstanceAndOpenWindow<MainWindow>("EasyMusic", this))
                {
                    Shutdown();
                }
            }
            else
            {
                WpfCodes.Program.Startup.HaveAnotherInstance("EasyMusic");
            }

            if (e.Args.Length != 0)
            {
                GlobalDatas.argPath = e.Args[0];
            }
        }

        private void CollectExceptions() => new WpfCodes.Program.Exception().UnhandledException += (p1, p2) =>
        {
            try
            {
                Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("程序发生了未捕获的错误，类型" + p2.Source.ToString(), p2.Exception));

                File.AppendAllText("Exception.log", Environment.NewLine + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + p2.Exception.ToString());
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("错误信息无法写入", ex));
            }
            finally
            {
                Environment.Exit(-1);
            }
        };

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GlobalDatas.trayIcon.Dispose();
        }


    }

}
