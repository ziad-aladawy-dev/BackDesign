namespace HUP.Core.Interfaces
{
    public interface ITransactionService
    {
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}