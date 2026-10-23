using UnityEngine;
using UnityEngine.SceneManagement;

public class ProximaFase : MonoBehaviour
{
    [Header("Nome da próxima cena")]
    public string nomeDaproximaFase = "Fase2";

    private void OnTriggerEnter(Collider other)
    {
     
            SceneManager.LoadScene(nomeDaproximaFase);
        
    }

    public void Fase()
    {
        SceneManager.LoadScene(nomeDaproximaFase);
    }
}