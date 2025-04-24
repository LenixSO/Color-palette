using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Xml.Linq;

public class ListField<T, TValue> : VisualElement where T : BaseField<TValue>
{
    private Label emptyList;
    private VisualElement content;
    private Button plusButton;
    private Button minusButton;

    private float contentSize => (elementSize * Mathf.Max(count, 1)) + elementSpacing * 4;

    private List<ListElement<T, TValue>> elementList = new();
    private IntegerField countField;
    private List<ListElement<T, TValue>> selectedElements = new();

    public int count => elementList.Count;
    public new T this[int id] => elementList[id].field;

    #region Preset
    //Style parameters
    private readonly float elementSize = 25;
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

    private void SetupDragDrop(ListElement<T, TValue> element)
    {
        bool dragging = false;
        float totalArea = (element.localBound.height * (elementList.Count - 1)) + (element.localBound.height / 2f);
        Vector3 initialPosition = Vector3.zero;

        int originIndex = 0;
        int currentIndex = 0;
        int indexOffset = 0;//if already shifted up or down
        float shiftOffset = .75f;
        float shiftAnimationTime = .05f;

        element.RegisterCallback<PointerDownEvent>((evt) =>
        {
            //setup values
            totalArea = (element.localBound.height * (elementList.Count - 1)) + (element.localBound.height / 2f);
            initialPosition = element.transform.position;
            indexOffset = 0;
            originIndex = elementList.IndexOf(element);
            currentIndex = originIndex;
            dragging = true;

            //alter element
            element.BringToFront();
            element.CapturePointer(evt.pointerId);
            for (int i = 0; i < elementList.Count; i++)
            {
                elementList[i].pickingMode = PickingMode.Ignore;
            }
        });

        element.RegisterCallback<PointerMoveEvent>((evt) =>
        {
            if (!dragging) return;

            //set position
            float finalYPosition = Mathf.Clamp(evt.deltaPosition.y + element.transform.position.y, 0, totalArea);
            Vector2 position = element.transform.position;
            position.y = finalYPosition;
            element.transform.position = position;

            //mshiftAnimationTimeove other elements out of the way
            Vector3 indexPosition = ElementPosition(currentIndex);
            float distanceFromOrigin = position.y - indexPosition.y;
            if (distanceFromOrigin > elementSize * shiftOffset && currentIndex < count - 1)
            {
                //element below is 1 index above this
                //Debug.Log("Past the element below");

                ShiftElements(1);
            }
            else if (distanceFromOrigin < -(elementSize * shiftOffset) && currentIndex > 0)
            {
                //element above is 1 index below this
                //Debug.Log("Past the element above");

                ShiftElements(-1);
            }

            void ShiftElements(int offset)
            {
                //move element
                int index = currentIndex + offset;
                if (indexOffset != (int)Mathf.Sign((offset))) index += indexOffset;//use only if going back to origin
                elementList[index].MoveTowards(indexPosition, shiftAnimationTime);

                //updateId
                currentIndex += offset;
                indexOffset = Mathf.Clamp(currentIndex - originIndex, -1, 1);
            }
        });

        element.RegisterCallback<PointerUpEvent>((evt) =>
        {
            if (!dragging) return;
            Vector3 finalPosition = ElementPosition(currentIndex);
            element.transform.position = finalPosition;

            //adjust indexes
            indexOffset = Mathf.Clamp(currentIndex - originIndex, -1, 1);
            elementList.Insert(currentIndex + Mathf.Clamp(indexOffset, 0, 1), element); //insert at +1 if origin is before current
            elementList.RemoveAt(originIndex - Mathf.Clamp(indexOffset, -1, 0)); //remove at +1 if origin is after current

            //adjust labels and enable interaction again
            for (int i = 0; i < elementList.Count; i++)
            {
                elementList[i].pickingMode = PickingMode.Position;
                elementList[i].field.label = $"Element {i}";
            }

            element.ReleasePointer(evt.pointerId);
            dragging = false;
        });
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
        //resizeElementsAtStart
        content.RegisterCallback<GeometryChangedEvent>(InitContentSize);
        void InitContentSize(GeometryChangedEvent evt)
        {
            ResizeContent();
            content.UnregisterCallback<GeometryChangedEvent>(InitContentSize);
        }

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
        ResizeContent();
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
                ResizeContent();
            }
            return;
        }

        //remove selected elements
    }

    public void AddElement(TValue value = default)
    {
        ListElement<T, TValue> element = new($"Element {count}");
        element.style.position = Position.Absolute;
        element.style.height = elementSize - elementSpacing;
        element.field.SetValueWithoutNotify(value);
        element.RegisterCallback<ChangeEvent<TValue>>(ElementValueChanged);
        elementList.Add(element);
        content.Add(element);
        ResizeContent();
        SetupDragDrop(element);
        UpdateListData();
    }
    private void ResizeContent()
    {
        //resize conten to fit all elements
        content.style.height = contentSize;

        //position elements
        for (int i = 0; i < count; i++)
        {
            int index = count - 1 - i;
            ListElement<T, TValue> element = elementList[index];
            element.transform.position = ElementPosition(index);
            element.style.width = content.worldBound.width;
        }
    }

    private Vector3 ElementPosition(int index) => Vector3.up * ((contentSize - elementSize * (count - 1 - index)) - (elementSize + elementSpacing * 3));

    private void ElementValueChanged(ChangeEvent<TValue> evt)
    {
        UpdateListData();
    }

    private void UpdateListData()
    {
        countField.SetValueWithoutNotify(count);
        emptyList.style.display = count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        //call change event
        var evt = CollectionChange<TValue>.GetPooled();
        evt.target = this;

        //add changes

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

public class CollectionChange<TValue> : EventBase<CollectionChange<TValue>>
{
    Dictionary<int, TValue> changedValues = new();
    Dictionary<int, TValue> addedValues = new();
    Dictionary<int, TValue> removedValues = new();
}