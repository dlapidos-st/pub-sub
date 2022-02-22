using System.Text.Json;

using ServiceTitan.Messaging.CapacityPlanning.Protos;
using ServiceTitan.Platform.PubSub;

namespace PubSub
{
    public class TenantConfigurationMessageHandler : IMessageHandler<TenantConfiguration>
    {
        private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        public ValueTask HandleAsync(TenantConfiguration message, IConsumeContext context)
        {
            Console.WriteLine(JsonSerializer.Serialize(message, this.jsonOptions));
            return ValueTask.CompletedTask;
        }
    }
}
