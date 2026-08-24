using System;
using System.Collections.Generic;
using System.Linq;
using DBH.Attributes;
using DBH.Base;
using DBH.Input.api.Extending;
using DBH.Input.api.Keys;
using UnityEngine;
using UnityEngine.InputSystem;
using Vault;

namespace DBH.Input.Controller {
    [Attributes.Controller]
    public class InputControllerInputSystem : DBHMono, IInputController {
        [Grab]
        private List<IIDisposableInputSystem> inputSystems;

        [SerializeField]
        private List<GroupStatus> groupStatuses;

        public override void OnStart() {
            foreach (var groupStatus in groupStatuses) {
                InputSystem.actions.FindActionMap(groupStatus.Group)
                    .actions.ForEach(action => {
                        if (groupStatus.EnabledOnStart) {
                            action.Enable();
                        } else {
                            action.Disable();
                        }
                    });
            }
        }

        private void OnDestroy() {
            inputSystems.ForEach(buttonInputSystem => buttonInputSystem.Deconstruct());
        }

        public void DisableGroup(string group) {
            InputSystem.actions.FindActionMap(group).actions.ForEach(action => action.Disable());
        }

        public void EnableGroup(string group) {
            InputSystem.actions.FindActionMap(group).actions.ForEach(action => action.Enable());
        }

        public void AddButton(InputKeys keys) {
            throw new NotImplementedException();
        }

        public void AddButton(DirectionKeys keys) {
            throw new NotImplementedException();
        }

        private void OnValidate() {
            foreach (var actionsActionMap in InputSystem.actions.actionMaps) {
                if (groupStatuses.All(status => status.Group != actionsActionMap.name)) {
                    groupStatuses.Add(new GroupStatus(actionsActionMap.name, true));
                }
            }
        }
    }

    [Serializable]
    public class GroupStatus {
        [SerializeField]
        private string group;

        [SerializeField]
        private bool enabledOnStart;

        public GroupStatus(string group, bool enabledOnStart) {
            this.group = group;
            this.enabledOnStart = enabledOnStart;
        }

        public string Group => group;

        public bool EnabledOnStart => enabledOnStart;
    }
}