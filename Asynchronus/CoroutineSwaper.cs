using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class CoroutineSwaper
    {
        public List<Func<IEnumerator>> CoroutineList = new List<Func<IEnumerator>>();

        public bool hasExitTime = true;
        private int currentIndex;
        private TaskManagerTask currentTask;
        private TaskManagerTask queuedTask;
        
        public CoroutineSwaper(IEnumerable<Func<IEnumerator>> coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                CoroutineList.Add(coroutine);
            }
        }

        public async void Play(int index)
        {
            if (currentIndex == index) return;


            if (hasExitTime && currentTask != null)
            {
                while (currentTask.Running)
                {
                    await Task.Delay((int)(Time.deltaTime * 1000));
                }
            }
            
            if (currentTask != null)
            {
                currentTask.Stop();
            }

            queuedTask = new TaskManagerTask(CoroutineList[index]());
            currentTask = queuedTask;
            currentIndex = index;
        }
    }
}