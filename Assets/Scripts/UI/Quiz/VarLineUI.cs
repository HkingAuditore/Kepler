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
    g
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

    private bool _isInputting;

    [ExecuteInEditMode]
    private void Start()
    {
        Generate();
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
                            UpdateData(target.mass);
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
                            UpdateData(((QuizAstralBody) target).period);
                            // inputField.text = ((QuizAstralBody) target).period.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isPeriodPublic;
                            break;
                        case ShowPropertyType.radius:
                            UpdateData( ((QuizAstralBody) target).radius);
                            // inputField.text = ((QuizAstralBody) target).radius.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isRadiusPublic;
                            break;
                        case ShowPropertyType.omega:
                            UpdateData(((QuizAstralBody) target).globalAngularVelocity);
                            // inputField.text = ((QuizAstralBody) target).globalAngularVelocity.ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isAngularVelocityPublic;
                            break;
                        case ShowPropertyType.g:
                            UpdateData(((QuizAstralBody) target).gravity         * 0.01f);
                            // inputField.text = (((QuizAstralBody) target).gravity * 0.01f).ToString("f2");
                            toggle.isOn     = ((QuizAstralBody) target).isGravityPublic;
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
                    inputField.text = target.mass.ToString("f2");
                    break;
                case ShowPropertyType.v:
                    inputField.text = (.1f * target.GetVelocity().magnitude).ToString("f2");
                    break;
                case ShowPropertyType.R:
                    inputField.text = target.size.ToString("f2");
                    break;
                case ShowPropertyType.g:
                    inputField.text = (target.gravity *0.01f).ToString("f2");
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
                break;
            case ShowPropertyType.v:
                break;
            case ShowPropertyType.R:
                break;
            case ShowPropertyType.T:
                UpdateData(((QuizAstralBody) target).period);
                // inputField.text = ((QuizAstralBody) target).period.ToString("f2");
                break;
            case ShowPropertyType.radius:
                // inputField.text = ((QuizAstralBody) target).radius.ToString("f2");
                break;
            case ShowPropertyType.omega:
                UpdateData(((QuizAstralBody) target).globalAngularVelocity);
                // inputField.text = ((QuizAstralBody) target).globalAngularVelocity.ToString("f2");
                break;
            case ShowPropertyType.g:
                UpdateData(target.gravity         * 0.01f);
                // inputField.text = (target.gravity * 0.01f).ToString("f2");
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
                this.target.Mass = GetData();
                UpdateData(this.target.Mass);
                break;
            case ShowPropertyType.v:
                // 10
                this.target.ChangeVelocity(GetData());
                UpdateData(this.target.GetVelocity().magnitude);
                break;
            case ShowPropertyType.R:
                // 1
                this.target.size = GetData();
                UpdateData(this.target.size);
                break;
            case ShowPropertyType.T:
                break;
            case ShowPropertyType.radius:
                // 1
                this.target.GetTransform().position =
                    (this.target.GetPosition() - GameManager.GetGameManager.quizBase.target.GetPosition()).normalized * GetData();
                UpdateData( ((QuizAstralBody) target).radius);
                break;
            case ShowPropertyType.omega:
                break;
            case ShowPropertyType.g:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        int k = 1;
        switch (property)
        {
            case ShowPropertyType.m:
                k = 21;
                break;
            case ShowPropertyType.v:
                k = 3;
                break;
            case ShowPropertyType.R:
                k = 6;
                break;
            case ShowPropertyType.T:
                k = 4;
                break;
            case ShowPropertyType.radius:
                k = 6;
                break;
            case ShowPropertyType.omega:
                k = -4;
                break;
            case ShowPropertyType.g:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        this.inputField.text                   = data.GetMantissa().ToString("f2");
        this.scientificCountingInputField.text = (data.GetExponent() + k).ToString();
    }

    private float GetData()
    {
        int k = 1;
        switch (property)
        {
            case ShowPropertyType.m:
                k = 21;
                break;
            case ShowPropertyType.v:
                k = 3;
                break;
            case ShowPropertyType.R:
                k = 6;
                break;
            case ShowPropertyType.T:
                k = 4;
                break;
            case ShowPropertyType.radius:
                k = 6;
                break;
            case ShowPropertyType.omega:
                k = -4;
                break;
            case ShowPropertyType.g:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return float.Parse(this.inputField.text) * Mathf.Pow(10, int.Parse(this.scientificCountingInputField.text) - k);
    }
    
}