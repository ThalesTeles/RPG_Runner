#!/usr/bin/env python3
"""Gera o Asset Pack de Halloween (skins) do RPG Runner.

Recolore os atlas de textura originais do KayKit (um por classe) em temas de
Halloween, mantendo o mapeamento UV. Saida: Assets/Resources/HalloweenAssets/

Convencao de nome (a mesma que o jogo usa para carregar):  <classe>_<skin>.png
Para adicionar uma skin nova, basta criar <classe>_<nome>.png nessa pasta.

Uso (na raiz do repositorio):  python3 Tools/gerar_skins_halloween.py
Requer: Pillow e numpy.
"""
import math
import shutil
from pathlib import Path

import numpy as np
from PIL import Image

RAIZ = Path(__file__).resolve().parent.parent
BASE = RAIZ / "Assets/Imports/personagens/KayKit/Characters/KayKit - Adventurers (for Unity)/Textures"
SAIDA = RAIZ / "Assets/Resources/HalloweenAssets"

# classe -> arquivo de textura original
CLASSES = {
    "knight": BASE / "Knight/knight_texture.png",
    "barbarian": BASE / "Barbarian/barbarian_texture.png",
    "mage": BASE / "Mage/mage_texture.png",
}

# skin -> (matiz alvo em graus, mult. saturacao, mult. brilho, cor p/ tingir cinzas, forca do tingimento)
TEMAS = {
    "abobora":  (28.0, 1.55, 1.05, (255, 130, 20), 0.40),
    "fantasma": (190.0, 0.25, 1.30, (190, 230, 255), 0.40),
    "zumbi":    (100.0, 0.85, 0.85, (110, 140, 70), 0.28),
    "vampiro":  (352.0, 1.10, 0.85, (90, 10, 20), 0.30),
    "bruxa":    (275.0, 1.00, 0.90, (120, 60, 160), 0.25),
}


def rgb_para_hsv(rgb):
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    mx, mn = rgb.max(-1), rgb.min(-1)
    d = mx - mn
    h = np.zeros_like(mx)
    m = d > 1e-6
    rm = m & (mx == r)
    gm = m & (mx == g) & ~rm
    bm = m & ~rm & ~gm
    h[rm] = ((g - b)[rm] / d[rm]) % 6
    h[gm] = (b - r)[gm] / d[gm] + 2
    h[bm] = (r - g)[bm] / d[bm] + 4
    h = h * 60.0
    s = np.where(mx > 1e-6, d / np.maximum(mx, 1e-6), 0.0)
    return h, s, mx


def hsv_para_rgb(h, s, v):
    h = (h % 360.0) / 60.0
    i = np.floor(h).astype(int) % 6
    f = h - np.floor(h)
    p, q, t = v * (1 - s), v * (1 - s * f), v * (1 - s * (1 - f))
    r = np.choose(i, [v, q, p, p, t, v])
    g = np.choose(i, [t, v, v, q, p, p])
    b = np.choose(i, [p, p, t, v, v, q])
    return np.stack([r, g, b], -1)


def aplicar_tema(img, matiz, mult_s, mult_v, cor_tinta, forca_tinta):
    arr = np.asarray(img.convert("RGBA"), dtype=np.float64) / 255.0
    rgb, alfa = arr[..., :3], arr[..., 3:]
    h, s, v = rgb_para_hsv(rgb)
    # mantem variedade de cores: o matiz novo varia um pouco conforme o matiz original
    novo_h = matiz + 25.0 * np.sin(np.radians(h))
    peso = np.clip((s - 0.08) / 0.17, 0.0, 1.0)      # pixels coloridos trocam de matiz
    h2 = np.where(peso > 0, novo_h, h)
    s2 = np.clip(s * mult_s, 0, 1)
    v2 = np.clip(v * mult_v, 0, 1)
    out = hsv_para_rgb(h2, s2, v2)
    # tinge os tons neutros (cinzas/metal) com a cor do tema
    tinta = np.array(cor_tinta, dtype=np.float64) / 255.0
    neutro = (1.0 - peso)[..., None]
    out = out * (1 - forca_tinta * neutro) + (tinta * v2[..., None]) * (forca_tinta * neutro)
    out = np.clip(out, 0, 1)
    final = np.concatenate([out, alfa], -1)
    return Image.fromarray((final * 255 + 0.5).astype(np.uint8), "RGBA")


def main():
    SAIDA.mkdir(parents=True, exist_ok=True)
    for classe, caminho in CLASSES.items():
        original = Image.open(caminho)
        shutil.copyfile(caminho, SAIDA / f"{classe}_original.png")
        for skin, params in TEMAS.items():
            aplicar_tema(original, *params).save(SAIDA / f"{classe}_{skin}.png", optimize=True)
            print("gerado", f"{classe}_{skin}.png")


if __name__ == "__main__":
    main()
