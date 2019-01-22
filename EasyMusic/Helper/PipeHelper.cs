using System.IO;
using System.Threading.Tasks;
using static FzLib.Basic.String;
using static FzLib.Program.Runtime.SinglePipe;

namespace EasyMusic.Helper
{
    public static class PipeHelper
    {
        private static Clinet clinet;
        public static void RegistClinet()
        {
            clinet = new Clinet(EasyMusic.Properties.Resources.AppName + "_Play");
            clinet.Start();
            clinet.GotMessage += ClinetGotMessage;


        }
        //C:\Users\autod\Documents\个人文件\音乐\2015\Avril Lavigne - Innocence.mp3

        private static async void ClinetGotMessage(object sender, PipeMessageEventArgs e)
        {
            if (e.Message.StartsWith("play"))
            {
                string path = e.Message.RemoveStart("play ");
                if (File.Exists(path))
                {
                    await App.Current.Dispatcher.Invoke(async () =>
                    {
                        MusicControlHelper.PlayNew(await MusicListHelper.AddMusic(path), true);
                        if (MainWindow.Current.Visibility != System.Windows.Visibility.Visible)
                        {
                            MainWindow.Current.Visibility = System.Windows.Visibility.Visible;
                        }
                        if (MainWindow.Current.WindowState == System.Windows.WindowState.Minimized)
                        {
                            MainWindow.Current.WindowState = System.Windows.WindowState.Normal;
                        }
                        MainWindow.Current.Activate();
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() =>  FzLib.Control.Dialog.DialogBox.ShowError($"文件{path}不存在"));
                }
            }
        }

        public static async Task Send(string message)
        {
            var server = new Server(EasyMusic.Properties.Resources.AppName + "_Play");

            await server.SendMessageAsync(message);
        }
    }
}
