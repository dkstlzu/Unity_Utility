using UnityEngine;
using System;
using System.Collections;

namespace dkstlzu.Utility
{
	public class TaskManagerTask
	{
		TaskManager.TaskState task;
		public bool Running => task.Running;
		public bool Paused => task.Paused;
		public bool Stopped => task.Stopped;

		public event Action<bool> Finished
		{
			add => task.Finished += value;
			remove => task.Finished -= value;
		}

		public TaskManagerTask(IEnumerator c, bool autoStart = true)
		{
			task = TaskManager.CreateTask(c);
			if (autoStart)
				Start();
		}

		public void Start()
		{
			task.Start();
		}

		public void Stop()
		{
			task.Stop();
		}

		public void Pause()
		{
			task.Pause();
		}

		public void Unpause()
		{
			task.Unpause();
		}

		public void Interrupt()
		{
			task.Interrupt();
		}
	}

	class TaskManager : MonoBehaviour
	{
		static TaskManager singleton;

		public class TaskState
		{
			public bool Running { get; private set; }
			public bool Paused { get; private set; }
			public bool Stopped { get; private set; }

			public event Action<bool> Finished;

			IEnumerator coroutine;

			public TaskState(IEnumerator c)
			{
				coroutine = c;
			}

			public void Pause()
			{
				Paused = true;
			}

			public void Unpause()
			{
				Paused = false;
			}

			public void Start()
			{
				Running = true;
				singleton.StartCoroutine(CallWrapper());
			}

			public void Stop()
			{
				Stopped = true;
				Running = false;
			}

			public void Interrupt()
			{
				Running = false;
			}

			IEnumerator CallWrapper()
			{
				yield return null;
				IEnumerator e = coroutine;
				while (Running)
				{
					if (Paused)
						yield return null;
					else
					{
						if (e != null && e.MoveNext())
						{
							yield return e.Current;
						}
						else
						{
							Running = false;
						}
					}
				}

				Finished?.Invoke(Stopped);
			}
		}

		public static TaskState CreateTask(IEnumerator coroutine)
		{
			if (singleton == null)
			{
				GameObject go = new GameObject("TaskManager");
				singleton = go.AddComponent<TaskManager>();
			}

			return new TaskState(coroutine);
		}
	}
}