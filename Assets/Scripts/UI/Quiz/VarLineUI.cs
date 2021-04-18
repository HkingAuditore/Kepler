using System;
using System.Collections;
using MathPlus;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public enum ShowPropertyType
{
    m,
    v,
    R,
    T,
    radius,
    omega,
    g,
    density
}

public class VarLineUI : MonoBehaviour
{

    public                         ShowPropertyType property;
    public                         bool             isQuiz;
    [Header("UI Elements")] public Text             header;
    public                         Toggle           toggle;
    public                         Text             unit;
    public                         InputField       inputField;
    public                         InputField       scientificCountingInputField;
    public                         bool             enableInput;
    public                         GameObject       add;
    public                         GameObject       minus;

    [Header("Texts")] public string headerString;
    public                   string unitString;

    [Header("Images")] public Sprite         editableImage;
    public                    Sprite         readOnlyImage;
    [SerializeField] private                  AstralBody _target;
    public AstralBody target
    {
        get => _target;
        set
        {
            _target                      =  value;
            if (this.property == ShowPropertyType.v)
            {
                _target.velocityChangedEvent.RemoveListener(this.OnVelocityChanged);
                _target.velocityChangedEvent.AddListener(this.OnVelocityChanged);

            }
            GenerateBasicInfo();
        }
    }

    private bool _isInputting = false;

    [ExecuteInEditMode]
    private void Start()
    {
        Generate();
        _isInputting = false;
    }

    [ContextMenu("快速应用")]
    public void Generate()
    {
        this.toggle.gameObject.SetActive(isQuiz);
        header.text                                                 = headerString;
        unit.text                                                   = unitString;
        inputField.readOnly                                         = !enableInput;
        inputField.interactable                                     = enableInput;
        inputField.gameObject.GetComponent<Image>().sprite          = enableInput ? editableImage : readOnlyImage;
        GenerateBasicInfo();
        add.SetActive(enableInput);
        minus.SetActive(enableInput);
        if (!enableInput)
        {
            Text text = inputField.placeholder.gameObject.GetComponent<Text>();
            text.text  = "请输入";
            var rectTransform = inputField.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(115f, rectTransform.sizeDelta.y);
            // rectTransform.anchoredPosition =
            //     new Vector2(rectTransform.anchoredPosition.x + 17.9f, rectTransform.anchoredPosition.y);
        }

        
    }

    private void GenerateBasicInfo()
    {
        if (isQuiz)
        {
                    switch (property)
                    {
                        case ShowPropertyType.m:
                            UpdateData((float)target.realMass);
                            // inputField.text = target.mass.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isMassPublic;
                            break;
                        case ShowPropertyType.v:
                            UpdateData(.1f         * target.GetVelocity().magnitude);
                            // inputField.text = (.1f * target.GetVelocity().magnitude).ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isVelocityPublic;
                            break;
                        case ShowPropertyType.R:
                            UpdateData(target.size);
                            // inputField.text = target.size.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isSizePublic;
                            break;
                        case ShowPropertyType.T:
                            Debug.Log("period:" + ((QuizAstralBody) target).period);
                            UpdateData(((QuizAstralBody) target).period);

                            // inputField.text = ((QuizAstralBody) target).period.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isPeriodPublic;
                            break;
                        case ShowPropertyType.radius:
                            UpdateData( ((QuizAstralBody) target).radius);
                            Debug.Log("radius:" + ((QuizAstralBody) target).radius);
                            // inputField.text = ((QuizAstralBody) target).radius.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isRadiusPublic;
                            break;
                        case ShowPropertyType.omega:
                            UpdateData(((QuizAstralBody) target).globalAngularVelocity);
                            // inputField.text = ((QuizAstralBody) target).globalAngularVelocity.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isAngularVelocityPublic;
                            break;
                        case ShowPropertyType.g:
                            UpdateData(((QuizAstralBody) target).gravity);
                            // inputField.text = (((QuizAstralBody) target).gravity * 0.01f).ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isGravityPublic;
                            break;
                        case ShowPropertyType.density:
                            UpdateData((float)((QuizAstralBody) target).density);
                            // inputField.text = (((QuizAstralBody) target).gravity * 0.01f).ToString("f2");
                            //TODO 
                            toggle.isOn = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

        }
        else
        {
            switch (property)
            {
                case ShowPropertyType.m:
                    inputField.text = target.Mass.ToString("f2");
                    break;
                case ShowPropertyType.v:
                    inputField.text = (.1f * target.GetVelocity().magnitude).ToString("f2");
                    break;
                case ShowPropertyType.R:
                    inputField.text = target.size.ToString("f2");
                    break;
                case ShowPropertyType.g:
                    inputField.text = (target.gravity).ToString("f2");
                    break;
                case ShowPropertyType.density:
                    inputField.text = (target.density).ToString("f2");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }


    private void Update()
    {
        switch (property)
        {
            case ShowPropertyType.m:
                // if(!_isClicking && !_isAdd)
                //     UpdateData((float)((QuizAstralBody) target).realMass);
                break;
            case ShowPropertyType.v:
                // if(!_isClicking && !_isAdd)
                //     UpdateData((float)((QuizAstralBody) target).GetVelocity().magnitude);
                break;
            case ShowPropertyType.R:
                // if(!_isClicking && !_isAdd)
                //     UpdateData((float)((QuizAstralBody) target).size);
                break;
            case ShowPropertyType.T:
                UpdateData(((QuizAstralBody) target).period);
                // inputField.text = ((QuizAstralBody) target).period.ToString("f2");
                break;
            case ShowPropertyType.radius:
                Debug.Log("radius log _isClicking:" + _isClicking);
                Debug.Log("radius log _isAdd:"      + _isAdd);
                Debug.Log("radius log _isInputting:" + _isInputting);
                if(!_isClicking && !_isAdd && !_isInputting)
                    UpdateData((float)((QuizAstralBody) target).radius);
                // inputField.text = ((QuizAstralBody) target).radius.ToString("f2");
                break;
            case ShowPropertyType.omega:
                UpdateData(((QuizAstralBody) target).globalAngularVelocity);
                // inputField.text = ((QuizAstralBody) target).globalAngularVelocity.ToString("f2");
                break;
            case ShowPropertyType.g:
                UpdateData(target.gravity);
                // inputField.text = (target.gravity * 0.01f).ToString("f2");
                break;
            case ShowPropertyType.density:
                // Debug.Log("density:" + target.density);
                UpdateData((float)target.density);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ChangePublicity()
    {
        switch (property)
        {
            case ShowPropertyType.m:
                ((QuizAstralBody) target).isMassPublic = toggle.isOn;
                break;
            case ShowPropertyType.v:
                ((QuizAstralBody) target).isVelocityPublic = toggle.isOn;
                break;
            case ShowPropertyType.R:
                ((QuizAstralBody) target).isSizePublic = toggle.isOn;
                break;
            case ShowPropertyType.T:
                ((QuizAstralBody) target).isPeriodPublic = toggle.isOn;
                break;
            case ShowPropertyType.radius:
                ((QuizAstralBody) target).isRadiusPublic = toggle.isOn;
                break;
            case ShowPropertyType.omega:
                ((QuizAstralBody) target).isAngularVelocityPublic = toggle.isOn;
                break;
            case ShowPropertyType.g:
                ((QuizAstralBody) target).isGravityPublic = toggle.isOn;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnEditEnd()
    {
        switch (property)
        {
            case ShowPropertyType.m:
                if(this.target.name =="Core")
                    GameManager.GetGameManager.CalculateMassScales(GetData());
                this.target.realMass = GetData();
                UpdateData((float)this.target.realMass);
                break;
            case ShowPropertyType.v:
                // 10
                this.target.ChangeVelocity(GetData());
                UpdateData(this.target.GetVelocity().magnitude);
                break;
            case ShowPropertyType.R:
                // 1
                this.target.size = (float)GetData() * Mathf.Pow(10,-this.GetK());
                UpdateData(this.target.size);
                break;
            case ShowPropertyType.T:
                break;
            case ShowPropertyType.radius:
                // 1
                float distance = (float)GetData() * Mathf.Pow(10, -this.GetK());
                this.target.GetTransform().position =
                    (this.target.GetPosition() - GameManager.GetGameManager.quizBase.target.GetPosition()).normalized *  distance ;
                UpdateData(distance);
                break;
            case ShowPropertyType.omega:
                break;
            case ShowPropertyType.g:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _isInputting = false;
    }

    private void OnVelocityChanged(Vector3 velocity)
    {
        Debug.Log("Called!");
        inputField.text = velocity.magnitude.ToString("f2");
    }


    private Vector3 _dragPos;
    public void OnInputFieldDrag()
    {
        _dragPos = Input.mousePosition;
    }
    public void WhileInputFieldDrag()
    {
        float cur = float.Parse(inputField.text);
        float step = 0.01f * cur;
        float tmp = cur + step * Vector3.Distance(Input.mousePosition, _dragPos) * Mathf.Sign(Input.mousePosition.y - _dragPos.y);
        tmp             = tmp > 0 ? tmp : 0;
        
        inputField.text = tmp.ToString("f2");
        OnEditEnd();
        _dragPos = Input.mousePosition;
    }


    private bool _isClicking = false;
    private bool _isAdd;
    public void WhileAddClicking(bool isAdd)
    {
        _isAdd      = isAdd;
        _isClicking = true;
    }

    private void FixedUpdate()
    {
        if(_isClicking)
        {
            Debug.Log("In");
            float cur  = float.Parse(inputField.text);
            float step = 0.01f * cur;
            float tmp  = cur + step * Time.fixedDeltaTime * (_isAdd ? 1 : -1);
            tmp = tmp > 0 ? tmp : 0;

            inputField.text = tmp.ToString("f2");
            OnEditEnd();
        }
    }


    public void OnClickEnd()
    {
        _isClicking = false;
    }

    private void UpdateData(float data)
    {
        int k = GetK();
        // int k = 0;
        this.inputField.text                   = data.GetMantissa().ToString("f2");
        this.scientificCountingInputField.text = (data.GetExponent() + k).ToString();
    }

    private int GetK()
    {
        int k;
        switch (property)
        {
            case ShowPropertyType.m:
                // k = 26 - GameManager.GetGameManager.globalMassScaler * 2;
                k = 0;
                break;
            case ShowPropertyType.v:
                k = 3;
                break;
            case ShowPropertyType.R:
                k = GameManager.GetGameManager.GetK(PropertyUnit.M);
                break;
            case ShowPropertyType.T:
                k = GameManager.GetGameManager.GetK(PropertyUnit.S);
                // k = 3  + GameManager.GetGameManager.globalMassScaler+  GameManager.GetGameManager.globalDistanceScaler * 3;
                break;
            case ShowPropertyType.radius:
                k = 1 + GameManager.GetGameManager.GetK(PropertyUnit.M);
                // k = 7 +  GameManager.GetGameManager.globalDistanceScaler * 2;
                break;
            case ShowPropertyType.omega:
                k = -4;
                break;
            case ShowPropertyType.g:
                k = 0;
                break;
            case ShowPropertyType.density:
                k = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return k;
    }

    private double GetData()
    {
        return double.Parse(this.inputField.text) * Mathf.Pow(10, int.Parse(this.scientificCountingInputField.text));
    }

    public void OnInputting()
    {
        _isInputting = true;
    }

    public void OnInputCancel()
    {
        _isInputting = false;
    }
    
}