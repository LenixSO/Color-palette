using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class VisualColorField<T> : BaseField<T> where T: Object
{
    private ObjectField objectField;
    private Label idLabel => objectField.labelElement;

    private int colorId;
    
    public VisualColorField() : this("VisualColorField"){}
    public VisualColorField(string label) : base(label, null)
    {
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexBasis = .5f;
        contentContainer.style.alignSelf = Align.Center;
        style.flexDirection = FlexDirection.Row;
            
        objectField = new ObjectField("0");
        objectField.objectType = typeof(T);
        objectField.RegisterCallback<ChangeEvent<Object>>(CallEvent);
        objectField.style.borderRightWidth = 20;
        objectField.style.maxWidth = 220;
        objectField.style.alignItems = Align.FlexEnd;
        objectField.style.flexGrow = 1;
        idLabel.SetBorder(1.5f, 2f, Color.black);
        idLabel.style.width = 20;
        idLabel.style.minWidth = 20;
        idLabel.style.unityTextAlign = TextAnchor.LowerCenter;
        contentContainer.Add(objectField);
    }

    public void SetReferenceColor(int id, Color color)
    {
        idLabel.text = id.ToString();
        idLabel.style.backgroundColor = color;
        float anchor = (1 - color.g) * 2 - (color.b + color.r);
        idLabel.style.color = ColorExtension.GrayShade(anchor);
    }

    private void CallEvent(ChangeEvent<Object> evt)
    {
        ChangeEvent<T> newEvt = ChangeEvent<T>.GetPooled((T)evt.previousValue, (T)evt.newValue);
        newEvt.target = this;
        SendEvent(newEvt);
    }
}
