using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skins (texturas) de personagem carregadas da pasta Assets/Resources/HalloweenAssets.
///
/// Convencao de nome dos arquivos:  &lt;classe&gt;_&lt;skin&gt;.png   (ex.: mage_abobora.png)
/// A "classe" e o prefixo da textura original do personagem (knight_texture, mage_texture, barbarian_texture).
/// Para criar uma skin nova basta colocar um PNG com esse padrao na pasta -- ela aparece sozinha no botao PERSONALIZAR.
/// </summary>
public static class SkinService
{
    public const string PastaResources = "HalloweenAssets";
    public const string SkinPadrao = "original";
    private const string ChavePrefs = "SkinEscolhida";

    public static string SkinAtual => PlayerPrefs.GetString(ChavePrefs, SkinPadrao);

    public static void Escolher(string skin)
    {
        PlayerPrefs.SetString(ChavePrefs, skin);
        PlayerPrefs.Save();
    }

    /// <summary>Nomes de todas as skins encontradas na pasta ("original" sempre primeiro).</summary>
    public static List<string> ListarSkins()
    {
        var nomes = new List<string>();
        foreach (Texture2D tex in Resources.LoadAll<Texture2D>(PastaResources))
        {
            string skin = NomeDaSkin(tex.name);
            if (skin != null && !nomes.Contains(skin))
                nomes.Add(skin);
        }

        nomes.Sort(StringComparer.OrdinalIgnoreCase);
        if (nomes.Remove(SkinPadrao))
            nomes.Insert(0, SkinPadrao);
        return nomes;
    }

    /// <summary>Uma textura qualquer da skin, usada como miniatura no botao.</summary>
    public static Texture2D Miniatura(string skin)
    {
        foreach (Texture2D tex in Resources.LoadAll<Texture2D>(PastaResources))
        {
            if (NomeDaSkin(tex.name) == skin)
                return tex;
        }
        return null;
    }

    /// <summary>Aplica a skin escolhida em um personagem (usado ao spawnar o jogador).</summary>
    public static void Aplicar(GameObject alvo)
    {
        if (alvo == null) return;
        Aplicar(alvo.GetComponentsInChildren<Renderer>(true), SkinAtual);
    }

    /// <summary>Aplica a skin escolhida em todos os personagens da cena atual (usado no menu).</summary>
    public static void AplicarNaCena()
    {
        Renderer[] todos = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Aplicar(todos, SkinAtual);
    }

    public static void Aplicar(Renderer[] renderers, string skin)
    {
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            // Olha primeiro nos materiais compartilhados para so criar copias (r.materials)
            // dos renderers que realmente tem uma textura de personagem.
            Material[] compartilhados = r.sharedMaterials;
            bool precisaTrocar = false;
            foreach (Material m in compartilhados)
            {
                if (m != null && TexturaNova(m.mainTexture, skin) != null)
                {
                    precisaTrocar = true;
                    break;
                }
            }
            if (!precisaTrocar) continue;

            Material[] materiais = r.materials; // copias: nao altera os assets do projeto
            foreach (Material m in materiais)
            {
                if (m == null) continue;
                Texture2D nova = TexturaNova(m.mainTexture, skin);
                if (nova == null) continue;

                m.mainTexture = nova;
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", nova); // shaders URP/Lit
            }
        }
    }

    // Retorna a textura da skin equivalente a textura atual, ou null se nao ha o que trocar.
    private static Texture2D TexturaNova(Texture atual, string skin)
    {
        if (atual == null) return null;

        string classe = ClasseDaTextura(atual.name);
        if (classe == null) return null;

        Texture2D nova = Resources.Load<Texture2D>(PastaResources + "/" + classe + "_" + skin);
        if (nova == null || nova == atual) return null;
        return nova;
    }

    // "mage_abobora" -> "mage"
    private static string ClasseDaTextura(string nomeTextura)
    {
        int i = nomeTextura.IndexOf('_');
        if (i <= 0) return null;
        return nomeTextura.Substring(0, i).ToLowerInvariant();
    }

    // "mage_abobora" -> "abobora"
    private static string NomeDaSkin(string nomeTextura)
    {
        int i = nomeTextura.IndexOf('_');
        if (i <= 0 || i >= nomeTextura.Length - 1) return null;
        return nomeTextura.Substring(i + 1).ToLowerInvariant();
    }
}
