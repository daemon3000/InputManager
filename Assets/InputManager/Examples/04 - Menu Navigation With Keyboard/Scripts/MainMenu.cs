using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class MainMenu : MonoBehaviour 
	{
		[SerializeField]
		[FormerlySerializedAs("m_startPage")]
		private MenuPage _startPage;

		[SerializeField]
		[FormerlySerializedAs("m_pages")]
		private MenuPage[] _pages;

		private MenuPage _currentPage;

		private void Start()
		{
			ChangePage(_startPage.ID);
		}

		public void ChangePage(string id)
		{
			if(_currentPage != null)
				_currentPage.gameObject.SetActive(false);

			_currentPage = FindPage(id);
			if(_currentPage != null)
			{
				_currentPage.gameObject.SetActive(true);
				EventSystem.current.SetSelectedGameObject(_currentPage.FirstSelected);
			}
		}

		private MenuPage FindPage(string id)
		{
			foreach(MenuPage page in _pages)
			{
				if(page.ID == id)
					return page;
			}

			Debug.LogError("Unable to find menu page with id: " + id);
			return null;
		}
	}
}
