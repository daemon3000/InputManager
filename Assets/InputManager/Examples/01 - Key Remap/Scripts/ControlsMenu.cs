using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using TeamUtility.IO;

public sealed class ControlsMenu : MonoBehaviour 
{
	private int _menuWidth = 400;
	private int _menuHeight = 300;
	private int _itemHeight = 30;
	private int _scanIndex = -1;
	private bool _showGUI = true;

	private void Start()
	{
		Load();
	}

	private void Update()
	{
		if(!_showGUI && InputManager.GetKeyDown(KeyCode.F1))
		{
			_scanIndex = -1;
			_showGUI = true;
		}
		
		UpdateStatus();
	}
	
	private void UpdateStatus()
	{
		guiText.text = "Status: ";
		if(InputManager.GetAxis("Horizontal") > 0.1f)
		{
			guiText.text = "Status: Moving Right";
		}
		else if(InputManager.GetAxis("Horizontal") < -0.1f)
		{
			guiText.text = "Status: Moving Left";
		}
		else if(InputManager.GetButton("Jump"))
		{
			guiText.text = "Status: Jumping";
		}
		else if(InputManager.GetButton("Fire1"))
		{
			guiText.text = "Status: Shooting Main Weapon";
		}
		else if(InputManager.GetButton("Fire2"))
		{
			guiText.text = "Status: Shooting Secondary Weapon";
		}
	}

	#region [OnGUI]
	private void OnGUI()
	{
		if(!_showGUI)
			return;

		Rect menuPosition = new Rect(Screen.width / 2 - _menuWidth / 2, 
		                             Screen.height / 2 - _menuHeight / 2,
		                             _menuWidth, _menuHeight);
		DisplayControlsMenu(menuPosition);
	}

	private void DisplayControlsMenu(Rect screenRect)
	{
		GUI.Box(screenRect, "");		//	background

		GUILayout.BeginArea(new Rect(screenRect.x + 5.0f, screenRect.y + 5.0f, screenRect.width - 10.0f, screenRect.height - (_itemHeight + 25.0f)));
		//	Move Right
		GUILayout.BeginHorizontal(GUILayout.Height(_itemHeight));
		GUILayout.Label("Move Right: " + GetKeyName("Horizontal", true), GUILayout.Width(screenRect.width / 2));
		GUI.enabled = _scanIndex != 0;
		if(GUILayout.Button("Scan"))
		{
			InputManager.StartKeyScan(HandleKeyScanResult, 10.0f, null, "Horizontal", true);
			_scanIndex = 0;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//	Move Left
		GUILayout.BeginHorizontal(GUILayout.Height(_itemHeight));
		GUILayout.Label("Move Left: " + GetKeyName("Horizontal", false), GUILayout.Width(screenRect.width / 2));
		GUI.enabled = _scanIndex != 1;
		if(GUILayout.Button("Scan"))
		{
			InputManager.StartKeyScan(HandleKeyScanResult, 10.0f, null, "Horizontal", false);
			_scanIndex = 1;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//	Fire1
		GUILayout.BeginHorizontal(GUILayout.Height(_itemHeight));
		GUILayout.Label("Fire1: " + GetKeyName("Fire1", true), GUILayout.Width(screenRect.width / 2));
		GUI.enabled = _scanIndex != 2;
		if(GUILayout.Button("Scan"))
		{
			InputManager.StartKeyScan(HandleKeyScanResult, 10.0f, null, "Fire1", true);
			_scanIndex = 2;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//	Fire2
		GUILayout.BeginHorizontal(GUILayout.Height(_itemHeight));
		GUILayout.Label("Fire2: " + GetKeyName("Fire2", true), GUILayout.Width(screenRect.width / 2));
		GUI.enabled = _scanIndex != 3;
		if(GUILayout.Button("Scan"))
		{
			InputManager.StartKeyScan(HandleKeyScanResult, 10.0f, null, "Fire2", true);
			_scanIndex = 3;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		//	Jump
		GUILayout.BeginHorizontal(GUILayout.Height(_itemHeight));
		GUILayout.Label("Jump: " + GetKeyName("Jump", true), GUILayout.Width(screenRect.width / 2));
		GUI.enabled = _scanIndex != 4;
		if(GUILayout.Button("Scan"))
		{
			InputManager.StartKeyScan(HandleKeyScanResult, 10.0f, null, "Jump", true);
			_scanIndex = 4;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(screenRect.x + 5.0f, screenRect.yMax - (_itemHeight + 20.0f), screenRect.width - 10.0f, _itemHeight));
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Close"))
		{
			if(InputManager.IsScanning)
				InputManager.CancelScan();

			_showGUI = false;
		}
		if(GUILayout.Button("Save And Close"))
		{
			if(InputManager.IsScanning)
				InputManager.CancelScan();
			Save();

			_showGUI = false;
		}

		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	#endregion

	private bool HandleKeyScanResult(KeyCode key, object[] args)
	{
		if(!IsValidKeyboardKey(key))
			return false;

		string axisName = (string)args[0];
		bool positive = (bool)args[1];

		if(key != KeyCode.None)
		{
			AxisConfiguration axisConfig = InputManager.GetAxisConfiguration("Keyboard", axisName);
			if(axisConfig != null)
			{
				SetKey(axisConfig, key, positive);
			}
		}
		_scanIndex = -1;

		return true;
	}

	private bool IsValidKeyboardKey(KeyCode key)
	{
		if((int)key >= (int)KeyCode.JoystickButton0)
			return false;
		if(key == KeyCode.LeftApple || key == KeyCode.RightApple)
			return false;
		if(key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
			return false;
		
		return true;
	}

	private void SetKey(AxisConfiguration axisConfig, KeyCode key, bool positive)
	{
		if(positive)
		{
			axisConfig.positive = (key == KeyCode.Backspace) ? KeyCode.None : key;
		}
		else
		{
			axisConfig.negative = (key == KeyCode.Backspace) ? KeyCode.None : key;
		}
	}

	private string GetKeyName(string axisName, bool positive)
	{
		AxisConfiguration axisConfig = InputManager.GetAxisConfiguration("Keyboard", axisName);
		if(axisConfig != null)
		{
			if(positive)
			{
				return (axisConfig.positive != KeyCode.None) ? axisConfig.positive.ToString() : string.Empty;
			}
			else
			{
				return (axisConfig.negative != KeyCode.None) ? axisConfig.negative.ToString() : string.Empty;
			}
		}

		return string.Empty;
	}

	private void Load()
	{
		if(PlayerPrefs.HasKey("ControlsMenu.InputConfig"))
		{
			string xml = PlayerPrefs.GetString("ControlsMenu.InputConfig");
			using(TextReader reader = new StringReader(xml))
			{
				InputLoaderXML loader = new InputLoaderXML(reader);
				InputManager.Load(loader);
			}
		}
	}

	private void Save()
	{
		StringBuilder output = new StringBuilder();
		InputSaverXML saver = new InputSaverXML(output);
		InputManager.Save(saver);

		PlayerPrefs.SetString("ControlsMenu.InputConfig", output.ToString());
	}
}
