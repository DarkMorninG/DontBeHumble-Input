using System;
using System.Collections.Generic;
using System.Linq;
using DBH.Attributes;
using DBH.Base;
using DBH.Input.api.Extending;
using DBH.Input.api.Keys;
using UnityEngine;
using Vault;

namespace DBH.Input.Controller {
    [Attributes.Controller]
    public class InputController : DBHMono, IInputController {
        [Grab]
        private List<IButtonInputSystem> buttonInputSystems;

        [Grab]
        private List<IDirectionInputSystem> directionInputSystems;

        [SerializeField]
        private List<InputKeys> inputButtonMap = new() {
            new InputKeys("Default", "Confirm", Lists.Of(KeyCode.F)),
        };

        [SerializeField]
        private List<DirectionKeys> inputDirectionMap = new() {
            new DirectionKeys("Default", "Movement"),
        };

        public List<IButtonInputSystem> ButtonInputSystems => buttonInputSystems;

        public List<IDirectionInputSystem> DirectionInputSystems => directionInputSystems;

        public void AddButton(InputKeys keys) {
            inputButtonMap.Add(keys);
        }

        public void AddButton(DirectionKeys keys) {
            inputDirectionMap.Add(keys);
        }


        public void DisableGroup(string group) {
            var disableButtonName = inputButtonMap.Where(keys => keys.Group.Equals(group))
                .Select(keys => keys.Name)
                .ToList();
            buttonInputSystems.Where(system => disableButtonName.Contains(system.MappedName()))
                .ForEach(system => system.Enabled = false);
            var inputDirectionNames = inputDirectionMap.Where(keys => keys.Group.Equals(group))
                .Select(keys => keys.Name)
                .ToString();
            directionInputSystems.Where(system => inputDirectionNames.Contains(system.MappedName()))
                .ForEach(system => system.Enabled = false);
        }

        public void EnableGroup(string group) {
            var disableButtonName = inputButtonMap.Where(keys => keys.Group.Equals(group))
                .Select(keys => keys.Name)
                .ToList();
            buttonInputSystems.Where(system => disableButtonName.Contains(system.MappedName()))
                .ForEach(system => system.Enabled = true);
            var inputDirectionNames = inputDirectionMap.Where(keys => keys.Group.Equals(group))
                .Select(keys => keys.Name)
                .ToString();
            directionInputSystems.Where(system => inputDirectionNames.Contains(system.MappedName()))
                .ForEach(system => system.Enabled = true);
        }


        private void Update() {
            inputButtonMap
                .Where(keys => keys.Input.Any(UnityEngine.Input.GetKeyDown))
                .Select(keys => buttonInputSystems.FindOptional(system => system.MappedName().Equals(keys.Name)))
                .Where(systems => systems.IsPresent())
                .Select(systems => systems.Get())
                .Where(system => system.Enabled)
                .ForEach(system => system.KeyPressed());

            inputButtonMap
                .Where(keys => keys.Input.Any(UnityEngine.Input.GetKeyUp))
                .Select(keys => buttonInputSystems.FindOptional(system => system.MappedName().Equals(keys.Name)))
                .Where(systems => systems.IsPresent())
                .Select(systems => systems.Get())
                .Where(system => system.Enabled)
                .ForEach(system => system.KeyReleased());
        }

        private void FixedUpdate() {
            directionInputSystems
                .Where(system => system.Enabled)
                .ForEach(system => system.FixedUpdate());
        }
    }
}