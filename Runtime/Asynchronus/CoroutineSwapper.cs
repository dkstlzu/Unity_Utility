using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class CoroutineSwapper
    {
        public List<Func<IEnumerator>> CoroutineList = new List<Func<IEnumerator>>();

        public bool HasExitTime = true;
        private int _currentIndex;
        private TaskManagerTask _currentTask;
        private TaskManagerTask _queuedTask;
        
        public CoroutineSwapper(IEnumerable<Func<IEnumerator>> coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                CoroutineList.Add(coroutine);
            }
        }

        public async void Play(int index)
        {
            if (_currentIndex == index) return;


            if (HasExitTime && _currentTask != null)
            {
                while (_currentTask.Running)
                {
                    await Task.Delay((int)(Time.deltaTime * 1000));
                }
            }
            
            if (_currentTask != null)
            {
                _currentTask.Stop();
            }

            _queuedTask = new TaskManagerTask(CoroutineList[index]());
            _currentTask = _queuedTask;
            _currentIndex = index;
        }
    }
}