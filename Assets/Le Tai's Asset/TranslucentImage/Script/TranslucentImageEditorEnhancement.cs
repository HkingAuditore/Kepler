#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LeTai.Asset.TranslucentImage
{
[ExecuteInEditMode]
[AddComponentMenu("UI/Translucent Image", 2)]
public partial class TranslucentImage
{
    protected override void Reset()
    {
        base.Reset();
        color = Color.white;

        material = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Le Tai's Asset/TranslucentImage/Material/Default-Translucent.mat");
        vibrancy   = material.GetFloat(_vibrancyPropId);
        brightness = material.GetFloat(_brightnessPropId);
        flatten    = material.GetFloat(_flattenPropId);

        source = source ? source : FindObjectOfType<TranslucentImageSource>();

        PrepareShader();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        SetVerticesDirty();

        Update();
    }
}
}
#endif
