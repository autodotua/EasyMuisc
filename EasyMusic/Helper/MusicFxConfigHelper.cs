﻿using EasyMusic.Info;
using FzLib.Control.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EasyMusic.GlobalDatas;

namespace EasyMusic.Helper
{
    public class MusicFxConfigHelper
    {
        private static readonly string path = ConfigPath + "\\MusicFxConfig.json";
        public MusicFxConfigHelper()
        {
            if (!File.Exists(path))
            {
                fxs = new Dictionary<string, MusicFxInfo>();
            }
            else
            {
                try
                {
                    fxs = JsonConvert.DeserializeObject<Dictionary<string, MusicFxInfo>>(File.ReadAllText(path));
                }
                catch (Exception ex)
                {
                    DialogBox.ShowException("读取音乐效果配置失败", ex);
                    fxs = new Dictionary<string, MusicFxInfo>();
                }
            }

        }

        private static MusicFxConfigHelper instance;
        public static MusicFxConfigHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MusicFxConfigHelper();
                }
                return instance;
            }
        }

        private Dictionary<string, MusicFxInfo> fxs;

        public MusicFxInfo Get(string path)
        {
            if (fxs.ContainsKey(path))
            {
                return fxs[path];
            }
            else
            {
                return new MusicFxInfo();
            }
        }

        public void Set(string path, MusicFxInfo info)
        {
            if (fxs.ContainsKey(path))
            {
                fxs[path] = info;
            }
            else
            {
                fxs.Add(path, info);
            }
            Save();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(fxs));
            }
            catch (Exception ex)
            {
                DialogBox.ShowException("保存音乐效果配置失败", ex);
            }
        }
    }
}
