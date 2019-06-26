﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xabe.FFmpeg.Enums;

namespace Xabe.FFmpeg.Streams
{
    /// <inheritdoc cref="IAudioStream" />
    public class AudioStream : IAudioStream, IFilterable
    {
        private readonly Dictionary<string, string> _audioFilters = new Dictionary<string, string>();
        private string _bitsreamFilter;
        private string _reverse;
        private string _seek;
        private string _split;
        private string _sampleRate;
        private string _channels;
        private string _bitrate;

        /// <inheritdoc />
        public IAudioStream Reverse()
        {
            _reverse = "-af areverse ";
            return this;
        }

        /// <inheritdoc />
        public string Build()
        {
            var builder = new StringBuilder();
            builder.Append(BuildAudioCodec());
            builder.Append(_bitsreamFilter);
            builder.Append(_sampleRate);
            builder.Append(_channels);
            builder.Append(_bitrate);
            builder.Append(_reverse);
            builder.Append(_split);
            return builder.ToString();
        }

        /// <inheritdoc />
        public string BuildInputArguments()
        {
            return _seek;
        }

        /// <inheritdoc />
        public string BuildAudioCodec()
        {
            if (Codec != null)
                return $"-c:a {Codec.ToString()} ";
            else
                return string.Empty;
        }

        /// <inheritdoc />
        public IAudioStream Split(TimeSpan startTime, TimeSpan duration)
        {
            _split = $"-ss {startTime.ToFFmpeg()} -t {duration.ToFFmpeg()} ";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream CopyStream()
        {
            _audioFilters["-c:v copy"] = string.Empty;
            return this;
        }

        /// <inheritdoc />
        public CodecType CodecType { get; } = CodecType.Audio;

        /// <inheritdoc />
        public IAudioStream SetChannels(int channels)
        {
            _channels = $"-ac:{Index} {channels} ";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream SetBitstreamFilter(BitstreamFilter filter)
        {
            _bitsreamFilter = $"-bsf:a {filter} ";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream ChangeBitrate(double bitRate)
        {
            _bitrate = $"-b:a:{Index} {bitRate} ";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream SetSampleRate(int sampleRate)
        {
            _sampleRate = $"-ar:{Index} {sampleRate} ";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream ChangeSpeed(double multiplication)
        {
            _audioFilters["atempo"] = $"{string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:N1}", MediaSpeedHelper.GetAudioSpeed(multiplication))}";
            return this;
        }

        /// <inheritdoc />
        public IAudioStream SetCodec(AudioCodec codec)
        {
            Codec = codec;
            return this;
        }

        /// <inheritdoc />
        public int Index { get; internal set; }

        /// <inheritdoc />
        public TimeSpan Duration { get; internal set; }

        /// <inheritdoc />
        public string Format { get; internal set; }

        /// <inheritdoc />
        public double Bitrate { get; set; }

        /// <inheritdoc />
        public int Channels { get; set; }

        /// <inheritdoc />
        public int SampleRate { get; set; }

        /// <inheritdoc />
        public AudioCodec Codec { get; private set; }

        /// <inheritdoc />
        public IEnumerable<string> GetSource()
        {
            return new[] { Source.FullName };
        }

        /// <inheritdoc />
        public FileInfo Source { get; internal set; }

        /// <inheritdoc />
        public IAudioStream SetSeek(TimeSpan? seek)
        {
            if (seek.HasValue)
            {
                _seek = $"-ss {seek.Value.ToFFmpeg()} ";
            }
            return this;
        }

        void ILocalStream.Split(TimeSpan startTime, TimeSpan duration)
        {
            Split(startTime, duration);
        }

        /// <inheritdoc />
        public IEnumerable<IFilterConfiguration> GetFilters()
        {
            if (_audioFilters.Any())
            {
                yield return new FilterConfiguration
                {
                    FilterType = "-filter:a",
                    StreamNumber = Index,
                    Filters = _audioFilters
                };
            }
        }
    }
}
