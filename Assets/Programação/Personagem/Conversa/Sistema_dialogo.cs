using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class Sistema_dialogo : MonoBehaviour
{
    [SerializeField] private float distanciaDoRaio;
    [SerializeField] private GameObject fundoPreto;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private string[] falas;
    [SerializeField] private int conversa;
    public float cronometro;
    public bool inicou, jachamada;
    [SerializeField] private string falaAtual, conversaAliado;

    private void Start()
    {
        inicou = false;
        jachamada = false;
        fundoPreto.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ChecaConversa();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fundoPreto.SetActive(false);
        }

    }

    private void ChecaConversa()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inicou = true;
            RaycastHit hit;
            Vector3 direcao = transform.TransformDirection(Vector3.forward);
            Debug.DrawRay(transform.position, direcao * distanciaDoRaio, Color.red);
            if (Physics.Raycast(transform.position, direcao, out hit, distanciaDoRaio))
            {
                
                if (hit.collider.CompareTag("NPC_Generico"))
                { 
                     conversa = Random.Range(0,falas.Length);
                    falaAtual = falas[conversa];
                    fundoPreto.SetActive(true);
                    texto.text = falaAtual;
                      
                }
                if (hit.collider.CompareTag("NPC_Aliado") && jachamada == false)
                {
                   Aliado_Ia aliado_Ia = hit.transform.GetComponent<Aliado_Ia>();
                    aliado_Ia.jaChamou = true;
                    jachamada = true;
                    fundoPreto.SetActive(true);
                    texto.text = conversaAliado;
                    inicou = true;
                    }
                }
        }
    }
}
