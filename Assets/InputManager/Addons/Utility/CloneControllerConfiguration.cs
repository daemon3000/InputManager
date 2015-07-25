using UnityEngine;
using System;
using System.Collections;

namespace TeamUtility.IO
{
	public class CloneControllerConfiguration : MonoBehaviour 
	{
		[SerializeField] private string _inputConfiguration;
		[SerializeField] 
		[Range(1, 3)]
		private int _numberOfClones = 1;

		private void Start()
		{
			InputConfiguration inputConfig = InputManager.GetInputConfiguration(_inputConfiguration);
			if(inputConfig == null)
			{
				Debug.LogWarningFormat("An input configuration named \'{0}\' does not exist", _inputConfiguration);
				return;
			}

			int axisCount = inputConfig.axes.Count;
			int joystickButtonCount = (int)KeyCode.Joystick1Button19 - (int)KeyCode.Joystick1Button0 + 1;

			//	All new axes will be added at the end of the list so it's safe to iterate normally to
			//	the initial number of axes
			for(int i = 0; i < axisCount; i++)
			{
				AxisConfiguration orig = inputConfig.axes[i];
				if(!orig.name.EndsWith("_P1"))
					continue;
				
				if(orig.joystick > 0)
				{
					Debug.LogErrorFormat("Cannot clone axis {0} of configuration {1} because joystick is {2} instead of 0", orig.name, _inputConfiguration, orig.joystick);
					continue;
				}
				
				if((orig.positive != KeyCode.None && ((int)orig.positive < (int)KeyCode.Joystick1Button0 || (int)orig.positive > (int)KeyCode.Joystick1Button19)) ||
				   (orig.altPositive != KeyCode.None && ((int)orig.altPositive < (int)KeyCode.Joystick1Button0 || (int)orig.altPositive > (int)KeyCode.Joystick1Button19)) ||
				   (orig.negative != KeyCode.None && ((int)orig.negative < (int)KeyCode.Joystick1Button0 || (int)orig.negative > (int)KeyCode.Joystick1Button19)) ||
				   (orig.altNegative != KeyCode.None && ((int)orig.altNegative < (int)KeyCode.Joystick1Button0 || (int)orig.altNegative > (int)KeyCode.Joystick1Button19)))
				{
					Debug.LogErrorFormat("Cannot clone axis {0} of configuration {1} because some key codes are invalid", orig.name, _inputConfiguration);
					continue;
				}
				
				for(int c = 0; c < _numberOfClones; c++)
				{
					string axisName = orig.name.Substring(0, orig.name.Length - 3) + "_P" + (c + 2);
					AxisConfiguration clone = InputManager.CreateEmptyAxis(_inputConfiguration, axisName);
					clone.Copy(orig);
					clone.name = axisName;
					if(orig.positive != KeyCode.None)
						clone.positive = (KeyCode)((int)orig.positive + (c + 1) * joystickButtonCount);
					if(orig.altPositive != KeyCode.None)
						clone.altPositive = (KeyCode)((int)orig.altPositive + (c + 1) * joystickButtonCount);
					if(orig.negative != KeyCode.None)
						clone.negative = (KeyCode)((int)orig.negative + (c + 1) * joystickButtonCount);
					if(orig.altNegative != KeyCode.None)
						clone.altNegative = (KeyCode)((int)orig.altNegative + (c + 1) * joystickButtonCount);
					clone.joystick = c + 1;
					clone.Initialize();
				}
			}
		}
	}
}