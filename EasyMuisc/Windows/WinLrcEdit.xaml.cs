using EasyMusic.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfCodes.Basic;
using static WpfControls.Dialog.DialogHelper;

namespace EasyMusic.Windows
{
    /// <summary>
    /// WinLrcEdit.xaml 的交互逻辑
    /// </summary>
    public partial class WinLrcEdit : Window
    {
        private string filePath = "";
        public WinLrcEdit(string path)
        {
            InitializeComponent();

            filePath = path;
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
                    txt.Text = File.ReadAllText(filePath, EncodingType.GetType(filePath));
                }
                catch (Exception ex)
                {
                    ShowException(ex, this);
                }
            }
        
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
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
                        File.WriteAllText(filePath, txt.Text, EncodingType.GetType(filePath));
                        SetButtonsStatus(false);
                        if (ShowMessage("重载歌词？", WpfControls.Dialog.DialogType.Information, MessageBoxButton.YesNo, this) == 0)
                        {
                            GlobalDatas.WinMain.InitialiazeLrc();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowException(ex, this);
                    }
                    break;
                case "btnSaveAs":
                    SaveFileDialog dialog = new SaveFileDialog() { FileName = new FileInfo(filePath).Name, DefaultExt = new FileInfo(filePath).DirectoryName };
                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, txt.Text, EncodingType.GetType(filePath));
                            SetButtonsStatus(false);

                        }
                        catch (Exception ex)
                        {
                            ShowException(ex, this);
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
    }
}
