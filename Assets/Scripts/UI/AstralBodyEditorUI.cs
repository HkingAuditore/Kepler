using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyEditorUI : MonoBehaviour
{
    public bool isEnableEdit = true;
    public AstralBody astralBody;
    public GravityTracing gravityTracing;

    public Slider massSlider;
    public PositionEditorUI positionEditorUI;
    public VelocityEditorUI velocityEditorUI;
    public GameObject normalPanel;
    public VectorUI forceUI;
    public VectorUI velocityUI;
    public LengthUI lengthUI;
    public OrbitPanelUI orbitPanelUI;
    private Text _massText;


    private void Awake()
    {
        _massText = massSlider.transform.parent.Find("MassText").GetComponent<Text>();
    }

    private void OnEnable()
    {
        forceUI.astralBody = astralBody;
        forceUI.gameObject.SetActive(true);
        velocityUI.astralBody = astralBody;
        velocityUI.gameObject.SetActive(true);
        if (isEnableEdit)
        {
            positionEditorUI.editingTarget = astralBody.transform;
            positionEditorUI.gameObject.SetActive(true);
            normalPanel.SetActive(false);
            InitMassEditor();
            lengthUI.astralBody = astralBody;
            lengthUI.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        forceUI.gameObject.SetActive(false);
        velocityUI.gameObject.SetActive(false);
        if (isEnableEdit)
        {
            positionEditorUI.gameObject.SetActive(false);
            positionEditorUI.editingTarget = null;
            normalPanel.SetActive(true);
            lengthUI.gameObject.SetActive(false);

        }

    }

    private void InitMassEditor()
    {
        _massText.text = "";
        Debug.Log(astralBody.mass);
        var tmpMass = astralBody.mass;
        massSlider.minValue = (int) (tmpMass / 10);
        massSlider.maxValue = (int) (tmpMass * 10);
        massSlider.value = tmpMass;

        _massText.text = astralBody.Mass + " kg";
    }

    public void EditMass()
    {
        astralBody.Mass = massSlider.value;
        _massText.text = astralBody.Mass + " kg";
    }

    public void EditVelocity()
    {
        velocityEditorUI.editingTarget = astralBody;
        velocityEditorUI.gameObject.SetActive(true);
    }

    public void GetConicSection()
    {
        var result = gravityTracing.GetConicSection(astralBody, 500);
        orbitPanelUI.orbit = result;
        orbitPanelUI.astralBody = astralBody;
        Debug.Log("曲线为：" + result);
        Debug.Log("长轴为：" + result.semiMajorAxis);
        Debug.Log("短轴为：" + result.semiMinorAxis);
        Debug.Log("几何中心为：" + result.geoCenter);
        Debug.Log("离心率为：" + result.eccentricity);
        Debug.Log("焦距为：" + result.focalLength);
        orbitPanelUI.gameObject.SetActive(true);
        orbitPanelUI.Init();
        // Debug.DrawLine(this.astralBody.AffectedPlanets[0].transform.position,
        //                this.astralBody.AffectedPlanets[0].transform.position + new Vector3(
        //                                                                                 1 * Mathf.Cos(result.angle* Mathf.Deg2Rad),
        //                                                                                 0,
        //                                                                                 1 * Mathf.Sin(result.angle* Mathf.Deg2Rad)) * 1000f,
        //               Color.magenta,
        //               1000f
        //               );

        gravityTracing.DrawMathOrbit(result, 20);
    }
}