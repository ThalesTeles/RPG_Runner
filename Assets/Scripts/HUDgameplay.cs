using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class HUDgameplay : MonoBehaviour
{
    public static HUDgameplay instancia;
    [SerializeField] private UnityEngine.UI.Image avatar;
    [SerializeField] private TextMeshProUGUI textoClasse;
    
    [Header("Sistema de Corações")]
    [SerializeField] private Transform containerCoracoes;
    [SerializeField] private GameObject prefabCoracao;
    
    [SerializeField] private TextMeshProUGUI textoTempo;
    
    [SerializeField] private DadosPersonagem[] personagens;
    
    private float tempoSobrevivido = 0f;
    private bool pausado = false;
    
    [SerializeField] private Sprite spritePlay;
    [SerializeField] private Sprite spritePause;
    [SerializeField] private UnityEngine.UI.Button botaoPause;
    [SerializeField] private TextMeshProUGUI textoCooldown;
    
    

    private HabilidadeBase habilidade;

    public void AlternarPause()
    {
        pausado = !pausado;

        if (pausado)
        {
            Time.timeScale = 0f;
            botaoPause.image.sprite = spritePlay;
        }
        else
        {
            Time.timeScale = 1f;
            botaoPause.image.sprite = spritePause;
        }
    }

    [System.Serializable]
    public class DadosPersonagem
    {
        public string nomeClasse;
        public Sprite icon;
    }
    
    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    void Start()
    {
        int indice = PlayerPrefs.GetInt("PersonagemEscolhido", 0);
        ConfigurarPerso(indice);
    }
    
    public void ConfigurarPerso(int indice)
    {
        if (personagens == null || indice < 0 || indice >= personagens.Length) return;
        DadosPersonagem dados = personagens[indice];
        avatar.sprite = dados.icon;
        textoClasse.text = dados.nomeClasse;
    }

    void Update()
    {
        tempoSobrevivido += Time.deltaTime;
        AtualizarTempo();
        AtualizarCooldown();
    }
    
    public void AtualizarVida(int vidasAtuais, int vidasMaximas)
    {
        if (containerCoracoes == null || prefabCoracao == null) return;

        while (containerCoracoes.childCount < vidasMaximas)
        {
            Instantiate(prefabCoracao, containerCoracoes);
        }

        for (int i = 0; i < containerCoracoes.childCount; i++)
        {
            Transform coracao = containerCoracoes.GetChild(i);
            
            if (i < vidasMaximas)
            {
                coracao.gameObject.SetActive(true);
                UnityEngine.UI.Image img = coracao.GetComponent<UnityEngine.UI.Image>();
                
                if (img != null)
                {
                    if (i < vidasAtuais)
                        img.color = Color.white; 
                    else
                        img.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); 
                }
            }
            else
            {
                coracao.gameObject.SetActive(false);
            }
        }
    }
    
    private void AtualizarTempo()
    {
        int minutos = Mathf.FloorToInt(tempoSobrevivido / 60f);
        int segundos = Mathf.FloorToInt(tempoSobrevivido % 60);
        textoTempo.text = string.Format("Tempo sobrevivido: {0:00}:{1:00}", minutos, segundos);
    }

    private void AtualizarCooldown()
    {
        if (habilidade == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                habilidade = player.GetComponent<HabilidadeBase>();
            return;
        }

        if (habilidade.EmCooldown)
        {
            textoCooldown.text = Mathf.CeilToInt(habilidade.CooldownAtual).ToString() + "s";
        }
        else
        {
            textoCooldown.text = "HABILIDADE PRONTA!";
        }
    }
}
