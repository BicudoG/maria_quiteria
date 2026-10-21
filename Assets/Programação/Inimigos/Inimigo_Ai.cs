using UnityEngine;
using UnityEngine.AI;

public class Inimigo_Ai : MonoBehaviour
{
    [Header("Missão")]
    public string tagMissao = "missao";
    public float distanciaMaximaDaMissao = 25f; // se ficar mais longe que isso, vai até a missão
    private Transform pontoMissao;

    [Header("Referências")]
    public string nomeJogador = "Player";
    public NavMeshAgent agent;
    public Transform alvoAtual; // pode ser NPC_Aliado ou o Jogador

    [Header("Layers")]
    public LayerMask layerChao;

    [Header("Patrulha")]
    public float raioPatrulha = 12f;
    public float distanciaChegada = 1.3f;

    [Header("Combate")]
    public float alcanceAtaque = 8f;
    public float alcanceVisao = 16f;
    public float cooldownAtaque = 1.5f;

    // Cache
    private sistema_Animacao anim;
    private Armas armas;
    private float proximoAtaque;
    private float tempoAnimTiro;
    private bool atirando;

    // Estados
    private enum Estado { Nenhum, Patrulhando, Perseguindo, Atacando }
    private Estado estadoAtual = Estado.Nenhum;

    // Patrulha
    private Vector3 pontoPatrulha;
    private bool temPonto;

    // Busca de alvos
    private float tempoProximaBusca;

    private void Awake()
    {
        anim = GetComponent<sistema_Animacao>();
        armas = GetComponent<Armas>();
        agent = GetComponent<NavMeshAgent>();
        GameObject missaoObj = GameObject.FindGameObjectWithTag(tagMissao);
        if (missaoObj != null)
            pontoMissao = missaoObj.transform;
    }

    private void Update()
    {
        if (agent == null || !agent.enabled) return;

        // Atualiza o melhor alvo a cada 0.4s
        if (Time.time >= tempoProximaBusca)
        {
            tempoProximaBusca = Time.time + 0.4f;
            AtualizarAlvo();
        }

        bool temAlvo = alvoAtual != null;
        bool noAlcanceAtaque = temAlvo &&
            (transform.position - alvoAtual.position).sqrMagnitude <= alcanceAtaque * alcanceAtaque;

        bool naVisao = temAlvo &&
            (transform.position - alvoAtual.position).sqrMagnitude <= alcanceVisao * alcanceVisao;

        // ========== PRIORIDADE DE COMPORTAMENTOS ==========
        if (noAlcanceAtaque)
        {
            MudarEstado(Estado.Atacando);
            Atacar();
        }
        else if (naVisao)
        {
            MudarEstado(Estado.Perseguindo);
            Perseguir();
        }
        else
        {
            MudarEstado(Estado.Patrulhando);
            Patrulhar();
        }

        // Reseta animação de tiro
        if (atirando && Time.time >= tempoAnimTiro)
        {
            atirando = false;
            anim.atirou = false;
        }
    }

    // ====================== COMPORTAMENTOS ======================

    private void Atacar()
    {
        agent.isStopped = true;

        if (alvoAtual != null)
            transform.LookAt(alvoAtual.position);

        if (Time.time >= proximoAtaque)
        {
            // Libera o tiro no script Armas
            if (armas != null)
                armas.jaAtacou = false;

            anim.mirar = false;
            anim.atirou = true;
            atirando = true;
            tempoAnimTiro = Time.time + 0.15f;

            proximoAtaque = Time.time + cooldownAtaque;
        }
        else
        {
            if (!atirando)
            {
                anim.mirar = true;
                anim.atirou = false;
            }
        }
    }

    private void Perseguir()
    {
        agent.isStopped = false;

        if (alvoAtual != null)
            agent.SetDestination(alvoAtual.position);
    }

    private void Patrulhar()
    {
        // Se não encontrou a missão, faz patrulha normal no lugar
        if (pontoMissao == null)
        {
            agent.isStopped = false; // ou aliado.isStopped / inimigo.isStopped

            if (!temPonto)
            {
                if (GerarPontoPatrulha())
                    temPonto = true;
                else
                    return;
            }

            agent.SetDestination(pontoPatrulha);

            if ((transform.position - pontoPatrulha).sqrMagnitude <= distanciaChegada * distanciaChegada)
                temPonto = false;

            return;
        }

        // ===== LÓGICA COM MISSÃO =====
        float distSqMissao = (transform.position - pontoMissao.position).sqrMagnitude;
        float maxDistSq = distanciaMaximaDaMissao * distanciaMaximaDaMissao;

        // Está longe da missão? Vai até ela primeiro
        if (distSqMissao > maxDistSq)
        {
            agent.isStopped = false;
            agent.SetDestination(pontoMissao.position);
            temPonto = false; // força gerar novo ponto quando chegar
            return;
        }

        // Está perto → patrulha ao redor da missão
        agent.isStopped = false;

        if (!temPonto)
        {
            if (GerarPontoPatrulhaAoRedorDaMissao())
                temPonto = true;
            else
                return;
        }

        agent.SetDestination(pontoPatrulha);

        if ((transform.position - pontoPatrulha).sqrMagnitude <= distanciaChegada * distanciaChegada)
            temPonto = false;
    }
    private bool GerarPontoPatrulhaAoRedorDaMissao()
    {
        if (pontoMissao == null) return false;

        Vector3 randomDir = Random.insideUnitSphere * raioPatrulha;
        randomDir.y = 0f;
        Vector3 pontoTentativa = pontoMissao.position + randomDir;

        if (NavMesh.SamplePosition(pontoTentativa, out NavMeshHit hit, raioPatrulha, NavMesh.AllAreas))
        {
            pontoPatrulha = hit.position;
            return true;
        }
        return false;
    }

    // ====================== SISTEMA DE ALVO (PRIORIDADE) ======================

    private void AtualizarAlvo()
    {
        Transform melhorAlvo = null;
        float menorDistSq = float.MaxValue;
        Vector3 minhaPos = transform.position;

        // 1º PRIORIDADE: NPC_Aliado vivos
        GameObject[] aliados = GameObject.FindGameObjectsWithTag("NPC_Aliado");
        for (int i = 0; i < aliados.Length; i++)
        {
            Sistema_Vida vida = aliados[i].GetComponent<Sistema_Vida>();
            if (vida == null || vida.Vida <= 0) continue;

            float distSq = (minhaPos - aliados[i].transform.position).sqrMagnitude;
            if (distSq < menorDistSq)
            {
                menorDistSq = distSq;
                melhorAlvo = aliados[i].transform;
            }
        }

        // Se encontrou um aliado vivo, usa ele
        if (melhorAlvo != null)
        {
            alvoAtual = melhorAlvo;
            return;
        }

        // 2º PRIORIDADE: Jogador
        GameObject playerObj = GameObject.Find(nomeJogador);
        if (playerObj != null)
        {
            Sistema_Vida vidaPlayer = playerObj.GetComponent<Sistema_Vida>();
            if (vidaPlayer == null || vidaPlayer.Vida > 0)
            {
                alvoAtual = playerObj.transform;
                return;
            }
        }

        // Nenhum alvo válido
        alvoAtual = null;
    }

    // ====================== AUXILIARES ======================

    private bool GerarPontoPatrulha()
    {
        Vector3 randomDir = Random.insideUnitSphere * raioPatrulha;
        randomDir += transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, raioPatrulha, NavMesh.AllAreas))
        {
            pontoPatrulha = hit.position;
            return true;
        }
        return false;
    }

    private void MudarEstado(Estado novo)
    {
        if (estadoAtual == novo) return;

        estadoAtual = novo;

        switch (novo)
        {
            case Estado.Patrulhando:
                // Andando com arma
                anim.anda = true;
                anim.andaArma = true;
                anim.arma = true;
                anim.mirar = false;
                anim.atirou = false;
                break;

            case Estado.Perseguindo:
                // Correndo com arma
                anim.anda = true;
                anim.andaArma = true;
                anim.arma = true;
                anim.mirar = false;
                anim.atirou = false;
                break;

            case Estado.Atacando:
                // Parado mirando / atirando
                anim.anda = false;
                anim.andaArma = false;
                anim.arma = true;
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcanceVisao);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcanceAtaque);
    }
}