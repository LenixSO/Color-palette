using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorReferenceField : BaseField<Color>
{
    private ColorField colorField;
    private Label countLabel => colorField.labelElement;

    private int count;

    public int ReferenceCount
    {
        get => count;
        set
        {
            count = value;
            colorField.label = count.ToString();
        }
    }
    
    public override Color value
    {
        get => colorField.value;
        set => colorField.value = value;
    }

    public ColorReferenceField() : this("ColorField"){}
    public ColorReferenceField(string label) : base(label, null)
    {
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexBasis = .5f;
        contentContainer.style.alignSelf = Align.Center;
        style.flexDirection = FlexDirection.Row;
            
        colorField = new ColorField("0");
        colorField.value = Color.white;
        colorField.style.borderRightWidth = 20;
        colorField.style.maxWidth = 220;
        colorField.style.alignItems = Align.FlexEnd;
        colorField.style.flexGrow = 1;
        countLabel.SetBorder(1.5f, 2f, Color.black);
        countLabel.style.backgroundColor = ColorExtension.GrayShade(.2f);
        countLabel.style.width = 20;
        countLabel.style.minWidth = 20;
        countLabel.style.unityTextAlign = TextAnchor.LowerCenter;
        contentContainer.Add(colorField);
    }
}
