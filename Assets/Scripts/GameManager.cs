using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Velocidade")]
    [SerializeField] private float velocidadeInicial = 10f;
    [SerializeField] private float velocidadeMaxima = 40f;
    [SerializeField] private float incrementoVelocidade = 1f;
    [SerializeField] private float intervaloAumento = 7f;

    [SerializeField] private float chanceSpawnInicial = 0.3f;  
    [SerializeField] private float chanceSpawnMaxima = 0.9f; 
    [SerializeField] private float incrementoChance = 0.05f;  
 
    private float velocidadeBase;

    public float multiplicadorDash = 1f;
    
    public float VelocidadeAtual => velocidadeBase * multiplicadorDash;
    public float ChanceSpawnAtual { get; private set; }

    private float timerAumento = 0f;

    void Awake()
    {
        if (instancia == null) instancia = this;
        ChanceSpawnAtual = chanceSpawnInicial;
        velocidadeBase = velocidadeInicial;
    }

    void Update()
    {
        // TODO [PASSO 4 - DIFICULDADE PROGRESSIVA]
        //  - some Time.deltaTime em timerAumento a cada frame
        //  - quando timerAumento >= intervaloAumento: zere o timer e aumente a dificuldade
        //      * velocidadeBase    += incrementoVelocidade  (nunca passando de velocidadeMaxima)
        //      * ChanceSpawnAtual  += incrementoChance      (nunca passando de chanceSpawnMaxima)
        //  Dica: Mathf.Min(valor, limite) resolve o "nunca passando".
    }

    public void PararJogo()
    {
        velocidadeBase = 0f;
        multiplicadorDash = 1f;
    }
}
