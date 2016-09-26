using UnityEngine;
using System;
using UnityEngine.Serialization;
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

	public sealed class PauseManager : MonoBehaviour 
	{
		public event Action Paused;
		public event Action Unpaused;

		[SerializeField]
		[FormerlySerializedAs("m_dontDestroyOnLoad")]
		private bool _dontDestroyOnLoad;

		private PauseManagerState _state;
		private bool _hardPause;
		private static PauseManager _instance;
		
		public static PauseManager Instance
		{
			get
			{
				return _instance;
			}
		}
		
		public static PauseManagerState State
		{
			get
			{
				return _instance._state;
			}
		}
		
		public static bool IsPaused
		{
			get
			{
				return (_instance._state == PauseManagerState.Paused);
			}
		}
		
		public static void Pause()
		{
			//	The game will be paused at the start of the next update cycle.
			if(_instance._state != PauseManagerState.Paused)
			{
				_instance._state = PauseManagerState.Pausing;
			}
		}
		
		public static void UnPause()
		{
			//	The game will be unpaused at the start of the next update cycle.
			if(_instance._state == PauseManagerState.Paused)
			{
				_instance._state = PauseManagerState.UnPausing;
			}
		}
		
		private void Awake()
		{
			if(_instance != null)
			{
				Destroy(this);
			}
			else
			{
				_instance = this;
				_state = PauseManagerState.Idle;
				_hardPause = false;
				SceneManager.sceneLoaded += HandleLevelWasLoaded;

				if(_dontDestroyOnLoad)
					DontDestroyOnLoad(gameObject);
			}
		}

		private void Update()
		{
			switch(_state)
			{
			case PauseManagerState.Pausing:
				Time.timeScale = 0.0f;
				_state = PauseManagerState.Paused;
				RaisePausedEvent();
				break;
			case PauseManagerState.UnPausing:
				Time.timeScale = 1.0f;
				_state = PauseManagerState.Idle;
				RaiseUnpausedEvent();
				break;
			default:
				break;
			}
			
			if(InputManager.GetButtonDown("Pause"))
			{
				if(_state == PauseManagerState.Idle)
				{
					_state = PauseManagerState.Pausing;
					_hardPause = true;
				}
				else if(_state == PauseManagerState.Paused)
				{
					if(_hardPause)
					{
						_state = PauseManagerState.UnPausing;
						_hardPause = false;
					}
				}
			}
		}
		
		private void HandleLevelWasLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if(_state != PauseManagerState.Idle && loadSceneMode == LoadSceneMode.Single)
			{
				Time.timeScale = 1.0f;
				_state = PauseManagerState.Idle;
			}
		}
		
		private void OnDestroy()
		{
			Paused = null;
			Unpaused = null;
			SceneManager.sceneLoaded -= HandleLevelWasLoaded;
		}
		
		private void OnApplicationQuit()
		{
			Paused = null;
			Unpaused = null;
		}
		
		private void RaisePausedEvent()
		{
			if(Paused != null)
				Paused();
		}
		
		private void RaiseUnpausedEvent()
		{
			if(Unpaused != null)
				Unpaused();
		}
	}
}