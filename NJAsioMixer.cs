using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;

namespace NewsBuddy
{


    public class NJFileReader : IWaveProvider
    {
        public readonly AudioFileReader reader;
        public bool isSounder;
        public bool isDone { get; set; }
        public bool isPlaying { get; set; }

        public string source { get; set; }

        public event Action DonePlaying;
        public WaveFormat WaveFormat { get; private set; }
        public NJFileReader(AudioFileReader reader, bool isSounder, string name)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
            this.isSounder = isSounder;
            this.source = name;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (isDone)
            {
                if (isPlaying)
                {
                    isPlaying = false;
                }
                return 0;
            }
            try
            {
                int read = reader.Read(buffer, offset, count);
                if (read == 0)
                {
                    isDone = true;
                    DonePlaying.Invoke();
                }
                if (!isPlaying)
                {
                    isPlaying = true;
                }
                return read;
            }
            catch
            {
                return 0;
            }

        }
    }

    public class NJAsioMixer
    {
        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public PlaybackStopTypes PlaybackStopType { get; set; }

        
        private AsioOut _outputASIO;


        private readonly MixingWaveProvider32 mixer;
        public NJFileReader currentSounder;
        public NJFileReader currentClip;

        public NJAsioMixer(string ASIOdriver, int ASIOoffset)
        {
            _outputASIO = new AsioOut(ASIOdriver);
            mixer = new MixingWaveProvider32();
            _outputASIO.ChannelOffset = ASIOoffset;
            _outputASIO.Init(mixer);
            _outputASIO.Play();
            
        }

        public void Play(NJFileReader NJF, float volume)
        {
            if (NJF.isSounder)
            {
                if (currentSounder == NJF)
                {
                    Trace.WriteLine("Current Sounder The Same");
                    SounderDone();
                } 
                else if (currentSounder != null)
                {
                    SounderDone();
                    currentSounder = NJF;
                    currentSounder.DonePlaying += SounderStop;
                    currentSounder.reader.Volume = volume;
                    AddToMixer(currentSounder);
                }
                else
                {
                    currentSounder = NJF;
                    currentSounder.DonePlaying += SounderStop;
                    currentSounder.reader.Volume = volume;
                    AddToMixer(currentSounder);
                }
            }
            else
            {
                if (currentClip == NJF)
                {
                    ClipDone();
                }
                else if (currentClip != null)
                {
                    ClipDone();
                    currentClip = NJF;
                    currentClip.DonePlaying += ClipStop;
                    currentClip.reader.Volume = volume;
                    AddToMixer(currentClip);
                }
                else
                {
                    currentClip = NJF;
                    currentClip.DonePlaying += ClipStop;
                    currentClip.reader.Volume = volume;
                    AddToMixer(currentClip);
                }
            }

        }

        public void AddToMixer(IWaveProvider waveProvider)
        {
            if (waveProvider.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                mixer.AddInputStream(waveProvider);
            }
            if (waveProvider.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                MonoToStereoProvider16 mono = new MonoToStereoProvider16(waveProvider);
                mixer.AddInputStream(mono);
            }
            if (waveProvider.WaveFormat.Channels == 2 && mixer.WaveFormat.Channels == 1)
            {
                StereoToMonoProvider16 stereo = new StereoToMonoProvider16(waveProvider);
                mixer.AddInputStream(stereo);
            }
        }

        public void SounderStop()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                SounderDone();
            });
        }
        public void ClipStop()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ClipDone();
            });
        }

        public void SetVolume(float volume, bool isSounder)
        {
            if (isSounder && currentSounder != null)
            {
                currentSounder.reader.Volume = volume;
            } 
            else if (currentClip != null)
            {
                currentClip.reader.Volume = volume;
            }
        }

        public void SounderDone()
        {  
            if (currentSounder != null)
            {
                mixer.RemoveInputStream(currentSounder);
                currentSounder.reader.Position = 0;
                currentSounder.isDone = false;
                currentSounder.isPlaying = false;
                currentSounder = null;
                Trace.WriteLine("Sounder Stopped");
            }
            else
            {
                Trace.WriteLine("Current Sounder is Null");
            }
            
        }

        public void ClipDone()
        {
            if (currentClip != null)
            {
                mixer.RemoveInputStream(currentClip);
                currentClip.reader.Position = 0;
                currentClip.isDone = false;
                currentClip.isPlaying = false;
                currentClip = null;
            }
        }

        public void Stop(NJFileReader path)
        {
            mixer.RemoveInputStream(path.reader);
        }

        public int GetTimeLeft(AudioFileReader path)
        {
            if (path.Position != 0)
            {
                int timeleft = (int)Math.Ceiling(path.TotalTime.TotalSeconds - path.CurrentTime.TotalSeconds);
                return timeleft;
            }
            else
            {
                return -1;
            }

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
