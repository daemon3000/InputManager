using UnityEngine;
using System.IO;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class ResetInputConfiguration : MonoBehaviour 
	{
		[SerializeField] private TextAsset m_defaultInputs;
		[SerializeField] private string m_inputConfigName;
		
		public void ResetInputs()
		{
			InputConfiguration inputConfig = InputManager.GetInputConfiguration(m_inputConfigName);
			InputConfiguration defInputConfig = null;

			using(StringReader reader = new StringReader(m_defaultInputs.text))
			{
				InputLoaderXML loader = new InputLoaderXML(reader);
				defInputConfig = loader.LoadSelective(m_inputConfigName);
			}

			if(defInputConfig != null)
			{
				if(defInputConfig.axes.Count == inputConfig.axes.Count)
				{
					for(int i = 0; i < defInputConfig.axes.Count; i++)
					{
						inputConfig.axes[i].Copy(defInputConfig.axes[i]);
					}
					InputManager.SetConfigurationDirty(m_inputConfigName);
				}
				else
				{
					Debug.LogError("Current and default input configurations don't have the same number of axes");
				}
			}
			else
			{
				Debug.LogError(string.Format(@"Default input profile doesn't contain an input configuration named '{0}'", m_inputConfigName));
			}
		}
	}
}