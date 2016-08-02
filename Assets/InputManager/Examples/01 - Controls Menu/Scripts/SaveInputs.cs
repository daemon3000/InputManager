using UnityEngine;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class SaveInputs : MonoBehaviour 
	{
		[SerializeField]
		[FormerlySerializedAs("m_exampleID")]
		private int _exampleID;

		public void Save()
		{
			string saveFolder = PathUtility.GetInputSaveFolder(_exampleID);
			if(!System.IO.Directory.Exists(saveFolder))
				System.IO.Directory.CreateDirectory(saveFolder);

			InputSaverXML saver = new InputSaverXML(saveFolder + "/input_config.xml");
			InputManager.Save(saver);
		}
	}
}