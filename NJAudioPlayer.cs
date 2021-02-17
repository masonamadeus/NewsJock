using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Windows.Threading;
using NAudio.MediaFoundation;
using System.Diagnostics;

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
        public event Action PlaybackStarted;
        public string source { get; }

        public NJAudioPlayer(string path, float volume)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            _audioFileReader = new AudioFileReader(path);
            _audioFileReader.Volume = volume;
            _outputDS = new DirectSoundOut(200);
            _outputDS.PlaybackStopped += OutputDS_PlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;
            source = path;
            _outputDS.Init(wc);
            //Play();
        }

        public void Play()
        {

            _outputDS.Play();

            PlaybackStarted?.Invoke();

        }

        public void Stop()
        {
            if (_outputDS != null)
            {
                _outputDS.Stop();
                PlaybackStopped?.Invoke();
            }

        }

        public void Pause()
        {
            if (_outputDS != null)
            {
                _outputDS.Pause();
                PlaybackPaused?.Invoke();
            }
        }

        private void OutputDS_PlaybackStopped(object sender, StoppedEventArgs s)
        {
            Dispose();
            PlaybackStopped?.Invoke();
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
            Trace.WriteLine("Disposing");
            if (_outputDS != null)
            {
                if (_outputDS.PlaybackState == PlaybackState.Playing)
                {
                    _outputDS.Stop();
                }
                _outputDS.PlaybackStopped -= OutputDS_PlaybackStopped;
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

        public bool IsPlaying()
        {
            if (_outputDS != null && _outputDS.PlaybackState == PlaybackState.Playing)
            {
                return true;
            } else
            {
                return false;
            }
            
        }
    }
}
