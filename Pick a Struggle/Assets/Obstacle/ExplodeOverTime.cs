using PurrNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ExplodeOverTime : NetworkIdentity
{
    [Header("Timer Settings")]
    public float timer;

    [Header("Explosion Particle Settings")]
    public GameObject explosionParts;
    public GameObject sparks;

    [Header("Eplosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 10f;
    public LayerMask playerMask;

    private void Update() {
        if(!isServer) return;

        timer -= Time.deltaTime;
        if(timer < 0) {
            Explode();
        }
    }

    public void Explode() {
        // Player hitbox controls
        Collider[] hitboxHits = Physics.OverlapSphere(transform.position, explosionRadius, playerMask);

        var closestHitbox = new Dictionary<Transform, PlayerHitbox>();
        var closestDist = new Dictionary<Transform, float>();

        foreach(var hit in hitboxHits) {
            // Ensure hitbox
            if(!hit.TryGetComponent<PlayerHitbox>(out var hitbox)) continue;

            // Find parent
            Transform playerRoot = FindPlayerRoot(hit.transform);
            if(playerRoot == null) continue;

            // Find closest hits for each player
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if(!closestDist.TryGetValue(playerRoot, out float best) || dist < best) {
                closestDist[playerRoot] = dist;
                closestHitbox[playerRoot] = hitbox;
            }
        }

        // Apply the explosion
        foreach(var (playerRoot, hitbox) in closestHitbox) {
            float falloff = Mathf.Clamp01(1f - closestDist[playerRoot] / explosionRadius);
            Vector3 dir = (hitbox.transform.position - transform.position).normalized;
            hitbox.ExplosionHit(dir, explosionForce * falloff);
        }

        ExplosionEffect();
        Destroy(gameObject);
    }

    [ObserversRpc(runLocally: true)]
    void ExplosionEffect() {
        Instantiate(explosionParts, this.transform.position, this.transform.rotation);
    }

    private Transform FindPlayerRoot(Transform t) {
        while(t != null) {
            if(t.CompareTag("Player")) 
                return t;
            t = t.parent;
        }
        return null;
    }
}
