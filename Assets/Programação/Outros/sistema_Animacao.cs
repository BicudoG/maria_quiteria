using UnityEngine;

public class sistema_Animacao : MonoBehaviour
{
    private Animator animator;
    public bool npc;
    public bool anda;
    public bool andaArma;
    public bool mirar;
    public bool atirou;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        // Agora seta true OU false corretamente
        animator.SetBool("anda", anda);
        if (!npc)
        {
        animator.SetBool("andaArma", andaArma);
        animator.SetBool("mirar", mirar);
        animator.SetBool("atirar", atirou);
        }
        
    }
}