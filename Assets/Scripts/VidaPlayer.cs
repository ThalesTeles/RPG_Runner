using UnityEngine;
using UnityEngine.SceneManagement;

public class VidaPlayer : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int vidasMaximas = 3;
    private int vidasAtuais;

    [Header("Colisão e Invulnerabilidade")]
    [SerializeField] private float tempoInvulnerabilidade = 1.5f;
    private float timerInvulneravel = 0f;
    private bool invulneravel = false;

    [Header("Morte")]
    [SerializeField] private float tempoAteMorte = 2.2f;
    private bool morreu = false;

    private HabilidadeLutador habilidadeEscudo;
    private HabilidadeBarbaro habilidadeBarbaro;
    private Animator animator;

    void Start()
    {
        vidasMaximas += PlayerPrefs.GetInt("VidaExtra", 0);
        vidasAtuais = vidasMaximas;
        
        habilidadeEscudo = GetComponent<HabilidadeLutador>();
        habilidadeBarbaro = GetComponent<HabilidadeBarbaro>();
        animator = GetComponent<Animator>();
        AtualizarHUD();
    }

    void Update()
    {
        if (invulneravel)
        {
            timerInvulneravel -= Time.deltaTime;
            if (timerInvulneravel <= 0f)
                invulneravel = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (morreu) return;

        // TODO [PASSO 1 - COLISAO]
        // Detecte a batida do jogador com algo perigoso e chame TomarDano() quando o dano deve valer.
        //  - o objeto "other" e perigoso se tiver a tag "obstacle" ou "enemy"
        //  - NAO leva dano se o escudo do Lutador estiver ativo (habilidadeEscudo.EscudoAtivo)
        //  - NAO leva dano se o dash do Barbaro estiver ativo (habilidadeBarbaro.habilidadeAtiva)
        //  - NAO leva dano enquanto estiver invulneravel
        //  Dica: habilidadeEscudo e habilidadeBarbaro podem ser null (nem toda classe tem as duas).
    }

    private void TomarDano()
    {
        // TODO [PASSO 2 - DANO]
        //  - tire 1 de vidasAtuais
        //  - deixe o jogador invulneravel por tempoInvulnerabilidade segundos
        //    (invulneravel = true e timerInvulneravel = tempoInvulnerabilidade; o Update ja faz a contagem)
        //  - atualize a barra de vida chamando AtualizarHUD()
        //  - se vidasAtuais chegou a 0 (ou menos), chame IniciarMorte()
    }

    private void IniciarMorte()
    {
        // TODO [PASSO 3 - GAME OVER]
        //  - marque morreu = true (assim OnTriggerEnter para de funcionar)
        //  - desligue o componente MovimentoPlayer deste objeto (GetComponent + enabled = false)
        //  - pare o mapa: GerarMapa.instancia.PararGeracao()   e o jogo: GameManager.instancia.PararJogo()
        //    (as instancias podem ser null: confira antes de chamar)
        //  - toque a animacao de morte: animator.SetTrigger("morreu")
        //  - depois de tempoAteMorte segundos, va para a tela de Game Over: Invoke(nameof(IrParaMorte), tempoAteMorte)
    }

    private void IrParaMorte()
    {
        SceneManager.LoadScene("Morte");
    }

    private void AtualizarHUD()
    {
        if (HUDgameplay.instancia != null)
        {
            float porcentagem = (float)vidasAtuais / vidasMaximas;
            HUDgameplay.instancia.AtualizarVida(vidasAtuais, vidasMaximas);
        }
    }
}
