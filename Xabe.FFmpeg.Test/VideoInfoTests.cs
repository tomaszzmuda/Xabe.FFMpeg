﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg.Enums;
using Xunit;

namespace Xabe.FFmpeg.Test
{
    public class VideoInfoTests
    {
        [Fact]
        public async Task AudioPopertiesTest()
        {
            IMediaInfo mediaInfo = await MediaInfo.Get(Resources.Mp3);

            Assert.True(File.Exists(mediaInfo.FileInfo.FullName));
            Assert.Equal(FileExtensions.Mp3, mediaInfo.FileInfo.Extension);
            Assert.Equal("audio.mp3", mediaInfo.FileInfo.Name);

            Assert.Equal("mp3", mediaInfo.Properties.AudioFormat);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Properties.AudioDuration);

            Assert.Equal(0, mediaInfo.Properties.FrameRate);
            Assert.Equal(0, mediaInfo.Properties.Height);
            Assert.Equal(0, mediaInfo.Properties.Width);
            Assert.Null(mediaInfo.Properties.Ratio);
            Assert.Null(mediaInfo.Properties.VideoFormat);
            Assert.Equal(TimeSpan.FromSeconds(0), mediaInfo.Properties.VideoDuration);

            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Properties.Duration);
            Assert.Equal(216916, mediaInfo.Properties.Size);
        }

        [Fact]
        public async Task IncorrectFormatTest()
        {
            await Assert.ThrowsAsync<ArgumentException>(async() => await MediaInfo.Get(Resources.Dll));
        }

        [Fact]
        public async Task MkvPropertiesTest()
        {
            IMediaInfo mediaInfo = await MediaInfo.Get(Resources.MkvWithAudio);

            Assert.True(File.Exists(mediaInfo.FileInfo.FullName));
            Assert.Equal(FileExtensions.Mkv, mediaInfo.FileInfo.Extension);
            Assert.Equal("SampleVideo_360x240_1mb.mkv", mediaInfo.FileInfo.Name);

            Assert.Equal("aac", mediaInfo.Properties.AudioFormat);
            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Properties.AudioDuration);

            Assert.Equal(25, mediaInfo.Properties.FrameRate);
            Assert.Equal(240, mediaInfo.Properties.Height);
            Assert.Equal(320, mediaInfo.Properties.Width);
            Assert.Equal("4:3", mediaInfo.Properties.Ratio);
            Assert.Equal("h264", mediaInfo.Properties.VideoFormat);
            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Properties.VideoDuration);

            Assert.Equal(TimeSpan.FromSeconds(9), mediaInfo.Properties.Duration);
            Assert.Equal(1055721, mediaInfo.Properties.Size);
        }

        [Fact]
        public async Task PropertiesTest()
        {
            IMediaInfo mediaInfo = await MediaInfo.Get(Resources.Mp4WithAudio);

            Assert.True(File.Exists(mediaInfo.FileInfo.FullName));
            Assert.Equal(FileExtensions.Mp4, mediaInfo.FileInfo.Extension);
            Assert.Equal("input.mp4", mediaInfo.FileInfo.Name);

            Assert.Equal("aac", mediaInfo.Properties.AudioFormat);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Properties.AudioDuration);

            Assert.Equal(25, mediaInfo.Properties.FrameRate);
            Assert.Equal(720, mediaInfo.Properties.Height);
            Assert.Equal(1280, mediaInfo.Properties.Width);
            Assert.Equal("16:9", mediaInfo.Properties.Ratio);
            Assert.Equal("h264", mediaInfo.Properties.VideoFormat);
            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Properties.VideoDuration);

            Assert.Equal(TimeSpan.FromSeconds(13), mediaInfo.Properties.Duration);
            Assert.Equal(2107842, mediaInfo.Properties.Size);
        }

        [Fact]
        public async Task ToStringTest()
        {
            IMediaInfo videoInfo = await MediaInfo.Get(Resources.Mp4WithAudio);
            string output = videoInfo.ToString();
            string expectedOutput =
                $"Video name: input.mp4{Environment.NewLine}Video extension : .mp4{Environment.NewLine}Video duration : 00:00:13{Environment.NewLine}Video format : h264{Environment.NewLine}Audio format : aac{Environment.NewLine}Audio duration : 00:00:13{Environment.NewLine}Aspect Ratio : 16:9{Environment.NewLine}Framerate : 16:9 fps{Environment.NewLine}Resolution : 1280 x 720{Environment.NewLine}Size : 2107842 b";
            Assert.EndsWith(expectedOutput, output);
        }
    }
}
