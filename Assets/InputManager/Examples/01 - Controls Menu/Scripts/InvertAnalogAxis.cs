using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class InvertAnalogAxis : MonoBehaviour 
	{
		[SerializeField] private string m_inputConfigName;
		[SerializeField] private string m_axisConfigName;
		[SerializeField] private Text m_status;

		private AxisConfiguration m_axisConfig;

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
			m_axisConfig = InputManager.GetAxisConfiguration(m_inputConfigName, m_axisConfigName);
			if(m_axisConfig != null)
			{
				m_status.text = m_axisConfig.invert ? "On" : "Off";
			}
			else
			{
				m_status.text = "Off";
				Debug.LogError(string.Format(@"Input configuration '{0}' does not exist or axis '{1}' does not exist", m_inputConfigName, m_axisConfigName));
			}
		}

		public void OnClick()
		{
			if(m_axisConfig != null)
			{
				m_axisConfig.invert = !m_axisConfig.invert;
				m_status.text = m_axisConfig.invert ? "On" : "Off";
			}
		}
	}
}