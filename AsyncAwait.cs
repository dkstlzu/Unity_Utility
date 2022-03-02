using System;
using System.Threading.Tasks;

namespace Utility
{
    public class AsyncAwait
    {
        public static async void Delay(Action action, float time)
        {
            await Task.Delay((int)(time * 1000));
            action();
        }
        
        public static async void Delay<T1>(Action<T1> action, float time, T1 parm1)
        {
            await Task.Delay((int)(time * 1000));
            action(parm1);
        }
        public static async void Delay<T1, T2>(Action<T1, T2> action, float time, T1 parm1, T2 parm2)
        {
            await Task.Delay((int)(time * 1000));
            action(parm1, parm2);
        }
        public static async void Delay<T1, T2, T3>(Action<T1, T2, T3> action, float time, T1 parm1, T2 parm2, T3 parm3)
        {
            await Task.Delay((int)(time * 1000));
            action(parm1, parm2, parm3);
        }

        // public static async Task<TResult> Delay<TResult>(Func<TResult> func, float time)
        // {
        //     await Task.Delay((int)(time * 1000));
        //     await Task.Run<TResult>(func);
        // }
        
        // public static async Task Delay<TResult, T1>(Func<T1, TResult> func, float time, T1 parm1)
        // {
        //     await Task.Delay((int)(time * 1000));
        //     await Task.Run<TResult>(func(parm1));
        // }
        // public static async Task Delay<T1, T2>(Func<T1, T2> func, float time, T1 parm1, T2 parm2)
        // {
        //     await Task.Delay((int)(time * 1000));
        //     func(parm1, parm2);
        // }
        // public static async Task Delay<T1, T2, T3>(Func<T1, T2, T3> func, float time, T1 parm1, T2 parm2, T3 parm3)
        // {
        //     await Task.Delay((int)(time * 1000));
        //     func(parm1, parm2, parm3);
        // }
    }
}