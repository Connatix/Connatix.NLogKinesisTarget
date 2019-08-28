using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using NLog;
using Amazon.Kinesis.Model;
using System.Timers;
using System.Collections.Concurrent;
using NLog.Common;
using System.Threading.Tasks;
using System.Threading;

namespace Connatix.NLogKinesisTarget
{
    [Target("ConnatixKinesisTarget")]
    public sealed class ConnatixKinesisTarget : AsyncTaskTarget
    {
        private object m_lock = new object();

        private ConnatixKinesisUpload m_upload;

        public string AwsKey { get; set; }

        public string AwsSecret { get; set; }

        [RequiredParameter]
        public string AwsRegion { get; set; }

        [RequiredParameter]
        public string Stream { get; set; }

        protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken token){
            try
            {
                if (m_upload == null)
                {
                    lock (m_lock){
                        if (m_upload == null){
                            m_upload = new ConnatixKinesisUpload(AwsKey, AwsSecret, AwsRegion);
                        }
                    }
                }

                List<string> messages = new List<string>();
                foreach (var logEvent in logEvents){
                    messages.Add(Layout.Render(logEvent));
                }

                await m_upload.WriteAsync(messages, Stream);
            }
            catch (Exception)
            {
            }
        }

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
        { 
            await WriteAsyncTask(new List<LogEventInfo>(){logEvent}, token);
        }
    }
}
