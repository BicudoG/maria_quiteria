using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class bandeira_Abaixando : MonoBehaviour
{
    private List<Transform> alvosTransforms = new List<Transform>();
    private Transform jogadorTransform;
    float contador;

    [Header("Bandeiras")]
    public Transform bandeiraPortuga;
    public Transform bandeiraBrasuca;
    public GameObject manager;
    public GameObject portuga;
    public GameObject brasil;
    public ProximaFase pro;

    [Header("Alturas")]
    public float alturaAlta = -0.0011f;   // altura máxima (Y)
    public float alturaBaixa = 0f;  // altura mínima (Y)

    [Header("Detecção")]
    public string tagDoJogador = "Player";
    public string tagInimigo = "Enemy"; // <-- mude para a tag correta
    public float distanciaLimite = 15f;

    [Header("Movimento")]
    public float velocidade = 0.00001f;

    private bool temJogador;
    private bool temInimigo;

    void Start()
    {
        // pro = GetComponent<ProximaFase>();  ← REMOVA ou comente esta linha

        if (pro == null)
            Debug.LogError("ProximaFase não está atribuído no Inspector!");

        BuscarAlvosNaCena();

        if (brasil != null) brasil.SetActive(false);
        if (portuga != null) portuga.SetActive(true);

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
            brasil.SetActive(true);
            Vector3 posP = bandeiraPortuga.localPosition;
            posP.y = Mathf.MoveTowards(posP.y, alturaBaixa, velocidade * Time.deltaTime);
            bandeiraPortuga.localPosition = posP;
            // Sobe a bandeira brasileira

            Vector3 pos = bandeiraBrasuca.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, alturaAlta, velocidade * Time.deltaTime);
            bandeiraBrasuca.localPosition = pos;

        }
        else
        {
            // Volta para baixo quando não tem jogador ou tem inimigo
            Vector3 pos = bandeiraBrasuca.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, alturaBaixa, velocidade * Time.deltaTime);
            bandeiraBrasuca.localPosition = pos;
        }

        if (bandeiraBrasuca.localPosition.y >= alturaAlta - 0.01f)
        {
            if (portuga != null) portuga.SetActive(false);

            contador += Time.deltaTime;

            if (contador >= 20f)
            {
                if (pro != null)
                    pro.Fase();

                contador = 0f; // evita chamar várias vezes
            }
        }
        else
        {
            contador = 0f;
        }


    }
}