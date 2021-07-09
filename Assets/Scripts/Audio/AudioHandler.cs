using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;

namespace JoyGodot.Assets.Scripts.Audio
{
    public class AudioHandler : IAudioHandler
    {
        public IEnumerable<AudioStreamRandomPitch> Values { get; }
        public JSONValueExtractor ValueExtractor { get; }

        protected IDictionary<string, AudioStreamRandomPitch> AudioStreams { get; set; }

        public AudioHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.AudioStreams = this.Load()
                .ToDictionary(
                    stream => stream.ResourceName, 
                    stream => stream);
        }

        public IEnumerable<AudioStreamRandomPitch> Load()
        {
            List<AudioStreamRandomPitch> audioStreams = new List<AudioStreamRandomPitch>();
            
            List<string> files = new List<string>(
                Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                "Sounds",
                "*.wav"));
            files.AddRange(
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() +
                    GlobalConstants.ASSETS_FOLDER +
                    "Sounds",
                    "*.ogg"));

            foreach (string file in files)
            {
                AudioStream audioStream = GD.Load<AudioStream>(file);
                int lastSlash = file.LastIndexOf('\\');
                int lastDot = file.LastIndexOf('.');

                string name = file.Substring(lastSlash + 1, lastDot - lastSlash - 1);
                audioStream.ResourceName = name;
                AudioStreamRandomPitch streamRandomPitch = new AudioStreamRandomPitch
                {
                    AudioStream = audioStream,
                    ResourceName = name,
                    RandomPitch = 1.2f
                };
                audioStreams.Add(streamRandomPitch);
            }

            return audioStreams;
        }

        public AudioStreamRandomPitch Get(string name)
        {
            return this.AudioStreams.TryGetValue(name, out AudioStreamRandomPitch audioStream) ? audioStream : null;
        }

        public bool Add(AudioStreamRandomPitch value)
        {
            if (this.AudioStreams.ContainsKey(value.ResourceName))
            {
                return false;
            }
            this.AudioStreams.Add(value.ResourceName, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (this.AudioStreams.ContainsKey(key))
            {
                return false;
            }

            return this.AudioStreams.Remove(key);
        }

        public void Dispose()
        { }
    }
}