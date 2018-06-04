using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace SharpDX_Testing
{
    public static class TCM_Audio
    {
        static XAudio2 xa2;
        static MasteringVoice mv;
        public static Dictionary<SourceVoice, AudioBuffer> sources;
        public static void Initialize()
        {
            sources = new Dictionary<SourceVoice, AudioBuffer>();
            xa2 = new XAudio2();
            mv = new MasteringVoice(xa2);

            //don't worry about this
            /*SharpDX.DirectSound.Gargle garg = new SharpDX.DirectSound.Gargle(new IntPtr());
            SharpDX.DirectSound.GargleSettings gargy = new SharpDX.DirectSound.GargleSettings();
            gargy.RateHz = 15;
            gargy.WaveShape = SharpDX.DirectSound.Gargle.WaveShapeSquare;
            garg.AllParameters = gargy;*/
        }
        public static void disposeAudio()
        {
            mv.Dispose();
            xa2.Dispose();
            foreach (SourceVoice s in sources.Keys)
            {
                finishSource(s);
            }
        }
        public static void setVolume()
        {
            //mv.SetChannelVolumes()
        }
        /// <summary>
        /// Plays a sound from the sounds folder.
        /// Sound List:
        /// (1) "buzz" - electric noise, raises pitch and volume.
        /// (2) "zzub" - buzz backwards.
        /// (3) "grunt" - a grunt (by me)
        /// (4) "pew" - pew pew (but only one)
        /// </summary>
        /// <param name="file">The name of the file, not including ".wav" or its location.</param>
        public static void playWAV(string file)
        {
            playSoundFile("../../sounds/" + file + ".wav");
        }

        public static void playSoundFile(string filename)
        {
            var ss = new SoundStream(File.OpenRead(filename));
            var waveFormat = ss.Format;
            var ab = new AudioBuffer
            {
                Stream = ss.ToDataStream(),
                AudioBytes = (int)ss.Length,
                Flags = BufferFlags.EndOfStream
            };
            ss.Close();

            var sv = new SourceVoice(xa2, waveFormat, true);
            //sv.BufferEnd += (context) => Console.WriteLine(" => event received: end of buffer");
            //sv.StreamEnd += () => finishPlaying(sv, ab);
            sv.SubmitSourceBuffer(ab, ss.DecodedPacketsInfo);
            sv.Start();
            sources.Add(sv, ab);
        }

        //todo: pause and resume audio bits
        /*
        public static void pauseAllAudio()
        {
            var iter = sources.GetEnumerator();
            bool hasNext = iter.MoveNext();
            while(hasNext)
            {
                SourceVoice a = iter.Current.Key;
                AudioBuffer b = iter.Current.Value;
                hasNext = iter.MoveNext();
                if (a.State.BuffersQueued <= 0)
                {
                    a.Stop();
                }
            }
        }
        public static void resumeAllAudio()
        {
            var iter = sources.GetEnumerator();
            bool hasNext = iter.MoveNext();
            while (hasNext)
            {
                SourceVoice a = iter.Current.Key;
                AudioBuffer b = iter.Current.Value;
                hasNext = iter.MoveNext();
                if (a.State.BuffersQueued <= 0)
                {
                    a.Start();
                }
            }
        }
        */
        public static void cleanOutSources()
        {
            List<SourceVoice> svremove = new List<SourceVoice>();
            var iter = sources.GetEnumerator();
            bool hasNext = iter.MoveNext();
            while (hasNext)
            {
                SourceVoice a = iter.Current.Key;
                AudioBuffer b = iter.Current.Value;
                hasNext = iter.MoveNext();
                if (a.State.BuffersQueued <= 0)
                {
                    svremove.Add(a);
                }
            }
            foreach (SourceVoice sv in svremove)
            {
                finishSource(sv);
                sources.Remove(sv);
            }
            svremove.Clear();
        }
        public static void clear()
        {
            List<SourceVoice> svremove = new List<SourceVoice>();
            var iter = sources.GetEnumerator();
            bool hasNext = iter.MoveNext();
            while (hasNext)
            {
                SourceVoice a = iter.Current.Key;
                AudioBuffer b = iter.Current.Value;
                hasNext = iter.MoveNext();
                svremove.Add(a);
            }
            foreach (SourceVoice sv in svremove)
            {
                finishSource(sv);
                sources.Remove(sv);
            }
            svremove.Clear();
        }
        private static void finishSource(SourceVoice s)
        {
            if (sources.ContainsKey(s))
            {
                s.DestroyVoice();
                s.Dispose();
                sources[s].Stream.Dispose();
            }
        }
    }
}
