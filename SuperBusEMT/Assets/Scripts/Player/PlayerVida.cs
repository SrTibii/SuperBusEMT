using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerVida : MonoBehaviour
{
    public int vidaActualPlayer;

    public TextMeshProUGUI comboText;

    private int comboCount = 0;

    public void RecibirDanoPlayer(int cantidadDano)
    {
        vidaActualPlayer -= cantidadDano; //Se va restando la vida
        comboCount=0; // Reiniciar el contador de combo del player al recibir daño
        comboText.text = "x " + comboCount;


        if (vidaActualPlayer <= 0)
        {
            MorirPlayer();
        }
    }

    
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
