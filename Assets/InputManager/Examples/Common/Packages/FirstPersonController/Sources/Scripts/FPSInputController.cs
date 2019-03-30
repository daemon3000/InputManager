using UnityEngine;
using System.Collections;

namespace Luminosity.IO.Examples
{
	[RequireComponent(typeof(CharacterMotor))]
	public class FPSInputController : MonoBehaviour
	{
		[SerializeField]
		private bool m_quitWithEscape = false;

		private CharacterMotor motor;

		private void Awake()
		{
			motor = GetComponent<CharacterMotor>();
		}

		private	void Update() 
		{
			// Get the input vector from keyboard or analog stick
			var directionVector = new Vector3(InputManager.GetAxis("Horizontal"), 0, InputManager.GetAxis("Vertical"));
			
			if (directionVector != Vector3.zero) 
			{
				// Get the length of the directon vector and then normalize it
				// Dividing by the length is cheaper than normalizing when we already have the length anyway
				var directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;
				
				// Make sure the length is no bigger than 1
				directionLength = Mathf.Min(1, directionLength);
				
				// Make the input vector more sensitive towards the extremes and less sensitive in the middle
				// This makes it easier to control slow speeds when using analog sticks
				directionLength = directionLength * directionLength;
				
				// Multiply the normalized direction vector by the modified length
				directionVector = directionVector * directionLength;
			}
			
			// Apply the direction to the CharacterMotor
			motor.inputMoveDirection = transform.rotation * directionVector;
			motor.inputJump = InputManager.GetButton("Jump");

			if(m_quitWithEscape && InputManager.GetKeyDown(KeyCode.Escape))
			{
				Quit();
			}
		}

		private void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}