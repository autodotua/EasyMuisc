﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EasyMuisc.Properties {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EasyMuisc.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找 System.Byte[] 类型的本地化资源。
        /// </summary>
        internal static byte[] bass {
            get {
                object obj = ResourceManager.GetObject("bass", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   查找 System.Byte[] 类型的本地化资源。
        /// </summary>
        internal static byte[] Bass_Net {
            get {
                object obj = ResourceManager.GetObject("Bass_Net", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
        internal static string ChangeLog {
            get {
                return ResourceManager.GetString("ChangeLog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似于 (图标) 的 System.Drawing.Icon 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Icon icon {
            get {
                object obj = ResourceManager.GetObject("icon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   查找类似 {\rtf1\ansi\ansicpg936\deff0\nouicompat\deflang1033\deflangfe2052{\fonttbl{\f0\fnil\fcharset134 \&apos;b5\&apos;c8\&apos;cf\&apos;df;}}
        ///{\*\generator Riched20 10.0.16299}\viewkind4\uc1 
        ///\pard\sa200\sl276\slmult1\b\f0\fs40\lang2052\&apos;c8\&apos;c8\&apos;bc\&apos;fc\par
        ///\fs32\&apos;c8\&apos;ab\&apos;be\&apos;d6\&apos;c8\&apos;c8\&apos;bc\&apos;fc\par
        ///
        ///\pard\fi426\sa200\sl240\slmult1\tx426\b0\fs22 Ctrl+\&apos;a1\&apos;fa\tab\&apos;cf\&apos;c2\&apos;d2\&apos;bb\&apos;c7\&apos;fa\par
        ///Ctrl+\&apos;a1\&apos;fb\tab\&apos;c9\&apos;cf\&apos;d2\&apos;bb\&apos;c7\&apos;fa\par
        ///Ctrl+\&apos;a1\&apos;fc\tab\&apos;d2\&apos;f4\&apos;c1\&apos;bf+\par
        ///Ctrl+\&apos;a1\&apos;fd\tab\&apos;d2\&apos;f4\&apos;c1\&apos;bf-\b\fs32\par
        ///
        ///\par [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string strHelp {
            get {
                return ResourceManager.GetString("strHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 20171216
        ///	完成播放暂停功能
        ///20171217
        ///	完成主要功能，包括
        ///		歌曲列表
        ///		歌曲列表保存
        ///		上一曲、下一曲
        ///		歌词显示
        ///		歌词动画
        ///		歌词点击跳转
        ///20171218
        ///	将控制区布局移植最底下
        ///	新增设置单曲循环/列表循环/随机播放模式切换
        ///	新增打开文件功能
        ///20171219
        ///	新增列表选项，包括打开文件、文件夹、清空列表
        ///	新增对txt歌词的支持
        ///	新增对列表的去重添加
        ///	新增淡出淡入
        ///	新增lrc歌词渐隐
        ///	新增自动歌词编码识别
        ///	新增设置歌词字体大小
        ///	新增记录没有被捕获的异常
        ///20171220
        ///	更改了ContextMenu的样式
        ///	修复了最后一句歌词有问题的BUG
        ///	将进度条改为了Slider，允许点哪放哪
        ///	新增收放歌曲列表按钮
        ///	新增缩放时自动收放歌曲列表
        ///	===尝试转制UWP在配置文件写入地方失败了，因为没权限===
        ///	偶然将资源文件整合了，但是不知道为什么Release无法生成
        ///20171222
        ///	新增热键控制歌曲的暂停播放、上下曲
        ///	新增音量调节
        ///	新增记忆关闭时正在播放的歌曲，下次启动自动选 [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string 日志 {
            get {
                return ResourceManager.GetString("日志", resourceCulture);
            }
        }
    }
}
