using System;
using UnityEngine;

namespace Spaceship
{
    public class SpaceshipController : MonoBehaviour
    {
        public SpaceshipMover spaceshipMover;

        private Vector4 _inputVector = new Vector4(0,0,0,0);

        private void Update()
        {
            _inputVector.x             = Input.GetAxis("Vertical");
            _inputVector.y             = Input.GetAxis("Horizontal");
            spaceshipMover.inputVector = this._inputVector;
        }
    }
}