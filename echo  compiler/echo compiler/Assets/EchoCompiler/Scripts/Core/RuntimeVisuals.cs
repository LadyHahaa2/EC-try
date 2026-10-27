using UnityEngine;

namespace EchoCompiler
{
    public static class RuntimeVisuals
    {
        private static Sprite squareSprite;
        private static Material neonMaterial;

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                    squareSprite.name = "GeneratedSquareSprite";
                }
                return squareSprite;
            }
        }

        public static Material NeonMaterial
        {
            get
            {
                if (neonMaterial == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null) neonMaterial = new Material(shader) { name = "GeneratedNeonMaterial" };
                }
                return neonMaterial;
            }
        }

        public static GameObject CreateBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = size;
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = SquareSprite;
            sprite.color = color;
            sprite.sortingOrder = sortingOrder;
            return go;
        }

        public static GameObject CreateColliderBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool trigger = false, int sortingOrder = 0)
        {
            var go = CreateBlock(parent, name, position, size, color, sortingOrder);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = trigger;
            return go;
        }

        public static void AddOutline(LineRenderer line, Vector2 size, Color color)
        {
            if (line == null) return;
            line.material = NeonMaterial;
            line.positionCount = 5;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
            line.startColor = color;
            line.endColor = color;
            var x = size.x * 0.5f;
            var y = size.y * 0.5f;
            line.SetPosition(0, new Vector3(-x, -y, 0f));
            line.SetPosition(1, new Vector3(x, -y, 0f));
            line.SetPosition(2, new Vector3(x, y, 0f));
            line.SetPosition(3, new Vector3(-x, y, 0f));
            line.SetPosition(4, new Vector3(-x, -y, 0f));
        }
    }
}
