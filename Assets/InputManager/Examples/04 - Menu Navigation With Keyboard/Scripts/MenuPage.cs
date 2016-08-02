using UnityEngine;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class MenuPage : MonoBehaviour 
	{
		[SerializeField] 
		[FormerlySerializedAs("m_id")]
		private string _id;
		[SerializeField]
		[FormerlySerializedAs("m_firstSelected")]
		private GameObject _firstSelected;

		public string ID { get { return _id; } }
		public GameObject FirstSelected { get { return _firstSelected; } }
	}
}