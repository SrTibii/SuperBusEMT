using UnityEngine;

public class DisparoEnemigo : MonoBehaviour
{
    
    public GameObject balaPrefab; // Prefab de la bala que se va a instanciar
    public float velocidadDisparo = 5f; // Velocidad a la que se moverá la bala
    public Transform AlturaArma; // Transform que representa la altura del arma desde donde se disparará la bala


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstanciarBala()
    {
        if (balaPrefab != null && AlturaArma != null)
        {
            // Instancia la bala en la posición de AlturaArmas
            GameObject bala = Instantiate(balaPrefab, AlturaArma.position, Quaternion.identity);

            // Agregar velocidad a la bala (asumiendo que tiene Rigidbody2D o Rigidbody)
            Rigidbody rb = bala.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Dirección hacia la derecha (asumiendo que el enemigo mira a la izquierda/derecha)
                // Ajusta la dirección según necesites
                rb.linearVelocity = transform.forward * velocidadDisparo;
            }
        }
    }

    void Vacio()
    {
       
    }


}
