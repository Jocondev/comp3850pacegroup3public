
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnZoneParticle : UdonSharpBehaviour
{
    [SerializeField] private ParticleSystem particles;

    void Start()
    {
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 23)
        {
            // Debug.Log("exited pet");
            particles.Stop();
            gameObject.SetActive(false);
        }
    }

    public void ResetZone()
    {
        // Debug.Log("restart zone");
        particles.Play();
        gameObject.SetActive(true);
    }
}
