using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class maria_quiteria : MonoBehaviour
{
    public CharacterController controller;
    public Animator mariaCorpo;
    public Camera cam;

    public float moveSpeed = 6f;
    public float rotationSpeed = 2f;
    public float gravity = -20f;

    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mariaCorpo = GetComponent<Animator>();
        cam = Camera.main;
     
    }

    void Update()
    {
        // Movimento (W/S)
        float move = Input.GetAxis("Vertical");
        Vector3 direction = transform.forward * move * moveSpeed;
        controller.Move(direction * Time.deltaTime);

        // Gravidade
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Rotação para o mouse
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.up, Vector3.zero);

            if (ground.Raycast(ray, out float distance))
            {
                Vector3 lookPoint = ray.GetPoint(distance);
                Vector3 lookDir = lookPoint - transform.position;
                lookDir.y = 0;

                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }
        }

        // Animação
        mariaCorpo.SetBool("estaParado", move == 0);
    }
}