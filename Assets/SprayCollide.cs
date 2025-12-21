using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SprayCollide : MonoBehaviour
{
    [Min(0f)] public float power = 0.01f;

    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>(32); // pre-size for fewer reallocs
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log(other.gameObject.tag);
        // Ultra-safe in case something weird disables/enables order
        if (collisionEvents == null)
            collisionEvents = new List<ParticleCollisionEvent>(32);

        if (!other.CompareTag("Enemy") && !other.CompareTag("Cage"))
            return;

        int num = ps.GetCollisionEvents(other, collisionEvents);
        if (num <= 0)
            return;

        if (other.CompareTag("Enemy"))
        {
            var hit = other.GetComponentInParent<Hittable>();
            if (hit != null)
            {
                hit.iced = Mathf.Clamp(hit.iced + num * power, 0, 1);
            }
        } else if (other.CompareTag("Cage"))
        {
            var cm = other.GetComponent<SecondCageManager>();

            cm.cm.StartCageHeal(num * power);
        }
        
            
    }
}
