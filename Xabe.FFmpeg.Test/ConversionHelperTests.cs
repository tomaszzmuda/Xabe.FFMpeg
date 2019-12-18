﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg.Enums;
using Xabe.FFmpeg.Model;
using Xabe.FFmpeg.Streams;
using Xunit;

namespace Xabe.FFmpeg.Test
{
    public class ConversionHelperTests

    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        public async Task ToGifTest(int loopCount, int delay)
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Gif);

            IConversionResult result = await Conversion.ToGif(Resources.Mp4, output, loopCount, delay)
                                             .SetPreset(ConversionPreset.UltraFast)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Empty(mediaInfo.AudioStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.Equal("gif", videoStream.Format);
            Assert.Equal("16:9", videoStream.Ratio);
            Assert.Equal(25, videoStream.FrameRate);
            Assert.Equal(1280, videoStream.Width);
            Assert.Equal(720, videoStream.Height);
        }

        public static IEnumerable<object[]> JoinFiles => new[]
        {
            new object[] {Resources.MkvWithAudio, Resources.Mp4WithAudio, 23, 1280, 720},
            new object[] {Resources.MkvWithAudio, Resources.MkvWithAudio, 19, 320, 240},
            new object[] {Resources.MkvWithAudio, Resources.Mp4, 23, 1280, 720}
        };

        [Theory]
        [MemberData(nameof(JoinFiles))]
        public async Task JoinWith(string firstFile, string secondFile, int duration, int width, int height)
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);

            IConversionResult result = await Conversion.Concatenate(output, firstFile, secondFile).ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(duration), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.Equal(width, videoStream.Width);
            Assert.Equal(height, videoStream.Height);
        }

        [Fact]
        public async Task AddAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);

            IConversionResult result = await Conversion.AddAudio(Resources.Mp4, Resources.Mp3, output)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Single(mediaInfo.AudioStreams);
            Assert.Equal("aac", mediaInfo.AudioStreams.First()
                                         .Format);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Duration);
        }

        [Fact]
        public async Task AddSubtitleTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
            string input = Resources.MkvWithAudio;

            IConversionResult result = await Conversion.AddSubtitle(input, output, Resources.SubtitleSrt)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo outputInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(3071), outputInfo.Duration);
            Assert.Single(outputInfo.SubtitleStreams);
            Assert.Single(outputInfo.VideoStreams);
            Assert.Single(outputInfo.AudioStreams);
        }

        [Fact]
        public async Task AddSubtitleWithLanguageTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
            string input = Resources.MkvWithAudio;

            var language = "pol";
            IConversionResult result = await Conversion.AddSubtitle(input, output, Resources.SubtitleSrt, language)
                                             .SetPreset(ConversionPreset.UltraFast)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo outputInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(3071), outputInfo.Duration);
            Assert.Single(outputInfo.SubtitleStreams);
            Assert.Single(outputInfo.VideoStreams);
            Assert.Single(outputInfo.AudioStreams);
            Assert.Equal(language, outputInfo.SubtitleStreams.First()
                                             .Language);
        }

        [Fact]
        public async Task BurnSubtitleTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
            string input = Resources.Mp4;

            IConversionResult result = await Conversion.AddSubtitles(input, output, Resources.SubtitleSrt)
                                             .SetPreset(ConversionPreset.UltraFast)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo outputInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.Duration);
        }

        [Fact]
        public async Task ChangeSizeTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
            string input = Resources.MkvWithAudio;

            IConversionResult result = await Conversion.ChangeSize(input, output, new VideoSize(640, 360))
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Single(mediaInfo.VideoStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.Equal(640, videoStream.Width);
            Assert.Equal(360, videoStream.Height);
        }

        [Fact]
        public async Task ExtractAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp3);
            IConversionResult result = await Conversion.ExtractAudio(Resources.Mp4WithAudio, output)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Empty(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            Assert.NotNull(audioStream);
            Assert.Equal("mp3", audioStream.Format);
        }

        [Fact]
        public async Task ExtractVideo()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(Resources.Mp4WithAudio));

            IConversionResult result = await Conversion.ExtractVideo(Resources.Mp4WithAudio, output)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Empty(mediaInfo.AudioStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.Equal("h264", videoStream.Format);
        }

        [Fact]
        public async Task SnapshotInvalidArgumentTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Png);
            await Assert.ThrowsAsync<ArgumentException>(async () => await Conversion.Snapshot(Resources.Mp4WithAudio, output, TimeSpan.FromSeconds(999))
                                                                                    .Start().ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task SnapshotTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Png);
            IConversionResult result = await Conversion.Snapshot(Resources.Mp4WithAudio, output, TimeSpan.FromSeconds(0))
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            Assert.True(File.Exists(output));
            Assert.Equal(1825653, (await File.ReadAllBytesAsync(output).ConfigureAwait(false)).LongLength);
        }

        [Fact]
        public async Task SplitVideoTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
            IConversionResult result = await Conversion.Split(Resources.Mp4WithAudio, output, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8))
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("aac", audioStream.Format);
            Assert.Equal("h264", videoStream.Format);
            Assert.Equal(TimeSpan.FromSeconds(8), audioStream.Duration);
            Assert.Equal(TimeSpan.FromSeconds(8), videoStream.Duration);
            Assert.Equal(TimeSpan.FromSeconds(8), mediaInfo.Duration);
        }

        [Fact]
        public async Task ToMp4Test()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Mp4);

            IConversionResult result = await Conversion.ToMp4(Resources.MkvWithAudio, output)
                                          .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("h264", videoStream.Format);
            Assert.Equal("aac", audioStream.Format);
        }

        [Fact]
        public async Task ToOgvTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Ogv);

            IConversionResult result = await Conversion.ToOgv(Resources.MkvWithAudio, output)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("theora", videoStream.Format);
            Assert.Equal("vorbis", audioStream.Format);
        }

        [Fact]
        public async Task ToTsTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Ts);

            IConversionResult result = await Conversion.ToTs(Resources.Mp4WithAudio, output)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("mpeg2video", videoStream.Format);
            Assert.Equal("mp2", audioStream.Format);
        }

        [Fact]
        public async Task ToWebMTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.WebM);

            IConversionResult result = await Conversion.ToWebM(Resources.Mp4WithAudio, output)
                                             .SetPreset(ConversionPreset.UltraFast)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("vp8", videoStream.Format);
            Assert.Equal("vorbis", audioStream.Format);
        }

        [Fact]
        public async Task WatermarkTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
            IConversionResult result = await Conversion.SetWatermark(Resources.Mp4WithAudio, output, Resources.PngSample, Position.Center)
                                             .Start().ConfigureAwait(false);

            Assert.True(result.Success);
            Assert.Contains("overlay=", result.Arguments);
            Assert.Contains(Resources.Mp4WithAudio, result.Arguments);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("aac", audioStream.Format);
            Assert.Equal("h264", videoStream.Format);
        }

        [Theory]
        [InlineData("https://bitdash-a.akamaihd.net/content/MI201109210084_1/m3u8s/f08e80da-bf1d-4e3d-8899-f0f6155f6efa.m3u8", true)]
        [InlineData("www.bitdash-a.akamaihd.net/content/MI201109210084_1/m3u8s/f08e80da-bf1d-4e3d-8899-f0f6155f6efa.m3u8", false)]
        public async Task SaveM3U8Stream(string input, bool success)
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), "mkv");

            Task<IConversionResult> ConversionAction() => Conversion.SaveM3U8Stream(new Uri(input), output, TimeSpan.FromSeconds(1))
                                                                    .Start();

            if (success)
            {
                IConversionResult result = await ConversionAction().ConfigureAwait(false);
                Assert.True(result.Success);
            }
            else
            {
                await Assert.ThrowsAsync<UriFormatException>(ConversionAction).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ConversionWithoutSpecificFormat()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Mp4);

            IConversionResult result = await Conversion.Convert(Resources.MkvWithAudio, output).Start().ConfigureAwait(false);

            Assert.True(result.Success);
            IMediaInfo mediaInfo = await MediaInfo.Get(output).ConfigureAwait(false);
            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Duration);
            Assert.Single(mediaInfo.VideoStreams);
            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("h264", videoStream.Format);
            Assert.Equal("aac", audioStream.Format);
        }

        [RunnableInDebugOnly]
        public async Task ConversionWithHardware()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Mp4);

            IConversionResult result = await Conversion.ConvertWithHardware(Resources.MkvWithAudio, output, HardwareAccelerator.cuvid, VideoCodec.H264_cuvid, VideoCodec.H264_nvenc).Start().ConfigureAwait(false);

            Assert.True(result.Success);
            Assert.Equal(TimeSpan.FromSeconds(10), result.MediaInfo.Value.Duration);
            Assert.Single(result.MediaInfo.Value.VideoStreams);
            Assert.Single(result.MediaInfo.Value.AudioStreams);
            IAudioStream audioStream = result.MediaInfo.Value.AudioStreams.First();
            IVideoStream videoStream = result.MediaInfo.Value.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.NotNull(audioStream);
            Assert.Equal("h264", videoStream.Format);
            Assert.Equal("aac", audioStream.Format);
        }
    }
}
