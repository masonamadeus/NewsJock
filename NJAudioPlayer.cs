using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace NewsBuddy
{
    public class NJAudioPlayer
    {

        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public PlaybackStopTypes PlaybackStopType { get; set; }

        private AudioFileReader _audioFileReader;
        private DirectSoundOut _outputDS;
        private AsioOut _outputASIO;
        private string _filepath;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;

        public NJAudioPlayer(string filePath, float volume)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

            _audioFileReader = new AudioFileReader(filePath) { Volume = volume };

            _outputDS = new DirectSoundOut(200);
            _outputDS.PlaybackStopped += OutputDS_PlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            _outputDS.Init(wc);
        }

        public void Play(double currentVolume)
        {

            _outputDS.Play();
           
            _audioFileReader.Volume = (float)currentVolume;

            if (PlaybackResumed != null)
            {
                PlaybackResumed();
            }
        }

        public void Stop()
        {
            if (_outputDS != null)
            {
                _outputDS.Stop();
            }
        }

        public void Pause()
        {
            if (_outputDS != null)
            {
                _outputDS.Pause();
                if (PlaybackPaused != null)
                {
                    PlaybackPaused();
                }
            }
        }

        private void OutputDS_PlaybackStopped(object sender, StoppedEventArgs s)
        {
            Dispose();
            if (PlaybackStopped != null)
            {
                PlaybackStopped();
            }
        }

        public void SetVolume(float value)
        {
            if (_outputDS != null)
            {
                _audioFileReader.Volume = value;
            }
        }

        public void Dispose()
        {
            if (_outputDS != null)
            {
                if (_outputDS.PlaybackState == PlaybackState.Playing)
                {
                    _outputDS.Stop();
                }
                _outputDS.Dispose();
                _outputDS = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }

        public double GetLengthInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public double GetPosition()
        {
            return _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds : 0;
        }

        public double GetTimeRemaining()
        {
            double TimeRemaining = GetLengthInSeconds() - GetPosition();
            return TimeRemaining;
        }
    }
}
