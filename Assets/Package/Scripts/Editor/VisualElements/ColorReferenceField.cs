using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorReferenceField : BaseField<Color>
{
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
        style.flexDirection = FlexDirection.Row;

        Button b = new Button();
        b.style.width = 20;
        b.text = "X";
        b.clicked += XClicked;
        Add(b);
            
        colorField = new ColorField();
        colorField.value = Color.white;
        colorField.RegisterCallback<ChangeEvent<Color>>(ColorChanged);
        Add(colorField);
    }

    private void XClicked()
    {
        onXClicked?.Invoke();
    }

    private void ColorChanged(ChangeEvent<Color> evt)
    {
        
    }
}
