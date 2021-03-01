using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;
using System.Threading;

namespace NewsBuddy
{


    public class NJFileReader : ISampleProvider
    {
        public readonly AudioFileReader reader;

        public bool isSounder;

        public bool isDone { get; set; }

        public bool isPlaying { get; set; }

        public bool toMono { get; set; }

        public bool toStereo { get; set; }

        public string source { get; set; }
        public float volumeLeft { get; private set; }
        public float volumeRight { get; private set; }

        public event Action DonePlaying;

        public WaveFormat WaveFormat { get; set; }

        private float[] sourceBuffer;



        public NJFileReader(AudioFileReader reader, bool isSounder, string name)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
            this.isSounder = isSounder;
            this.source = name;
            toMono = false;
            toStereo = false;

        }



        public void MakeMono()
        {
            toMono = true;
            toStereo = false;
            if (isSounder)
            {
                if (Settings.Default.ASIOSounderLeft)
                {
                    volumeLeft = 1;
                    volumeRight = 0;
                } else
                {
                    volumeLeft = 0;
                    volumeRight = 1;
                }
            }
            else
            {
                if (Settings.Default.ASIOClipLeft)
                {
                    volumeLeft = 1;
                    volumeRight = 0;
                }
                else
                {
                    volumeLeft = 0;
                    volumeRight = 1;
                }
            }
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
        }

        public void MakeStereo()
        {
            toStereo = true;
            toMono = false;
            volumeRight = 0.5f;
            volumeLeft = 0.5f;
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        }



        public int Read(float[] buffer, int offset, int count)
        {
            if (isDone)
            {
                if (isPlaying)
                {
                    isPlaying = false;
                    Trace.WriteLine("NotPlayingAnoymore");
                }
                return 0;
            }

            if (toStereo)
            {
                var sourceSamplesRequired = count / 2;
                var outIndex = offset;
                EnsureSourceBuffer(sourceSamplesRequired);
                int sourceSamplesRead = reader.Read(sourceBuffer, 0, sourceSamplesRequired);
                if (sourceSamplesRead < sourceSamplesRequired)
                {
                    isDone = true;
                    DonePlaying?.Invoke();
                }
                for (var n=0; n < sourceSamplesRead; n++)
                {
                    buffer[outIndex++] = sourceBuffer[n]; // add left vol multiplier here;
                    buffer[outIndex++] = sourceBuffer[n];
                }
                if (!isPlaying)
                {
                    isPlaying = true;
                }
                Trace.WriteLine(sourceSamplesRead * 2);
                return sourceSamplesRead * 2;
            }   

            if (!toStereo && !toMono)
            {
                try
                {
                    int read = reader.Read(buffer, offset, count);
                    if (read < count)
                    {
                        isDone = true;
                        DonePlaying?.Invoke();
                    }
                    if (!isPlaying)
                    {
                        isPlaying = true;
                    }
                    //Trace.WriteLine(read);
                    return read;
                }
                catch
                {
                    return 0;
                }
            }
            return 0;

        }

        private void EnsureSourceBuffer(int count)
        {
            if (sourceBuffer == null || sourceBuffer.Length < count)
            {
                sourceBuffer = new float[count];
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


        private readonly MixingSampleProvider mixer;
        public NJFileReader currentSounder;
        public NJFileReader currentClip;

        public NJAsioMixer(string ASIOdriver, int ASIOoffset)
        {
            _outputASIO = new AsioOut(ASIOdriver);
            mixer = new MixingSampleProvider(
                WaveFormat.CreateIeeeFloatWaveFormat(44100,2));
            mixer.ReadFully = true;
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
        
        public void AddToMixer(NJFileReader waveProvider)
        {
            if (waveProvider.reader.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                waveProvider.MakeStereo();
            }
            if (waveProvider.reader.WaveFormat.Channels == 2 && mixer.WaveFormat.Channels == 1)
            {
                waveProvider.MakeMono();
            }
            mixer.AddMixerInput(waveProvider);
        } 

        public void SounderStop()
        {
            Trace.WriteLine("Trying to stop sounder");
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
                mixer.RemoveMixerInput(currentSounder);
                currentSounder.reader.Position = 0;
                currentSounder.isDone = false;
                currentSounder.isPlaying = false;
                currentSounder.DonePlaying -= SounderStop;
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
                mixer.RemoveMixerInput(currentClip);
                currentClip.reader.Position = 0;
                currentClip.isDone = false;
                currentClip.isPlaying = false;
                currentClip.DonePlaying -= ClipStop;
                currentClip = null;
            }
        }

        public void Stop(NJFileReader path)
        {
            mixer.RemoveMixerInput(path.reader);
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
