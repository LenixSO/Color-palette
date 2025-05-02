using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphicColorField<T> : BaseField<Color> where T : Object
{
    public GraphicColorField(string label) : base(label, null)
    {
    }
}
