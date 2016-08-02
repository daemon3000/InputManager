using UnityEngine;

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
	}
}
