using UnityEngine;

public class Manager : MonoBehaviour
{
    public Transform inicio;
    public GameObject maria;
    void Start()
    {
        Instantiate(maria, inicio.position, inicio.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        string tagDoObjeto = other.tag;
        Debug.Log("A tag do objeto é: " + tagDoObjeto);
    }
    void Update()
    {
        
    }
}
