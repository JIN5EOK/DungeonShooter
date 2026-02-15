using UnityEngine;

namespace DungeonShooter
{
    public class EntityInputContext
    {
        public Vector2 LastMoveDirection { get; private set; }

        public Vector2 MoveInput
        {
            get => _moveInput;
            set
            {
                if (value != Vector2.zero) 
                    LastMoveDirection = value.normalized;
                
                _moveInput = value;
            }
        }

        private Vector2 _moveInput;
        public bool InteractInput { get; set; }
        public bool DashInput { get; set; }
        public Skill SkillInput { get; set; }
    }
}