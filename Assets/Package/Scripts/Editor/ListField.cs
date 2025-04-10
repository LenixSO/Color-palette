using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using System;

public class ListField<T, TValue> : VisualElement where T : BaseField<TValue>
{
    private VisualElement content;
    private Button plusButton;
    private Button minusButton;

    private int count;
    private List<T> elementList = new();

    #region Preset
    //Style parameters
    public static readonly float elementSpacing = 2f;
    public static readonly Color bgColor = ColorExtension.GrayShade(.26f);
    public static readonly Color lighterColor = ColorExtension.GrayShade(.2f);
    public static readonly Color darkerColor = ColorExtension.GrayShade(.4f);
    public static readonly Color borderColor = ColorExtension.GrayShade(.1f);
    private static readonly float borderSize = 1.4f;
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

        IntegerField count = new("");
        count.style.width = 50;
        count.style.height = 18;
        count.style.position = Position.Absolute;
        count.style.alignSelf = Align.FlexEnd;

        content = new();
        content.style.backgroundColor = bgColor;
        content.style.paddingBottom = elementSpacing;
        content.SetBorder(borderSize, borderColor, borderRadius);

        VisualElement buttonsWindow = new();
        buttonsWindow.style.flexDirection = FlexDirection.Row;
        buttonsWindow.style.borderTopWidth = 0;
        buttonsWindow.style.marginRight = 15;
        buttonsWindow.style.width = 50;
        buttonsWindow.style.height = 22;
        buttonsWindow.style.backgroundColor = bgColor;
        buttonsWindow.SetBorder(borderSize, borderColor, borderRadius);
        buttonsWindow.style.alignSelf = Align.FlexEnd;

        plusButton = ListButtons(EditorGUIUtility.IconContent("Toolbar Plus").image);
        minusButton = ListButtons(EditorGUIUtility.IconContent("Toolbar Minus").image);

        buttonsWindow.Add(plusButton);
        buttonsWindow.Add(minusButton);

        foldout.Add(content);
        foldout.Add(buttonsWindow);
        header.Add(foldout);
        header.Add(count);

        Add(header);

        //Test
        content.Add(new ListElement<T, TValue>("testElement"));
        content.Add(new ListElement<T, TValue>("testElement"));
        content.Add(new ListElement<T, TValue>("testElement"));
    }
}

public class ListElement<T, TValue> : VisualElement where T : BaseField<TValue>
{
    private T element;

    public ListElement(string label = "ListElement")
    {
        style.flexDirection = FlexDirection.Row;
        style.paddingTop = ListField<T, TValue>.elementSpacing;
        style.paddingLeft = 5f;
        style.paddingRight = 5f;
        this.SetClickEffect(ListField<T, TValue>.bgColor, ListField<T, TValue>.lighterColor, ListField<T, TValue>.darkerColor);

        Button dragButton = new();

        Image img = new();
        img.image = EditorGUIUtility.IconContent("align_vertically_bottom").image;

        element = Activator.CreateInstance<T>();
        element.label = label;
        element.style.flexGrow = 1;

        Add(img);
        Add(element);
    }
}