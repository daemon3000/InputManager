using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace TeamUtility.IO.Examples
{
	public enum PauseManagerState
	{
		Pausing,
		UnPausing,
		Idle,
		Paused
	}

	public class PauseManager : MonoBehaviour 
	{
		[SerializeField]
		private bool m_dontDestroyOnLoad;

		private PauseManagerState m_state;
		private Action m_pausedHandler;
		private Action m_unpausedHandler;
		private bool m_hardPause;
		private static PauseManager m_instance;

		public static bool Exists
		{
			get { return m_instance != null; }
		}

		public static event Action Paused
		{
			add { m_instance.m_pausedHandler += value; }
			remove { m_instance.m_pausedHandler -= value; }
		}

		public static event Action Unpaused
		{
			add { m_instance.m_unpausedHandler += value; }
			remove { m_instance.m_unpausedHandler -= value; }
		}

		public static PauseManagerState State
		{
			get
			{
				return m_instance.m_state;
			}
		}
		
		public static bool IsPaused
		{
			get
			{
				return (m_instance.m_state == PauseManagerState.Paused);
			}
		}
		
		public static void Pause()
		{
			//	The game will be paused at the start of the next update cycle.
			if(m_instance.m_state != PauseManagerState.Paused)
			{
				m_instance.m_state = PauseManagerState.Pausing;
			}
		}
		
		public static void UnPause()
		{
			//	The game will be unpaused at the start of the next update cycle.
			if(m_instance.m_state == PauseManagerState.Paused)
			{
				m_instance.m_state = PauseManagerState.UnPausing;
			}
		}
		
		private void Awake()
		{
			if(m_instance != null)
			{
				Destroy(this);
			}
			else
			{
				m_instance = this;
				m_state = PauseManagerState.Idle;
				m_hardPause = false;
				SceneManager.sceneLoaded += HandleLevelWasLoaded;

				if(m_dontDestroyOnLoad)
					DontDestroyOnLoad(gameObject);
			}
		}

		private void Update()
		{
			switch(m_state)
			{
			case PauseManagerState.Pausing:
				Time.timeScale = 0.0f;
				m_state = PauseManagerState.Paused;
				RaisePausedEvent();
				break;
			case PauseManagerState.UnPausing:
				Time.timeScale = 1.0f;
				m_state = PauseManagerState.Idle;
				RaiseUnpausedEvent();
				break;
			default:
				break;
			}
			
			if(InputManager.GetButtonDown("Pause"))
			{
				if(m_state == PauseManagerState.Idle)
				{
					m_state = PauseManagerState.Pausing;
					m_hardPause = true;
				}
				else if(m_state == PauseManagerState.Paused)
				{
					if(m_hardPause)
					{
						m_state = PauseManagerState.UnPausing;
						m_hardPause = false;
					}
				}
			}
		}
		
		private void HandleLevelWasLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if(m_state != PauseManagerState.Idle && loadSceneMode == LoadSceneMode.Single)
			{
				Time.timeScale = 1.0f;
				m_state = PauseManagerState.Idle;
			}
		}
		
		private void OnDestroy()
		{
			m_pausedHandler = null;
			m_unpausedHandler = null;
			SceneManager.sceneLoaded -= HandleLevelWasLoaded;
		}
		
		private void OnApplicationQuit()
		{
			m_pausedHandler = null;
			m_unpausedHandler = null;
		}
		
		private void RaisePausedEvent()
		{
			if(m_pausedHandler != null)
				m_pausedHandler();
		}
		
		private void RaiseUnpausedEvent()
		{
			if(m_unpausedHandler != null)
				m_unpausedHandler();
		}
	}
}