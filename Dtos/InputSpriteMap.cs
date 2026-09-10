using System;
using System.Collections.Generic;
using DBH.Input.Attributes;
using DBH.Input.Controller;
using UnityEngine;

namespace DBH.Input.Dtos {
    [CreateAssetMenu(fileName = "InputSpriteMap", menuName = "Input/Sprite Map", order = 0)]
    public class InputSpriteMap : ScriptableObject {
        [SerializeField]
        private List<SpriteToLayout> spriteLayout;

        public List<SpriteToLayout> SpriteLayout => spriteLayout;
    }

    [Serializable]
    public class SpriteToLayout {
        [SerializeField]
        [InputActionPath]
        private string inputPath;

        [SerializeField]
        private List<SpriteToSchema> spriteToSchemata;

        public string InputPath => inputPath;

        public List<SpriteToSchema> SpriteToSchemata => spriteToSchemata;
    }

    [Serializable]
    public class SpriteToSchema {
        [SerializeField]
        private InputSchema inputSchema;

        [SerializeField]
        private Sprite sprite;

        public InputSchema InputSchema => inputSchema;

        public Sprite Sprite => sprite;
    }
}
