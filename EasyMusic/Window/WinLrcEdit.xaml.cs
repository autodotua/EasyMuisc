using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static WpfControls.Dialog.DialogHelper;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinLrcEdit.xaml 的交互逻辑
    /// </summary>
    public partial class WinLrcEdit : WindowBase
    {
        private string filePath = "";
        private WinLrcEdit()
        {
            InitializeComponent();

        }

        public static void Edit(Window owner, string path)
        {
            WinLrcEdit win = new WinLrcEdit() { Owner = owner };
            win.filePath = path;
            win.Load();
            win.Show();
        }

        public static void ShowOnly(Window owner, IEnumerable<string> texts)
        {
            WinLrcEdit win = new WinLrcEdit() { Owner = owner };
            win.rich.Visibility = Visibility.Visible;
            foreach (var line in texts)
            {
                win.rich.AppendText(line + Environment.NewLine);
            }
            win.rich.Document.LineHeight = 1;
            win.rich.Document.TextAlignment = TextAlignment.Center;
            win.rich.FontSize = GlobalDatas.Setting.TextLrcFontSize;
            win.Header.Background = GlobalDatas.Setting.BackgroundColor;
            win.Show();
        }


        private void Load()
        {
            Title = "歌词编辑 - " + filePath;

            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                try
                {
                    File.Create(filePath);
                }
                catch
                {
                    ShowError("歌词文件不存在且创建失败！");
                    Close();
                    return;
                }

            }
            else
            {
                if (file.Length > 1024 * 1024)
                {
                    ShowError("文件大小超过1MB，怀疑有误！");
                    Close();
                    return;
                }
                try
                {
                    txt.Text = File.ReadAllText(filePath, WpfCodes.Basic.String.GetEncoding(filePath));
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "btnReload":
                    Load();
                    break;
                case "btnSave":
                    try
                    {
                        File.WriteAllText(filePath, txt.Text, WpfCodes.Basic.String.GetEncoding(filePath));
                        SetButtonsStatus(false);
                        if (ShowMessage("重载歌词？", WpfControls.Dialog.DialogType.Information, MessageBoxButton.YesNo, this) == 0)
                        {
                            MainWindow.Current.InitializeLrc();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowException(ex);
                    }
                    break;
                case "btnSaveAs":
                    SaveFileDialog dialog = new SaveFileDialog() { FileName = new FileInfo(filePath).Name, DefaultExt = new FileInfo(filePath).DirectoryName };
                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, txt.Text, WpfCodes.Basic.String.GetEncoding(filePath));
                            SetButtonsStatus(false);

                        }
                        catch (Exception ex)
                        {
                            ShowException(ex);
                        }
                    }
                    break;
            }
        }

        private void SetButtonsStatus(bool status)
        {
            btnSaveAs.IsEnabled = btnSave.IsEnabled = btnReload.IsEnabled = status;
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetButtonsStatus(true);
        }

        private void rich_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                rich.FontSize += e.Delta / 120.0;
                e.Handled = true;
            }
        }
    }
}
