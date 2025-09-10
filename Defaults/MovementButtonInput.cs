using DBH.Attributes;
using DBH.Input.api.Extending;
using UnityEngine;

namespace DBH.Input.Defaults {
    [Bean]
    public class MovementButtonInput : DirectionInputSystem {
        public delegate void MovementPressed(Vector2 direction);
        public event MovementPressed OnMovementPressed;

        public override string MappedName() {
            return "Movement";
        }

        protected override void InputChanged(Vector2 input) {
            OnMovementPressed?.Invoke(input);
        }
    }
}