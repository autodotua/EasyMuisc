namespace EasyMusic.Info
{
    public class MusicFxInfo
    {
        public MusicFxInfo()
        {
        }

        public MusicFxInfo(int pitch, int tempo)
        {
            Pitch = pitch;
            Tempo = tempo;
        }

        public int Pitch { get; set; }
        public int Tempo { get; set; }
    }
}