using UnityEngine;

public class EmissiveOscillator : MonoBehaviour
{
    private Material emissiveMaterial;

    private MeshRenderer emissiveRenderer;

    private void Start()
    {
        emissiveRenderer = GetComponent<MeshRenderer>();
        emissiveMaterial = emissiveRenderer.material;
    }

    private void Update()
    {
        var c = Color.Lerp(
                           Color.white, Color.black,
                           Mathf.Sin(Time.time * Mathf.PI) * 0.5f + 0.5f
                          );
        emissiveMaterial.SetColor("_Emission", c);
//		emissiveRenderer.UpdateGIMaterials();
        DynamicGI.SetEmissive(emissiveRenderer, c);
    }
}