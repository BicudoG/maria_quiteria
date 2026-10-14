using System;
using UnityEngine;

public class Colisao : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

}
