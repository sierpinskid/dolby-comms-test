using System;
using System.Threading.Tasks;
using UnityEngine;

internal static class TaskUtils
{
    public static void LogIfFailed(this Task task)
        => task.ContinueWith(res => Debug.LogException(res.Exception), TaskContinuationOptions.OnlyOnFaulted);

    public static void ContinueOrLogError(this Task task, Action continueWith)
        => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
                return;
            }

            continueWith?.Invoke();
        }, TaskScheduler.FromCurrentSynchronizationContext());

    public static void ContinueOrLogError<T>(this Task<T> task, Action<T> continueWith)
        => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
                return;
            }

            continueWith?.Invoke(t.Result);
        }, TaskScheduler.FromCurrentSynchronizationContext());
}