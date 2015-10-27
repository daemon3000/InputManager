using UnityEngine;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	[ExecuteInEditMode]
	public class CrossPlatformInput : MonoBehaviour 
	{
		public GameObject desktopUI;
		public GameObject mobileUI;
		public string mobileInputConfig;

		private void Start()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
#if UNITY_ANDROID || UNITY_IPHONE
			InputManager.SetInputConfiguration(mobileInputConfig, PlayerID.One);
#endif
		}

#if UNITY_EDITOR
		private void Update()
		{
			if(!UnityEditor.EditorApplication.isPlaying && desktopUI != null && mobileUI != null)
			{
#if UNITY_STANDALONE || UNITY_WEBPLAYER
				desktopUI.SetActive(true);
				mobileUI.SetActive(false);
#elif UNITY_ANDROID || UNITY_IPHONE
				desktopUI.SetActive(false);
				mobileUI.SetActive(true);
#endif
			}
		}
#endif

#if UNITY_ANDROID || UNITY_IPHONE
        private void LateUpdate()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
            InputManager.SetRemoteButtonValue(InputManager.PlayerOneConfiguration.name, "Jump", false, false);
        }
#endif

        public void AddVertical(float value)
		{
			InputManager.SetRemoteAxisValue(InputManager.PlayerOneConfiguration.name, "Vertical", InputManager.GetAxis("Vertical") + value);
		}

		public void AddHorizontal(float value)
		{
			InputManager.SetRemoteAxisValue(InputManager.PlayerOneConfiguration.name, "Horizontal", InputManager.GetAxis("Horizontal") + value);
		}

		public void SetMouseX(float value)
		{
			InputManager.SetRemoteAxisValue(InputManager.PlayerOneConfiguration.name, "LookHorizontal", value);
		}

		public void SetMouseY(float value)
		{
			InputManager.SetRemoteAxisValue(InputManager.PlayerOneConfiguration.name, "LookVertical", value);
		}

		public void Jump()
		{
			InputManager.SetRemoteButtonValue(InputManager.PlayerOneConfiguration.name, "Jump", true, true);
		}
	}
}
