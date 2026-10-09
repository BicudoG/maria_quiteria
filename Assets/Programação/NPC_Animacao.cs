using UnityEngine;

public class NPC_Animacao : MonoBehaviour
{
    public Animator animator;
    private Vector3 posicaoAnterior;

    public bool anda;
    public bool andaArma;
    public bool mirar;
    public bool atirou;

    [Header("Detecção de Movimento")]
    public float limiarMovimento = 0.001f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        posicaoAnterior = transform.position;
    }

    void Update()
    {
        if (animator == null) return;

        Vector3 deslocamento = transform.position - posicaoAnterior;
        deslocamento.y = 0f;

        anda = deslocamento.sqrMagnitude > limiarMovimento * limiarMovimento;

        posicaoAnterior = transform.position;
    }
}