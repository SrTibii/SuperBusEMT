using UnityEngine;
using UnityEngine.SceneManagement;


public class Enemigo : MonoBehaviour
{
    public int vidaActualEnemigo = 100;

    [SerializeField] private PlayerVida playerVida; // Referencia al script PlayerVida para acceder a la vida del player

    public void RecibirDanoEnemigo(int cantidadDano)
    {
     
        vidaActualEnemigo -= cantidadDano; //Se va restando la vida
        playerVida.AumentarCombo(); // Aumentar el combo del player al dañar al enemigo
        
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
