#define DEBUG 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static WpfControls.Dialog.DialogHelper;


namespace EasyMuisc
{
   static class Program
    {

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "EasyMusic", out bool isSingle);
            if(!isSingle)
            {
                ShowError("请勿运行多个实例！这会导致热键异常等问题。");
                Environment.Exit(0);
            }


            App app = new App();
            //app.InitializeComponent();
#if(!DEBUG)
            try
            {
#endif
                ShareStaticResources.mainWindow = new MainWindow() { path = args.Length != 0 ? args[0] : null };
                app.Run(ShareStaticResources.mainWindow);
                ShareStaticResources.trayIcon.Visible = false;
#if(!DEBUG)
            }
            catch(Exception ex)
            {
                UnhandledException(ex);
            }
#endif
            //app.Run(new EasyMuisc.Windows.FloatLyrics());
        }

        //private static void UnhandledException(Exception ex)
        //{
        //    Dialog.DialogHelper.ShowException("程序发生了未处理的异常，将立刻关闭", ex);
        //    string logName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+ "\\EasyMusic\\UnhandledException.log";
        //    if (File.Exists(logName))
        //    {
        //        string oldFile = File.ReadAllText(logName);
        //        File.WriteAllText(logName,
        //        oldFile
        //        + Environment.NewLine + Environment.NewLine
        //        + DateTime.Now.ToString()
        //        + Environment.NewLine
        //        + ex.ToString());
        //    }
        //    else
        //    {
        //        File.WriteAllText(logName,
        //          DateTime.Now.ToString()
        //          + Environment.NewLine
        //           + ex.ToString());
        //    }
        //    //App.Current.Shutdown();
        //    Environment.Exit(0);
        //    return;
        //}

    }
}
