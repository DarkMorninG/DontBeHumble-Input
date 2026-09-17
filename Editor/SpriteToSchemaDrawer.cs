using System;
using System.Collections.Generic;
using DBH.Input.Controller;
using DBH.Input.Dtos;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Editor;

namespace DBH.Input.Editor {
    [CustomPropertyDrawer(typeof(SpriteToSchema))]
    public sealed class SpriteToSchemaDrawer : PropertyDrawer, IDisposable {
        private const int FieldCount = 3;

        private readonly Dictionary<string, InputControlPathEditor> controlEditors = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(
                SingleLine(position, 0),
                property.isExpanded,
                label,
                true);

            if (property.isExpanded) {
                using (new EditorGUI.IndentLevelScope()) {
                    var schemaProperty = property.FindPropertyRelative("inputSchema");
                    var bindingProperty = property.FindPropertyRelative("inputBinding");
                    var spriteProperty = property.FindPropertyRelative("sprite");

                    if (schemaProperty == null || bindingProperty == null || spriteProperty == null) {
                        EditorGUI.HelpBox(
                            SingleLine(position, 1),
                            "SpriteToSchema serialized fields do not match its property drawer.",
                            MessageType.Error);
                        EditorGUI.EndProperty();
                        return;
                    }

                    EditorGUI.PropertyField(SingleLine(position, 1), schemaProperty);
                    DrawControlSelector(SingleLine(position, 2), schemaProperty, bindingProperty);
                    EditorGUI.PropertyField(SingleLine(position, 3), spriteProperty);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded) {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight * (FieldCount + 1) +
                   EditorGUIUtility.standardVerticalSpacing * FieldCount;
        }

        public void Dispose() {
            foreach (var controlEditor in controlEditors.Values) {
                controlEditor.Dispose();
            }

            controlEditors.Clear();
        }

        private void DrawControlSelector(
            Rect position,
            SerializedProperty schemaProperty,
            SerializedProperty bindingProperty) {
            var pathProperty = bindingProperty.FindPropertyRelative("m_Path");
            if (pathProperty == null) {
                EditorGUI.HelpBox(position, "InputBinding does not expose a serialized control path.", MessageType.Error);
                return;
            }

            var editor = GetControlEditor(pathProperty, bindingProperty);
            editor.SetExpectedControlLayout("Button");
            editor.SetControlPathsToMatch(GetDevicePaths((InputSchema)schemaProperty.enumValueIndex));
            editor.OnGUI(
                position,
                EditorGUIUtility.TrTextContent("Button"),
                pathProperty,
                () => ApplyControlPath(bindingProperty));
        }

        private InputControlPathEditor GetControlEditor(
            SerializedProperty pathProperty,
            SerializedProperty bindingProperty) {
            var targetId = pathProperty.serializedObject.targetObject.GetInstanceID();
            var key = $"{targetId}:{pathProperty.propertyPath}";
            if (controlEditors.TryGetValue(key, out var editor)) {
                return editor;
            }

            editor = new InputControlPathEditor(
                pathProperty,
                new InputControlPickerState(),
                () => ApplyControlPath(bindingProperty),
                EditorGUIUtility.TrTextContent("Button"));
            controlEditors.Add(key, editor);
            return editor;
        }

        private static void ApplyControlPath(SerializedProperty bindingProperty) {
            Clear(bindingProperty.FindPropertyRelative("m_Name"));
            Clear(bindingProperty.FindPropertyRelative("m_Id"));
            Clear(bindingProperty.FindPropertyRelative("m_Interactions"));
            Clear(bindingProperty.FindPropertyRelative("m_Processors"));
            Clear(bindingProperty.FindPropertyRelative("m_Groups"));
            Clear(bindingProperty.FindPropertyRelative("m_Action"));
            bindingProperty.FindPropertyRelative("m_Flags").intValue = 0;
            bindingProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void Clear(SerializedProperty property) {
            property.stringValue = string.Empty;
        }

        private static IEnumerable<string> GetDevicePaths(InputSchema inputSchema) {
            return inputSchema switch {
                InputSchema.KeyboardAndMouse => new[] { "<Keyboard>", "<Mouse>" },
                InputSchema.Gamepad => new[] { "<Gamepad>" },
                InputSchema.Touch => new[] { "<Touchscreen>" },
                InputSchema.Joystick => new[] { "<Joystick>" },
                InputSchema.XR => new[] { "<XRController>" },
                _ => Array.Empty<string>()
            };
        }

        private static Rect SingleLine(Rect position, int line) {
            return new Rect(
                position.x,
                position.y + line * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing),
                position.width,
                EditorGUIUtility.singleLineHeight);
        }
    }
}
