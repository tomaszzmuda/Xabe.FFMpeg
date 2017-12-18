﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace Xabe.FFmpeg
{
    internal static class EnumExtensions
    {
        internal static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType()
                                .GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;

            return value.ToString();
        }
    }
}
