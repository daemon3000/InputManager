using UnityEngine;

namespace Luminosity.IO
{
    [RequireComponent(typeof(GenericGamepadStateAdapter))]
    public abstract class GamepadProfileSelector : MonoBehaviour
    {
        protected GenericGamepadStateAdapter Adapter { get; private set; }

        protected virtual void Start()
        {
            Adapter = GetComponent<GenericGamepadStateAdapter>();
            Adapter.GamepadConnected += OnAssignGamepadProfile;

            OnAssignGamepadProfile(GamepadIndex.GamepadOne);
            OnAssignGamepadProfile(GamepadIndex.GamepadTwo);
            OnAssignGamepadProfile(GamepadIndex.GamepadThree);
            OnAssignGamepadProfile(GamepadIndex.GamepadFour);
        }

        protected abstract void OnAssignGamepadProfile(GamepadIndex gamepad);
    }
}
