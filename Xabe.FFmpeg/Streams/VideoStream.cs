﻿using System;
using System.IO;
using System.Text;
using Xabe.FFmpeg.Enums;

namespace Xabe.FFmpeg
{
    /// <inheritdoc />
    public class VideoStream : IVideoStream
    {
        private string _bitsreamFilter;
        private string _burnSubtitles;
        private string _codec;
        private string _frameCount;
        private string _loop;
        private string _reverse;
        private string _scale;
        private string _seek;
        private string _size;
        private string _preset;
        private string _split;
        private string _speed;
        private string _rotate;

        /// <inheritdoc />
        public int Width { get; internal set; }

        /// <inheritdoc />
        public int Height { get; internal set; }

        /// <inheritdoc />
        public double FrameRate { get; internal set; }

        /// <inheritdoc />
        public string Ratio { get; internal set; }

        /// <inheritdoc />
        public FileInfo Source { get; internal set; }

        /// <inheritdoc />
        public string Build()
        {
            var builder = new StringBuilder();
            builder.Append(_scale);
            builder.Append(_codec);
            builder.Append(_preset);
            builder.Append(_bitsreamFilter);
            builder.Append(_seek);
            builder.Append(_frameCount);
            builder.Append(_loop);
            builder.Append(_split);
            builder.Append(_reverse);
            builder.Append(_rotate);
            builder.Append(_size);
            builder.Append(BuildFilter());
            return builder.ToString();
        }

        /// <inheritdoc />
        public IVideoStream SetPresset(ConversionSpeed speed)
        {
            _preset = $"-preset {speed.ToString().ToLower()} ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream ChangeSpeed(double multiplication)
        {
            _speed = MediaSpeedHelper.GetVideoSpeed(multiplication);
            return this;
        }

        /// <inheritdoc />
        public IVideoStream Rotate(RotateDegrees rotateDegrees)
        {
            if(rotateDegrees == RotateDegrees.Invert)
                _rotate = "-vf \"transpose=2,transpose=2\" ";
            else
                _rotate = $"-vf \"transpose={(int)rotateDegrees}\" ";
            return this;
        }

        /// <inheritdoc />
        public CodecType CodecType { get; } = CodecType.Video;

        /// <inheritdoc />
        public double Bitrate { get; internal set; }

        /// <inheritdoc />
        public IVideoStream CopyStream()
        {
            _codec = "-c:v copy ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetLoop(int count, int delay)
        {
            _loop = $"-loop {count} ";
            if(delay > 0)
                _loop += $"-final_delay {delay / 100} ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream AddSubtitles(string subtitlePath, string encode, string style, VideoSize originalSize)
        {
            _burnSubtitles = $"\"subtitles='{subtitlePath}'".Replace("\\", "\\\\")
                                                            .Replace(":", "\\:");

            if(!string.IsNullOrEmpty(encode))
                _burnSubtitles += $":charenc={encode}";
            if(!string.IsNullOrEmpty(style))
                _burnSubtitles += $":force_style=\'{style}\'";
            if(originalSize != null)
                _burnSubtitles += $":original_size={originalSize}";
            _burnSubtitles += "\" ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream Reverse()
        {
            _reverse = "-vf reverse ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetScale(VideoSize size)
        {
            if(size != null)
                _scale = $"-vf scale={size} ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetSize(VideoSize size)
        {
            if(size != null)
                _size = $"-s {size} ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetCodec(VideoCodec codec, int bitrate = 0)
        {
            _codec = $"-codec:v {codec} ";

            if(bitrate > 0)
                _codec += $"-b:v {bitrate}k ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetBitstreamFilter(BitstreamFilter filter)
        {
            _bitsreamFilter = $"-bsf:v {filter} ";
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetSeek(TimeSpan seek)
        {
            if(seek != null)
            {
                if(seek > Duration)
                    throw new ArgumentException("Seek can not be greater than video duration");
                _seek = $"-ss {seek} ";
            }
            return this;
        }

        /// <inheritdoc />
        public IVideoStream SetOutputFramesCount(int number)
        {
            _frameCount = $"-frames:v {number} ";
            return this;
        }

        /// <inheritdoc />
        public TimeSpan Duration { get; internal set; }

        /// <inheritdoc />
        public string Format { get; internal set; }

        /// <inheritdoc />
        public int Index { get; internal set; }

        private string BuildFilter()
        {
            var builder = new StringBuilder();
            builder.Append("-filter:v ");
            builder.Append(_preset);
            builder.Append(_burnSubtitles);
            builder.Append(_speed);

            string filter = builder.ToString();
            if(filter == "-filter:v ")
                return "";
            return filter;
        }

        /// <inheritdoc />
        public IVideoStream Split(TimeSpan startTime, TimeSpan duration)
        {
            _split = $"-ss {startTime.ToFFmpeg()} -t {duration.ToFFmpeg()} ";
            return this;
        }

        void IStream.Split(TimeSpan startTime, TimeSpan duration)
        {
            Split(startTime, duration);
        }
    }
}
