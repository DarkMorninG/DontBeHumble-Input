using DBH.Base;
using DBH.Input.api.Extending;
using DBH.Input.api.Keys;
using UnityEngine;
using UnityEngine.InputSystem;
using Vault;

namespace DBH.Input.Controller {
    [Attributes.Controller]
    public class InputControllerInputSystem : DBHMono, IInputController {
        public void DisableGroup(string group) {
            InputSystem.actions.FindActionMap(group).actions.ForEach(action => action.Disable());
        }

        public void EnableGroup(string group) {
            InputSystem.actions.FindActionMap(group).actions.ForEach(action => action.Enable());
        }

        [ContextMenu("Enable")]
        public void Enable() {
            EnableGroup("Player");
        }

        public void AddButton(InputKeys keys) {
            throw new System.NotImplementedException();
        }

        public void AddButton(DirectionKeys keys) {
            throw new System.NotImplementedException();
        }
    }
}