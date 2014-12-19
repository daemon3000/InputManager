using UnityEngine;
using System.IO;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class ResetGamepadInputs : MonoBehaviour 
	{
		[SerializeField] private TextAsset m_defaultInputs;

		public void ResetInputs()
		{
			using(StringReader reader = new StringReader(m_defaultInputs.text))
			{
				InputLoaderXML loader = new InputLoaderXML(reader);
				InputManager.Load(loader);
			}
		}
	}
}