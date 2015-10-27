using UnityEngine;
using System;
using System.Collections;

namespace TeamUtility.IO.Examples
{
    public class TPSInputController : MonoBehaviour
    {
        [SerializeField]
        private PlayerID _playerID;
        [SerializeField]
        private float _speed;

        private void Update()
        {
            Vector3 moveVector = new Vector3(InputManager.GetAxis("Horizontal", _playerID), 0,
                                             InputManager.GetAxis("Vertical", _playerID));
            
            transform.position += moveVector.normalized * _speed * Time.deltaTime;
        }
    }
}
