using FzLib.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using static EasyMusic.GlobalDatas;
using static EasyMusic.Helper.MusicControlHelper;
using static FzLib.Control.Dialog.DialogBox;

namespace EasyMusic.Helper
{
    public static class HotKeyHelper
    {
        private static HotKey hotKey = new HotKey();
        public static Dictionary<string, HotKey.HotKeyInfo> HotKeys { get; set; } = null;

        /// <summary>
        /// 注册全局热键
        /// </summary>
        public static bool RegistGolbalHotKey()
        {
            hotKey.UnregisterAll();

            if (HotKeys == null)
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath + "\\HotKeyConfig.json");

                    HotKeys = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, HotKey.HotKeyInfo>>(json);
                }
                catch
                {
                    HotKeys = new Dictionary<string, HotKey.HotKeyInfo>()
                {
                    {"下一曲",new HotKey.HotKeyInfo(Key.Right,ModifierKeys.Control) },
                    {"上一曲",new HotKey.HotKeyInfo(Key.Left,ModifierKeys.Control) },
                    {"音量加",new HotKey.HotKeyInfo(Key.Up,ModifierKeys.Control) },
                    {"音量减",new HotKey.HotKeyInfo(Key.Down,ModifierKeys.Control) },
                    {"播放暂停",new HotKey.HotKeyInfo(Key.OemQuestion,ModifierKeys.Control) },
                    {"悬浮歌词",new HotKey.HotKeyInfo(Key.OemPeriod,ModifierKeys.Control) },
                    {"收放列表",new HotKey.HotKeyInfo(Key.OemComma,ModifierKeys.Control) },
                };
                }
            }

            string error = "";

            hotKey.KeyPressed += (p1, p2) =>
            {
                if (HotKeys.ContainsValue(p2.HotKey))
                {
                    string command = HotKeys.First(p => p.Value.Equals(p2.HotKey)).Key;
                    switch (command)
                    {
                        case "下一曲":
                            PlayNext(true);
                            break;

                        case "上一曲":
                            PlayLast();
                            break;

                        case "音量加":
                            Volumn += 0.05;
                            break;

                        case "音量减":
                            Volumn -= 0.05;
                            break;

                        case "播放暂停":
                            Music.PlayOrPause();
                            break;

                        case "悬浮歌词":
                            MainWindow.Current.ChangeMusicListVisibility();
                            break;

                        case "收放列表":
                            MainWindow.Current.OpenOrCloseFloatLrc();
                            break;
                    }
                }
            };

            foreach (var key in HotKeys)
            {
                try
                {
                    if (key.Value != null)
                    {
                        hotKey.Register(key.Value);
                    }
                }
                catch
                {
                    error += key.Key + "（" + key.Value.ToString() + "）";
                }
            }

            if (error != "")
            {
                trayIcon.ShowMessage("以下热键无法注册，可能已被占用：" + error.TrimEnd('、'));
                return false;
            }
            return true;
        }

        public static void UnregistAll()
        {
            hotKey.UnregisterAll();
        }

        //public static bool TryUpdateHotKey(string name, HotKey.HotKeyInfo oldValue, HotKey.HotKeyInfo newValue)
        //{
        //        if (hotKey.RegisteredKeys.Contains(oldValue))
        //        {
        //            try
        //            {
        //                hotKey.Unregister(oldValue);
        //            }
        //            catch (Exception ex)
        //            {
        //                ShowException("卸载热键失败：", ex, true);
        //                return false;
        //            }
        //        }

        //    try
        //        {
        //            hotKey.Register(newValue);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ShowException("注册热键失败：", ex, true);
        //            return false;
        //        }
        //}

        public static void SaveHotKeys()
        {
            try
            {
                File.WriteAllText(ConfigPath + "\\HotKeyConfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(HotKeys));
            }
            catch (Exception ex)
            {
                ShowException("保存热键配置失败", ex);
            }
        }
    }
}