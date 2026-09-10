using System;
using System.Collections.Generic;
using System.Linq;
using DBH.Attributes;
using DBH.Base;
using DBH.Input.api.Extending;
using DBH.Input.api.Keys;
using DBH.Input.Dtos;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Vault;

namespace DBH.Input.Controller {
    [DBH.Attributes.Controller]
    public class InputControllerInputSystem : DBHMono, IInputController {
        [Grab]
        private List<IIDisposableInputSystem> inputSystems;

        [SerializeField]
        private InputSpriteMap inputSpriteMap;

        [SerializeField]
        private List<GroupStatus> groupStatuses;

        public delegate void SchemaChange(InputSchema inputSchema);

        public event SchemaChange OnSchemaChange;

        private InputSchema currentSchema = InputSchema.Unknown;

        public InputSchema CurrentSchema => currentSchema;
        private IDisposable buttonPressSubscription;


        private void OnEnable() {
            InputSystem.onDeviceChange += OnDeviceChange;
            buttonPressSubscription =
                InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
        }

        private void OnDisable() {
            InputSystem.onDeviceChange -= OnDeviceChange;
            buttonPressSubscription?.Dispose();
            buttonPressSubscription = null;
        }

        private void OnAnyButtonPressed(InputControl button) {
            var device = button.device;
            var nextScheme = ConvertToScheme(device);
            if (nextScheme == currentSchema) return;
            currentSchema = nextScheme;
            OnSchemaChange?.Invoke(currentSchema);
        }


        private void OnDeviceChange(InputDevice inputDevice, InputDeviceChange inputDeviceChange) {
            var foundInputScheme = InputControlScheme.FindControlSchemeForDevice(inputDevice, InputSystem.actions.controlSchemes);
            currentSchema = ConvertToScheme(foundInputScheme);
        }


        private static InputSchema ConvertToScheme(InputControlScheme? foundInputScheme) {
            if (foundInputScheme != null) {
                return foundInputScheme.Value.name switch {
                    "Keyboard&Mouse" => InputSchema.KeyboardAndMouse,
                    "Gamepad" => InputSchema.Gamepad,
                    "Touch" => InputSchema.Touch,
                    "Joystick" => InputSchema.Joystick,
                    "XR" => InputSchema.XR,
                    _ => InputSchema.Unknown
                };
            }

            return InputSchema.Unknown;
        }


        private static InputSchema ConvertToScheme(InputDevice inputDevice) {
            return inputDevice switch {
                Keyboard or Mouse => InputSchema.KeyboardAndMouse,
                Gamepad => InputSchema.Gamepad,
                Joystick => InputSchema.Joystick,
                _ => InputSchema.Unknown
            };
        }

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

        public string IconToInput(AbstractButtonInputSystem buttonInputSystem) {
            return inputSpriteMap.SpriteLayout
                .Find(layout => layout.InputPath.Equals(buttonInputSystem.InputAction.name))
                .SpriteToSchemata
                .Where(schema => schema.InputSchema == currentSchema)
                .Select(schema => schema.Sprite.name)
                .Aggregate((s, s1) => s + "+" + s1);
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