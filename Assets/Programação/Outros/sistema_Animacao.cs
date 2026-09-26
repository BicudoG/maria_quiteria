using UnityEngine;

public class sistema_Animacao : MonoBehaviour
{
    private Animator animator;
    public bool anda;
    public bool andaArma;
    public bool mirar;
    public bool atirou;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animator != null)
        {
            if(anda) animator.SetBool("anda", true);
            if(andaArma) animator.SetBool("andaArma", true);
            if(mirar) animator.SetBool("mirar", true);
            if(atirou) animator.SetBool("atirar",true);
        }
    }
}
