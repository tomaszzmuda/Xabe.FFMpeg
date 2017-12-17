﻿// ReSharper disable InconsistentNaming
namespace Xabe.FFmpeg.Enums
{
    /// <summary>
    ///     Audio codec ("ffmpeg -codecs")
    /// </summary>
    public class AudioCodec
    {
        /// <summary>
        ///     Audio codec
        /// </summary>
        public string Codec { get; }

        /// <summary>
        ///     AAC (Advanced Audio Coding) (decoders: aac aac_fixed ) (encoders: aac libvo_aacenc )
        /// </summary>
        public static AudioCodec aac = new AudioCodec("aac");

        /// <summary>
        ///      Vorbis (decoders: vorbis libvorbis ) (encoders: vorbis libvorbis )
        /// </summary>
        public static AudioCodec libvorbis = new AudioCodec("libvorbis");

        /// <inheritdoc />
        public override string ToString()
        {
            return Codec;
        }

        /// <inheritdoc />
        public AudioCodec(string format)
        {
            Codec = format;
        }
    }
}
