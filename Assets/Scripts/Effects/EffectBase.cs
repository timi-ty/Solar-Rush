using UnityEngine;
using System.Collections;

public class EffectBase : MonoBehaviour
{

    #region Properties
    [Header("Sound Effects")]
    public AudioClip sfxClip;
    #endregion

    protected virtual void Start()
    {
        AudioManager.PlayGameClip(sfxClip);
    }
}
