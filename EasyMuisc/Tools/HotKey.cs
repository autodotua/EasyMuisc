using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace EasyMuisc.Tools
{
    ///<summary>
    /// 直接构造类实例即可注册
    /// 自动完成注销
    /// 注意注册时会抛出异常
    /// 注册系统热键类
    /// 热键会随着程序结束自动解除,不会写入注册表
    ///</summary>
    public class HotKey
    {
        #region Member

        int KeyId;         //热键编号
        IntPtr Handle;     //窗体句柄
        Window Window;     //热键所在窗体
        uint ControlKey;   //热键控制键
        uint Key;          //热键主键

        public delegate void OnHotKeyEventHandler();     //热键事件委托
        public event OnHotKeyEventHandler OnHotKey = null;  //热键事件

        static Hashtable KeyPair = new Hashtable();         //热键哈希表
        private const int WM_HOTKEY = 0x0312;       // 热键消息编号

        public enum KeyFlags    //控制键编码
        {
            MOD_NONE = 0x0,
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8
        }

        #endregion

        ///<summary>
        /// 构造函数
        ///</summary>
        ///<param name="win">注册窗体</param>
        ///<param name="control">控制键</param>
        ///<param name="key">主键</param>
        public HotKey(Window win, HotKey.KeyFlags control, System.Windows.Forms.Keys key)
        {
            Handle = new WindowInteropHelper(win).Handle;
            Window = win;
            ControlKey = (uint)control;
            Key = (uint)key;
            KeyId = (int)ControlKey + (int)Key * 10;

            if (KeyPair.ContainsKey(KeyId))
            {
                throw new Exception("热键已经被注册!");
            }

            //注册热键
            if (false == RegisterHotKey(Handle, KeyId, ControlKey, Key))
            {
                throw new Exception("热键注册失败!");
            }

            //消息挂钩只能连接一次!!
            if (KeyPair.Count == 0)
            {
                if (false == InstallHotKeyHook(this))
                {
                    throw new Exception("消息挂钩连接失败!");
                }
            }

            //添加这个热键索引
            KeyPair.Add(KeyId, this);
        }

        //析构函数,解除热键
        //~HotKey()
        //{
        //    HotKey.UnregisterHotKey(Handle, KeyId);
        //}

        #region core

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //安装热键处理挂钩
        static private bool InstallHotKeyHook(HotKey hk)
        {
            if (hk.Window == null || hk.Handle == IntPtr.Zero)
            {
                return false;
            }

            //获得消息源
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(hk.Handle);
            if (source == null)
            {
                return false;
            }

            //挂接事件
            source.AddHook(HotKeyHook);
            return true;
        }

        //热键处理过程
        static private IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                HotKey hk = (HotKey)KeyPair[(int)wParam];
                if (hk.OnHotKey != null)
                {
                    hk.OnHotKey();
                }
            }
            return IntPtr.Zero;
        }

        #endregion
    }
}
