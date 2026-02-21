using UnityEngine;
using UnityEngine.SceneManagement;

public class DañoAlPlayer : MonoBehaviour
{
    public int dañoPorGolpe = 30;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Daño al player");

            PlayerVida playerVida = other.GetComponent<PlayerVida>(); //Cogemos la referencia del script del player

            if (playerVida != null)
            {
                playerVida.RecibirDañoPlayer(dañoPorGolpe);
            }
        }
    }
}
