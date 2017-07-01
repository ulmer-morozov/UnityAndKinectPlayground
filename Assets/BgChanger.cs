using System;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class BgChanger : MonoBehaviour
    {
        public int CurrentTexture;

        public Texture2D Texture0;
        public Texture2D Texture1;
        public Texture2D Texture2;
        public Texture2D Texture3;
        public Texture2D Texture4;
        public Texture2D Texture5;
        public Texture2D Texture6;
        public Texture2D Texture7;
        public Texture2D Texture8;
        public Texture2D Texture9;

        private Texture2D[] _textures;

        public void Awake()
        {
            _textures = new[]
                {
                    Texture0,
                    Texture1,
                    Texture2,
                    Texture3,
                    Texture4,
                    Texture5,
                    Texture6,
                    Texture7,
                    Texture8,
                    Texture9
                }
                .Where(x => x != null)
                .ToArray();
        }

        public void Start()
        {
            SetTexture(CurrentTexture);
        }

        private void SetTexture(Texture texture)
        {
            if (texture == null)
            {
                Debug.LogError("Не удалось выставить текстуру, так как он null");
                return;
            }

            var objRenderer = GetComponent<Renderer>();
            if (objRenderer == null)
            {
                Debug.LogError("Не удалось выставить текстуру, так Render is null");
                return;
            }

            objRenderer.material.mainTexture = texture;
        }

        private void SetTexture(int texturePos)
        {
            if (_textures.Length == 0)
            {
                Debug.Log("нет выставленных текстур для чейнджера");
                return;
            }

            if (texturePos >= _textures.Length || CurrentTexture < 0)
                texturePos = 0;

            var texture = _textures[texturePos];
            CurrentTexture = texturePos;
            SetTexture(texture);
        }

        public void Update()
        {
            if (_textures.Length != 0 && Input.GetKeyDown("space"))
            {
                CurrentTexture++;
                CurrentTexture = CurrentTexture % _textures.Length;
                SetTexture(CurrentTexture);
            }
        }
    }
}
