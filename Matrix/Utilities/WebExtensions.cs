namespace Matrix.Utilities;

public static class TaskExtensions
{
    private static void CheckTaskSuccess(
        Task task,
        Action<Exception>? errorFunction,
        Action? cancelFunction)
    {
        if (task.IsFaulted)
        {
            if (errorFunction != null)
            {
                errorFunction.Invoke(task.Exception);
            }
        }

        if (task.IsCanceled)
        {
            if (cancelFunction != null)
            {
                cancelFunction.Invoke();
            }
        }
    }

    private static bool CheckTaskFailure(Task t)
    {
        return t.IsFaulted && t.Exception != null;
    }
    
    public static Task<TNewResult> OnSuccess<TResult, TNewResult>(
        this Task<TResult> task,
        Func<TResult, TNewResult> onSuccessFunction,
        Action<Exception>? onErrorFunction = null,
        Action? cancelFunction = null)
    {
        return task.ContinueWith(t =>
        {
            CheckTaskSuccess(t, onErrorFunction, cancelFunction);
            return onSuccessFunction(t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    public static Task<TResult> OnFailure<TResult>(
        this Task<TResult> task,
        Func<Exception, TResult> onFailureFunction)
    {
        return task.ContinueWith(t =>
        {
            if (CheckTaskFailure(task))
            {
                return onFailureFunction(t.Exception);
            }

            return task.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    public static Task<TResult> FinishWith<TResult>(
        this Task<TResult> task,
        Action finishFunction,
        Action<Exception>? onErrorFunction = null,
        Action? cancelFunction = null)
    {
        return task.ContinueWith(t =>
        {
            CheckTaskSuccess(t, onErrorFunction, cancelFunction);
            finishFunction.Invoke();
            return t.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }
}