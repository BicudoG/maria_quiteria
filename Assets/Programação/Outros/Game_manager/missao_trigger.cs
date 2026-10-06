using TMPro;
using UnityEngine;

public class MissaoTrigger : MonoBehaviour
{
    [Header("Cameras")]
    public Camera cameraPrincipal;
    public Camera cameraSecundaria;

    [Header("Dialogo")]
    public GameObject dialogoBox;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Falas")]
    [SerializeField] private string[] falas;
    [SerializeField] private float tempoEntreFalas = 5f;

    private bool fala = false;
    private int conversa = 0;
    private float falaTime = 0f;

    private maria_quiteria maria;
    private Rigidbody rbMaria;
    private CharacterController ccMaria;

    private void Start()
    {
        // Só busca a Main Camera se não foi arrastada no Inspector
        if (cameraPrincipal == null)
            cameraPrincipal = Camera.main;

        // Segurança: se a "principal" for a mesma da secundária, procura outra
        if (cameraPrincipal == cameraSecundaria)
        {
            Debug.LogError("cameraPrincipal e cameraSecundaria são a mesma câmera! " +
                           "Remova a tag MainCamera da secundária ou arraste a principal no Inspector.", this);
            cameraPrincipal = null;
        }

        TrocarCamera(false);

        if (dialogoBox != null)
            dialogoBox.SetActive(false);
    }

    private void Update()
    {
        if (!fala) return;

        // unscaledDeltaTime funciona mesmo se o jogo estiver pausado (timeScale = 0)
        falaTime += Time.unscaledDeltaTime;

        if (falaTime >= tempoEntreFalas)
        {
            falaTime = 0f;
            conversa++;

            if (conversa >= falas.Length)
                FinalizarDialogo();
            else if (texto != null)
                texto.text = falas[conversa];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Maria_quiteria_Civil") || fala)
            return;

        maria = other.GetComponent<maria_quiteria>();
        rbMaria = other.GetComponent<Rigidbody>();
        ccMaria = other.GetComponent<CharacterController>();

        if (maria != null)
            maria.enabled = false;
        else
            Debug.LogWarning("Script maria_quiteria não encontrado!");

        if (rbMaria != null)
        {
            rbMaria.linearVelocity = Vector3.zero;
            rbMaria.angularVelocity = Vector3.zero;
            rbMaria.isKinematic = true;
        }

        if (ccMaria != null)
            ccMaria.enabled = false;

        TrocarCamera(true);

        if (dialogoBox != null)
            dialogoBox.SetActive(true);

        conversa = 0;
        falaTime = 0f;
        fala = true;

        if (texto != null && falas.Length > 0)
            texto.text = falas[0];
    }

    private void FinalizarDialogo()
    {
        fala = false;

        if (dialogoBox != null)
            dialogoBox.SetActive(false);

        TrocarCamera(false);

        if (maria != null)
            maria.enabled = true;

        if (rbMaria != null)
            rbMaria.isKinematic = false;

        if (ccMaria != null)
            ccMaria.enabled = true;

        Debug.Log("Diálogo finalizado! Movimento restaurado.");

        Destroy(gameObject);
    }

    // true = câmera do diálogo | false = câmera normal do jogo
    private void TrocarCamera(bool dialogo)
    {
        // Desliga primeiro, liga depois (evita conflito se houver algum problema de referência)
        if (dialogo)
        {
            SetCamera(cameraPrincipal, false);
            SetCamera(cameraSecundaria, true);
        }
        else
        {
            SetCamera(cameraSecundaria, false);
            SetCamera(cameraPrincipal, true);
        }
    }

    private void SetCamera(Camera cam, bool ativa)
    {
        if (cam == null) return;

        cam.enabled = ativa;

        var listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = ativa;
    }
}