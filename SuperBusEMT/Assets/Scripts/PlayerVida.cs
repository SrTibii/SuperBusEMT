using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVida : MonoBehaviour
{
    public int vidaActualPlayer;

    public void RecibirDañoPlayer(int cantidadDaño)
    {
        vidaActualPlayer -= cantidadDaño; //Se va restando la vida

        if (vidaActualPlayer <= 0)
        {
            MorirPlayer();
        }
    }

    public void MorirPlayer()
    {
        Destroy(this.gameObject);
        Debug.Log("Player muerto");

        //Cuando se llame al metodo la escena se volverá a cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
