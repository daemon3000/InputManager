#region [Copyright (c) 2018 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial 
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using UnityEngine;
using System.IO;

namespace Luminosity.IO.Examples
{
	public class ResetControlScheme : MonoBehaviour 
	{
		[SerializeField]
		private TextAsset m_defaultInputProfile;
		[SerializeField]
		private string m_controlSchemeName;
		
		public void ResetInputs()
		{
			ControlScheme controlScheme = InputManager.GetControlScheme(m_controlSchemeName);
			ControlScheme defControlScheme = null;

			using(StringReader reader = new StringReader(m_defaultInputProfile.text))
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