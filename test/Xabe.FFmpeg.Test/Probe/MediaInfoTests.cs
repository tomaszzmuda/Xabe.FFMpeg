﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg.Test.Fixtures;
using Xunit;

namespace Xabe.FFmpeg.Test
{
    public class MediaInfoTests : IClassFixture<StorageFixture>
    {
        private readonly StorageFixture _storageFixture;

        public MediaInfoTests(StorageFixture storageFixture)
        {
            _storageFixture = storageFixture;
        }

        [Fact]
        public async Task AudioPopertiesTest()
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp3);

            Assert.True(File.Exists(mediaInfo.Path));
            Assert.Equal(FileExtensions.Mp3, Path.GetExtension(mediaInfo.Path));
            Assert.EndsWith("audio.mp3", mediaInfo.Path);

            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            Assert.NotNull(audioStream);
            Assert.Equal("mp3", audioStream.Codec);
            Assert.Equal(13, audioStream.Duration.Seconds);

            Assert.Empty(mediaInfo.VideoStreams);

            Assert.Equal(13, mediaInfo.Duration.Seconds);
            Assert.Equal(216916, mediaInfo.Size);
        }

        [Fact]
        public async Task GetMultipleStreamsTest()
        {
            IMediaInfo videoInfo = await FFmpeg.GetMediaInfo(Resources.MultipleStream);

            Assert.Single(videoInfo.VideoStreams);
            Assert.Equal(2, videoInfo.AudioStreams.Count());
            Assert.Equal(8, videoInfo.SubtitleStreams.Count());
        }

        [Fact]
        public async Task GetVideoBitrateTest()
        {
            IMediaInfo info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
            IVideoStream videoStream = info.VideoStreams.First();

            Assert.Equal(860233, videoStream.Bitrate);
        }

        [Fact]
        public async Task IncorrectFormatTest()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await FFmpeg.GetMediaInfo(Resources.Dll));
        }

        [Fact]
        public async Task Mp4PropertiesTest()
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(Resources.BunnyMp4);

            Assert.True(mediaInfo.Streams.Any());
        }

        [Fact]
        public async Task MkvPropertiesTest()
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

            Assert.True(File.Exists(mediaInfo.Path));
            Assert.Equal(FileExtensions.Mkv, Path.GetExtension(mediaInfo.Path));
            Assert.EndsWith("SampleVideo_360x240_1mb.mkv", mediaInfo.Path);

            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            Assert.NotNull(audioStream);
            Assert.Equal("aac", audioStream.Codec);
            Assert.Equal(1, audioStream.Index);
            Assert.Equal(9, audioStream.Duration.Seconds);

            Assert.Single(mediaInfo.VideoStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.Equal(0, videoStream.Index);
            Assert.Equal(25, videoStream.Framerate);
            Assert.Equal(240, videoStream.Height);
            Assert.Equal(320, videoStream.Width);
            Assert.Equal("4:3", videoStream.Ratio);
            Assert.Equal("h264", videoStream.Codec);
            Assert.Equal(9, videoStream.Duration.Seconds);

            Assert.Equal(9, mediaInfo.Duration.Seconds);
            Assert.Equal(1055721, mediaInfo.Size);
        }

        [Fact]
        public async Task PropertiesTest()
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio);

            Assert.True(File.Exists(mediaInfo.Path));
            Assert.Equal(FileExtensions.Mp4, Path.GetExtension(mediaInfo.Path));
            Assert.EndsWith("input.mp4", mediaInfo.Path);

            Assert.Single(mediaInfo.AudioStreams);
            IAudioStream audioStream = mediaInfo.AudioStreams.First();
            Assert.NotNull(audioStream);
            Assert.Equal("aac", audioStream.Codec);
            Assert.Equal(13, audioStream.Duration.Seconds);

            Assert.Single(mediaInfo.VideoStreams);
            IVideoStream videoStream = mediaInfo.VideoStreams.First();
            Assert.NotNull(videoStream);
            Assert.Equal(25, videoStream.Framerate);
            Assert.Equal(720, videoStream.Height);
            Assert.Equal(1280, videoStream.Width);
            Assert.Equal("16:9", videoStream.Ratio);
            Assert.Equal("h264", videoStream.Codec);
            Assert.Equal(13, videoStream.Duration.Seconds);

            Assert.Equal(13, mediaInfo.Duration.Seconds);
            Assert.Equal(2107842, mediaInfo.Size);
        }

        [Theory]
        [InlineData("檔")]
        [InlineData("אספירין")]
        [InlineData("एस्पिरि")]
        [InlineData("阿司匹林")]
        [InlineData("アセチルサリチル酸")]
        public async Task GetMediaInfo_NonUTF8CharactersInPath(string path)
        {
            string output = _storageFixture.GetTempFileName($"{path}{FileExtensions.Mp4}");
            File.Copy(Resources.Mp4WithAudio, output, true);


            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(output);

            Assert.NotNull(mediaInfo);
            Assert.Equal(FileExtensions.Mp4, Path.GetExtension(mediaInfo.Path));
        }

        [Fact]
        public async Task RTSP_NotExistingStream_CancelledAfter30Seconds()
        {
            string output = _storageFixture.GetTempFileName(FileExtensions.WebM);

            var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo(@"rtsp://192.168.1.123:554/"));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task RTSP_NotExistingStream_CancelledAfter2Seconds()
        {
            string output = _storageFixture.GetTempFileName(FileExtensions.WebM);
            var cancellationTokenSource = new CancellationTokenSource(2000);
            var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo(@"rtsp://192.168.1.123:554/", cancellationTokenSource.Token));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task CalculateFramerate_SloMoVideo_CorrectFramerateIsReturned()
        {
            IMediaInfo info = await FFmpeg.GetMediaInfo(Resources.SloMoMp4);
            IVideoStream videoStream = info.VideoStreams.First();

            // It does not has to be the same
            Assert.Equal(116, (int) videoStream.Framerate);
            Assert.Equal(3, videoStream.Duration.Seconds);
        }

        [Fact]
        public async Task MediaInfo_SpecialCharactersInName_WorksCorrectly()
        {
            string output = _storageFixture.GetTempFileName(FileExtensions.Mp4);
            var nameWithSpaces = new FileInfo(output).Name;
            output = output.Replace(nameWithSpaces, "Crime d'Amour" + ".mp4");
            IMediaInfo info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                                                                 .AddStream(info.VideoStreams.First())
                                                                 .AddParameter("-re", ParameterPosition.PreInput)
                                                                 .SetOutput(output)
                                                                 .Start();

            IMediaInfo outputMediaInfo = await FFmpeg.GetMediaInfo(output);
            Assert.NotNull(outputMediaInfo.Streams);
            Assert.Contains("Crime d'Amour", conversionResult.Arguments);
        }


        [Fact]
        public async Task MediaInfo_NameWithSpaces_WorksCorrectly()
        {
            string output = _storageFixture.GetTempFileName(FileExtensions.Mp4);
            var nameWithSpaces = new FileInfo(output).Name.Replace("-", " ");
            output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
            IMediaInfo info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                                                                 .AddStream(info.VideoStreams.First())
                                                                 .AddParameter("-re", ParameterPosition.PreInput)
                                                                 .SetOutput(output)
                                                                 .Start();

            IMediaInfo outputMediaInfo = await FFmpeg.GetMediaInfo(output);
            Assert.NotNull(outputMediaInfo.Streams);
        }

        [Fact]
        public async Task MediaInfo_EscapedString_WorksCorrectly()
        {
            string output = _storageFixture.GetTempFileName(FileExtensions.Mp4);
            var nameWithSpaces = new FileInfo(output).Name.Replace("-", " ");
            output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
            IMediaInfo info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                                                                 .AddStream(info.VideoStreams.First())
                                                                 .AddParameter("-re", ParameterPosition.PreInput)
                                                                 .SetOutput(output)
                                                                 .Start();

            IMediaInfo outputMediaInfo = await FFmpeg.GetMediaInfo($"\"{output}\"");
            Assert.NotNull(outputMediaInfo.Streams);
        }
    }
}
