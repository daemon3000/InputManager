using UnityEngine;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public static class PathUtility
	{
		public static string GetInputSaveFolder(int example)
		{
			return string.Format("{0}/example_{1}", Application.persistentDataPath, example);
		}
	}
}