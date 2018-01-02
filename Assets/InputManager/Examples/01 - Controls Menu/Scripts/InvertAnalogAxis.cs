using UnityEngine;
using UnityEngine.UI;

namespace TeamUtility.IO.Examples
{
	public class InvertAnalogAxis : MonoBehaviour 
	{
		[SerializeField]
		private string m_controlSchemeName;
		[SerializeField]
		private string m_actionName;
		[SerializeField]
		private int m_bindingIndex;
		[SerializeField]
		private Text m_status;

		private InputAction m_inputAction;

		private void Awake()
		{
			InitializeInputAction();
			InputManager.Loaded += InitializeInputAction;
		}

		private void OnDestroy()
		{
			InputManager.Loaded -= InitializeInputAction;
		}

		private void InitializeInputAction()
		{
			m_inputAction = InputManager.GetAction(m_controlSchemeName, m_actionName);
			if(m_inputAction != null)
			{
				m_status.text = m_inputAction.Bindings[m_bindingIndex].Invert ? "On" : "Off";
			}
			else
			{
				m_status.text = "Off";
				Debug.LogErrorFormat("Input configuration '{0}' does not exist or axis '{1}' does not exist", m_controlSchemeName, m_actionName);
			}
		}

		public void OnClick()
		{
			if(m_inputAction != null)
			{
				m_inputAction.Bindings[m_bindingIndex].Invert = !m_inputAction.Bindings[m_bindingIndex].Invert;
				m_status.text = m_inputAction.Bindings[m_bindingIndex].Invert ? "On" : "Off";
			}
		}
	}
}