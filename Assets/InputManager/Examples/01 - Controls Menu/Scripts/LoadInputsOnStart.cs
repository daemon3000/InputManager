using UnityEngine;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class LoadInputsOnStart : MonoBehaviour 
	{
		[SerializeField] private int m_exampleID;

		private void Awake()
		{
			string savePath = PathUtility.GetInputSaveFolder(m_exampleID) + "/input_config.xml";
			if(System.IO.File.Exists(savePath))
			{
				InputLoaderXML loader = new InputLoaderXML(savePath);
				InputManager.Load(loader);
			}
		}
	}
}