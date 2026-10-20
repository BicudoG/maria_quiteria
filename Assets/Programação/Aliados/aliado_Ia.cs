using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Aliado_Ia : MonoBehaviour
{
    [Header("Referências")]
    public string nomeJogador = "Player";
    public NavMeshAgent aliado;
    public Transform jogador;
    public Transform inimigo;

    [Header("Layers")]
    public LayerMask layerChao;
    public LayerMask layerJogador;
    public LayerMask layerInimigo;

    [Header("Patrulha")]
    public float raioPatrulha = 12f;
    public float distanciaChegada = 1.3f;

    [Header("Combate")]
    public float alcanceAtaque = 8f;
    public float alcanceVisao = 16f;
    public float cooldownAtaque = 1.4f;

    [Header("Estados")]
    public bool jaChamou;

    // Cache
    private sistema_Animacao anim;
    private Armas armas;

    // Controle de tiro
    private float proximoAtaque;
    private float tempoAnimTiro;
    private bool atirando;

    // Máquina de estados simples
    private enum Estado { Nenhum, Patrulhando, Seguindo, Parado, Atacando }
    private Estado estadoAtual = Estado.Nenhum;

    // Patrulha
    private Vector3 pontoPatrulha;
    private bool temPonto;

    private void Awake()
    {
        anim = GetComponent<sistema_Animacao>();
        armas = GetComponent<Armas>();
        aliado = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.Find(nomeJogador);
        if (playerObj != null)
            jogador = playerObj.transform;
    }

    private void Update()
    {
        if (aliado == null || !aliado.enabled) return;

        AtualizarAlvoInimigo();

        bool temInimigoPerto = inimigo != null &&
            (transform.position - inimigo.position).sqrMagnitude <= alcanceAtaque * alcanceAtaque;

        bool temJogadorPerto = jogador != null &&
            (transform.position - jogador.position).sqrMagnitude <= alcanceAtaque * alcanceAtaque;

        // Prioridade de comportamentos
        if (temInimigoPerto)
        {
            print("viinimigo");
            MudarEstado(Estado.Atacando);
            Atacar();
        }
        else if (jaChamou && jogador != null)
        {
            MudarEstado(Estado.Seguindo);
            Seguir();
        }
        else if (temJogadorPerto)
        {
            MudarEstado(Estado.Parado);
            FicarParado();
        }
        else
        {
            MudarEstado(Estado.Patrulhando);
            Patrulhar();
        }

        // Reseta apenas a animação de tiro
        if (atirando && Time.time >= tempoAnimTiro)
        {
            atirando = false;
            anim.atirou = false;
        }
    }

    // ====================== COMPORTAMENTOS ======================

    private void Atacar()
    {
        aliado.isStopped = true;

        if (inimigo != null)
            transform.LookAt(inimigo.position);

        if (Time.time >= proximoAtaque)
        {
            // Libera o tiro no script Armas
            if (armas != null)
                armas.jaAtacou = false;

            // Animação
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

    private IEnumerator ResetarFlagArma()
    {
        yield return null; // espera exatamente 1 frame
        if (armas != null)
            armas.jaAtacou = true;
    }

    private void Seguir()
    {
        aliado.isStopped = false;
        aliado.SetDestination(jogador.position);
    }

    private void FicarParado()
    {
        aliado.isStopped = true;

        if (jogador != null)
            transform.LookAt(jogador.position);
    }

    private void Patrulhar()
    {
        aliado.isStopped = false;

        if (!temPonto)
        {
            if (GerarPontoPatrulha())
                temPonto = true;
            else
                return;
        }

        aliado.SetDestination(pontoPatrulha);

        if ((transform.position - pontoPatrulha).sqrMagnitude <= distanciaChegada * distanciaChegada)
            temPonto = false;
    }

    // ====================== AUXILIARES ======================

    private void AtualizarAlvoInimigo()
    {
        // Só procura novo inimigo se o atual morreu ou não existe
        if (inimigo != null)
        {
            Sistema_Vida vida = inimigo.GetComponent<Sistema_Vida>();
            if (vida != null && vida.Vida > 0)
                return;
        }

        GameObject[] todos = GameObject.FindGameObjectsWithTag("Inimigo");
        float menorDistSq = float.MaxValue;
        Transform melhor = null;
        Vector3 minhaPos = transform.position;

        for (int i = 0; i < todos.Length; i++)
        {
            Sistema_Vida vida = todos[i].GetComponent<Sistema_Vida>();
            if (vida == null || vida.Vida <= 0) continue;

            float distSq = (minhaPos - todos[i].transform.position).sqrMagnitude;
            if (distSq < menorDistSq)
            {
                menorDistSq = distSq;
                melhor = todos[i].transform;
            }
        }

        inimigo = melhor;
    }

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
                // Andando sem arma
                anim.anda = true;
                anim.andaArma = false;
                anim.arma = true;          // ← sem arma
                anim.mirar = false;
                anim.atirou = false;
                break;

            case Estado.Seguindo:
                // Andando com arma
                anim.anda = true;
                anim.andaArma = true;
                anim.arma = true;           // ← com arma
                anim.mirar = false;
                anim.atirou = false;
                break;

            case Estado.Parado:
                // Parado mirando
                anim.anda = false;
                anim.andaArma = false;
                anim.arma = true;           // ← com arma
                anim.mirar = true;
                anim.atirou = false;
                break;

            case Estado.Atacando:
                // Parado mirando / atirando
                anim.anda = false;
                anim.andaArma = false;
                      
                                            // mirar e atirou são controlados no método Atacar()
                break;
        }
    }

    // ====================== MÉTODOS PÚBLICOS ======================

    public void ChamarAliado()
    {
        jaChamou = true;
    }

    public void LiberarAliado()
    {
        jaChamou = false;
    }
}