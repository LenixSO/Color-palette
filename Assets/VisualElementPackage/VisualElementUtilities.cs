using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementUtilities
{
    public static void SetBorder(this VisualElement element, float thickness, Color? color = null, float radius = 0)
    {
        element.style.borderTopWidth = thickness;
        element.style.borderBottomWidth = thickness;
        element.style.borderLeftWidth = thickness;
        element.style.borderRightWidth = thickness;

        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
        element.style.borderBottomRightRadius = radius;

        Color borderColor = color ?? Color.white;
        element.style.borderTopColor = borderColor;
        element.style.borderBottomColor = borderColor;
        element.style.borderLeftColor = borderColor;
        element.style.borderRightColor = borderColor;
    }

    public static void SetMargin(this VisualElement element, float thickness)
    {
        element.style.marginTop = thickness;
        element.style.marginBottom = thickness;
        element.style.marginLeft = thickness;
        element.style.marginRight = thickness;
    }
    public static void SetPadding(this VisualElement element, float thickness)
    {
        element.style.paddingTop = thickness;
        element.style.paddingBottom = thickness;
        element.style.paddingLeft = thickness;
        element.style.paddingRight = thickness;
    }
    public static void SetClickEffect(this VisualElement element, Color defaultColor, Color? hoverColor = null, Color? clickColor = null, 
        TrickleDown trickleDownClick = TrickleDown.NoTrickleDown)
    {
        Color normalColor = defaultColor;
        Color highlightColor = hoverColor ?? defaultColor;
        Color downColor = clickColor ?? defaultColor;

        element.style.backgroundColor = normalColor;

        element.RegisterCallback<MouseEnterEvent>((evt) =>
        {
            //Debug.Log("mouse enter");
            element.style.backgroundColor = highlightColor;
        });
        element.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            //Debug.Log($"mouse leave: {evt.pressedButtons}");
            bool holdingButton = evt.pressedButtons > 0;
            if (holdingButton) return;
            element.style.backgroundColor = normalColor;
        });


        element.RegisterCallback<MouseDownEvent>((evt) =>
        {
            element.CaptureMouse();
            element.style.backgroundColor = downColor;
            //Debug.Log("mouse down");
        },trickleDownClick);
        element.RegisterCallback<MouseUpEvent>((evt) =>
        {
            element.ReleaseMouse();
            bool mouseOver = element.ContainsPoint(evt.localMousePosition);
            element.style.backgroundColor = mouseOver ? highlightColor : normalColor;
            //Debug.Log($"mouse up: {element.ContainsPoint(evt.localMousePosition)}");
        });
    }
}
