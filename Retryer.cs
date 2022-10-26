using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace botSolution
{
    public static class Retry
    {

        

        public static T RetryMethod<T>(Func<T> method, int numRetries, int retryTimeout, Action onFailureAction)
        {
            //Guard.IsNotNull(method, "method");
            T retval = default(T);
            do
            {
                try
                {
                    retval = method();
                    return retval;
                }
                catch
                {
                    onFailureAction();
                    if (numRetries <= 0) throw; // improved to avoid silent failure
                    Thread.Sleep(retryTimeout);
                }
            } while (numRetries-- > 0);
            return retval;
        }

        
        
        
        public static  void Do(Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
         
    }
}
