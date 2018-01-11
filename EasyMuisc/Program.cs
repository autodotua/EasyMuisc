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
            //app.DispatcherUnhandledException += App_DispatcherUnhandledException;
                app.Run(new MainWindow() { path = args.Length != 0 ? args[0] : null });

        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowAlert(e.Exception.Message);
            string logName = "UnhandledException.log";
            if (File.Exists(logName))
            {
                string oldFile = File.ReadAllText(logName);
                File.WriteAllText(logName,
                oldFile
                + Environment.NewLine + Environment.NewLine
                + DateTime.Now.ToString()
                + Environment.NewLine
                + e.Exception.ToString());
            }
            else
            {
                File.WriteAllText(logName,
                  DateTime.Now.ToString()
                  + Environment.NewLine
                   + e.Exception.ToString());
            }
           App.Current.Shutdown();
            return;
        }

        private static void ShowAlert(string message)
        {

            MessageBox.Show(message, "发生异常", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }
}
