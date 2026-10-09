using UnityEngine;
using UnityEngine.SceneManagement;

public class ProximaFase : MonoBehaviour
{
    [Header("Nome da próxima cena")]
    public string nomeDaProximaFase = "Fase2"; // Coloque o nome exato da cena aqui

    private void OnTriggerEnter(Collider other)
    {
       
        
            // Carrega a próxima fase
            SceneManager.LoadScene(nomeDaProximaFase);
        
    }
}