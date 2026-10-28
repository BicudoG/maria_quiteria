using UnityEngine;
using UnityEngine.SceneManagement;


public class Derrota : MonoBehaviour
{

    public static Derrota Instance;

    private bool isPaused = false;

    void Awake()
    {
        // Garante que só existe um GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== FUNÇÃO PAUSE ==========
    // Pausa o jogo, mas os botões da UI continuam funcionando
    public void Pause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pausa a física, animações e Update
            Debug.Log("Jogo Pausado");
        }
        else
        {
            Time.timeScale = 1f; // Retoma o jogo
            Debug.Log("Jogo Retomado");
        }
    }

    // ========== FUNÇÃO RESTART ==========
    // Reinicia a fase atual
    public void Restart()
    {
        Time.timeScale = 1f; // Garante que o tempo volta ao normal
        isPaused = false;

        // Carrega novamente a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Debug.Log("Fase Reiniciada");
    }

    // Opcional: método para verificar se está pausado
    public bool IsPaused()
    {
        return isPaused;
    }
}
