using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using NLog;
using Amazon.Kinesis.Model;
using System.Timers;
using System.Collections.Concurrent;

namespace Connatix.NLogKinesisTarget
{
    [Target("ConnatixKinesisTarget")]
    public sealed class ConnatixKinesisTarget : TargetWithLayout
    {
        private ConnatixKinesisUpload m_upload;
        private readonly ConcurrentQueue<string> m_logMessages;
        private readonly Timer m_timer;
        private DateTime m_lastProcessed = DateTime.MinValue;

        public ConnatixKinesisTarget()
        {
            m_logMessages = new ConcurrentQueue<string>();

            m_timer = new Timer(300);
            m_timer.Elapsed += WriteToAmazon;
            m_timer.Start();
        }

        public string AwsKey { get; set; }

        public string AwsSecret { get; set; }

        [RequiredParameter]
        public string AwsRegion { get; set; }

        [RequiredParameter]
        public string Stream { get; set; }

        [RequiredParameter]
        public int BatchSize { get; set; }

        [RequiredParameter]
        public int BatchInterval { get; set; }

        [RequiredParameter]
        public int MaxSize { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (m_logMessages.Count < MaxSize)
            {
                m_logMessages.Enqueue(Layout.Render(logEvent));
            }
        }

        private void WriteToAmazon(object sender, ElapsedEventArgs args)
        {
            if (m_logMessages.Count == 0 || m_logMessages.Count < BatchSize && m_lastProcessed.AddSeconds(BatchInterval) > DateTime.Now)
            {
                return;
            }

            m_timer.Stop();

            try
            {
                if (m_upload == null){
                    m_upload = new ConnatixKinesisUpload(AwsKey, AwsSecret, AwsRegion);
                }

                if (m_logMessages.Count > 0)
                {
                    List<string> messagesToWrite = new List<string>();
                    for (int i = 0; i < BatchSize; i++)
                    {
                        if (m_logMessages.TryDequeue(out var message))
                        {
                            messagesToWrite.Add(message);
                        }else{
                            break;
                        }
                    }

                    if (messagesToWrite.Count > 0)
                    {
                        PutRecordsResponse response = m_upload.Write(messagesToWrite, Stream);
                        if (response.FailedRecordCount > 0)
                        {
                            for (int i = 0; i < response.Records.Count; i++)
                            {
                                var record = response.Records[i];
                                if (!string.IsNullOrEmpty(record.ErrorCode))
                                {
                                    m_logMessages.Enqueue(messagesToWrite[i]);
                                }
                            }
                        }

                    }
                }
            }
            catch{

            }
            finally
            {
                m_lastProcessed = DateTime.Now;
                m_timer.Start();
            }
        }
    }
}
