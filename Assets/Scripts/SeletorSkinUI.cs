using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Botao PERSONALIZAR do menu: abre um painel com as skins carregadas da pasta
/// Assets/Resources/HalloweenAssets (ver SkinService). O aluno clica numa skin e ela
/// e aplicada na hora nos personagens do menu e salva para a partida.
///
/// Este componente se cria sozinho quando a cena "Menu" carrega e monta a propria UI por codigo,
/// entao nao e preciso editar nenhuma cena nem prefab.
/// </summary>
public class SeletorSkinUI : MonoBehaviour
{
    private const string NomeCenaMenu = "Menu";

    private static readonly Color CorFundoCartao = new Color(0.20f, 0.12f, 0.30f, 1f);
    private static readonly Color CorCartaoSelecionado = new Color(1f, 0.50f, 0.05f, 1f);
    private static readonly Color CorLaranja = new Color(1f, 0.50f, 0.05f, 1f);
    private static readonly Color CorRoxo = new Color(0.30f, 0.05f, 0.45f, 1f);

    private GameObject painel;
    private readonly Dictionary<string, Image> cartoes = new Dictionary<string, Image>();

    // ---------- criacao automatica no menu ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
        SceneManager.sceneLoaded += AoCarregarCena;
        CriarSeForMenu(SceneManager.GetActiveScene());
    }

    private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        CriarSeForMenu(cena);
    }

    private static void CriarSeForMenu(Scene cena)
    {
        if (cena.name != NomeCenaMenu) return;
        if (FindFirstObjectByType<SeletorSkinUI>() != null) return;
        new GameObject("SeletorSkinUI").AddComponent<SeletorSkinUI>();
    }

    // ---------- ciclo de vida ----------

    private void Start()
    {
        SkinService.AplicarNaCena(); // mostra no menu a skin que ja estava salva
        ConstruirUI();
        painel.SetActive(false);
    }

    // ---------- logica ----------

    private void AlternarPainel()
    {
        painel.SetActive(!painel.activeSelf);
    }

    private void EscolherSkin(string skin)
    {
        SkinService.Escolher(skin);
        SkinService.AplicarNaCena();
        AtualizarDestaque();
    }

    private void AtualizarDestaque()
    {
        string atual = SkinService.SkinAtual;
        foreach (KeyValuePair<string, Image> par in cartoes)
            par.Value.color = par.Key == atual ? CorCartaoSelecionado : CorFundoCartao;
    }

    private static string NomeBonito(string skin)
    {
        switch (skin)
        {
            case "original": return "Original";
            case "abobora": return "Abóbora";
            case "fantasma": return "Fantasma";
            case "zumbi": return "Zumbi";
            case "vampiro": return "Vampiro";
            case "bruxa": return "Bruxa";
            default: return char.ToUpperInvariant(skin[0]) + skin.Substring(1);
        }
    }

    // ---------- montagem da UI ----------

    private void ConstruirUI()
    {
        GameObject canvasGO = new GameObject("CanvasSkins");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // botao fixo no menu (lado esquerdo, espelhando o botao de instrucoes)
        GameObject botao = CriarBotao(canvasGO.transform, "BotaoPersonalizar", "PERSONALIZAR", CorRoxo, 34f);
        PosicionarCentro(botao, new Vector2(-610f, 237f), new Vector2(300f, 105f));
        botao.GetComponent<Button>().onClick.AddListener(AlternarPainel);

        ConstruirPainel(canvasGO.transform);
    }

    private void ConstruirPainel(Transform pai)
    {
        // fundo escuro que cobre a tela e bloqueia cliques no menu enquanto o painel esta aberto
        painel = CriarImagem(pai, "PainelSkins", new Color(0f, 0f, 0f, 0.65f));
        Esticar(painel);

        GameObject caixa = CriarImagem(painel.transform, "Caixa", new Color(0.10f, 0.06f, 0.16f, 0.98f));
        PosicionarCentro(caixa, Vector2.zero, new Vector2(1280f, 640f));
        Outline contorno = caixa.AddComponent<Outline>();
        contorno.effectColor = CorLaranja;
        contorno.effectDistance = new Vector2(5f, -5f);

        GameObject titulo = CriarTexto(caixa.transform, "PERSONALIZAR PERSONAGEM", 54f, CorLaranja);
        PosicionarTopo(titulo, -40f, 80f);

        GameObject sub = CriarTexto(caixa.transform, "Escolha a skin de Halloween do seu personagem", 30f, Color.white);
        PosicionarTopo(sub, -115f, 50f);

        // grade de cartoes (um por skin encontrada na pasta)
        GameObject grade = new GameObject("Grade", typeof(RectTransform));
        grade.transform.SetParent(caixa.transform, false);
        PosicionarCentro(grade, new Vector2(0f, -10f), new Vector2(1200f, 340f));
        GridLayoutGroup layout = grade.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(180f, 220f);
        layout.spacing = new Vector2(20f, 20f);
        layout.childAlignment = TextAnchor.UpperCenter;

        List<string> skins = SkinService.ListarSkins();
        if (skins.Count == 0)
        {
            GameObject aviso = CriarTexto(caixa.transform,
                "Nenhuma skin encontrada em Assets/Resources/" + SkinService.PastaResources, 32f, Color.white);
            PosicionarCentro(aviso, Vector2.zero, new Vector2(1100f, 80f));
        }

        foreach (string skin in skins)
            CriarCartao(grade.transform, skin);

        GameObject fechar = CriarBotao(caixa.transform, "BotaoFechar", "FECHAR", CorRoxo, 36f);
        PosicionarBase(fechar, 40f, new Vector2(280f, 90f));
        fechar.GetComponent<Button>().onClick.AddListener(AlternarPainel);

        AtualizarDestaque();
    }

    private void CriarCartao(Transform pai, string skin)
    {
        GameObject cartao = CriarImagem(pai, "Cartao_" + skin, CorFundoCartao);
        Button b = cartao.AddComponent<Button>();
        b.targetGraphic = cartao.GetComponent<Image>();
        string nomeSkin = skin; // copia local para o clique
        b.onClick.AddListener(() => EscolherSkin(nomeSkin));
        cartoes[skin] = cartao.GetComponent<Image>();

        GameObject miniatura = new GameObject("Miniatura", typeof(RectTransform));
        miniatura.transform.SetParent(cartao.transform, false);
        RectTransform rt = miniatura.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(12f, 62f);
        rt.offsetMax = new Vector2(-12f, -12f);
        RawImage raw = miniatura.AddComponent<RawImage>();
        raw.texture = SkinService.Miniatura(skin);
        raw.raycastTarget = false;

        GameObject nome = CriarTexto(cartao.transform, NomeBonito(skin), 30f, Color.white);
        RectTransform nrt = nome.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0f, 0f);
        nrt.anchorMax = new Vector2(1f, 0f);
        nrt.pivot = new Vector2(0.5f, 0f);
        nrt.anchoredPosition = new Vector2(0f, 8f);
        nrt.sizeDelta = new Vector2(0f, 46f);
    }

    // ---------- utilitarios de UI ----------

    private static GameObject CriarImagem(Transform pai, string nome, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        Image img = go.AddComponent<Image>();
        img.color = cor;
        return go;
    }

    private static GameObject CriarTexto(Transform pai, string texto, float tamanho, Color cor)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(pai, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanho;
        tmp.color = cor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return go;
    }

    private static GameObject CriarBotao(Transform pai, string nome, string texto, Color cor, float tamanhoTexto)
    {
        GameObject go = CriarImagem(pai, nome, cor);
        Button botao = go.AddComponent<Button>();
        botao.targetGraphic = go.GetComponent<Image>();
        Outline contorno = go.AddComponent<Outline>();
        contorno.effectColor = CorLaranja;
        contorno.effectDistance = new Vector2(3f, -3f);

        GameObject label = CriarTexto(go.transform, texto, tamanhoTexto, Color.white);
        Esticar(label);
        return go;
    }

    private static void Esticar(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void PosicionarCentro(GameObject go, Vector2 pos, Vector2 tamanho)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;
    }

    private static void PosicionarTopo(GameObject go, float y, float altura)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(0f, altura);
    }

    private static void PosicionarBase(GameObject go, float y, Vector2 tamanho)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = tamanho;
    }
}
