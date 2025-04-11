using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using System;

public class ListField<T, TValue> : VisualElement where T : BaseField<TValue>
{
    private Label emptyList;
    private VisualElement content;
    private Button plusButton;
    private Button minusButton;

    private List<ListElement<T, TValue>> elementList = new();
    private IntegerField countField;
    private List<ListElement<T, TValue>> selectedElements = new();

    public int count => elementList.Count;

    #region Preset
    //Style parameters
    public static readonly float elementSpacing = 2f;
    public static readonly Color bgColor = ColorExtension.GrayShade(.26f);
    public static readonly Color lighterColor = ColorExtension.GrayShade(.2f);
    public static readonly Color darkerColor = ColorExtension.GrayShade(.4f);
    public static readonly Color borderColor = ColorExtension.GrayShade(.1f);
    private static readonly float borderSize = 1.1f;
    private static readonly float borderRadius = 3f;
    private static readonly float buttonPaddingHorizontal = 4f;
    private static readonly float buttonPaddingVertical = 2f;

    private static Button ListButtons(Texture image)
    {
        Button button = new();
        button.style.flexGrow = 1;
        button.SetBorder(0, default);
        button.SetMargin(0);
        button.SetPadding(buttonPaddingHorizontal);
        button.style.paddingTop = buttonPaddingVertical;
        button.style.paddingBottom = buttonPaddingVertical;
        button.SetClickEffect(bgColor, darkerColor, lighterColor, TrickleDown.TrickleDown);
        Image iconPlus = new();
        iconPlus.image = image;
        iconPlus.style.flexGrow = 1;
        button.Add(iconPlus);

        return button;
    }

    #endregion

    public ListField(string label = "ListField")
    {
        VisualElement header = new();
        header.style.flexDirection = FlexDirection.Column;

        Foldout foldout = new();
        foldout.text = label;
        foldout.style.flexGrow = 1;

        countField = new("");
        countField.style.width = 50;
        countField.style.height = 18;
        countField.style.position = Position.Absolute;
        countField.style.alignSelf = Align.FlexEnd;
        countField.RegisterCallback<NavigationSubmitEvent>(ElementCountChanged);

        content = new();
        content.style.backgroundColor = bgColor;
        content.style.paddingBottom = elementSpacing;
        content.SetBorder(borderSize, borderRadius, borderColor);

        VisualElement buttonsWindow = new();
        buttonsWindow.SetBorder(borderSize, borderRadius, borderColor);
        buttonsWindow.style.flexDirection = FlexDirection.Row;
        buttonsWindow.style.borderTopWidth = 0;
        buttonsWindow.style.marginRight = 15;
        buttonsWindow.style.width = 55;
        buttonsWindow.style.height = 22;
        buttonsWindow.style.backgroundColor = bgColor;
        buttonsWindow.style.alignSelf = Align.FlexEnd;

        plusButton = ListButtons(EditorGUIUtility.IconContent("Toolbar Plus").image);
        plusButton.clicked += AddNewElement;
        minusButton = ListButtons(EditorGUIUtility.IconContent("Toolbar Minus").image);
        minusButton.clicked += RemoveElement;

        buttonsWindow.Add(plusButton);
        buttonsWindow.Add(minusButton);

        emptyList = new("List is empty");
        emptyList.SetMargin(5);
        emptyList.style.marginBottom = 2;
        content.Add(emptyList);
        
        foldout.Add(content);
        foldout.Add(buttonsWindow);
        header.Add(foldout);
        header.Add(countField);

        Add(header);
    }

    private void ElementCountChanged(NavigationSubmitEvent evt)
    {
        int difference = countField.value - count;

        Debug.Log($"cound changed by {difference}");
    }

    private void AddNewElement()
    {
        AddElement();
    }

    private void RemoveElement()
    {
        if(selectedElements.Count <= 0)
        {
            if(elementList.Count > 0)
            {
                var element = elementList[^1];
                element.UnregisterCallback<ChangeEvent<TValue>>(ElementValueChanged);
                elementList.Remove(element);
                content.Remove(element);
                UpdateListData();
            }
            return;
        }

        //remove selected elements
    }

    public void AddElement(TValue value = default)
    {
        ListElement<T, TValue> element = new($"Element {count}");
        element.field.SetValueWithoutNotify(value);
        element.RegisterCallback<ChangeEvent<TValue>>(ElementValueChanged);
        content.Add(element);
        elementList.Add(element);
        UpdateListData();
    }

    private void ElementValueChanged(ChangeEvent<TValue> evt)
    {
        UpdateListData();
    }

    private void UpdateListData()
    {
        countField.SetValueWithoutNotify(count);
        emptyList.style.display = count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        //call change event
        var evt = ChangeEvent<CollectionChange>.GetPooled();
        evt.target = this;
        SendEvent(evt);
    }

    public TValue ValueAt(int id)
    {
        if (id < 0 || id >= count) return default;
        return elementList[id].Value;
    }
}

public class ListElement<T, TValue> : VisualElement where T : BaseField<TValue>
{
    public T field {  get; private set; }

    public TValue Value
    {
        get => field.value;
        set => field.value = value;
    }

    public ListElement(string label = "ListElement", TValue value = default)
    {
        style.flexDirection = FlexDirection.Row;
        style.marginTop = ListField<T, TValue>.elementSpacing;
        style.paddingLeft = 5f;
        style.paddingRight = 5f;
        this.SetClickEffect(ListField<T, TValue>.bgColor, ListField<T, TValue>.lighterColor, ListField<T, TValue>.darkerColor);

        Button dragButton = new();

        Image img = new();
        img.image = EditorGUIUtility.IconContent("align_vertically_bottom").image;

        field = Activator.CreateInstance<T>();
        field.label = label;
        field.style.flexGrow = 1;
        field.SetValueWithoutNotify(value);

        Add(img);
        Add(field);
    }
}

public class CollectionChange
{
    
}