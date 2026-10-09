using TMPro;
using UnityEngine;

public class MissaoTrigger : MonoBehaviour
{
    public GameObject missao2;
    private sistema_Animacao animacaoMaria;
    private Animator animatorMaria;
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

    [Header("Ao Finalizar a Conversa")]
    public UnityEngine.Events.UnityEvent onDialogoFinalizado;   // Evento que você pode configurar no Inspector
    [SerializeField] private MonoBehaviour scriptParaAtivar;   // Arraste o script que quer ativar
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

        // Continua forçando a animação de parado todo frame
        ForcarAnimacaoParado();

        falaTime += Time.deltaTime;

        if (falaTime >= tempoEntreFalas)
        {
            falaTime = 0f;
            conversa++;

            if (conversa >= falas.Length)
            {
                FinalizarDialogo();
            }
            else
            {
                if (texto != null)
                    texto.text = falas[conversa];
            }
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

        animacaoMaria = other.GetComponent<sistema_Animacao>();
        animatorMaria = other.GetComponent<Animator>();

        // Desativa o sistema de animação
        if (animacaoMaria != null)
            animacaoMaria.enabled = false;

        // Força parado imediatamente
        ForcarAnimacaoParado();
    }

    private void FinalizarDialogo()
    {
        fala = false;

        // Fecha o diálogo
        if (dialogoBox != null)
            dialogoBox.SetActive(false);

        // Volta as câmeras
        if (cameraSecundaria != null)
            cameraSecundaria.enabled = false;

        if (cameraPrincipal != null)
            cameraPrincipal.enabled = true;

        // Reativa o movimento da Maria
        if (maria != null)
            maria.enabled = true;

        if (rbMaria != null)
            rbMaria.isKinematic = false;

        if (ccMaria != null)
            ccMaria.enabled = true;

        // ===== NOVIDADES =====

        // 1. Ativa o evento (você configura no Inspector o que acontece)
        onDialogoFinalizado?.Invoke();

        // 2. Ativa outro script
        if (scriptParaAtivar != null)
            scriptParaAtivar.enabled = true;

        Debug.Log("Diálogo finalizado! Movimento restaurado + evento e script ativados.");

        // Reativa o sistema de animação
        if (animacaoMaria != null)
            animacaoMaria.enabled = true;
        missao2.SetActive(true);
        // Destrói o trigger
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
    private void ForcarAnimacaoParado()
    {
        if (animatorMaria != null)
        {



            // Força o Animator a ir para o estado Idle (troque "Idle" pelo nome real do seu estado parado)
            animatorMaria.Play("Parado", 0, 0f);
        }
    }
}