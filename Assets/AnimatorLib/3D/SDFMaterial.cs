using UnityEngine;

public struct SDFMaterialData
{
    public Vector4 Albedo;
    public float Smoothness;
    public float SpecularPower;
    public Vector4 Ambient;
    public float AmbientStrength;
    public float Opacity;
    public int Lit;
}

[CreateAssetMenu(fileName = "SDFMaterial", menuName = "SDF/SDFMaterial")]
public class SDFMaterial : ScriptableObject
{
    public Color Albedo;
    [Range(0, 1)]
    public float Smoothness = 0.5f;
    public float SpecularPower = 3.0f;
    public Color Ambient;
    [Range(0, 1)]
    public float AmbientStrength = 0.2f;
    [Range(0, 1)]
    public float Opacity = 1.0f;
    public bool Lit = true;

    public SDFMaterialData ToMaterialData()
    {
        return new SDFMaterialData
        {
            Albedo = Albedo,
            Smoothness = Smoothness,
            SpecularPower = SpecularPower,
            Ambient = Ambient,
            AmbientStrength = AmbientStrength,
            Opacity = Opacity,
            Lit = Lit ? 1 : 0,
        };
    }
}
