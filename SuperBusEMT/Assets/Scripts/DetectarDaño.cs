using UnityEngine;
using UnityEngine.SceneManagement;

public class DetectarDaño : MonoBehaviour
{

    public int dañoPorGolpe = 30;

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar si el que entra es el propio player o sus partes
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemigo"))
        {
            Debug.Log("Daño al Enemigo");
            Enemigo enemigo = other.GetComponent<Enemigo>(); //Cogemos la referencia del script del Enemigo

            if (enemigo != null)
            {
                enemigo.RecibirDañoEnemigo(dañoPorGolpe);
            }


        } 
    }
}
