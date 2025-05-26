using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Button = UnityEngine.UIElements.Button;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomEditor(typeof(ColorPalette))]
public class PalletePropertyDrawer : Editor
{
    private ColorPalette palette;
    private VisualElement root;
    private ListField<ColorReferenceField, Color> colorsRoot;
    private Foldout referencesRoot;
    private PopupWindow Popup;
    private ListField<ObjectField, Object> folders;
    private ListField<VisualColorField<Graphic>, Graphic> graphicList;
    private ListField<VisualColorField<SpriteRenderer>, SpriteRenderer> rendererList;

    public override VisualElement CreateInspectorGUI()
    {
        palette = target as ColorPalette;
        EditorUtility.SetDirty(palette);
        root = new();
        root.name = "root";
        if (palette == null) return root;

        Button read = new();
        read.text = "Read Folders";
        read.clicked += ReadProjectElements;
        root.Add(read);

        folders = new("Prefab Folders");
        for (int i = 0; i < palette.searchFolders.Length; i++)
        {
            //Debug.Log(AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder) == null);
            string folder = palette.searchFolders[i];
            folders.AddElement(AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder));
        }

        folders.RegisterCallback<CollectionChangeEvent<Object>>(UpdatePrefabFolders);
        root.Add(folders);

        colorsRoot = new("Colors");
        colorsRoot.style.top = 5;
        colorsRoot.SetInteractable(false);
        root.Add(colorsRoot);
        SetupColors();
        colorsRoot.RegisterCallback<CollectionChangeEvent<Color>>(OnColorsChanged);

        referencesRoot = new Foldout();
        referencesRoot.text = "Component References";
        referencesRoot.style.top = 5;
        root.Add(referencesRoot);
        //graphics list
        graphicList = new("Graphics");
        graphicList.SetInteractable(false);
        referencesRoot.Add(graphicList);
        SetupReferences();

        Button apply = new();
        apply.style.top = 5;
        apply.text = "ApplyPallete";
        apply.clicked += ApplyPaletteChanges;
        root.Add(apply);
        
        SetupColorsPopup();
        
        return root;
    }

    private void SetupColorsPopup()
    {
        Popup ??= new PopupWindow();
        Popup.text = "Colors";
        Popup.style.backgroundColor = ColorExtension.GrayShade(.2f);
        Popup.style.position = Position.Absolute;
        Popup.RegisterCallback<FocusOutEvent>((evt) =>
        {
            bool isChild = false;
            VisualElement element = Popup.ElementAt(0);
            int children = Popup.ElementAt(0).childCount;
            for (int i = 0; i < children; i++)
            {
                if (element.ElementAt(i) == evt.relatedTarget)
                {
                    isChild = true;
                    break;
                }
            }

            if (!isChild) CloseColorsPopup();
        });

        VisualElement window = new VisualElement();
        window.style.flexDirection = FlexDirection.Row;
        window.style.flexWrap = Wrap.Wrap;
        Popup.Add(window);
    }
    
    private void ReadProjectElements()
    {
        palette.Colors.Clear();
        palette.Graphics.Clear();
        palette.graphicsLookUp.Clear();
        palette.Renderers.Clear();
        palette.renderersLookUp.Clear();

        string[] prefabs = AssetDatabase.FindAssets( "t:Prefab" , palette.searchFolders);
        //string[] scenes =  AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" });
        //GetPrefabAssetPathOfNearestInstanceRoot	Retrieves the asset path of the nearest Prefab instance root the specified object is part of.
        foreach (var prefab in prefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath( prefab );
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( path );

            Graphic[] graphics = go.GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; i++)
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(graphics[i]))
                {
                    //Debug.Log($"{go} | {graphics[i]} \n {PrefabUtility.IsPartOfPrefabInstance(graphics[i])}");
                    palette.Graphics.Add(graphics[i]);
                    Color color = graphics[i].color;
                    //add color if doesn't exist
                    if (!palette.Colors.Contains(color)) palette.Colors.Add(color);
                    //set a lookup inder to link object to color
                    palette.graphicsLookUp.Add(palette.Colors.IndexOf(color));
                }
            }

            SpriteRenderer[] renderers = go.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(renderers[i]))
                {
                    //Debug.Log($"{go} | {graphics[i]} \n {PrefabUtility.IsPartOfPrefabInstance(graphics[i])}");
                    palette.Renderers.Add(renderers[i]);
                    Color color = renderers[i].color;
                    //add color if doesn't exist
                    if (!palette.Colors.Contains(color)) palette.Colors.Add(color);
                    //set a lookup inder to link object to color
                    palette.renderersLookUp.Add(palette.Colors.IndexOf(color));
                }
            }
        }
        
        SetupColors();
        SetupReferences();
    }

    private void UpdatePrefabFolders(CollectionChangeEvent<Object> evt)
    {
        foreach (var changedValue in evt.changedValues.Values)
        {
            Debug.Log($"{changedValue.newValue} (was {changedValue.previousValue})");
        }

        //Debug.Log("update");
        string[] searchFolders = new string[folders.count];
        for (int i = 0; i < searchFolders.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(folders.ValueAt(i));
            searchFolders[i] = path;
        }

        palette.searchFolders = searchFolders;
    }

    private void SetupColors()
    {
        colorsRoot.ClearList();
        for (int i = 0; i < palette.Colors.Count; i++)
        {
            var element = colorsRoot.AddElement(palette.Colors[i]);
            element.ReferenceCount = GetReferenceCount(i);
        }
    }

    private IEnumerable<int> GraphicsOf(int id) =>
        from g in palette.graphicsLookUp where g == id select g;

    private IEnumerable<int> RenderersOf(int id) =>
        from r in palette.renderersLookUp where r == id select r;

    private int GetReferenceCount(int id)
    {
        var graphics = GraphicsOf(id);
        var renderers = RenderersOf(id);
        return graphics.Count() + renderers.Count();
    }

    private void OnColorsChanged(CollectionChangeEvent<Color> evt)
    {
        //add colors to list
        for (int i = 0; i < evt.addedValues.Count; i++) palette.Colors.Add(default);
        foreach (var addedValue in evt.addedValues)
            palette.Colors[addedValue.Key] = addedValue.Value;

        //update colors on references
        foreach (var changedValue in evt.changedValues)
        {
            int id = changedValue.Key;
            Color newColor = changedValue.Value.newValue;
            palette.Colors[id] = newColor;
            //Graphic
            for (int i = 0; i < palette.Graphics.Count; i++)
            {
                if (palette.graphicsLookUp[i] != id) continue;
                palette.Graphics[i].color = newColor;
                graphicList?.FieldAt(i)?.SetReferenceColor(id, newColor);
            }

            //SpriteRender
        }

        //remove old references
        foreach (var removedValue in evt.removedValues)
        {
            palette.Colors.Remove(removedValue.Value);
            //update references?
        }
    }

    private void SetupReferences()
    {
        graphicList.ClearList();
        for (int i = 0; i < palette.Graphics.Count; i++)
        {
            var element = graphicList.AddElement(palette.Graphics[i]);
            element.value = palette.Graphics[i];
            element.SetInteractable(false);
            int id = palette.graphicsLookUp[i];
            element.SetReferenceColor(id, palette.Colors[id]);
            element.onColorClicked += ChoseColorPopup;
        }

        //spriterender list
    }

    private void ChoseColorPopup(VisualColorField<Graphic> graphicReference)
    {
        int currentId = palette.Graphics.IndexOf(graphicReference.value);

        VisualElement clickedReference = graphicReference.idLabel;
        Vector3 position = clickedReference.worldTransform.GetPosition() + (Vector3)(clickedReference.worldBound.size * .5f);
        position -= root.worldTransform.GetPosition();
        Popup.transform.position = position;
        Popup.style.flexBasis = root.worldBound.width - position.x;
        Popup.style.width = root.worldBound.width - position.x;

        VisualElement window = Popup[0];
        window.Clear();
        for (int i = 0; i < palette.Colors.Count; i++)
        {
            bool currentSelected = i == palette.graphicsLookUp[currentId];
            int colorID = i;
            Button color = ColorButton(currentSelected, colorID, graphicReference, 
                palette.Graphics, palette.graphicsLookUp, graphicList);
            window.Add(color);
        }

        root.Add(Popup);
        Popup.FocusElement(true);
    }

    private void CloseColorsPopup()
    {
        if (!root.Contains(Popup)) return;
        root.Remove(Popup);
    }

    private Button ColorButton<T>(bool selected, int colorId, VisualColorField<T> element,
        List<T> references, List<int> lookup, ListField<VisualColorField<T>, T> list) where T : Object
    {
        Button button = new Button();
        button.name = "color pick";
        button.SetBorder(selected ? 1.5f : 1, borderColor: Color.white);
        button.text = selected ? "<b>-" : "";
        button.style.fontSize = 25;
        button.style.color = ColorExtension.ContrastGray(palette.Colors[colorId]);
        button.style.left = 8;
        button.style.width = 20;
        button.style.height = 20;
        button.style.backgroundColor = palette.Colors[colorId];
        button.clicked += () =>
        {
            UpdateElementReference(colorId, element, references, lookup, list);
            CloseColorsPopup();
        };
        return button;
    }

    private void UpdateElementReference<T>(int newId, VisualColorField<T> element, 
        List<T> references, List<int> lookup, ListField<VisualColorField<T>, T> list) where T : Object
    {
        int graphicId = references.IndexOf(element.value);
        int oldId = lookup[graphicId];
        lookup[graphicId] = newId;

        var reference = list.FieldAt(graphicId);
        reference.SetReferenceColor(newId, palette.Colors[newId]);

        colorsRoot.FieldAt(oldId).ReferenceCount--;
        colorsRoot.FieldAt(newId).ReferenceCount++;
    }
    
    private void ApplyPaletteChanges()
    {
        for (int i = 0; i < palette.graphicsLookUp.Count; i++)
            ApplyElementColor(palette.Graphics[i]);
        for (int i = 0; i < palette.renderersLookUp.Count; i++)
            ApplyElementColor(palette.Renderers[i]);
        
        AssetDatabase.Refresh();
        ReadProjectElements();
    }
    private void ApplyElementColor(Graphic element)
    {
        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(element);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        element.color = palette.Colors[palette.graphicsLookUp[palette.Graphics.IndexOf(element)]];

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();
    }
    private void ApplyElementColor(SpriteRenderer element)
    {
        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(element);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        element.color = palette.Colors[palette.renderersLookUp[palette.Renderers.IndexOf(element)]];

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();
    }
}