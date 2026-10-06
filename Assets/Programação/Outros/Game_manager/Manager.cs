using UnityEngine;

public class Manager : MonoBehaviour
{
    public Transform inicio;
    public GameObject maria;
    void Start()
    {
        Instantiate(maria, inicio.position, inicio.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
