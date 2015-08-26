using System;
using System.Globalization;
using System.Linq;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    internal static class LocalHelper
    {
        #region Constants and Fields

        public static readonly StringComparer FileSystemNameComparer = StringComparer.OrdinalIgnoreCase;

        private const long KiloByte = 1L << 10;
        private const long MegaByte = 1L << 20;
        private const long GigaByte = 1L << 30;

        #endregion

        #region Public Methods

        public static string FormatFileSize(this long size)
        {
            #region Argument Check

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    @"The value cannot be negative.");
            }

            #endregion

            if (size < KiloByte)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} B", size);
            }

            if (size < MegaByte)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:0.#} KB", ((decimal)size) / KiloByte);
            }

            if (size < GigaByte)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:0.#} MB", ((decimal)size) / MegaByte);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:0.#} GB", ((decimal)size) / GigaByte);
        }

        #endregion
    }
}