using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtension
{
    /// <summary>
    /// Creates a shade of gray.
    /// </summary>
    /// <param name="insensity">The closest to 1, the whiter it is.</param>
    public static Color GrayShade(float insensity)
    {
        insensity = Mathf.Clamp01(insensity);

        return new Color(insensity, insensity, insensity, 1);
    }
}
