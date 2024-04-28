namespace IngBackendApi.Interfaces.Service;

public interface IBackgroundTaskService
{
    Task ScheduleTaskAsync(
        string assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        TimeSpan delay,
        bool removePreviousTask = true
    );
    Task EnqueueTaskAsync(
        string assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        bool removePreviousTask = true
    );
}
