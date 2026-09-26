using UnityEngine;

public class maria_quiteria : MonoBehaviour
{
    private CharacterController Maria;
    private Animator mariaCorpo;
    private bool madeira;
    void Start()
    {
        Maria = GetComponent<CharacterController>();
        mariaCorpo = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Movimento();
       
    }

    void Movimento()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direcao = new Vector3(horizontal, 0f, vertical);
        Vector3 movimento = new Vector3(horizontal, -9.81f, vertical);
        Maria.Move(movimento * Time.deltaTime * 5);
        if(!madeira) mariaCorpo.SetBool("estaParado", direcao == Vector3.zero);
       
    }
}
