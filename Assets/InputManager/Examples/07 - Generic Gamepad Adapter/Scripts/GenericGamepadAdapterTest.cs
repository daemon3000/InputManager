using UnityEngine;
using UnityEngine.UI;

namespace Luminosity.IO.Examples
{
    public class GenericGamepadAdapterTest : MonoBehaviour
    {
        [SerializeField]
        private GamepadIndex m_selectedGamepad = GamepadIndex.GamepadOne;

        [Header("Templates")]
        [SerializeField]
        private GameObject m_gamepadStateTemplate = null;
        [SerializeField]
        private GameObject m_gamepadButtonTemplate = null;
        [SerializeField]
        private GameObject m_gamepadAxisTemplate = null;

        [Header("Roots")]
        [SerializeField]
        private RectTransform m_gamepadStateRoot = null;
        [SerializeField]
        private RectTransform m_gamepadButtonRoot = null;
        [SerializeField]
        private RectTransform m_gamepadAxisRoot = null;

        private Text[] m_gamepadStateText;
        private Text[] m_gamepadButtonText;
        private Text[] m_gamepadAxisText;

        private void Start()
        {
            CreateGamepadStateFields();
            CreateGamepadButtonFields();
            CreateGamepadAxisFields();
        }

        private void Update()
        {
            for(int i = 0; i < m_gamepadStateText.Length; i++)
                m_gamepadStateText[i].text = GamepadState.IsConnected((GamepadIndex)i) ? "Connected" : "Not Connected";

            for(int i = 0; i < m_gamepadButtonText.Length; i++)
                m_gamepadButtonText[i].text = GamepadState.GetButton((GamepadButton)i, m_selectedGamepad).ToString();

            for(int i = 0; i < m_gamepadAxisText.Length; i++)
                m_gamepadAxisText[i].text = GamepadState.GetAxis((GamepadAxis)i, m_selectedGamepad).ToString();

            GenericGamepadStateAdapter adapter = GamepadState.Adapter as GenericGamepadStateAdapter;
            GenericGamepadProfile profile = adapter[m_selectedGamepad];

            if(profile != null && profile.DPadType == GamepadDPadType.Axis)
            {
                if(GamepadState.GetButtonDown(GamepadButton.DPadUp, m_selectedGamepad))
                    Debug.Log("DPadUp was pressed!");
                if(GamepadState.GetButtonDown(GamepadButton.DPadDown, m_selectedGamepad))
                    Debug.Log("DPadDown was pressed!");
                if(GamepadState.GetButtonDown(GamepadButton.DPadLeft, m_selectedGamepad))
                    Debug.Log("DPadLeft was pressed!");
                if(GamepadState.GetButtonDown(GamepadButton.DPadRight, m_selectedGamepad))
                    Debug.Log("DPadRight was pressed!");

                if(GamepadState.GetButtonUp(GamepadButton.DPadUp, m_selectedGamepad))
                    Debug.Log("DPadUp was released!");
                if(GamepadState.GetButtonUp(GamepadButton.DPadDown, m_selectedGamepad))
                    Debug.Log("DPadDown was released!");
                if(GamepadState.GetButtonUp(GamepadButton.DPadLeft, m_selectedGamepad))
                    Debug.Log("DPadLeft was released!");
                if(GamepadState.GetButtonUp(GamepadButton.DPadRight, m_selectedGamepad))
                    Debug.Log("DPadRight was released!");
            }
        }

        private void CreateGamepadStateFields()
        {
            m_gamepadStateText = new Text[4];

            for(int i = 0; i < m_gamepadStateText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadStateTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadStateRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = "Gamepad " + (i + 1) + ":";

                m_gamepadStateText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadStateText[i].text = "Not Connected";
            }
        }

        private void CreateGamepadButtonFields()
        {
            m_gamepadButtonText = new Text[14];

            for(int i = 0; i < m_gamepadButtonText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadButtonTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadButtonRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = ((GamepadButton)i) + ":";

                m_gamepadButtonText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadButtonText[i].text = "False";
            }
        }

        private void CreateGamepadAxisFields()
        {
            m_gamepadAxisText = new Text[8];

            for(int i = 0; i < m_gamepadAxisText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadAxisTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadAxisRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = ((GamepadAxis)i) + ":";

                m_gamepadAxisText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadAxisText[i].text = "0";
            }
        }
    }
}