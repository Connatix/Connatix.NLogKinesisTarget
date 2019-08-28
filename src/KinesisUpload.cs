using Amazon;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Connatix.NLogKinesisTarget
{
    public class ConnatixKinesisUpload
    {
        private readonly AmazonKinesisClient m_client;

        public ConnatixKinesisUpload(string awsKey, string awsSecret, string region)
        {
            if (string.IsNullOrEmpty(awsKey) || string.IsNullOrEmpty(awsSecret)){
                m_client = new AmazonKinesisClient(RegionEndpoint.GetBySystemName(region));
            }else{
                m_client = new AmazonKinesisClient(awsKey, awsSecret, RegionEndpoint.GetBySystemName(region));
            }
        }

        public async Task<PutRecordsResponse> WriteAsync(List<string> logMessages, string streamName)
        {
            var request = new PutRecordsRequest
            {
                StreamName = streamName
            };

            foreach (string message in logMessages)
            {
                using (var ms = GenerateStreamFromString(message))
                {
                    request.Records.Add(new PutRecordsRequestEntry
                    {
                        Data = ms,
                        PartitionKey = Guid.NewGuid().ToString()
                    });
                }
            }

            return await m_client.PutRecordsAsync(request);
        }

        private static MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}

