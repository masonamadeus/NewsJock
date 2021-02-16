using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace NewsBuddy
{
    class NJAudioPlayer
    {

        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public enum PlaybackState
        {
            Playing, Stopped, Paused
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

            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            _outputDS.Init(wc);
        }

        public void Play(PlaybackState playbackState, double currentVolume)
        {
            if (playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused)
            {
                _outputDS.Play();
                
            }
        }

    }
}
