namespace IngBackendApi.Services;

using Hangfire;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;

public class BackgroundTaskService(IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient)
    : IBackgroundTaskService
{
    private readonly IRepository<BackgroundTask, string> _backgroundTaskRepository =
        unitOfWork.Repository<BackgroundTask, string>();

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public async Task ScheduleTaskAsync(
        string assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        TimeSpan delay,
        bool removePreviousTask = true
    )
    {
        if (removePreviousTask)
        {
            var task = await _backgroundTaskRepository.GetByIdAsync(assignedId);
            if (task != null)
            {
                _backgroundJobClient.Delete(task.TaskId);
                await _backgroundTaskRepository.DeleteByIdAsync(task.Id);
            }
        }
        var newTaskId = _backgroundJobClient.Schedule(methodCall, delay);

        await _backgroundTaskRepository.AddAsync(
            new BackgroundTask { Id = assignedId, TaskId = newTaskId }
        );
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task EnqueueTaskAsync(
        string assignedId,
        System.Linq.Expressions.Expression<Action> methodCall,
        bool removePreviousTask = true
    )
    {
        if (removePreviousTask)
        {
            var task = await _backgroundTaskRepository.GetByIdAsync(assignedId);
            if (task != null)
            {
                _backgroundJobClient.Delete(task.TaskId);
                await _backgroundTaskRepository.DeleteByIdAsync(task.Id);
            }
        }
        var newTaskId = _backgroundJobClient.Enqueue(methodCall);

        await _backgroundTaskRepository.AddAsync(
            new BackgroundTask { Id = assignedId, TaskId = newTaskId }
        );
        await _unitOfWork.SaveChangesAsync();
    }
}
