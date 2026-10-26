using UnityEngine;
using System.Collections.Generic;

public class TagObjectsManager : MonoBehaviour
{
    [Header("Configuração")]
    public string tagDosObjetos = "Inimigo"; // Coloque aqui a tag que você usa
    public ProximaFase pro;
    public float contador;
    private List<GameObject> listaDeObjetos = new List<GameObject>();

    void Start()
    {
        // Pega todos os objetos com a mesma tag e coloca na lista
        GameObject[] objetosEncontrados = GameObject.FindGameObjectsWithTag(tagDosObjetos);
        listaDeObjetos.AddRange(objetosEncontrados);

        Debug.Log($"Encontrei {listaDeObjetos.Count} objetos com a tag '{tagDosObjetos}'");
    }

    void Update()
    {
        // Remove da lista os objetos que já foram destruídos
        listaDeObjetos.RemoveAll(obj => obj == null);

        // Se a lista ficou vazia, todos foram destruídos
        if (listaDeObjetos.Count == 0)
        {
            contador += Time.deltaTime;

            if (contador >= 20f)
            {
                TodosDestruidos();
                contador = 0f; // evita chamar várias vezes
            }
        }
    }

    void TodosDestruidos()
    {
        Debug.Log("Todos os objetos foram destruídos!");
        pro.Fase();
        enabled = false;
    }
}