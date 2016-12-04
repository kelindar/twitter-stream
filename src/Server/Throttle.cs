using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Throttle
    {
        private readonly TimeSpan waitTime;
        private readonly SemaphoreSlim throttleActions;
        private readonly SemaphoreSlim throttlePeriods;
        private volatile int pending;

        public Throttle(int concurrentAction, TimeSpan waitTime)
        {
            this.throttleActions = new SemaphoreSlim(concurrentAction, concurrentAction);
            this.throttlePeriods = new SemaphoreSlim(concurrentAction, concurrentAction);
            this.waitTime = waitTime;
        }

        public void Enqueue(Action action, CancellationToken cancel)
        {
            if (pending > 50)
                return;

            // If we've received a new action, append it to the current task queue
            // using ContinueWith().
            pending++;
            throttleActions.WaitAsync(cancel).ContinueWith(t =>
            {
                try
                {
                    // Enter the throttle period semaphore (or wait)
                    throttlePeriods.Wait(cancel);

                    // Delay the task by the specified wait period and then release the throttle
                    Task.Delay(waitTime).ContinueWith((tt) => throttlePeriods.Release(1));

                    // Execute the action
                    action();
                }
                finally
                {
                    // Once we're done executing the action, relese the semaphore and decrement the pending actions
                    throttleActions.Release(1);
                    pending--;
                }
            });
        }
    }
}