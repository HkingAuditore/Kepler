using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;

public class LengthUI : MonoBehaviour
{
    public SplineComputer lengthSpline;
    public LengthCalculator lengthCalculator;
    public Text lengthText;
    public AstralBody astralBody;

    private void Update()
    {
        ShowLength();
    }


    private void OnEnable()
    {
        InitLength();
    }

    private void OnDisable()
    {
        RemoveLength();
    }

    private void InitLength()
    {
        var nodeOri = astralBody.gameObject.AddComponent<Node>();
        nodeOri.transformNormals = false;
        nodeOri.transformSize = false;
        nodeOri.transformTangents = false;
        var nodeTarget = astralBody.affectedPlanets[0].gameObject.AddComponent<Node>();
        nodeTarget.transformNormals = false;
        nodeTarget.transformSize = false;
        nodeTarget.transformTangents = false;
        lengthSpline.SetPoints(new[]
                               {
                                   new SplinePoint(new Vector3(0, 0, 0)),
                                   new SplinePoint(new Vector3(0, 0, 0))
                               });
        nodeOri.AddConnection(lengthSpline, 0);
        Debug.Log(nodeOri.HasConnection(lengthSpline, 0));
        nodeTarget.AddConnection(lengthSpline, 1);
    }

    private void RemoveLength()
    {
        lengthSpline.SetPoints(new SplinePoint[] { });
        Destroy(astralBody.gameObject.GetComponent<Node>());
        Destroy(astralBody.affectedPlanets[0].gameObject.GetComponent<Node>());
    }

    public void ShowLength()
    {
        lengthText.text = "距离:" + lengthCalculator.length.ToString("f2") + " m";
    }
}