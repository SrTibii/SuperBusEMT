using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerVida : MonoBehaviour
{
    [Header("Valores Vida")]
    public float vidaActualPlayer; //Vida actual player
    public float vidaPlayerMaxima = 100f; //Vida maxima que puede tener el player

    [Header("Interfaz Vida")]
    public Image barraSalud; // Referencia a la barra de salud en la UI
    public TextMeshProUGUI vidaText; // Referencia al texto que muestra la vida actual

    [Header("Regeneración de Vida")]
    public float tiempoParaRegenerar = 5f; // Tiempo sin recibir daño para empezar a regenerar
    public float cantidadRegeneracionPorSegundo = 20f; // Cantidad de vida que se regenera por segundo
    private float temporizadorRegeneracion = 0f; // Temporizador para controlar el tiempo sin recibir daño
    private bool estaRegenerando = false; // Bandera para saber si el player está regenerando vida
    private Coroutine corutinaRegeneracion; // Referencia a la corrutina de regeneración para poder detenerla si el player recibe daño nuevamente

    [Header("Combo")]
    public TextMeshProUGUI comboText; // Referencia al texto que muestra el combo actual
    private int comboCount = 0; //Contador de combo del player

    private void Start()
    {
        // Inicializar la vida al máximo al empezar
        vidaActualPlayer = vidaPlayerMaxima;
    }

    private void Update()
    {
        ActualizarInterfazVida(); // Actualizar la interfaz de vida cada frame
    }

    void ActualizarInterfazVida()
    {
        barraSalud.fillAmount = vidaActualPlayer / vidaPlayerMaxima; // Actualizar la barra de salud
        //vidaText.text = vidaActualPlayer.ToString("F0"); // Actualizar el texto de vida
    }

    public void RecibirDanoPlayer(int cantidadDano)
    {
        vidaActualPlayer -= cantidadDano; //Se va restando la vida
        comboCount = 0; //Reiniciar el contador de combo del player al recibir daño
        comboText.text = "x " + comboCount; //Actualizar el texto del combo al recibir daño

        // Detener la regeneración actual si está en curso
        if (corutinaRegeneracion != null)
        {
            StopCoroutine(corutinaRegeneracion); // Detener la corrutina de regeneración si el player recibe daño nuevamente
            estaRegenerando = false; // Reiniciar la bandera de regeneración
        }

        // Reiniciar el temporizador de regeneración
        temporizadorRegeneracion = 0f;

        // Iniciar la corrutina de espera para regeneración
        corutinaRegeneracion = StartCoroutine(EsperarParaRegenerar());

        if (vidaActualPlayer <= 0)
        {
            MorirPlayer(); //Llamar al método de morir del player si la vida llega a 0 o menos
        }
    }

    private IEnumerator EsperarParaRegenerar()
    {
        // Esperar el tiempo especificado sin recibir daño
        while (temporizadorRegeneracion < tiempoParaRegenerar)
        {
            // Si la vida ya está al máximo, no necesitamos regenerar
            if (vidaActualPlayer >= vidaPlayerMaxima)
            {
                yield break;
            }

            temporizadorRegeneracion += Time.deltaTime;
            yield return null;
        }

        // Si llegamos aquí, ha pasado el tiempo sin recibir daño
        if (vidaActualPlayer < vidaPlayerMaxima)
        {
            // Iniciar la regeneración progresiva
            corutinaRegeneracion = StartCoroutine(RegenerarVida());
        }
    }

    private IEnumerator RegenerarVida()
    {
        estaRegenerando = true;

        // Regenerar vida hasta llegar al máximo
        while (vidaActualPlayer < vidaPlayerMaxima)
        {
            // Aumentar la vida progresivamente
            vidaActualPlayer += cantidadRegeneracionPorSegundo * Time.deltaTime;

            // Asegurar que no nos pasamos del máximo
            if (vidaActualPlayer > vidaPlayerMaxima)
            {
                vidaActualPlayer = vidaPlayerMaxima;
            }

            // Actualizar la interfaz
            ActualizarInterfazVida();

            yield return null; // Esperar un frame
        }

        estaRegenerando = false;
        temporizadorRegeneracion = 0f;
    }

    // Método para aumentar el combo del player, se llama desde el script del enemigo al recibir daño
    public void AumentarCombo()
    {
        comboCount++;
        comboText.text = "x " + comboCount;
    }

    public void MorirPlayer()
    {
        Destroy(this.gameObject);
        Debug.Log("Player muerto");

        //Cuando se llame al metodo la escena se volver� a cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}