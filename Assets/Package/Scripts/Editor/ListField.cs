using System.Collections.Generic;
using UnityEngine.UIElements;

public class ListField<T> : VisualElement where T : VisualElement
{
    private VisualElement content;
    
    private List<T> elementList = new();

    public ListField(string label = "ListField")
    {
        VisualElement header = new();
        header.style.flexDirection = FlexDirection.Row;
        
        Foldout foldout = new();
        foldout.text = label;
        foldout.style.flexGrow = 1;
        foldout.Add(new Label("testElement"));
        foldout.Add(new Label("testElement"));
        foldout.Add(new Label("testElement"));
        foldout.Add(new Label("testElement"));
        
        IntegerField count = new("");
        count.style.width = 50;
        count.style.height = 18;
        
        header.Add(foldout);
        header.Add(count);
        
        Add(header);
    }
}

public class ListElement<T> : VisualElement where T : VisualElement
{
    private T element;
}