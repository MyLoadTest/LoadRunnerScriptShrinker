using System;
using System.Linq;
using Omnifactotum.Annotations;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    internal sealed class LogRecord
    {
        #region Constructors

        public LogRecord(LogRecordType type, [NotNull] string text)
        {
            #region Argument Check

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            #endregion

            Type = type;
            Text = text;
        }

        #endregion

        #region Public Properties

        public LogRecordType Type
        {
            get;
        }

        [NotNull]
        public string Text
        {
            get;
        }

        #endregion
    }
}