//In Progress
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PartSysSelfDestruct : MonoBehaviour
{
    private ParticleSystem mParticleSystem;

    private void OnEnable() 
    {
        mParticleSystem = GetComponent<ParticleSystem>();

        float effectDuration = mParticleSystem.main.duration;

        Invoke("DestroyAfterDuration", effectDuration + 0.5f);
    }

    void DestroyAfterDuration(){
        Destroy(gameObject);
    }
}
