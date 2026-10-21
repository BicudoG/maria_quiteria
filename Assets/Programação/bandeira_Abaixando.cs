using System.Collections.Generic;
using UnityEngine;

public class bandeira_Abaixando : MonoBehaviour
{
    private List<Transform> alvosTransforms = new List<Transform>();
    private Transform jogadorTransform;

    [Header("Bandeiras")]
    public Transform bandeiraPortuga;
    public Transform bandeiraBrasuca;

    [Header("Alturas")]
    public float alturaAlta = 5f;   // altura máxima (Y)
    public float alturaBaixa = 0f;  // altura mínima (Y)

    [Header("Detecção")]
    public string tagDoJogador = "Player";
    public string tagInimigo = "Enemy"; // <-- mude para a tag correta
    public float distanciaLimite = 15f;

    [Header("Movimento")]
    public float velocidade = 2f;

    private bool temJogador;
    private bool temInimigo;

    void Start()
    {
        BuscarAlvosNaCena();

        GameObject jogador = GameObject.FindWithTag(tagDoJogador);
        if (jogador != null)
            jogadorTransform = jogador.transform;
        else
            Debug.LogError("Jogador não encontrado com a tag: " + tagDoJogador);
    }

    void BuscarAlvosNaCena()
    {
        alvosTransforms.Clear();
        GameObject[] inimigos = GameObject.FindGameObjectsWithTag(tagInimigo);
        foreach (GameObject go in inimigos)
            alvosTransforms.Add(go.transform);
    }

    void Update()
    {
        if (jogadorTransform == null) return;

        // Resetar flags
        temJogador = false;
        temInimigo = false;

        // Verificar jogador
        float distJogador = Vector3.Distance(transform.position, jogadorTransform.position);
        if (distJogador <= distanciaLimite)
            temJogador = true;

        // Verificar inimigos
        foreach (Transform alvo in alvosTransforms)
        {
            if (alvo == null) continue;
            if (Vector3.Distance(transform.position, alvo.position) <= distanciaLimite)
            {
                temInimigo = true;
                break;
            }
        }

        // Lógica da bandeira
        if (temJogador && !temInimigo)
        {
            // Sobe a bandeira brasileira
            Vector3 pos = bandeiraBrasuca.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, alturaAlta, velocidade * Time.deltaTime);
            bandeiraBrasuca.localPosition = pos;

            // (Opcional) abaixa a portuguesa
            // Vector3 posP = bandeiraPortuga.localPosition;
            // posP.y = Mathf.MoveTowards(posP.y, alturaBaixa, velocidade * Time.deltaTime);
            // bandeiraPortuga.localPosition = posP;
        }
        else
        {
            // Volta para baixo quando não tem jogador ou tem inimigo
            Vector3 pos = bandeiraBrasuca.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, alturaBaixa, velocidade * Time.deltaTime);
            bandeiraBrasuca.localPosition = pos;
        }
    }
}