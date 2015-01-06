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
			InputManager.SetConfiguration(mobileInputConfig);
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

		private void LateUpdate()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			InputManager.SetRemoteButtonValue("Jump", false, false);
		}

		public void AddVertical(float value)
		{
			InputManager.SetRemoteAxisValue("Vertical", InputManager.GetAxis("Vertical") + value);
		}

		public void AddHorizontal(float value)
		{
			InputManager.SetRemoteAxisValue("Horizontal", InputManager.GetAxis("Horizontal") + value);
		}

		public void SetMouseX(float value)
		{
			InputManager.SetRemoteAxisValue("LookHorizontal", value);
		}

		public void SetMouseY(float value)
		{
			InputManager.SetRemoteAxisValue("LookVertical", value);
		}

		public void Jump()
		{
			InputManager.SetRemoteButtonValue("Jump", true, true);
		}
	}
}