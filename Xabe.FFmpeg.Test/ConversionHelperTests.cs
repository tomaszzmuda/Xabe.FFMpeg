﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg.Enums;
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

            bool result = await ConversionHelper.ToGif(Resources.Mp4, output, loopCount, delay)
                                                .Start();

            Assert.True(result);
            var mediaInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(0), mediaInfo.Properties.Duration);
            Assert.Equal("gif", mediaInfo.Properties.VideoFormat);
            Assert.Null(mediaInfo.Properties.AudioFormat);
            Assert.Equal("16:9", mediaInfo.Properties.Ratio);
            Assert.Equal(25, mediaInfo.Properties.FrameRate);
            Assert.Equal(1280, mediaInfo.Properties.Width);
            Assert.Equal(720, mediaInfo.Properties.Height);
        }

        [Fact]
        public async Task AddAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);

            bool result = await ConversionHelper.AddAudio(Resources.Mp4, Resources.Mp3, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal("aac", outputInfo.Properties.AudioFormat);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.Properties.Duration);
        }

        [Fact]
        public async Task AddSubtitleTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
            string input = Resources.MkvWithAudio;

            bool result = await ConversionHelper.AddSubtitle(input, output, Resources.SubtitleSrt, "pol")
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(3071), outputInfo.Properties.Duration);
        }

        [Fact]
        public async Task BurnSubtitleTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
            string input = Resources.Mp4;

            bool result = await ConversionHelper.BurnSubtitle(input, output, Resources.SubtitleSrt)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.Properties.Duration);
        }

        [Fact]
        public async Task ChangeSizeTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
            string input = Resources.MkvWithAudio;

            bool result = await ConversionHelper.ChangeSize(input, output, new VideoSize(640, 360))
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(640, outputInfo.Properties.Width);
            Assert.Equal(360, outputInfo.Properties.Height);
        }

        [Fact]
        public async Task ExtractAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp3);
            bool result = await ConversionHelper.ExtractAudio(Resources.Mp4WithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal("mp3", outputInfo.Properties.AudioFormat);
            Assert.Null(outputInfo.Properties.VideoFormat);
        }

        [Fact]
        public async Task ExtractVideo()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(Resources.Mp4WithAudio));

            bool result = await ConversionHelper.ExtractVideo(Resources.Mp4WithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal("h264", outputInfo.Properties.VideoFormat);
            Assert.Null(outputInfo.Properties.AudioFormat);
        }

        public static IEnumerable<object[]> JoinFiles => new[]
        {
            new object[] {Resources.MkvWithAudio, Resources.Mp4WithAudio, 23, 1280, 720},
            new object[] {Resources.MkvWithAudio, Resources.MkvWithAudio, 19, 320, 240},
            new object[] {Resources.MkvWithAudio, Resources.Mp4, 23, 1280, 720 }
        };

        [Theory]
        [MemberData(nameof(JoinFiles))]
        public async Task JoinWith(string firstFile, string secondFile, int duration, int width, int height)
        {
        string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);

        bool result = await ConversionHelper.Concatenate(output, firstFile, secondFile);

        Assert.True(result);
        var outputInfo = await MediaInfo.Get(output);
        Assert.Equal(TimeSpan.FromSeconds(duration), outputInfo.Properties.Duration);
        Assert.Equal(width, outputInfo.Properties.Width);
        Assert.Equal(height, outputInfo.Properties.Height);
        }

        [Fact]
        public async Task SnapshotTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Png);
            bool result = await ConversionHelper.Snapshot(Resources.Mp4WithAudio, output)
                                                .Start();

            Assert.True(result);
            Assert.True(File.Exists(output));
            Assert.Equal(1890492, (await File.ReadAllBytesAsync(output)).LongLength);
        }

        [Fact]
        public async Task SplitVideoTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
            bool result = await ConversionHelper.Split(Resources.Mp4WithAudio, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8), output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal("aac", outputInfo.Properties.AudioFormat);
            Assert.Equal("h264", outputInfo.Properties.VideoFormat);
            Assert.Equal(TimeSpan.FromSeconds(8), outputInfo.Properties.AudioDuration);
            Assert.Equal(TimeSpan.FromSeconds(8), outputInfo.Properties.VideoDuration);
            Assert.Equal(TimeSpan.FromSeconds(8), outputInfo.Properties.Duration);
        }

        [Fact]
        public async Task ToMp4Test()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Mp4);

            bool result = await ConversionHelper.ToMp4(Resources.MkvWithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(9), outputInfo.Properties.Duration);
            Assert.Equal("h264", outputInfo.Properties.VideoFormat);
            Assert.Equal("aac", outputInfo.Properties.AudioFormat);
        }

        [Fact]
        public async Task ToOgvTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Ogv);

            bool result = await ConversionHelper.ToOgv(Resources.MkvWithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(9), outputInfo.Properties.Duration);
            Assert.Equal("theora", outputInfo.Properties.VideoFormat);
            Assert.Equal("vorbis", outputInfo.Properties.AudioFormat);
        }

        [Fact]
        public async Task ToTsTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.Ts);

            bool result = await ConversionHelper.ToTs(Resources.Mp4WithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.Properties.Duration);
            Assert.Equal("h264", outputInfo.Properties.VideoFormat);
            Assert.Equal("aac", outputInfo.Properties.AudioFormat);
        }

        [Fact]
        public async Task ToWebMTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + FileExtensions.WebM);

            bool result = await ConversionHelper.ToWebM(Resources.Mp4WithAudio, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.Properties.Duration);
            Assert.Equal("vp8", outputInfo.Properties.VideoFormat);
            Assert.Equal("vorbis", outputInfo.Properties.AudioFormat);
        }

        [Fact]
        public async Task WatermarkTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp3);
            bool result = await ConversionHelper.SetWatermark(Resources.Mp4WithAudio, Resources.PngSample, Position.Center, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = await MediaInfo.Get(output);
            Assert.Equal("mp3", outputInfo.Properties.AudioFormat);
            Assert.Equal("png", outputInfo.Properties.VideoFormat);
        }
    }
}
