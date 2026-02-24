using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerAtaque : MonoBehaviour
{
    [Header("Ataque Settings")]
    [SerializeField] private GameObject cuboAtaque; // El cubo que está dentro del player
    [SerializeField] private float duracionAtaque = 0.2f; // Tiempo que permanece activo

    public InputActionReference triggerAtaque;

    private bool atacando = false;

    private void Awake()
    {
        // Asegurarse de que el cubo empieza desactivado
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
        }
        else
        {
            Debug.LogError("No se ha asignado el cubo de ataque en el inspector");
        }
    }

    private void OnEnable()
    {
        triggerAtaque.action.performed += OnTriggerPressedAtaque;
        triggerAtaque.action.Enable();
    }

    private void OnDisable()
    {
        triggerAtaque.action.performed -= OnTriggerPressedAtaque;
        triggerAtaque.action.Disable();
    }

    private void OnTriggerPressedAtaque(InputAction.CallbackContext context)
    {
        // Prevenir múltiples ataques a la vez
        if (atacando) return;

        Debug.Log("Ataque presionado");
        StartCoroutine(MostrarCuboAtaque());
    }

    private System.Collections.IEnumerator MostrarCuboAtaque()
    {
        atacando = true;

        // Activar el cubo
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(true);
            Debug.Log("Cubo de ataque ACTIVADO");
        }

        // Esperar el tiempo de duración
        yield return new WaitForSeconds(duracionAtaque);

        // Desactivar el cubo
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
            Debug.Log("Cubo de ataque DESACTIVADO");
        }

        atacando = false;
    }
}