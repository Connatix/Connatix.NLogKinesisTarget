using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using NLog;
using Amazon.Kinesis.Model;
using System.Timers;
using System.Collections.Concurrent;
using NLog.Common;

namespace Connatix.NLogKinesisTarget
{
    [Target("ConnatixKinesisTarget")]
    public sealed class ConnatixKinesisTarget : TargetWithLayout
    {
        private ConnatixKinesisUpload m_upload;

        public string AwsKey { get; set; }

        public string AwsSecret { get; set; }

        [RequiredParameter]
        public string AwsRegion { get; set; }

        [RequiredParameter]
        public string Stream { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            base.Write(logEvent);
        }

        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            try
            {
                if (m_upload == null)
                {
                    m_upload = new ConnatixKinesisUpload(AwsKey, AwsSecret, AwsRegion);
                }

                List<string> messages = new List<string>();
                foreach (var log in logEvents)
                {
                    messages.Add(Layout.Render(log.LogEvent));
                }

                m_upload.Write(messages, Stream);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
