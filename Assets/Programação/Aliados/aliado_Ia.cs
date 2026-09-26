using UnityEngine;
using UnityEngine.AI;

public class Aliado_Ia : MonoBehaviour
{
    public string nome;
    public NavMeshAgent aliado;
    public Transform jogador, inimigo;
    public LayerMask cao, isJogador, isInimigo;
    public Vector3 pontoDeCaminhada;
    public bool pontoConferido;
    public float raioPonto;
    private float tempoProximaBusca = 0f;

    //ataque
    public float tempoDoUltimoAtaque;
    public bool jaAtacou, jaChamou;
    public float alcansoAtaque, visaoalcance;
    public bool jogadorNaVisao, jogadorNoalcance, inimigoNoAlcance, InimigoNaVisao;

    // Criado um temporizador real para o método ResetAttack funcionar
    private float acumuladorTempo = 0f;
    private sistema_Animacao scriptAnimacao;
    private void Awake()
    {
        scriptAnimacao = GetComponent<sistema_Animacao>();
        GameObject objJogador = GameObject.Find(nome);
        if (objJogador != null) jogador = objJogador.transform;

        aliado = GetComponent<NavMeshAgent>();
        GetComponent<Armas>();

        // Busca o primeiro inimigo vivo logo no início do jogo
        BuscarProximoInimigo();
    }


    private void Update()
    {
        if (aliado == null || !aliado.enabled) return;

        // NOVO: Recalcula quem é o inimigo vivo mais próximo a cada meio segundo para focar no mais perto primeiro
        if (Time.time >= tempoProximaBusca)
        {
            tempoProximaBusca = Time.time + 0.5f;
            BuscarProximoInimigo();
        }

        jogadorNaVisao = Physics.CheckSphere(transform.position, visaoalcance, isJogador);
        jogadorNoalcance = Physics.CheckSphere(transform.position, alcansoAtaque, isJogador);

        // AJUSTADO: Evita checagens de esfera desnecessárias se não houver nenhum inimigo vivo na cena
        inimigoNoAlcance = inimigo != null && Physics.CheckSphere(transform.position, alcansoAtaque, isInimigo);
        InimigoNaVisao = inimigo != null && Physics.CheckSphere(transform.position, visaoalcance, isInimigo);

        if (jaChamou)
        {
            Seguindo();
        }

        if (!jogadorNaVisao && !jogadorNoalcance) Patrulhando();
        if (jogadorNaVisao && jogadorNoalcance) parado();

        // CORRIGIDO: Executa a contagem do tempo frame a frame de forma contínua enquanto o aliado estiver esperando o reset do ataque
        if (jaAtacou)
        {
            ContarTempoReset();
        }
    }

    private void Patrulhando()
    {
        scriptAnimacao.anda = true;
        scriptAnimacao.andaArma = false;
        if (!pontoConferido) ProcurandoNovo();
        if (pontoConferido == true)
            aliado.SetDestination(pontoDeCaminhada);
        Vector3 distanciaDeCaminhada = transform.position - pontoDeCaminhada;
        if (distanciaDeCaminhada.magnitude < 1f)
            pontoConferido = false;
    }

    private void ProcurandoNovo()
    {
        float ramdoZ = Random.Range(-raioPonto, raioPonto);
        float ramdoX = Random.Range(-raioPonto, raioPonto);

        pontoDeCaminhada = new Vector3(transform.position.x + ramdoX, transform.position.y, transform.position.z + ramdoZ);
        if (Physics.Raycast(pontoDeCaminhada, -transform.up, 2f, cao))
            pontoConferido = true;
    }

    void parado()
    {
        if (!inimigoNoAlcance)
        {
            scriptAnimacao.andaArma = true;
            scriptAnimacao.anda = false;
            scriptAnimacao.mirar = false;
            aliado.SetDestination(transform.position);
            if (jogador != null) transform.LookAt(jogador);
        }

        if (inimigoNoAlcance)
        {
            aliado.SetDestination(transform.position);
            if (inimigo != null) transform.LookAt(inimigo);
            scriptAnimacao.andaArma = false;
            scriptAnimacao.mirar = true;
            if (!jaAtacou)
            {
                Armas armas = GetComponent<Armas>();
                jaAtacou = true;
                scriptAnimacao.mirar = false;
                scriptAnimacao.atirou = true;
                if (armas != null) armas.jaAtacou = false;
                acumuladorTempo = 0f;
                Invoke(nameof(ResetAttack), tempoDoUltimoAtaque);
            }
        }
    }

    void Seguindo()
    {
        if (jogador != null) aliado.SetDestination(jogador.position);
        scriptAnimacao.andaArma = true;
        scriptAnimacao.anda = false;
        scriptAnimacao.mirar = false;
    }


    private void ContarTempoReset()
    {
        acumuladorTempo += Time.deltaTime;
    }

    private void ResetAttack()
    {
        Armas armas = GetComponent<Armas>();

        if (acumuladorTempo >= 6f)
        {
            jaAtacou = false;
            scriptAnimacao.atirou = false;
            if (armas != null) armas.jaAtacou = true;
        }
        else
        {
            Invoke(nameof(ResetAttack), 0.1f);
        }
    }

    void BuscarProximoInimigo()
    {
        // Encontra todos os objetos com a tag "Inimigo" na cena
        GameObject[] inimigosExistentes = GameObject.FindGameObjectsWithTag("Inimigo");
        float menorDistancia = Mathf.Infinity;
        Transform inimigoMaisProximo = null;

        foreach (GameObject objInimigo in inimigosExistentes)
        {
            // Verifica se o inimigo tem o script Sistema_Vida e se ele ainda está VIVO
            Sistema_Vida vidaInimigo = objInimigo.GetComponent<Sistema_Vida>();
            if (vidaInimigo != null && vidaInimigo.Vida > 0)
            {
                // Calcula a distância até este inimigo
                float distancia = Vector3.Distance(transform.position, objInimigo.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    inimigoMaisProximo = objInimigo.transform;
                }
            }
        }

        // Define o alvo do aliado como o inimigo vivo mais próximo encontrado (ou null se todos morreram)
        inimigo = inimigoMaisProximo;
    }
}