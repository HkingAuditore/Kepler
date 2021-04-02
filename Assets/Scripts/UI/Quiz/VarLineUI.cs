﻿using System;
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
    [Header("UI Elements")] public Text             header;
    public                         Toggle           toggle;
    public                         Text             unit;
    public                         InputField       inputField;
    public                         bool             enableInput;
    public                         GameObject       add;
    public                         GameObject       minus;

    [Header("Texts")] public string headerString;
    public                   string unitString;

    [Header("Images")] public Sprite         editableImage;
    public                    Sprite         readOnlyImage;
    [SerializeField] private                  QuizAstralBody _target;
    public QuizAstralBody target
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
            rectTransform.sizeDelta = new Vector2(193f, rectTransform.sizeDelta.y);
            rectTransform.anchoredPosition =
                new Vector2(rectTransform.anchoredPosition.x + 17.9f, rectTransform.anchoredPosition.y);
        }
    }

    private void GenerateBasicInfo()
    {
        switch (property)
        {
            case ShowPropertyType.m:
                inputField.text = target.mass.ToString("f2");
                toggle.isOn     = target.isMassPublic;
                break;
            case ShowPropertyType.v:
                inputField.text = target.GetVelocity().magnitude.ToString("f2");
                toggle.isOn     = target.isVelocityPublic;
                break;
            case ShowPropertyType.R:
                inputField.text = target.size.ToString("f2");
                toggle.isOn     = target.isSizePublic;
                break;
            case ShowPropertyType.T:
                inputField.text = target.period.ToString("f2");
                toggle.isOn     = target.isPeriodPublic;
                break;
            case ShowPropertyType.radius:
                inputField.text = target.radius.ToString("f2");
                toggle.isOn     = target.isRadiusPublic;
                break;
            case ShowPropertyType.omega:
                inputField.text = target.globalAngularVelocity.ToString("f2");
                toggle.isOn     = target.isAngularVelocityPublic;
                break;
            case ShowPropertyType.g:
                inputField.text = target.gravity.ToString("f2");
                toggle.isOn     = target.isGravityPublic;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                inputField.text = target.period.ToString("f2");
                break;
            case ShowPropertyType.radius:
                inputField.text = target.radius.ToString("f2");
                break;
            case ShowPropertyType.omega:
                inputField.text = target.globalAngularVelocity.ToString("f2");
                break;
            case ShowPropertyType.g:
                inputField.text = target.gravity.ToString("f2");
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
                target.isMassPublic = toggle.isOn;
                break;
            case ShowPropertyType.v:
                target.isVelocityPublic = toggle.isOn;
                break;
            case ShowPropertyType.R:
                target.isSizePublic = toggle.isOn;
                break;
            case ShowPropertyType.T:
                target.isPeriodPublic = toggle.isOn;
                break;
            case ShowPropertyType.radius:
                target.isRadiusPublic = toggle.isOn;
                break;
            case ShowPropertyType.omega:
                target.isAngularVelocityPublic = toggle.isOn;
                break;
            case ShowPropertyType.g:
                target.isGravityPublic = toggle.isOn;
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
                this.target.ChangeMass(float.Parse(inputField.text));
                break;
            case ShowPropertyType.v:
                this.target.ChangeVelocity(float.Parse(inputField.text));
                break;
            case ShowPropertyType.R:
                this.target.size = float.Parse(inputField.text);
                break;
            case ShowPropertyType.T:
                break;
            case ShowPropertyType.radius:
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
    
    
    
}