using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TextGradient : BaseMeshEffect
{
    /// <summary>
    /// グラジエント方向
    /// </summary>
    private enum GradientDirectionType { Horizontal, Vertical }
    /// <summary>
    /// グラジエント·エフェクト方向
    /// </summary>
    private enum TextEffectDirectionType { Increase, Decrease }
    /// <summary>
    /// グラジエント方向（基本値）
    /// </summary>
    [SerializeField]
    private GradientDirectionType gradientDirectionType = GradientDirectionType.Horizontal;
    /// <summary>
    /// グラジエント·エフェクト方向（基本値）
    /// </summary>
    [SerializeField]
    private TextEffectDirectionType textEffectDirectionType = TextEffectDirectionType.Increase;
    /// <summary>
    /// グラジエント·エフェクト（基本値）
    /// </summary>
    [SerializeField]
    private bool textWave = false;
    /// <summary>
    /// グラジエント·エフェクトの速度（基本値）
    /// </summary>
    [SerializeField]
    private float waveSpeed = 2;
    /// <summary>
    /// グラジエント色相値
    /// </summary>
    [SerializeField]
    private Gradient gradient = null;
    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField]
    private Text text = null;

    /// <summary>
    /// テキストVertices最小値
    /// </summary>
    private float min = 0;
    /// <summary>
    /// テキストVertices最大値
    /// </summary>
    private float max = 0;
    /// <summary>
    /// Verticesのposition値
    /// </summary>
    private float value = 0;
    /// <summary>
    /// グラジエント·エフェクトの速度
    /// </summary>
    private float gradientWaveTime = 0;

    /// <summary>
    /// verticesリスト
    /// </summary>
    List<UIVertex> vertices = new List<UIVertex>();

    private void Update()
    {
        // エフェクト更新
        if (this.textWave == true)
        {
            this.gradientWaveTime += this.waveSpeed * Time.deltaTime;
            this.text.FontTextureChanged();
        }
    }
    
    public override void ModifyMesh(VertexHelper vh)
    {
        // Textの文字のメッシュを取得
        vh.GetUIVertexStream(this.vertices);
        // 縦横チェック
        this.DirectionCheck(this.vertices);


        for (int i = 0; i < this.vertices.Count; i++)
        {
            var v = this.vertices[i];

            //グラジエント方向
            if(this.gradientDirectionType == GradientDirectionType.Horizontal)
                this.value = v.position.x;
            else
                this.value = v.position.y;

            float curXNormalized = Mathf.InverseLerp(this.min, this.max, this.value);

            // グラジエント·エフェクト方向
            if(this.textEffectDirectionType == TextEffectDirectionType.Increase)
                curXNormalized = Mathf.PingPong(curXNormalized - this.gradientWaveTime, 1f);
            else
                curXNormalized = Mathf.PingPong(curXNormalized + this.gradientWaveTime, 1f);

            // 色相値セット
            Color c = gradient.Evaluate(curXNormalized);
            v.color = new Color(c.r, c.g, c.b ,1);

            // 修正されたverticesセット
            vertices[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }

    /// <summary>
    /// 縦横チェック
    /// </summary>
    public void DirectionCheck(List<UIVertex> vertices)
    {
        // 横の場合
        if(this.gradientDirectionType == GradientDirectionType.Horizontal)
        {
            this.min = vertices.Min(x => x.position.x);
            this.max = vertices.Max(x => x.position.x);
        }
        // 縦の場合
        else if(this.gradientDirectionType == GradientDirectionType.Vertical)
        {
            this.min = vertices.Min(x => x.position.y);
            this.max = vertices.Max(x => x.position.y);
        }
    }
}