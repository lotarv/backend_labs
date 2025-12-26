using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniverseLabs.Common;
using UniverseLabs.Oms.Consumer.Config;

namespace UniverseLabs.Oms.Consumer.Base;

public abstract class BaseBatchKafkaConsumer<T> : IHostedService
    where T : class
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<BaseBatchKafkaConsumer<T>> _logger;
    private readonly string _topic;
    private readonly int _collectBatchSize;
    private readonly TimeSpan _collectTimeout;

    protected BaseBatchKafkaConsumer(
        IOptions<KafkaSettings> kafkaSettings,
        string topic,
        ILogger<BaseBatchKafkaConsumer<T>> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
            GroupId = kafkaSettings.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 5_000,
            SessionTimeoutMs = 60_000,
            HeartbeatIntervalMs = 3_000,
            MaxPollIntervalMs = 300_000
        };

        _logger = logger;
        _topic = topic;
        _collectBatchSize = kafkaSettings.Value.CollectBatchSize;
        _collectTimeout = TimeSpan.FromMilliseconds(kafkaSettings.Value.CollectTimeoutMs);
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartConsuming(_topic, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopConsuming();
        return Task.CompletedTask;
    }

    private async Task StartConsuming(string topic, CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);
        _logger.LogInformation("Started consuming from topic: {Topic}", topic);

        try
        {
            var batch = new List<(Message<T> message, ConsumeResult<string, string> result)>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(_collectTimeout);

                if (consumeResult?.Message != null)
                {
                    batch.Add((
                        new Message<T>
                        {
                            Key = consumeResult.Message.Key,
                            Body = consumeResult.Message.Value.FromJson<T>()
                        },
                        consumeResult));
                }

                var timeoutReached = consumeResult == null;
                if (batch.Count >= _collectBatchSize || (timeoutReached && batch.Count > 0))
                {
                    await ProcessBatch(batch, cancellationToken);
                    batch.Clear();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer cancelled");
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Consume error occurred");
        }
        finally
        {
            StopConsuming();
        }
    }

    private async Task ProcessBatch(
        List<(Message<T> message, ConsumeResult<string, string> result)> batch,
        CancellationToken token)
    {
        try
        {
            await ProcessMessages(batch.Select(x => x.message).ToArray(), token);
            _consumer.Commit(batch.Last().result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing batch");
        }
    }

    private void StopConsuming()
    {
        _logger.LogInformation("Stopping consuming from topic: {Topic}", _topic);
        _consumer.Close();
        _consumer.Dispose();
    }

    protected abstract Task ProcessMessages(Message<T>[] messages, CancellationToken token);
}
