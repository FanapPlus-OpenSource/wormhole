﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Newtonsoft.Json;
using Wormhole.Api.Model;
using Wormhole.Job;
using Wormhole.Kafka;

namespace Wormhole.Worker
{
    public class MessageConsumer : ConsumerBase
    {
        private readonly NebulaContext _nebulaContext;
        private readonly string _topicName;
        

        public MessageConsumer(IKafkaConsumer<Null, string> consumer,  NebulaContext nebulaContext, ILoggerFactory logger, string topicName) : base(consumer, logger, ConsumerDiagnosticProvider.GetStat(typeof(MessageConsumer).FullName, topicName))
        {
            _nebulaContext = nebulaContext;
            _topicName = topicName;
            ICollection<KeyValuePair<string, object>> config = new Collection<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("group.id","GroupId")
            };

            consumer.Initialize(config, OnMessageRecived);
        }
        

        public override string Topic => _topicName;

        private void OnMessageRecived(object sender, Message<Null, string> message)
        {
            Logger.LogDebug(message.Value);
            var publishInput = JsonConvert.DeserializeObject<PublishInput>(message.Value);

            var jobIds = NebulaWorker.GetJobIds(publishInput.Tenant, publishInput.Category, publishInput.Tags);
            if (jobIds == null || jobIds.Count < 1)
                return;

            var queue = _nebulaContext.GetDelayedJobQueue<HttpPushOutgoingQueueStep>(QueueType.Delayed);
            var step = new HttpPushOutgoingQueueStep
            {
                Payload = publishInput.Payload.ToString(),
                Category = publishInput.Category
            };

            foreach (var jobId in jobIds)
                queue.Enqueue(step, DateTime.UtcNow, jobId).GetAwaiter().GetResult();
        }
    }
}