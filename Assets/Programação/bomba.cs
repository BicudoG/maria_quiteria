using UnityEngine;
using System.Collections;
public class bomba : MonoBehaviour
{
    [Header("Configuração")]
    public float tempoEspera = 3f;
    public float raio = 8f; // Distância máxima para aplicar o dano

    void Start()
    {
        StartCoroutine(AplicarDano());
    }

    IEnumerator AplicarDano()
    {
        yield return new WaitForSeconds(tempoEspera);

        Sistema_Vida[] todosComVida = FindObjectsByType<Sistema_Vida>(FindObjectsSortMode.None);

        int quantidadeAtingida = 0;

        foreach (Sistema_Vida sistema in todosComVida)
        {
            // Calcula a distância entre este objeto e o alvo
            float distancia = Vector3.Distance(transform.position, sistema.transform.position);

            // Só aplica o dano se estiver dentro do raio
            if (distancia <= raio)
            {
                sistema.Vida -= 100;
                quantidadeAtingida++;
            }
        }

        Debug.Log($"Dano de 100 aplicado em {quantidadeAtingida} objetos próximos");
    }
}
