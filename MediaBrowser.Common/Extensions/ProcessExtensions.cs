using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Process"/>.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Asynchronously wait for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="timeout">The duration to wait before cancelling waiting for the task.</param>
        /// <returns>True if the task exited normally, false if the timeout elapsed before the process exited.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="Process.EnableRaisingEvents"/> is not set to true for the process.</exception>
        public static Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
        {
            using (var cancelTokenSource = new CancellationTokenSource(timeout))
            {
                return WaitForExitAsync(process, cancelTokenSource.Token);
            }
        }

        /// <summary>
        /// Asynchronously wait for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> to observe while waiting for the process to exit.</param>
        /// <returns>True if the task exited normally, false if cancelled before the process exited.</returns>
        public static Task<bool> WaitForExitAsync(this Process process, CancellationToken cancelToken)
        {
            if (!process.EnableRaisingEvents)
            {
                throw new InvalidOperationException("EnableRisingEvents must be enabled to async wait for a task to exit.");
            }

            // Add an event handler for the process exit event
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.Exited += (_, _) => tcs.TrySetResult(true);

            // Return immediately if the process has already exited
            if (process.HasExitedSafe())
            {
                return Task.FromResult(true);
            }

            // Register with the cancellation token then await
            using (var cancelRegistration = cancelToken.Register(() => tcs.TrySetResult(process.HasExitedSafe())))
            {
                return tcs.Task;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the associated process has been terminated using
        /// <see cref="Process.HasExited"/>. This is safe to call even if there is no operating system process
        /// associated with the <see cref="Process"/>.
        /// </summary>
        /// <param name="process">The process to check the exit status for.</param>
        /// <returns>
        /// True if the operating system process referenced by the <see cref="Process"/> component has
        /// terminated, or if there is no associated operating system process; otherwise, false.
        /// </returns>
        private static bool HasExitedSafe(this Process process)
        {
            try
            {
                return process.HasExited;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }
    }
}
