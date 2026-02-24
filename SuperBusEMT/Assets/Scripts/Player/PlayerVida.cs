using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVida : MonoBehaviour
{
    public int vidaActualPlayer;

    public void RecibirDanoPlayer(int cantidadDano)
    {
        vidaActualPlayer -= cantidadDano; //Se va restando la vida

        if (vidaActualPlayer <= 0)
        {
            MorirPlayer();
        }
    }

    public void MorirPlayer()
    {
        Destroy(this.gameObject);
        Debug.Log("Player muerto");

        //Cuando se llame al metodo la escena se volver� a cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
