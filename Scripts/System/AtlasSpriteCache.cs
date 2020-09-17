using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// アトラスのスプライトをキャッシュするやつ
/// </summary>
[Serializable]
public class AtlasSpriteCache
{
    /// <summary>
    /// アトラス
    /// </summary>
    [SerializeField]
    private SpriteAtlas atlas = null;

    /// <summary>
    /// スプライトキャッシュ先
    /// </summary>
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    /// <summary>
    /// construct
    /// </summary>
    public AtlasSpriteCache()
    {
    }

    /// <summary>
    /// construct
    /// </summary>
    public AtlasSpriteCache(SpriteAtlas atlas)
    {
        this.atlas = atlas;
    }

    /// <summary>
    /// スプライト取得
    /// </summary>
    public Sprite GetSprite(string spriteName)
    {
        if (!this.spriteCache.ContainsKey(spriteName))
        {
            var sprite = this.atlas.GetSprite(spriteName);
            this.spriteCache.Add(spriteName, sprite);
        }
        return this.spriteCache[spriteName];
    }
}
