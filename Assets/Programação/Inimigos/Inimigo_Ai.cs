using UnityEngine;
using UnityEngine.AI;

public class Inimigo_Ai : MonoBehaviour
{
    public string nome;
    public NavMeshAgent inimigo;
    public Transform jogador;
    public LayerMask cao, isJogador;
    public Vector3 pontoDeCaminhada;
    public bool pontoConferido;
    public float raioPonto;

    public float tempoDoUltimoAtaque;
    bool jaAtacou;
    public float alcansoAtaque, visaoalcance;
    public bool jogadorNaVisao, jogadorNoalcance;

    private Armas scriptArmas;
    private sistema_Animacao scriptAnimacao;

    private void Start()
    {
        scriptAnimacao = GetComponent<sistema_Animacao>();
    }
    private void Awake()
    {
        jogador = GameObject.Find(nome)?.transform;
        inimigo = GetComponent<NavMeshAgent>();
        scriptArmas = GetComponent<Armas>();

    }

    private void Update()
    {
        if (inimigo == null || !inimigo.enabled) return;

        jogadorNaVisao = Physics.CheckSphere(transform.position, visaoalcance, isJogador);
        jogadorNoalcance = Physics.CheckSphere(transform.position, alcansoAtaque, isJogador);

        if (!jogadorNaVisao && !jogadorNoalcance) Patrulhando();
        if (jogadorNaVisao && !jogadorNoalcance) Correndo();
        if (jogadorNaVisao && jogadorNoalcance) Atacandoo();
    }

    private void Patrulhando()
    {
        if (!pontoConferido) ProcurandoNovo();
        if (pontoConferido) inimigo.SetDestination(pontoDeCaminhada);
        scriptAnimacao.anda = true;
        scriptAnimacao.andaArma = false;
        Vector3 distanciaDeCaminhada = transform.position - pontoDeCaminhada;
        if (distanciaDeCaminhada.magnitude < 1f) pontoConferido = false;
    }

    private void ProcurandoNovo()
    {
        float ramdoZ = Random.Range(-raioPonto, raioPonto);
        float ramdoX = Random.Range(-raioPonto, raioPonto);

        pontoDeCaminhada = new Vector3(transform.position.x + ramdoX, transform.position.y, transform.position.z + ramdoZ);
        if (Physics.Raycast(pontoDeCaminhada, -transform.up, 2f, cao)) pontoConferido = true;
    }

    private void Correndo()
    {
        if (jogador != null) inimigo.SetDestination(jogador.position);
        scriptAnimacao.andaArma = true;
        scriptAnimacao.anda = false;
        scriptAnimacao.mirar = false;
    }

    private void Atacandoo()
    {
        inimigo.SetDestination(transform.position);
        scriptAnimacao.andaArma = false;
        if (jogador != null) transform.LookAt(jogador);

        if (!jaAtacou)
        {
            scriptAnimacao.mirar = false;
            scriptAnimacao.atirou = true;
            jaAtacou = true;
            if (scriptArmas != null) scriptArmas.jaAtacou = false; // Sinaliza à arma que ela pode atirar uma vez

            Invoke(nameof(ResetAttack), tempoDoUltimoAtaque);
        }
    }
    private void ResetAttack()
    {
        jaAtacou = false;
        if (scriptArmas != null) scriptArmas.jaAtacou = true;
        scriptAnimacao.atirou = false;
        scriptAnimacao.mirar = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visaoalcance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcansoAtaque);
    }
}