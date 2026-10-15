using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Sistema_Vida : MonoBehaviour
{
    public bool aliado;
    public bool inimigo;
    public bool jogador;
    public int Vida;
    private bool estaMorto = false;
    private void Awake()
    {
        if (CompareTag("Inimigo")) inimigo = true;
        if (CompareTag("NPC_Aliado")) aliado = true;
        if (!aliado && !inimigo && jogador) jogadorvida();
        if (!aliado && inimigo && !jogador) inimigovida();
        if (aliado && !inimigo && !jogador) aliadovida();
    }
    private void Update()
    {
        if (estaMorto) return;
        if ((inimigo || aliado) && Vida <= 0)
        {
            Morrer();
        }
    }
    private void Morrer()
    {
        Vida = 0;
        estaMorto = true;
        // Desativa os componentes de inteligência artificial
        if (inimigo)
        {
            Inimigo_Ai scriptInimigo = GetComponent<Inimigo_Ai>();
            if (scriptInimigo != null) scriptInimigo.enabled = false;
        }
        else if (aliado)
        {
            Aliado_Ia scriptAliado = GetComponent<Aliado_Ia>();
            if (scriptAliado != null) scriptAliado.enabled = false;
        }
        // Desativa o NavMeshAgent para liberar a gravidade
        NavMeshAgent agente = GetComponent<NavMeshAgent>();
        if (agente != null) agente.enabled = false;
        // Gerencia e configura o Rigidbody para simular a queda física
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.isKinematic = false;

        // === CORREÇÃO: Destrói depois de 5 segundos ===
        StartCoroutine(DestruirDepoisDe5Segundos());
    }

    private IEnumerator DestruirDepoisDe5Segundos()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    public void jogadorvida() => Vida = 100;
    public void inimigovida() => Vida = 100;
    public void aliadovida() => Vida = 100;
}