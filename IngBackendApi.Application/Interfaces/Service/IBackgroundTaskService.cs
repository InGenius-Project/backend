namespace IngBackendApi.Interfaces.Service;

public interface IBackgroundTaskService
{
    Task ScheduleTaskAsync(
        Guid assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        TimeSpan delay,
        bool removePreviousTask = true
    );
    Task EnqueueTaskAsync(
        Guid assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        bool removePreviousTask = true
    );
}
