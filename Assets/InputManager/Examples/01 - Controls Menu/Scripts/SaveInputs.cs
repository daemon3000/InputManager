using UnityEngine;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class SaveInputs : MonoBehaviour 
	{
		[SerializeField] private int m_exampleID;

		public void Save()
		{
			string saveFolder = PathUtility.GetInputSaveFolder(m_exampleID);
			if(!System.IO.Directory.Exists(saveFolder))
				System.IO.Directory.CreateDirectory(saveFolder);

			InputSaverXML saver = new InputSaverXML(saveFolder + "/input_config.xml");
			InputManager.Save(saver);
		}
	}
}