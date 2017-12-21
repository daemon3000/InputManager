using UnityEngine;

namespace TeamUtility.IO.Examples
{
    public class TPSInputController : MonoBehaviour
    {
        [SerializeField]
        private PlayerID m_playerID;
        [SerializeField]
        private float m_speed;

        private void Update()
        {
            Vector3 moveVector = new Vector3(InputManager.GetAxis("Horizontal", m_playerID), 0,
                                             InputManager.GetAxis("Vertical", m_playerID));
            
            transform.position += moveVector.normalized * m_speed * Time.deltaTime;
        }
    }
}
