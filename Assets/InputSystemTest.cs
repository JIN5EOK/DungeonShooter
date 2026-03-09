using System;
using DungeonShooter;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class InputSystemTest : MonoBehaviour
    {
        private PlayerInput _playerInput;
        public void Start()
        {
            // var _playerInput = GetComponent<PlayerInput>();
            // _playerInput.defaultActionMap = "Touch";
            // // TODO: 하드코딩된 주소 개선 필요 
            // _playerInput.actions["Move"].performed += (value) =>
            // {
            //     var vec2 = value.ReadValue<Vector2>();
            //     GameObject.FindGameObjectWithTag("Player").GetComponent<EntityBase>().EntityContext.InputContext.MoveInput = vec2;
            //     Debug.Log(vec2);
            // };
        }
    }
}