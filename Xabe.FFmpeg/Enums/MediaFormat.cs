﻿namespace Xabe.FFmpeg.Enums
{
    /// <summary>
    ///     Video format ("ffmpeg -formats")
    /// </summary>
    public class MediaFormat
    {
        /// <summary>
        ///     Create new media format
        /// </summary>
        /// <param name="format">Media format name</param>
        public MediaFormat(string format)
        {
            Format = format;
        }

        /// <summary>
        ///     Video codec
        /// </summary>
        public string Format { get; }

        /// <summary>
        ///     AVI (Audio Video Interleaved)
        /// </summary>
        public static MediaFormat Avi => new MediaFormat("avi");

        /// <summary>
        ///     MPEG-2 PS (DVD VOB)
        /// </summary>
        public static MediaFormat Dvd => new MediaFormat("dvd");

        /// <summary>
        ///     FLV (Flash Video)
        /// </summary>
        public static MediaFormat Flv => new MediaFormat("flv");

        /// <summary>
        ///     raw H.264 video
        /// </summary>
        public static MediaFormat H264 => new MediaFormat("h264");

        /// <summary>
        ///     raw HEVC video
        /// </summary>
        public static MediaFormat Hevc => new MediaFormat("hevc");

        /// <summary>
        ///     Matroska
        /// </summary>
        public static MediaFormat Matroska => new MediaFormat("matroska");

        /// <summary>
        ///     Quicktime / MOV
        /// </summary>
        public static MediaFormat Mov => new MediaFormat("mov");

        /// <summary>
        ///     MP4 (MPEG-4 Part 14)
        /// </summary>
        public static MediaFormat Mp4 => new MediaFormat("mp4");

        /// <summary>
        ///     MPEG-1 Systems / MPEG program stream
        /// </summary>
        public static MediaFormat Mpeg => new MediaFormat("mpeg");

        /// <summary>
        ///     MPEG-TS (MPEG-2 Transport Stream)
        /// </summary>
        public static MediaFormat Mpegts => new MediaFormat("mpegts");

        /// <summary>
        ///     Ogg
        /// </summary>
        public static MediaFormat Ogg => new MediaFormat("ogg");

        /// <summary>
        ///     Raw video
        /// </summary>
        public static MediaFormat Rawvideo => new MediaFormat("rawvideo");

        /// <summary>
        ///     GdiGrab
        /// </summary>
        public static MediaFormat GdiGrab => new MediaFormat("gdigrab");

        /// <summary>
        ///    AVFoundation
        /// </summary>
        public static MediaFormat AVFoundation => new MediaFormat("avfoundation");

        /// <summary>
        ///     X11Grab
        /// </summary>
        public static MediaFormat X11Grab => new MediaFormat("x11grab");

        /// <summary>
        ///    ALSA
        /// </summary>
        public static MediaFormat ALSA => new MediaFormat("alsa");

        /// <summary>
        ///    OSS
        /// </summary>
        public static MediaFormat OSS => new MediaFormat("oss");

        /// <summary>
        ///    Pulse
        /// </summary>
        public static MediaFormat Pulse => new MediaFormat("pulse");

        /// <summary>
        ///    DShow
        /// </summary>
        public static MediaFormat DShow => new MediaFormat("dshow");

        /// <summary>
        ///    JACK
        /// </summary>
        public static MediaFormat JACK => new MediaFormat("jack");

        /// <summary>
        ///     Hash
        /// </summary>
        public static MediaFormat Hash => new MediaFormat("hash");

        /// <inheritdoc />
        public override string ToString()
        {
            return Format;
        }
    }
}
