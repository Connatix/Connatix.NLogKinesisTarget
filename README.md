# Connatix.NLogKinesisTarget

Custom NLog target for streaming events to AWS Kinesis Stream in batches

# Usage

1. Add the library as a package reference to any .NET Core 2.0 App
2. Add an NLog.config to your project
3. Configure copy to output for the NLog.config

```
<ItemGroup>
    <None Include="NLog.config" CopyToOutputDirectory="Always">
    </None>
</ItemGroup>
```

# Sample NLog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog 
  xmlns="http://www.nlog-project.org/schemas/NLog.xsd" 
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" autoReload="true" throwExceptions="true" internalLogLevel="Off" internalLogFile="nlog-internal.log">
  <!-- Load the ASP.NET Core plugin -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
    <add assembly="Connatix.NLogKinesisTarget"/>
  </extensions>
  <targets>
    <target xsi:type="ConnatixKinesisTarget" name ="kinesis" BatchSize="<BATCH_SIZE>" MaxSize="<MAX_SIZE>" BatchInterval="<BATCH_INTERVAL>" AwsKey="<AWS_KEY>" AwsSecret="<AWS_SECRET>" AwsRegion="<AWS_REGION>" Stream="<STREAM_NAME>">
      <layout type='JsonLayout' includeAllProperties="true">
        <attribute name="machineName" layout="${machinename}" />
        <attribute name="siteName" layout="${iis-site-name}" />
        <attribute name="processname" layout="${processname}" />
        <attribute name="@timestamp" layout="${date:universalTime=True:format=yyyy-MM-ddTHH\:mm\:ssZ}" />
        <attribute name="level" layout="${level}" />
        <attribute name="username" layout="${aspnet-user-identity}" />
        <attribute name="message" layout="${message}" />
        <attribute name="logger" layout="${logger}" />
        <attribute name="url" layout="${aspnet-request-url:IncludeHost=True:IncludePort=True:IncludeQueryString=True}" />
        <attribute name="referrer" layout="${aspnet-request-referrer}" />
        <attribute name="ip" layout="${aspnet-request:header=X-Forwarded-For}" />
        <attribute name="ua" layout="${aspnet-request:header=User-Agent}" />
        <attribute name="exception" layout="${exception:format=ToString:maxInnerExceptionLevel=2}" />
      </layout>
    </target>
  </targets>
  <rules>
    <logger name="*" minlevel="Warn" writeTo="kinesis" />
  </rules>
</nlog>
```

# Configurable Fields

**AWS_KEY:** AWS key with write permissions on the Kinesis Stream. 

**AWS_SECRET:** Secret for the provided AWS_KEY. 

**AWS_REGION:** AWS Region (https://docs.aws.amazon.com/general/latest/gr/rande.html) *e.g. us-east-1*. 

**STREAM_NAME:** AWS Kinesis stream name to write events to. 

**BATCH_SIZE:** Maximum number of events to be sent in 1 batch to AWS Kinesis Stream. 

**MAX_SIZE:** The maximum queue size of events. In case the limit is reached, new events are not being queued for sending to AWS Kinesis Stream. 

**BATCH_INTERVAL:** Interval in milliseconds between sending events to AWS Kinesis. 
