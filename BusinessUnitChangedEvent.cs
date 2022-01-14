namespace PubSub;

using System.Threading.Tasks;

using ServiceTitan.Platform.PubSub;

public class BusinessUnitChangedEvent : IMessageHandler<BusinessUnit>
{
    public ValueTask HandleAsync(BusinessUnit message, IConsumeContext context)
    {
        Console.WriteLine($"Handled: TenantId: {message.TenantId}, Id: {message.Id}, Name: {message.Name}");
        return ValueTask.CompletedTask;
    }
}
