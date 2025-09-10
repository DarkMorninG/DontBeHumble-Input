using DBH.Attributes;
using DBH.Input.api.Extending;

namespace DBH.Input.Defaults {
    [Bean]
    public class InteractButtonReleaseInput : ButtonInputSystem {
        public delegate void InteractButtonPressed();

        public event InteractButtonPressed OnAbilityButtonPressed;

        public override string MappedName() {
            return "Interact";
        }

        public override void OnKeyReleased() {
            OnAbilityButtonPressed?.Invoke();
        }
    }
}