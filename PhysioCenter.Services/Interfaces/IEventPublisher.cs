using PhysioCenter.Services.Events;

namespace PhysioCenter.Services.Interfaces;

public interface IEventPublisher
{
    void Publish(SessionEvent sessionEvent);
}
