using UnityEngine;
using UnityEngine.UI;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Button))]
	public class VirtualInputButton : MonoBehaviour
	{
		[SerializeField]
		private string _inputConfiguration;
		[SerializeField]
		private string _buttonName;

		private void Awake()
		{
			Button button = GetComponent<Button>();
			button.onClick.AddListener(HandleButtonClicked);
		}

		private void HandleButtonClicked()
		{
			InputManager.SetRemoteButtonValue(_inputConfiguration, _buttonName, true, true);
		}

		private void LateUpdate()
		{
			InputManager.SetRemoteButtonValue(_inputConfiguration, _buttonName, false, false);
		}
	}
}