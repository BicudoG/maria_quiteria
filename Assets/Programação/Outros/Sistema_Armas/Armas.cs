using System;
using UnityEngine;

public class Armas : MonoBehaviour
{
    [Header("Configuração de Armas")]
    [SerializeField] private bool[] armaAtual = new bool[3]; // 0 = Mosquete, 1 = Blunderbuss, 2 = Hand Mortar
    [SerializeField] private bool npc;

    [Header("Alcance e Visão")]
    [SerializeField] private float alcance = 55f;
    [SerializeField] private float anguloVisao = 45f;

    [Header("Referências")]
    [SerializeField] private Transform fonte;
    [SerializeField] private Transform[] fonteMosquete;
    [SerializeField] private GameObject efeitoMosquetePrefab;   // ← Prefab do efeito de partícula
    [SerializeField] private GameObject bomba;
    [SerializeField] private sistema_Animacao anima;

    [Header("Configurações de Ataque")]
    public float forcaDoArremesso = 10f;
    [SerializeField] private float tempoEntreDisparos = 1.5f;

    [HideInInspector] public bool jaAtacou;
    private float tempoProximoAtaque;

    private void Awake()
    {
        if (anima == null)
            anima = GetComponent<sistema_Animacao>();

        if (armaAtual == null || armaAtual.Length < 3)
            armaAtual = new bool[3];

        if (!armaAtual[0] && !armaAtual[1] && !armaAtual[2])
            DefinirArmaAtiva(0);
    }

    private void Update()
    {
        if (Time.time < tempoProximoAtaque) return;

        if (!npc)
        {
            if (Input.GetMouseButtonDown(0) && !jaAtacou)
            {
                RealizarAtaque();
            }
        }
        else
        {
            if (!jaAtacou)
            {
                RealizarAtaque();
            }
        }
    }

    private void RealizarAtaque()
    {
        jaAtacou = true;
        tempoProximoAtaque = Time.time + tempoEntreDisparos;

        if (anima != null)
            anima.atirou = true;

        Atacar();

        if (!npc)
        {
            Invoke(nameof(ResetDisparo), tempoEntreDisparos);
        }
    }

    private void ResetDisparo()
    {
        jaAtacou = false;
    }

    private void DefinirArmaAtiva(int indice)
    {
        for (int i = 0; i < armaAtual.Length; i++)
            armaAtual[i] = (i == indice);
    }

    private void Atacar()
    {
        if (armaAtual == null || armaAtual.Length < 3) return;

        if (armaAtual[0]) Mosquete();
        else if (armaAtual[1]) Blunderbuss();
        else if (armaAtual[2]) HandMortar();
    }

    // ==================== SISTEMA DE PARTÍCULA (usando Prefab) ====================
    private void TocarEfeitoNaBoca(Transform bocaDaArma)
    {
        if (efeitoMosquetePrefab == null || bocaDaArma == null) return;

        // Cria uma instância do Prefab na posição e rotação da boca da arma
        GameObject efeito = Instantiate(efeitoMosquetePrefab, bocaDaArma.position, transform.rotation);

        // Pega o ParticleSystem e dá Play
        ParticleSystem ps = efeito.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        // Destrói o efeito depois de alguns segundos (ajuste o tempo se necessário)
        Destroy(efeito, 3f);
    }

    // ==================== ARMAS ====================
    private void Mosquete()
    {
        alcance = 55f;
        if (fonteMosquete == null || fonteMosquete.Length == 0) return;

        Transform pontoAtual = fonteMosquete[0];

        TocarEfeitoNaBoca(pontoAtual);

        Vector3 direcao = transform.forward;
        if (Physics.Raycast(pontoAtual.position, direcao, out RaycastHit hit, alcance))
        {
            AplicarDano(hit.collider);
        }

        Debug.DrawRay(pontoAtual.position, direcao * alcance, Color.red, 1f);
    }

    private void Blunderbuss()
    {
        alcance = 5f;
        if (fonte == null) return;

        TocarEfeitoNaBoca(fonte);

        Collider[] alvos = Physics.OverlapSphere(fonte.position, alcance);
        foreach (var colisor in alvos)
        {
            if (colisor.gameObject == gameObject) continue;

            Vector3 direcaoParaAlvo = (colisor.transform.position - fonte.position).normalized;
            float angulo = Vector3.Angle(fonte.forward, direcaoParaAlvo);

            if (angulo < anguloVisao * 0.5f)
            {
                AplicarDano(colisor);
            }
        }
    }

    private void HandMortar()
    {
        if (fonteMosquete == null || fonteMosquete.Length == 0 || bomba == null) return;

        TocarEfeitoNaBoca(fonteMosquete[0]);

        // Pega a posição local do ponto de origem
        Vector3 posicaoLocal = fonteMosquete[0].localPosition;

        GameObject novaBomba = Instantiate(bomba, fonteMosquete[0].position, fonteMosquete[0].rotation);
        Rigidbody rb = novaBomba.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direcao = fonteMosquete[0].forward;
            rb.AddForce(direcao * forcaDoArremesso, ForceMode.Impulse);
        }
    }

    // ==================== DANO ====================
    private void AplicarDano(Collider colisor)
    {
        Sistema_Vida vidaAlvo = colisor.GetComponent<Sistema_Vida>();
        if (vidaAlvo == null)
            vidaAlvo = colisor.GetComponentInParent<Sistema_Vida>();

        if (vidaAlvo == null)
        {
            Debug.LogWarning("Objeto " + colisor.name + " não tem Sistema_Vida");
            return;
        }

        if (!npc || gameObject.CompareTag("NPC_Aliado"))
        {
            if (colisor.CompareTag("Inimigo") || colisor.CompareTag("Maria_incompleta"))
            {
                vidaAlvo.Vida -= 20;
                Debug.Log("Dano aplicado! Vida restante: " + vidaAlvo.Vida);
            }
        }
        else if (gameObject.CompareTag("Inimigo"))
        {
            if (colisor.CompareTag("Player") || colisor.CompareTag("NPC_Aliado") || colisor.CompareTag("Maria_incompleta"))
            {
                vidaAlvo.Vida -= 20;
                Debug.Log("Dano aplicado pelo inimigo! Vida restante: " + vidaAlvo.Vida);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (fonte == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fonte.position, alcance);
    }
}