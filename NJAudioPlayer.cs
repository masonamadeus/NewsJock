using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Windows.Threading;
using NAudio.MediaFoundation;
using System.Diagnostics;
using System.Windows;
using NAudio.Wave.SampleProviders;

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

        public bool isDisposed = false;

        WaveChannel32 _wave { get; set; }

        string _ASIOname { get; set; }

        int _offset { get; set; }

        public NJAudioPlayer()
        {

        }

        /// <summary>
        /// Create a DirectSound Player with the specified file.
        /// </summary>
        /// <param name="DSpath"></param>
        public NJAudioPlayer(string DSpath)
        {
            if (Settings.Default.DSDevice != null)
            {
                PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
                _audioFileReader = new AudioFileReader(DSpath);
                source = DSpath;
                var wc = new WaveChannel32(_audioFileReader);
                wc.PadWithZeroes = false;
                _outputDS = new DirectSoundOut(Settings.Default.DSDevice.Guid, Settings.Default.DSLatency);
                _outputDS.PlaybackStopped += Output_PlaybackStopped;
                _outputDS.Init(wc);
            } else
            {
                MessageBox.Show("You have not selected an output device.", "No Output Device", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Pass a GUID to direct the output to a specific device.
        /// </summary>
        /// <param name="DSpath"></param>
        /// <param name="guid"></param>
        public NJAudioPlayer(string DSpath, Guid guid)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            _audioFileReader = new AudioFileReader(DSpath);
            source = DSpath;
            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            _outputDS = new DirectSoundOut(guid, Settings.Default.DSLatency);
            _outputDS.PlaybackStopped += Output_PlaybackStopped;
            _outputDS.Init(wc);
        }

        /// <summary>
        /// Pass an ASIO Driver name to direct output to specific device.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ASIOname"></param>
        public NJAudioPlayer(string path, string ASIOname)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            _audioFileReader = new AudioFileReader(path);
            source = path;
            _ASIOname = ASIOname;
            _wave = new WaveChannel32(_audioFileReader);
            _wave.PadWithZeroes = false;
        }


        public NJAudioPlayer(string path, string ASIOname, int ASIOchannel)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            _audioFileReader = new AudioFileReader(path);
            source = path;
            _offset = ASIOchannel;
            _ASIOname = ASIOname;
            _wave = new WaveChannel32(_audioFileReader);
            _wave.PadWithZeroes = false;

        }

        public void ASIODispose()
        {
            if (_outputASIO != null)
            {
                if (_outputASIO.PlaybackState == PlaybackState.Playing)
                {
                    _outputASIO.Stop();
                }
                _outputASIO.PlaybackStopped -= Output_PlaybackStopped;
                _outputASIO.Dispose();
                _outputASIO = null;
            }
        }

        public void Play(float volume)
        {

            if (Settings.Default.AudioOutType == 1 && Settings.Default.SeparateOutputs)
            {
                _outputASIO = new AsioOut(_ASIOname);
                _outputASIO.ChannelOffset = _offset;          
                _outputASIO.PlaybackStopped += Output_PlaybackStopped;
                _outputASIO.Init(_wave);
                _outputASIO.Play();
            } 
            else if (_outputDS != null & Settings.Default.AudioOutType == 0)
            {
                _outputDS.Play();
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Volume = volume;
                PlaybackStarted?.Invoke();
            }

        }

        public void Stop()
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;
            }
            if (_outputDS != null)
            {
                _outputDS.Stop();
                PlaybackStopped?.Invoke();
            } 
            else if (_outputASIO != null)
            {
                _outputASIO.Stop();
                ASIODispose();
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

        private void Output_PlaybackStopped(object sender, StoppedEventArgs s)
        {
            _audioFileReader.Position = 0;
            PlaybackStopped?.Invoke();
            Trace.WriteLine("Output_PlaybackStopped Fired.");
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
                _outputDS.PlaybackStopped -= Output_PlaybackStopped;
                _outputDS.Dispose();
                _outputDS = null;
            }

            if (_outputASIO != null)
            {
                if (_outputASIO.PlaybackState == PlaybackState.Playing)
                {
                    _outputASIO.Stop();
                }
                _outputASIO.PlaybackStopped -= Output_PlaybackStopped;
                _outputASIO.Dispose();
                _outputASIO = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            if (_wave != null)
            {
                _wave.Dispose();
            }

            isDisposed = true;
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
