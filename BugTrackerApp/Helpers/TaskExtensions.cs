////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\Helpers\TaskExtensions.cs </file>
///
/// <copyright file="TaskExtensions.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the task extensions class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

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


////////////////////////////////////////////////////////////////////////////////////////////////////
// namespace: BugTrackerApp.Helpers
//
// summary:	Helper classes.
////////////////////////////////////////////////////////////////////////////////////////////////////

namespace BugTrackerApp.Helpers
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A task extensions. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class TaskExtensions
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   A Task&lt;T&gt; extension method that continue with success. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="task">     The task to act on. </param>
        /// <param name="action">   The action. </param>
        ///
        /// <returns>   An asynchronous result that yields a T. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Task<T> ContinueWithSuccess<T>(this Task<T> task, Action<T> action)
        {
            task.ContinueWith(t => action(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            return task;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   A Task&lt;T&gt; extension method that continue with success. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="task">     The task to act on. </param>
        /// <param name="action">   The action. </param>
        ///
        /// <returns>   An asynchronous result that yields a T. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Task ContinueWithSuccess(this Task task, Action action)
        {
            task.ContinueWith(t => action(), TaskContinuationOptions.OnlyOnRanToCompletion);
            return task;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   A Task&lt;T&gt; extension method that continue with failure. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="task">     The task to act on. </param>
        /// <param name="action">   The action. </param>
        ///
        /// <returns>   An asynchronous result that yields a T. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Task<T> ContinueWithFailure<T>(this Task<T> task, Action<Exception> action)
        {
            task.ContinueWith(t => action(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   A Task&lt;T&gt; extension method that continue with failure. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="task">     The task to act on. </param>
        /// <param name="action">   The action. </param>
        ///
        /// <returns>   An asynchronous result that yields a T. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Task ContinueWithFailure(this Task task, Action<Exception> action)
        {
            task.ContinueWith(t => action(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
    }
}