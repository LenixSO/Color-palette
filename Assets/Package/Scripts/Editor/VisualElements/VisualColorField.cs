using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class VisualColorField<T> : BaseField<T> where T: Object
{
    private ObjectField objectField;
    public Button idLabel { get; private set; }

    private int colorId;

    public Action<VisualColorField<T>> onColorClicked;
    public Action<int, T> onReferenceChanged;

    public override T value
    {
        get => (T)objectField.value;
        set => objectField.value = value;
    }
    
    public VisualColorField() : this("VisualColorField"){}
    public VisualColorField(string label) : base(label, null)
    {
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexBasis = .5f;
        contentContainer.style.alignSelf = Align.Center;
        style.flexDirection = FlexDirection.Row;
            
        objectField = new ObjectField();
        objectField.objectType = typeof(T);
        objectField.RegisterCallback<ChangeEvent<Object>>(CallEvent);
        objectField.style.borderRightWidth = 20;
        objectField.style.maxWidth = 220;
        objectField.style.alignItems = Align.FlexEnd;
        objectField.style.flexGrow = 1;
        idLabel = new Button(IdClicked);
        idLabel.text = "0";
        idLabel.SetBorder(1.5f, 2f, Color.black);
        idLabel.style.width = 22;
        idLabel.style.minWidth = 22;
        idLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        contentContainer.Insert(1,idLabel);
        contentContainer.Add(objectField);
    }

    private void IdClicked()
    {
        onColorClicked?.Invoke(this);
    }

    public void SetReferenceColor(int id, Color color)
    {
        colorId = id;
        idLabel.text = $"<b>{id}";
        idLabel.style.backgroundColor = color;
        idLabel.style.color = ColorExtension.ContrastGray(color);
        onReferenceChanged?.Invoke(colorId, (T)objectField.value);
    }

    private void CallEvent(ChangeEvent<Object> evt)
    {
        ChangeEvent<T> newEvt = ChangeEvent<T>.GetPooled((T)evt.previousValue, (T)evt.newValue);
        newEvt.target = this;
        SendEvent(newEvt);
    }

    public void SetInteractable(bool value)
    {
        objectField.SetEnabled(value);
    }
}
