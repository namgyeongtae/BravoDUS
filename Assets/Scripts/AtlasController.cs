using UnityEngine;
using UnityEngine.U2D;

public class AtlasController
{
    public static Sprite GetSprite(string name, string subName)
    {
        SpriteAtlas atlas = Managers.Resource.Load<SpriteAtlas>(name);

        if (atlas == null)
        {
            Debug.LogError($"SpriteAtlas not found: {name}");
            return null;
        }

        var sprite = atlas.GetSprite(subName);
        if (sprite == null)
            Debug.LogError($"Sprite not found: {subName}");

        return sprite;
    }
}
