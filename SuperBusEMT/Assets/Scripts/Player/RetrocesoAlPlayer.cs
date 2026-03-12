using UnityEngine;

public class RetrocesoAlPlayer : MonoBehaviour
{
    public float knockbackFuerza = 25f;

    private bool jugadorEnKnockback = false;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.3f; // 0.3 segundos de knockback

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || jugadorEnKnockback) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Desactiva movimiento del jugador durante knockback
        jugadorEnKnockback = true;
        knockbackTimer = knockbackDuration;

        // Dirección: MUCHO más atrás que arriba
        Vector3 away = (other.transform.position - transform.position).normalized;
        Vector3 dir = (away * 10f + Vector3.up * 1f).normalized;

        // Aplica knockback FLUIDO
        rb.linearVelocity = dir * knockbackFuerza;
    }

    void FixedUpdate()
    {
        if (jugadorEnKnockback)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0)
            {
                jugadorEnKnockback = false;
            }
        }
    }

    // ESTO es lo que tu script del jugador debe comprobar:
    public bool EstaEnKnockback()
    {
        return jugadorEnKnockback;
    }
}
