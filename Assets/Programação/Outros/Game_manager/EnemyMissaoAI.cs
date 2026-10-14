using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MissaoManager : MonoBehaviour
{
    [Header("Configurações")]
    public string missaoTag = "missao";
    public float intervaloAtualizacao = 0.4f;

    [Header("Referências")]
    public Pai_IA iaInimigo;                     // Seu script de IA

    [Header("Lista de Missões (em ordem)")]
    public GameObject[] missoes;                 // Arraste todas as missões aqui na ordem que quiser

    // ==================== VARIÁVEIS INTERNAS ====================
    private List<Transform> missoesAtivas = new List<Transform>();
    private Transform missaoAtual;
    private int indiceAtual = 0;
    private float tempoProximaAtualizacao;

    void Start()
    {
        // Ativa só a primeira missão no começo
        AtivarMissaoPorIndice(0);
        EncontrarProximaMissao();
    }

    void Update()
    {
        if (Time.time >= tempoProximaAtualizacao)
        {
            AtualizarListaMissoes();
            tempoProximaAtualizacao = Time.time + intervaloAtualizacao;
        }

        VerificarMissaoDestruida();
    }

    // ==================== SISTEMA PRINCIPAL ====================

    public void AtualizarListaMissoes()
    {
        missoesAtivas.Clear();

        // Usa a lista manual se existir, senão busca por tag
        if (missoes != null && missoes.Length > 0)
        {
            foreach (GameObject obj in missoes)
            {
                if (obj != null && obj.activeInHierarchy)
                    missoesAtivas.Add(obj.transform);
            }
        }
        else
        {
            GameObject[] objetos = GameObject.FindGameObjectsWithTag(missaoTag);
            foreach (GameObject obj in objetos)
            {
                if (obj != null && obj.activeInHierarchy)
                    missoesAtivas.Add(obj.transform);
            }
        }
    }

    public void EncontrarProximaMissao()
    {
        AtualizarListaMissoes();

        if (missoesAtivas.Count > 0)
        {
            missaoAtual = missoesAtivas[0];
            Debug.Log($"<color=green>Nova missão ativa:</color> {missaoAtual.name}");
        }
        else
        {
            missaoAtual = null;
            Debug.LogWarning("Nenhuma missão ativa restante!");
        }
    }

    void VerificarMissaoDestruida()
    {
        // Se a missão atual sumiu (foi destruída ou desativada)
        if (missaoAtual == null || !missaoAtual.gameObject.activeInHierarchy)
        {
            Debug.Log("<color=yellow>Missão perdida!</color>");

            // 1. Ativa a patrulha
            if (iaInimigo != null)
                iaInimigo.ActivatePatrol();

            // 2. Ativa a próxima missão da lista
            AtivarProximaMissao();

            // 3. Atualiza a referência
            EncontrarProximaMissao();
        }
    }

    // ==================== ATIVAR PRÓXIMA MISSÃO ====================

    void AtivarProximaMissao()
    {
        if (missoes == null || missoes.Length == 0) return;

        // Desativa a missão atual (se ainda existir)
        if (indiceAtual < missoes.Length && missoes[indiceAtual] != null)
        {
            missoes[indiceAtual].SetActive(false);
        }

        // Avança para a próxima
        indiceAtual++;

        // Se ainda tiver missão na lista, ativa ela
        if (indiceAtual < missoes.Length && missoes[indiceAtual] != null)
        {
            missoes[indiceAtual].SetActive(true);
            Debug.Log($"<color=cyan>Próxima missão ativada:</color> {missoes[indiceAtual].name}");
        }
        else
        {
            Debug.Log("<color=orange>Todas as missões foram concluídas!</color>");
        }
    }

    void AtivarMissaoPorIndice(int index)
    {
        if (missoes == null || missoes.Length == 0) return;

        // Desativa todas
        for (int i = 0; i < missoes.Length; i++)
        {
            if (missoes[i] != null)
                missoes[i].SetActive(false);
        }

        // Ativa só a desejada
        if (index >= 0 && index < missoes.Length && missoes[index] != null)
        {
            missoes[index].SetActive(true);
            indiceAtual = index;
        }
    }

    // ==================== MÉTODOS PÚBLICOS ====================

    public void AtivarSistemaPatrulha()
    {
        EncontrarProximaMissao();
        if (iaInimigo != null)
            iaInimigo.ActivatePatrol();
    }

    public void AtivarSistemaPerseguicao()
    {
        EncontrarProximaMissao();
        if (iaInimigo != null)
            iaInimigo.ActivateChase();
    }

    public Transform GetMissaoAtual()
    {
        return missaoAtual;
    }
}