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
        const string topic = "routing-api";
        const string groupId = "my-group-1";

        pubSubBuilder
            .AddKafka()
            .Produce(produceBuilder =>
            {
                ProducerConfig kafkaProducerConfig = new()
                {
                    BootstrapServers = kafkaBrokers
                };

                produceBuilder
                    .ForMessage<BusinessUnit>(topic, pipelineBuilder =>
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
                    .AddKafkaConsumer(new(kafkaConsumerConfig, new[] { topic }), new DefaultTypeIdResolver(), consume =>
                       consume
                           .UseJsonDeserializer()
                           .ForMessage<BusinessUnit>(consumePipelineBuilder =>
                               consumePipelineBuilder
                                    .Handle<BusinessUnit, BusinessUnitChangedEvent>()));
            })
            ;
    });

var app = builder.Build();

app.MapPost("/message", async context =>
{
    int.TryParse(context.Request.Query["count"], out int count);

    count = count < 1 ? 1 : count;

    DateTime now = DateTime.Now;
    string tenantId = $"tenant-{now.Second}";

    IBus bus = context.RequestServices.GetRequiredService<IBus>();
    for (int index = 0; index < count; index++)
    {
        BusinessUnit businessUnit = new() { TenantId = tenantId, Id = DateTime.Now.Ticks, Name = now.ToString() };

        await bus.PublishAsync(businessUnit);
    }

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("OK");
});

app.Run();