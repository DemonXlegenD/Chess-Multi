using System;
using UnityEngine;

[Serializable]
public struct SerializableColor
{
    [Range(0, 1)] public float r;
    [Range(0, 1)] public float g;
    [Range(0, 1)] public float b;
    [Range(0, 1)] public float a;

    public SerializableColor(float r, float g, float b, float a = 1.0f)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    // Conversion vers une couleur Unity
    public Color ToUnityColor()
    {
        return new Color(r, g, b, a);
    }

    // Conversion en code hexadécimal
    public string ToHex()
    {
        Color32 color32 = ToUnityColor();
        return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
    }

    // Couleurs par défaut
    public static SerializableColor White => new SerializableColor(1, 1, 1, 1);
    public static SerializableColor Black => new SerializableColor(0, 0, 0, 1);
    public static SerializableColor Red => new SerializableColor(1, 0, 0, 1);
    public static SerializableColor Green => new SerializableColor(0, 1, 0, 1);
    public static SerializableColor Blue => new SerializableColor(0, 0, 1, 1);
    public static SerializableColor Yellow => new SerializableColor(1, 1, 0, 1);
    public static SerializableColor Cyan => new SerializableColor(0, 1, 1, 1);
    public static SerializableColor Magenta => new SerializableColor(1, 0, 1, 1);
    public static SerializableColor Transparent => new SerializableColor(0, 0, 0, 0);
}
