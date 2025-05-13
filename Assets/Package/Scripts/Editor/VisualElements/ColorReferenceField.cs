using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorReferenceField : BaseField<Color>
{
    private VisualElement content;
    private ColorField colorField;
    
    public override Color value
    {
        get => colorField.value;
        set => colorField.value = value;
    }

    public event Action onXClicked;

    public ColorReferenceField() : this("ColorField"){}
    public ColorReferenceField(string label) : base(label, null)
    {
        contentContainer.style.backgroundColor = ColorExtension.GrayShade(.2f);
        labelElement.style.backgroundColor = ColorExtension.GrayShade(.4f);
        content = new();
        content.style.backgroundColor = ColorExtension.GrayShade(.6f);
        content.style.flexDirection = FlexDirection.Row;
        //content.style.flexShrink = 1f;
        contentContainer.Add(content);
        style.flexDirection = FlexDirection.Row;

        Button b = new Button();
        b.style.width = 20;
        b.text = "X";
        b.clicked += XClicked;
        content.Add(b);
            
        colorField = new ColorField();
        colorField.value = Color.white;
        colorField.style.flexShrink = 1f;
        colorField.RegisterCallback<ChangeEvent<Color>>(ColorChanged);
        content.Add(colorField);
    }

    private void XClicked()
    {
        onXClicked?.Invoke();
    }

    private void ColorChanged(ChangeEvent<Color> evt)
    {
        
    }
}
