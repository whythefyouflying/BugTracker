using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BugTrackerApp
{
    public static class TaskExtensions
    {
        public static Task<T> ContinueWithSuccess<T>(this Task<T> task, Action<T> action)
        {
            task.ContinueWith(t => action(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            return task;
        }

        public static Task ContinueWithSuccess(this Task task, Action action)
        {
            task.ContinueWith(t => action(), TaskContinuationOptions.OnlyOnRanToCompletion);
            return task;
        }

        public static Task<T> ContinueWithFailure<T>(this Task<T> task, Action<Exception> action)
        {
            task.ContinueWith(t => action(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public static Task ContinueWithFailure(this Task task, Action<Exception> action)
        {
            task.ContinueWith(t => action(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
    }
}