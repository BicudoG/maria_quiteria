using UnityEngine;

public class Pai_IA : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeNormal = 2f;
    public float velocidadePerseguicao = 4.5f;
    public float tempoEntrePontos = 3f;          // Tempo parado entre um ponto e outro
    public float raioPatrulha = 10f;             // Até onde ele pode escolher pontos aleatórios

    [Header("Visão (só na frente)")]
    public float distanciaVisao = 12f;
    public float anguloVisao = 60f;              // Ângulo total de visão (30° para cada lado)
    public LayerMask layerJogador;              // Layer do jogador
    public Transform jogador;                   // Arraste a Maria aqui

    [Header("Estados")]
    public bool perseguindo = false;

    // Interno
    private Vector3 pontoAtual;
    private float tempoEspera;
    private bool esperando = false;

    private void Start()
    {
        EscolherNovoPonto();
    }

    private void Update()
    {
        DetectarJogador();

        if (perseguindo)
        {
            Perseguir();
        }
        else
        {
            Patrulhar();
        }
    }

    // ===================== PATRULHA =====================
    private void Patrulhar()
    {
        if (esperando)
        {
            tempoEspera -= Time.deltaTime;
            if (tempoEspera <= 0f)
            {
                esperando = false;
                EscolherNovoPonto();
            }
            return;
        }

        // Move em direção ao ponto
        transform.position = Vector3.MoveTowards(transform.position, pontoAtual, velocidadeNormal * Time.deltaTime);

        // Olha para o ponto
        Vector3 direcao = (pontoAtual - transform.position).normalized;
        if (direcao != Vector3.zero)
        {
            Quaternion rotacao = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacao, 5f * Time.deltaTime);
        }

        // Chegou no ponto?
        if (Vector3.Distance(transform.position, pontoAtual) < 0.3f)
        {
            esperando = true;
            tempoEspera = tempoEntrePontos;
        }
    }

    private void EscolherNovoPonto()
    {
        Vector2 pontoAleatorio = Random.insideUnitCircle * raioPatrulha;
        pontoAtual = new Vector3(transform.position.x + pontoAleatorio.x, transform.position.y, transform.position.z + pontoAleatorio.y);
    }

    // ===================== PERSEGUIÇÃO =====================
    private void Perseguir()
    {
        if (jogador == null) return;

        transform.position = Vector3.MoveTowards(transform.position, jogador.position, velocidadePerseguicao * Time.deltaTime);

        // Olha para o jogador
        Vector3 direcao = (jogador.position - transform.position).normalized;
        direcao.y = 0;
        if (direcao != Vector3.zero)
        {
            Quaternion rotacao = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacao, 8f * Time.deltaTime);
        }
    }

    // ===================== VISÃO SÓ NA FRENTE =====================
    private void DetectarJogador()
    {
        if (jogador == null) return;

        Vector3 direcaoParaJogador = (jogador.position - transform.position).normalized;
        float angulo = Vector3.Angle(transform.forward, direcaoParaJogador);

        // Só detecta se estiver dentro do ângulo de visão e da distância
        if (angulo < anguloVisao / 2f && Vector3.Distance(transform.position, jogador.position) <= distanciaVisao)
        {
            // Raycast para não ver através de paredes
            if (Physics.Raycast(transform.position + Vector3.up * 1.2f, direcaoParaJogador, out RaycastHit hit, distanciaVisao))
            {
                if (hit.transform == jogador || hit.transform.CompareTag("Maria_quiteria_Civil"))
                {
                    perseguindo = true;
                    return;
                }
            }
        }

        // Se perdeu de vista por um tempo, volta a patrulhar (opcional)
        // perseguindo = false;
    }

    // Desenha a visão no Editor (só para debug)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioPatrulha);

        Gizmos.color = Color.red;
        Vector3 direita = Quaternion.Euler(0, anguloVisao / 2f, 0) * transform.forward * distanciaVisao;
        Vector3 esquerda = Quaternion.Euler(0, -anguloVisao / 2f, 0) * transform.forward * distanciaVisao;

        Gizmos.DrawLine(transform.position, transform.position + direita);
        Gizmos.DrawLine(transform.position, transform.position + esquerda);
        Gizmos.DrawLine(transform.position + direita, transform.position + esquerda);
    }
}