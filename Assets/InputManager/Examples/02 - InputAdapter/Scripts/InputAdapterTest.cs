using UnityEngine;
using System;
using System.Collections;
using TeamUtility.IO;

public sealed class InputAdapterTest : MonoBehaviour 
{
	public GUIText statusGUI;
	
	private void Update()
	{
		statusGUI.text = "Status: ";
		
		if(InputAdapter.GetAxis("MoveHorizontal") != 0.0f || InputAdapter.GetAxis("MoveVertical") != 0.0f)
		{
			statusGUI.text = "Status: Moving";
		}
		else if(InputAdapter.GetButton("Jump"))
		{
			statusGUI.text = "Status: Jumping";
		}
		else if(InputAdapter.GetButton("Use"))
		{
			statusGUI.text = "Status: Using";
		}
		else if(InputAdapter.GetMouseButton(0) || InputAdapter.GetTriggerButton(InputTriggerButton.Right))
		{
			statusGUI.text = "Status: Attacking";
		}
	}
}
