using UnityEngine;
using System.Collections;


[RequireComponent(typeof(ParticleSystem))]
public class CometDebris : EffectBase
{
    #region Components
    [Header("Components")]
    public ParticleSystem sparkSystem;
    #endregion

    public void Initialize(Color color)
    {
        Color.RGBToHSV(color, out float hue, out float sat, out float val);

        ParticleSystemRenderer mParticleSystemRenderer = GetComponent<ParticleSystemRenderer>();

        mParticleSystemRenderer.material.color = Color.HSVToRGB(hue, sat, val);

        ParticleSystem.MainModule main = sparkSystem.main;

        ParticleSystem.MinMaxGradient startColor = new ParticleSystem.MinMaxGradient(Color.HSVToRGB(hue + 0.06f, 0.25f, val), Color.HSVToRGB(hue, 0.20f, val));
        main.startColor = startColor;
    }
}
