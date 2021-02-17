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

        public event Action PlaybackStopped;
        public event Action PlaybackPaused;
        public event Action PlaybackStarted;
        public string source { get; }

        public NJAudioPlayer(string path, float volume)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            _audioFileReader = new AudioFileReader(path);
            _audioFileReader.Volume = volume;
            source = path;
            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            if (Settings.Default.AudioOutType == 1)
            {
                _outputASIO = new AsioOut(Settings.Default.ASIOindex);
                _outputASIO.PlaybackStopped += OutputDS_PlaybackStopped;
                _outputASIO.Init(wc);
            } else
            {
                _outputDS = new DirectSoundOut(Settings.Default.AudioLatency);
                _outputDS.PlaybackStopped += OutputDS_PlaybackStopped;
                _outputDS.Init(wc);
            }

        }

        public void Play()
        {
            if (_outputASIO != null)
            {
                _outputASIO.Play();
            } 
            else if (_outputDS != null)
            {
                _outputDS.Play();
            }

            PlaybackStarted?.Invoke();

        }

        public void Stop()
        {
            if (_outputDS != null)
            {
                _outputDS.Stop();
                PlaybackStopped?.Invoke();
            } 
            else if (_outputASIO != null)
            {
                _outputASIO.Stop();
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
            else if (_outputASIO != null)
            {
                _outputASIO.Pause();
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
            if (_outputDS != null || _outputASIO != null)
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

            if (_outputASIO != null)
            {
                if (_outputASIO.PlaybackState == PlaybackState.Playing)
                {
                    _outputASIO.Stop();
                }
                _outputASIO.PlaybackStopped -= OutputDS_PlaybackStopped;
                _outputASIO.Dispose();
                _outputASIO = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }

        public double GetLengthInSeconds()
        {
            return _audioFileReader != null ? _audioFileReader.TotalTime.TotalSeconds : 0;
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
            } 
            else if (_outputASIO != null && _outputASIO.PlaybackState == PlaybackState.Playing)
            {
                return true;
            } 
            else 
            {
                return false;
            }
            
        }
    }
}
