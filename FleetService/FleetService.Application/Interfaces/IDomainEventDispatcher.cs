using Shared.BaseClasses;

namespace FleetService.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(Entity entity);
}