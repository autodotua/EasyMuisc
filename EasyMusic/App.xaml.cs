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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0 || e.Args[0]!= "restart")
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


    }

}
