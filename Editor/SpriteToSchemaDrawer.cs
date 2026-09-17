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

        private static readonly string[] KeyboardAndMousePaths = { "<Keyboard>", "<Mouse>" };
        private static readonly string[] GamepadPaths = { "<Gamepad>" };
        private static readonly string[] TouchPaths = { "<Touchscreen>" };
        private static readonly string[] JoystickPaths = { "<Joystick>" };
        private static readonly string[] XrPaths = { "<XRController>" };

        private readonly Dictionary<string, ControlEditorEntry> controlEditors = new();

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
            foreach (var entry in controlEditors.Values) {
                entry.Editor.Dispose();
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

            var schema = (InputSchema)schemaProperty.enumValueIndex;
            var entry = GetControlEditor(pathProperty, bindingProperty, schema);
            if (entry.Schema != schema) {
                entry.Editor.SetControlPathsToMatch(GetDevicePaths(schema));
                entry.Schema = schema;
            }

            entry.Editor.OnGUI(
                position,
                EditorGUIUtility.TrTextContent("Button"),
                pathProperty,
                () => ApplyControlPath(bindingProperty));
        }

        private ControlEditorEntry GetControlEditor(
            SerializedProperty pathProperty,
            SerializedProperty bindingProperty,
            InputSchema schema) {
            var targetId = pathProperty.serializedObject.targetObject.GetInstanceID();
            var key = $"{targetId}:{pathProperty.propertyPath}";
            if (controlEditors.TryGetValue(key, out var entry)) {
                return entry;
            }

            var editor = new InputControlPathEditor(
                pathProperty,
                new InputControlPickerState(),
                () => ApplyControlPath(bindingProperty),
                EditorGUIUtility.TrTextContent("Button"));
            editor.SetExpectedControlLayout("Button");
            editor.SetControlPathsToMatch(GetDevicePaths(schema));

            entry = new ControlEditorEntry(editor, schema);
            controlEditors.Add(key, entry);
            return entry;
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
                InputSchema.KeyboardAndMouse => KeyboardAndMousePaths,
                InputSchema.Gamepad => GamepadPaths,
                InputSchema.Touch => TouchPaths,
                InputSchema.Joystick => JoystickPaths,
                InputSchema.XR => XrPaths,
                _ => Array.Empty<string>()
            };
        }

        private sealed class ControlEditorEntry {
            public readonly InputControlPathEditor Editor;
            public InputSchema Schema;

            public ControlEditorEntry(InputControlPathEditor editor, InputSchema schema) {
                Editor = editor;
                Schema = schema;
            }
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
