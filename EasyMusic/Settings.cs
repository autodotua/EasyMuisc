using EasyMusic.Enum;
using FzLib.DataStorage.Serialization;
using System;
using System.Windows.Media;

namespace EasyMusic
{
    public class Settings : JsonSerializationBase
    {
        public double NormalLrcFontSize { get; set; } = 24;
        public double HighlightLrcFontSize { get; set; } = 36;
        public double TextLrcFontSize { get; set; } = 28;
        public double Volumn { get; set; } = 1;
        public double LrcDefautOffset { get; set; } = 0.2;

        public double FloatLyricsHighlightFontSize { get; set; } = 40;
        public double FloatLyricsNormalFontSize { get; set; } = 30;
        public double FloatLyricsHeight { get; set; } = 200;
        public double FloatLyricsWidth { get; set; } = 800;
        public double FloatLyricsTop { get; set; } = 100;
        public double FloatLyricsLeft { get; set; } = 100;
        public double FloatLyricsThickness { get; set; } = 1.5;
        public double FloatLyricsBlurRadius { get; set; } = 5;

        public double Top { get; set; } = 200;
        public double Left { get; set; } = 300;
        public double Height { get; set; } = 600;
        public double Width { get; set; } = 1000;

        public int UpdateSpeed { get; set; } = 30;
        public int AnimationFps { get; set; } = 60;
        public int Pitch { get; set; } = 0;
        public int Tempo { get; set; } = 100;
        public int SampleRate { get; set; } = 48000;
        public int ThresholdValueOfListenTime { get; set; } = 30;

        public int FloatLyricsFontEffect { get; set; } = 1;

        public bool Topmost { get; set; } = false;
        public bool ShowLrc { get; set; } = true;
        public bool SaveLrcOffsetByTag { get; set; } = true;
        public bool PreferMusicInfo { get; set; } = true;
        public bool LrcAnimation { get; set; } = true;
        public bool MaxWindow { get; set; } = false;
        public bool RecordListenHistory { get; set; } = true;
        public MusicFxRemainMode MusicFxMode { get; set; } = MusicFxRemainMode.Each;
        public bool ShowTray { get; set; } = true;

        public bool ShowFloatLyric { get; set; } = true;
        public bool FloatLyricsFontBold { get; set; } = false;
        public bool LyricsFontBold { get; set; } = false;

        public SolidColorBrush BackgroundColor { get; set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0xEB, 0xF1, 0xDD));
        public SolidColorBrush FloatLyricsFontColor { get; set; } = Brushes.White;
        public SolidColorBrush FloatLyricsBorderColor { get; set; } = Brushes.Black;
        public SolidColorBrush LyricsFontColor { get; set; } = Brushes.Black;

        public string LastMusic { get; set; } = "";
        public string LastMusicList { get; set; } = "";
        public string ConfigPath { get; set; } = @"%APPDATA%\EasyMusic";
        public string FloatLyricsFont { get; set; } = "微软雅黑";
        public string LyricsFont { get; set; } = "微软雅黑";

        public CycleMode CycleMode { get; set; } = CycleMode.ListCycle;

        public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

        public TimeSpan VolumnChangeTime { get; set; } = TimeSpan.FromMilliseconds(800);
        public bool ShowOneLineInFloatLyric { get; set; } = false;
    }
}