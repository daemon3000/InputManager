using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class InvertAnalogAxis : MonoBehaviour 
	{
		[SerializeField]
		[FormerlySerializedAs("m_inputConfigName")]
		private string _inputConfigName;
		[SerializeField]
		[FormerlySerializedAs("m_axisConfigName")]
		private string _axisConfigName;
		[SerializeField]
		[FormerlySerializedAs("m_status")]
		private Text _status;

		private AxisConfiguration _axisConfig;

		private void Awake()
		{
			InitAxisConfig();
			InputManager.Instance.Loaded += InitAxisConfig;
		}

		private void OnDestroy()
		{
			if(InputManager.Instance != null)
				InputManager.Instance.Loaded -= InitAxisConfig;
		}

		private void InitAxisConfig()
		{
			_axisConfig = InputManager.GetAxisConfiguration(_inputConfigName, _axisConfigName);
			if(_axisConfig != null)
			{
				_status.text = _axisConfig.invert ? "On" : "Off";
			}
			else
			{
				_status.text = "Off";
				Debug.LogError(string.Format(@"Input configuration '{0}' does not exist or axis '{1}' does not exist", _inputConfigName, _axisConfigName));
			}
		}

		public void OnClick()
		{
			if(_axisConfig != null)
			{
				_axisConfig.invert = !_axisConfig.invert;
				_status.text = _axisConfig.invert ? "On" : "Off";
			}
		}
	}
}