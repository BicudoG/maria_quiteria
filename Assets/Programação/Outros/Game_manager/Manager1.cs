using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager1 : MonoBehaviour
{
    private sistema_Animacao maria;
    public GameObject game;   
    public GameObject mosquete;
    public GameObject [] bonecos;
    public Collider meuColisor;
    private void Start()
    {
       

        // Desativa o colisor ao iniciar a fase
        if (meuColisor != null)
        {
            meuColisor.enabled = false;
        }
    }
    private void Update()
    {
        if(bonecos == null)
        {
            SceneManager.LoadScene("Fase_1");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        maria = other.GetComponent<sistema_Animacao>();

        if (maria != null)
        {
            maria.andaArma = true;
            game.SetActive(false); // Desativa o objeto de pickup
            mosquete.SetActive(true);
        }
    }
    public void Ativar()
    {
        meuColisor.enabled=true;
    }
}