using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemigo : MonoBehaviour
{
    
    public int vidaActualEnemigo = 100;


    public void RecibirDanoEnemigo(int cantidadDano)
    {
        vidaActualEnemigo -= cantidadDano; //Se va restando la vida

        if (vidaActualEnemigo <= 0)
        {
            MorirEnemigo();
        }
    }

    public void MorirEnemigo()
    {
        Destroy(this.gameObject);
        Debug.Log("Enemigo Muerto");
    }
}
