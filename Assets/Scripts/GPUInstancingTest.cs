using UnityEngine;

public class GPUInstancingTest : MonoBehaviour
{
    public int       instances = 5000;
    public Transform prefab;

    public float radius = 50f;

    private void Start()
    {
        var properties = new MaterialPropertyBlock();
        for (var i = 0; i < instances; i++)
        {
            var t = Instantiate(prefab);
            t.localPosition = Random.insideUnitSphere * radius;
            t.SetParent(transform);

            properties.SetColor(
                                "_Color", new Color(Random.value, Random.value, Random.value)
                               );

            var r = t.GetComponent<MeshRenderer>();
            if (r)
                r.SetPropertyBlock(properties);
            else
                for (var ci = 0; ci < t.childCount; ci++)
                {
                    r = t.GetChild(ci).GetComponent<MeshRenderer>();
                    if (r) r.SetPropertyBlock(properties);
                }
        }
    }
}