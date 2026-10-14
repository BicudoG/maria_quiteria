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
    public float limiarLigar = 0.02f;      // quanto precisa andar para ligar
    public float limiarDesligar = 0.005f;  // quanto precisa parar para desligar

    private bool isWalking = false;

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
        float distancia = deslocamento.magnitude;

        // Histerese para evitar flicker
        if (distancia > limiarLigar)
            isWalking = true;
        else if (distancia < limiarDesligar)
            isWalking = false;

        anda = isWalking;

        // === ESTA LINHA É A QUE FALTAVA ===
        animator.SetBool("anda", anda);

        posicaoAnterior = transform.position;
    }
}