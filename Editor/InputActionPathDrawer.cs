using System;
using DBH.Input.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace DBH.Input.Editor {
    [CustomPropertyDrawer(typeof(InputActionPathAttribute))]
    public sealed class InputActionPathDrawer : PropertyDrawer {
        private const string EmptyLabel = "Select an input action...";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                EditorGUI.HelpBox(position, $"{nameof(InputActionPathAttribute)} can only be used on a string field.", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var buttonRect = EditorGUI.PrefixLabel(fieldRect, label);
            var actionAsset = InputSystem.actions;
            var buttonContent = GetButtonContent(property, actionAsset);

            using (new EditorGUI.DisabledScope(actionAsset == null)) {
                if (EditorGUI.DropdownButton(buttonRect, buttonContent, FocusType.Keyboard)) {
                    var targets = property.serializedObject.targetObjects;
                    var propertyPath = property.propertyPath;
                    var dropdown = new InputActionDropdown(
                        new AdvancedDropdownState(),
                        actionAsset,
                        path => AssignPath(targets, propertyPath, path));

                    dropdown.Show(buttonRect);
                }
            }

            DrawValidationMessage(position, property, actionAsset);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                return EditorGUIUtility.singleLineHeight * 2f;
            }

            return HasValidationMessage(property, InputSystem.actions)
                ? EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing
                : EditorGUIUtility.singleLineHeight;
        }

        private static GUIContent GetButtonContent(SerializedProperty property, InputActionAsset actionAsset) {
            if (property.hasMultipleDifferentValues) {
                return EditorGUIUtility.TrTextContent("\u2014");
            }

            var path = property.stringValue;
            if (string.IsNullOrEmpty(path)) {
                return EditorGUIUtility.TrTextContent(EmptyLabel);
            }

            var content = EditorGUIUtility.TrTextContent(path);
            if (actionAsset != null && actionAsset.FindAction(path, false) == null) {
                content.tooltip = $"'{path}' does not exist in the project-wide Input Actions asset.";
            }

            return content;
        }

        private static void DrawValidationMessage(
            Rect position,
            SerializedProperty property,
            InputActionAsset actionAsset) {
            if (!HasValidationMessage(property, actionAsset)) {
                return;
            }

            var helpRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight);

            var message = actionAsset == null
                ? "No project-wide Input Actions asset is configured in Project Settings."
                : $"Input action '{property.stringValue}' no longer exists.";
            EditorGUI.HelpBox(helpRect, message, MessageType.Warning);
        }

        private static bool HasValidationMessage(SerializedProperty property, InputActionAsset actionAsset) {
            if (property.hasMultipleDifferentValues) {
                return false;
            }

            if (actionAsset == null) {
                return true;
            }

            return !string.IsNullOrEmpty(property.stringValue) &&
                   actionAsset.FindAction(property.stringValue, false) == null;
        }

        private static void AssignPath(Object[] targets, string propertyPath, string path) {
            Undo.RecordObjects(targets, "Select Input Action");

            foreach (var target in targets) {
                var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(propertyPath);
                if (property == null) {
                    continue;
                }

                property.stringValue = path;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private sealed class InputActionDropdown : AdvancedDropdown {
            private readonly InputActionAsset actionAsset;
            private readonly Action<string> onSelected;

            public InputActionDropdown(
                AdvancedDropdownState state,
                InputActionAsset actionAsset,
                Action<string> onSelected) : base(state) {
                this.actionAsset = actionAsset;
                this.onSelected = onSelected;
                minimumSize = new Vector2(280f, 320f);
            }

            protected override AdvancedDropdownItem BuildRoot() {
                var root = new AdvancedDropdownItem("Input Actions");
                root.AddChild(new ActionItem("None", string.Empty));

                foreach (var actionMap in actionAsset.actionMaps) {
                    var mapItem = new AdvancedDropdownItem(actionMap.name);
                    foreach (var action in actionMap.actions) {
                        mapItem.AddChild(new ActionItem(action.name, $"{actionMap.name}/{action.name}"));
                    }

                    root.AddChild(mapItem);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item) {
                if (item is ActionItem actionItem) {
                    onSelected(actionItem.Path);
                }
            }
        }

        private sealed class ActionItem : AdvancedDropdownItem {
            public string Path { get; }

            public ActionItem(string name, string path) : base(name) {
                Path = path;
            }
        }
    }
}
