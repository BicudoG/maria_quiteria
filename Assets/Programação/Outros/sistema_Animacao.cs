using UnityEngine;

public class sistema_Animacao : MonoBehaviour
{
    public Animator animator;
    public bool npc;
    public bool militar;
    public bool anda;
    public bool andaArma = true;
    public bool mirar;
    public bool atirou;
    public bool arma;

    [Header("Duração da animação de tiro")]
    public float duracaoTiro = 1.0f; // Ajuste esse valor

    float contador;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!npc) { 
        
        float vertical = Input.GetAxis("Vertical");

        // Se estiver pressionando W ou S, define 'anda' como true
        if (vertical != 0)
        {
            anda = true;
        }
        else
        {
            anda = false;
        }
        }
        
        if (animator == null) return;

        animator.SetBool("anda", anda);

        if (npc || militar)
        {
            animator.SetBool("andaArma", andaArma);
            animator.SetBool("arma", arma); 
            animator.SetBool("mirar", mirar);
            animator.SetBool("atirar", atirou);

            // Reseta o bool depois do tempo
            if (atirou)
            {
                contador += Time.deltaTime;

                if (contador >= duracaoTiro)
                {
                    atirou = false;
                    contador = 0f;
                }
            }
            else
            {
                contador = 0f;
            }
        }
    }
}