using Confluent.Kafka;

using PubSub;

using ServiceTitan.Platform.PubSub;
using ServiceTitan.Platform.PubSub.Kafka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
  .Services
    .AddSingleton<BusinessUnitChangedEvent>()
    .AddPubSub(pubSubBuilder =>
    {
        const string kafkaBrokers = "localhost:29092,localhost:39092";
        const string groupId = "my-sample-group-1";
        const string topicName = "my-sample-topic";

        pubSubBuilder
            .AddKafka(kafkaBuilder =>
            {
                kafkaBuilder
                    .SetClientConfiguration(new() { BootstrapServers = kafkaBrokers })
                    .CreateTopicIfNotExist(topicName, topicConfiguration =>
                    {
                        topicConfiguration.NumPartitions = 2;
                        topicConfiguration.ReplicationFactor = 2;
                    });
            })
            .Produce(produceBuilder =>
            {
                ProducerConfig kafkaProducerConfig = new() { BootstrapServers = kafkaBrokers };
                produceBuilder
                    .ForMessage<BusinessUnit>(topicName, pipelineBuilder =>
                        pipelineBuilder
                            .SetKey<BusinessUnit>(model => model.TenantId!)
                            .UseJsonSerializer()
                            .UseKafkaSender(new(kafkaProducerConfig)));
            })
            .Consume(consumeBuilder =>
            {
                ConsumerConfig kafkaConsumerConfig = new()
                {
                    BootstrapServers = kafkaBrokers,
                    GroupId = groupId,
                    AllowAutoCreateTopics = true,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = true
                };

                consumeBuilder
                    .AddKafkaConsumer(new(kafkaConsumerConfig, new[] { topicName }), new DefaultTypeIdResolver(), consume =>
                       consume
                           .UseJsonDeserializer()
                           .ForMessage<BusinessUnit>(consumePipelineBuilder =>
                               consumePipelineBuilder
                                    .Handle<BusinessUnit, BusinessUnitChangedEvent>()));
            });
    });

var app = builder.Build();

app.MapPost("/message", async context =>
{
    int.TryParse(context.Request.Query["count"], out int count);
    count = count < 1 ? 1 : count;

    DateTime now = DateTime.Now;
    string tenantId = $"tenant-{now.Second}";

    IBus bus = context.RequestServices.GetRequiredService<IBus>();

    await Parallel
            .ForEachAsync(Enumerable.Range(1, count), new ParallelOptions() { MaxDegreeOfParallelism = 4 }, async (_, _) =>
                await bus.PublishAsync(new BusinessUnit() { TenantId = tenantId, Id = DateTime.Now.Ticks, Name = now.ToString() }));

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();