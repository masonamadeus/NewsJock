using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace NewsBuddy
{
    class AutoDisposeFileReader : IWaveProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
    public class NJAsioMixer
    {
        private string source { get; }
        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public PlaybackStopTypes PlaybackStopType { get; set; }

        
        private AsioOut _outputASIO;

        public event Action PlaybackStopped;
        public event Action PlaybackPaused;
        public event Action PlaybackStarted;

        private readonly MixingWaveProvider32 mixer;

        public NJAsioMixer(string ASIOdriver)
        {
            _outputASIO = new AsioOut(ASIOdriver);
            mixer = new MixingWaveProvider32();
            _outputASIO.Init(mixer);
            _outputASIO.Play();
        }

        public void Play(string path)
        {
            var input = new AudioFileReader(path);
            mixer.AddInputStream(new AutoDisposeFileReader(input));
        }

        public void KillMixer()
        {
            if (_outputASIO != null)
            {
                if (_outputASIO.PlaybackState == PlaybackState.Playing)
                {
                    _outputASIO.Stop();
                }
                _outputASIO.Dispose();
            }
        }
    }
}
