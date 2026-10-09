using TMPro;
using UnityEngine;

public class Sistema_dialogo : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float distanciaDoRaio = 3f;
    [SerializeField] private float tempoEntreFalas = 5f;

    [Header("Diálogos de Missão (Pai)")]
    [TextArea(2, 4)]
    [SerializeField] private string[] falasMissao;          // Sequência completa da missão

    [Header("Diálogos de NPCs Genéricos")]
    [TextArea(2, 4)]
    [SerializeField] private string[] falasGenericas;       // Falas aleatórias dos NPCs comuns

    [Header("Diálogo do Aliado")]
    [TextArea(2, 4)]
    [SerializeField] private string conversaAliado;

    // Encontrados automaticamente
    private GameObject fundoBranco;
    private TextMeshProUGUI texto;

    // Controle interno
    private int conversa = 0;
    private float falaTime = 0f;
    private bool emConversa = false;
    private bool jachamada = false;
    private bool ehDialogoMissao = false;   // Controla se é diálogo de missão ou não

    private void Start()
    {
        fundoBranco = GameObject.FindWithTag("Fundo_preto");
        texto = FindFirstObjectByType<TextMeshProUGUI>();

        if (fundoBranco == null)
            Debug.LogWarning("Não encontrei nenhum objeto com a Tag 'Fundo_preto'!");
        else
            fundoBranco.SetActive(false);

        if (texto == null)
            Debug.LogWarning("Não encontrei nenhum TextMeshProUGUI na cena!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !emConversa)
        {
            TentarIniciarConversa();
        }

        // Só avança automaticamente se for diálogo de missão
        if (emConversa && ehDialogoMissao)
        {
            falaTime += Time.deltaTime;

            if (falaTime >= tempoEntreFalas)
            {
                falaTime = 0f;
                AvancarFalaMissao();
            }
        }
    }

    private void TentarIniciarConversa()
    {
        RaycastHit hit;
        Vector3 origem = transform.position + Vector3.up * 1.2f;
        Vector3 direcao = transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(origem, direcao * distanciaDoRaio, Color.red, 2f);

        if (Physics.Raycast(origem, direcao, out hit, distanciaDoRaio))
        {
            Debug.Log($"Acertou: {hit.collider.name} | Tag: {hit.collider.tag}");

            // NPC Genérico → fala aleatória
            if (hit.collider.CompareTag("NPC_Generico"))
            {
                if (falasGenericas == null || falasGenericas.Length == 0)
                {
                    Debug.LogWarning("Nenhuma fala genérica cadastrada!");
                    return;
                }

                int index = Random.Range(0, falasGenericas.Length);
                IniciarDialogoSimples(falasGenericas[index]);
            }
            // NPC Aliado
            else if (hit.collider.CompareTag("NPC_Aliado") && !jachamada)
            {
                Aliado_Ia aliado = hit.transform.GetComponent<Aliado_Ia>();
                if (aliado != null)
                    aliado.jaChamou = true;

                jachamada = true;
                IniciarDialogoSimples(conversaAliado);
            }
            // Pai / Missão → sequência
            else if (hit.collider.CompareTag("Pai"))
            {
                IniciarDialogoMissao();
            }
            else
            {
                Debug.Log($"Tag não tratada: {hit.collider.tag}");
            }
        }
        else
        {
            Debug.Log("Raycast não acertou nada");
        }
    }

    // Diálogo simples (fecha depois de um tempo ou pode ser fechado)
    private void IniciarDialogoSimples(string fala)
    {
        emConversa = true;
        ehDialogoMissao = false;
        falaTime = 0f;

        if (fundoBranco != null)
            fundoBranco.SetActive(true);

        if (texto != null)
            texto.text = fala;

        // Fecha automaticamente depois do tempo
        Invoke(nameof(FinalizarDialogo), tempoEntreFalas);
    }

    // Diálogo de missão (sequência)
    private void IniciarDialogoMissao()
    {
        if (falasMissao == null || falasMissao.Length == 0)
        {
            Debug.LogWarning("Array de falas de missão está vazio!");
            return;
        }

        emConversa = true;
        ehDialogoMissao = true;
        conversa = 0;
        falaTime = 0f;

        if (fundoBranco != null)
            fundoBranco.SetActive(true);

        if (texto != null)
            texto.text = falasMissao[0];

        Debug.Log("Iniciando diálogo de missão");
    }

    private void AvancarFalaMissao()
    {
        conversa++;

        if (conversa >= falasMissao.Length)
        {
            FinalizarDialogo();
        }
        else
        {
            if (texto != null)
                texto.text = falasMissao[conversa];
        }
    }

    private void FinalizarDialogo()
    {
        // Guarda se era missão antes de resetar
        bool eraMissao = ehDialogoMissao;

        emConversa = false;
        ehDialogoMissao = false;
        conversa = 0;
        falaTime = 0f;

        if (fundoBranco != null)
            fundoBranco.SetActive(false);

        if (texto != null)
            texto.text = "";

        // Se era diálogo de missão, destroi o objeto com a tag "missao"
        if (eraMissao)
        {
            GameObject objetoMissao = GameObject.FindWithTag("missao");
            if (objetoMissao != null)
            {
                Destroy(objetoMissao);
                Debug.Log("Objeto da missão destruído!");
            }
            else
            {
                Debug.LogWarning("Nenhum objeto com a tag 'missao' foi encontrado.");
            }
        }

        Debug.Log("Diálogo finalizado!");
    }
}