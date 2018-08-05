using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EasyMusic
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //    public App()
        //    {
        //     if(   WpfCodes.Program.Startup.HaveAnotherInstance("EasyMusic"))
        //        {
        //          WpfControls.Dialog.DialogHelper.  ShowError("请勿运行多个实例！这会导致热键异常等问题。");
        //            Environment.Exit(0);
        //        }
        //    }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (await WpfCodes.Program.Startup.CheckAnotherInstanceAndOpenWindow<MainWindow>("EasyMusic", this))
            {
                Shutdown();
            }
            if (e.Args.Length != 0)
            {
                GlobalDatas.argPath = e.Args[0];
            }
            TaskScheduler.UnobservedTaskException += (p1, p2) => { if (!p2.Observed) ShowException(p2.Exception, 3); };//Task
            AppDomain.CurrentDomain.UnhandledException += (p1, p2) => ShowException((Exception)p2.ExceptionObject, 2);//UI
            DispatcherUnhandledException += (p1, p2) => ShowException(p2.Exception, 1);//Thread
        }
        private void ShowException(Exception ex, int type)
        {
            try
            {
                Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("程序发生了未捕获的错误，类型" + type.ToString(), ex));

                File.AppendAllText("Exception.log", Environment.NewLine + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + ex.ToString());
            }
            catch (Exception ex2)
            {
                Dispatcher.Invoke(() => WpfControls.Dialog.DialogHelper.ShowException("错误信息无法写入", ex2));
            }
            finally
            {
                Current.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GlobalDatas.trayIcon.Dispose();
        }


        //void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //   ShowAlert(e.Exception.Message);
        //    string logName = "UnhandledException.log";
        //    if (File.Exists(logName))
        //    {
        //        string oldFile = File.ReadAllText(logName);
        //        File.WriteAllText(logName,
        //        oldFile
        //        + Environment.NewLine + Environment.NewLine
        //        + DateTime.Now.ToString()
        //        + Environment.NewLine
        //        + e.Exception.ToString());
        //    }
        //    else
        //    {
        //        File.WriteAllText(logName,
        //          DateTime.Now.ToString()
        //          +Environment.NewLine
        //           + e.Exception.ToString());
        //    }
        //    Current.Shutdown();
        //    return;
        //}
        //private void ShowAlert(string message)
        //{

        //        MessageBox.Show(message, "发生异常", MessageBoxButton.OK, MessageBoxImage.Error);

        //}
    }

}
