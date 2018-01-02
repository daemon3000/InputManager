using UnityEngine;
using System.IO;

namespace TeamUtility.IO.Examples
{
	public class ResetInputConfiguration : MonoBehaviour 
	{
		[SerializeField]
		private TextAsset m_defaultInputs;
		[SerializeField]
		private string m_controlSchemeName;
		
		public void ResetInputs()
		{
			ControlScheme controlScheme = InputManager.GetControlScheme(m_controlSchemeName);
			ControlScheme defControlScheme = null;

			using(StringReader reader = new StringReader(m_defaultInputs.text))
			{
				InputLoaderXML loader = new InputLoaderXML(reader);
				defControlScheme = loader.Load(m_controlSchemeName);
			}

			if(defControlScheme != null)
			{
				if(defControlScheme.Actions.Count == controlScheme.Actions.Count)
				{
					for(int i = 0; i < defControlScheme.Actions.Count; i++)
					{
						controlScheme.Actions[i].Copy(defControlScheme.Actions[i]);
					}

					InputManager.Reinitialize();
				}
				else
				{
					Debug.LogError("Current and default control scheme don't have the same number of actions");
				}
			}
			else
			{
				Debug.LogErrorFormat("Default input profile doesn't contain a control scheme named '{0}'", m_controlSchemeName);
			}
		}
	}
}