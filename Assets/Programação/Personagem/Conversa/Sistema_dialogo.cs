using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sistema_dialogo : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float distanciaDoRaio = 3f;
    [SerializeField] private float tempoEntreFalas = 5f;

    [Header("Diálogos de Missão (Pai)")]
    [TextArea(2, 4)]
    [SerializeField] private string[] falasMissao;

    [Header("Diálogos de NPCs Genéricos")]
    [TextArea(2, 4)]
    [SerializeField] private string[] falasGenericas;

    [Header("Diálogo do Aliado")]
    [TextArea(2, 4)]
    [SerializeField] private string conversaAliado;

    [Header("Referências (opcional)")]
    public missa1 Manager1;
    public Manager1 top;

    // Encontrados automaticamente
    private GameObject fundoBranco;
    private TextMeshProUGUI texto;

    // Controle interno
    private int conversa = 0;
    private float falaTime = 0f;
    private bool emConversa = false;
    private bool jachamada = false;
    private bool ehDialogoMissao = false;

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
            else if (hit.collider.CompareTag("NPC_Aliado") && !jachamada)
            {
                Aliado_Ia aliado = hit.transform.GetComponent<Aliado_Ia>();
                if (aliado != null)
                    aliado.jaChamou = true;

                jachamada = true;
                IniciarDialogoSimples(conversaAliado);
            }
            else if (hit.collider.CompareTag("Pai") || hit.collider.CompareTag("missao"))
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

    private void IniciarDialogoSimples(string fala)
    {
        emConversa = true;
        ehDialogoMissao = false;
        falaTime = 0f;

        if (fundoBranco != null)
            fundoBranco.SetActive(true);

        if (texto != null)
            texto.text = fala;

        Invoke(nameof(FinalizarDialogo), tempoEntreFalas);
    }

    private void IniciarDialogoMissao()
    {
        // Tenta encontrar os scripts se não estiverem atribuídos
        if (Manager1 == null)
            Manager1 = FindFirstObjectByType<missa1>();

        if (top == null)
            top = FindFirstObjectByType<Manager1>();

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
        bool eraMissao = ehDialogoMissao;

        emConversa = false;
        ehDialogoMissao = false;
        conversa = 0;
        falaTime = 0f;

        if (fundoBranco != null)
            fundoBranco.SetActive(false);

        if (texto != null)
            texto.text = "";

        if (eraMissao)
        {
            // Destroi o objeto da missão
            GameObject objetoMissao = GameObject.FindWithTag("missao");
            if (objetoMissao != null)
            {
                Destroy(objetoMissao);
                Debug.Log("Objeto da missão destruído!");
            }

            // Ativa o Manager1 se existir
            if (top != null)
            {
                top.Ativar();
            }

            // Carrega a próxima cena (se tiver o script missa1)
            if (Manager1 != null)
            {
                SceneManager.LoadScene("Tuturial");
            }
        }

        Debug.Log("Diálogo finalizado!");
    }
}