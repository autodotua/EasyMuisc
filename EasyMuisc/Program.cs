#define DEBUG 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


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

        private static void UnhandledException(Exception ex)
        {
            Dialog.DialogHelper.ShowException("程序发生了未处理的异常，将立刻关闭", ex);
            string logName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+ "\\EasyMusic\\UnhandledException.log";
            if (File.Exists(logName))
            {
                string oldFile = File.ReadAllText(logName);
                File.WriteAllText(logName,
                oldFile
                + Environment.NewLine + Environment.NewLine
                + DateTime.Now.ToString()
                + Environment.NewLine
                + ex.ToString());
            }
            else
            {
                File.WriteAllText(logName,
                  DateTime.Now.ToString()
                  + Environment.NewLine
                   + ex.ToString());
            }
            //App.Current.Shutdown();
            Environment.Exit(0);
            return;
        }

    }
}
