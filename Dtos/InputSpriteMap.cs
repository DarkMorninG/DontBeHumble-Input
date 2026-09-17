using System;
using System.Collections.Generic;
using DBH.Input.Controller;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DBH.Input.Dtos {
    [CreateAssetMenu(fileName = "InputSpriteMap", menuName = "Input/Sprite Map", order = 0)]
    public class InputSpriteMap : ScriptableObject {
        [SerializeField]
        private List<SpriteToSchema> spriteToSchemata;

        public List<SpriteToSchema> SpriteToSchemata => spriteToSchemata;
    }


    [Serializable]
    public class SpriteToSchema {
        [SerializeField]
        private InputSchema inputSchema;

        [SerializeField]
        private InputBinding inputBinding;

        [SerializeField]
        private Sprite sprite;

        public InputSchema InputSchema => inputSchema;

        public InputBinding InputBinding => inputBinding;

        public Sprite Sprite => sprite;
    }
}