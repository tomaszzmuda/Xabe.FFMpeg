﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xabe.FFmpeg.Model;

namespace Xabe.FFmpeg
{
    internal static class FFmpegExecutablesHelper
    {
        internal const string FFmpegExecutableName = "ffmpeg";
        internal const string FFprobeExecutableName = "ffprobe";

        internal static string SelectFFmpegPath(IEnumerable<IFileInfo> files)
        {
            return files.FirstOrDefault(x => CompareFileNames(x.Name, FFmpegExecutableName))
                        ?.FullName;
        }

        internal static string SelectFFprobePath(IEnumerable<IFileInfo> files)
        {
            return files.FirstOrDefault(x => CompareFileNames(x.Name, FFprobeExecutableName))
                        ?.FullName;
        }

        private static bool CompareFileNames(string fileName, string expectedName)
        {
            return fileName.Equals(expectedName, StringComparison.InvariantCultureIgnoreCase)
                   || fileName.Equals($"{expectedName}.exe", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
