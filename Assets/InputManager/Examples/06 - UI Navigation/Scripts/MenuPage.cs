using UnityEngine;

namespace Luminosity.IO.Examples
{
	public class MenuPage : MonoBehaviour 
	{
		[SerializeField] 
		private string m_id = null;
		[SerializeField]
		private GameObject m_firstSelected = null;

		public string ID { get { return m_id; } }
		public GameObject FirstSelected { get { return m_firstSelected; } }
	}
}