# 🏰 RPG RUNNER

---

## Sobre o Jogo

O **RPG Runner** é um endless runner 3D com elementos de RPG desenvolvido em Unity para a GameJam da Profia Academy. O jogador escolhe entre três classes de personagens e corre por uma vila medieval infinita, desviando de obstáculos e inimigos enquanto acumula experiência e sobe de nível.

---

## Como Jogar

| Tecla | Ação |
|---|---|
| `A` | Mover para a esquerda |
| `D` | Mover para a direita |
| `Espaço` | Pular |
| `E` | Usar habilidade especial |
| `II` (botão) | Pausar / Retomar |

---

## Personagens

### Lutador
- **Habilidade:** Levanta o escudo e fica **imune a qualquer dano por 3 segundos**

### Bárbaro
- **Habilidade:** Executa um **dash** que **destrói inimigos** ao redor com efeito de partículas

### Mago
- **Habilidade:** Dispara um **projétil mágico** que **elimina inimigos** ao acertar

---

## Features

### Geração Procedural de Mapas
- Chunks de terreno geradas infinitamente e aleatoriamente
- 4 tipos de chunks: vila (x3) e mercado (x1)
- Obstáculos e inimigos spawnados aleatoriamente em pontos fixos dentro de cada chunk

### Dificuldade Progressiva
- Velocidade do cenário aumenta com o tempo
- Chance de spawn de obstáculos e inimigos aumenta gradualmente
- Inimigos: Goblin e Esqueleto
- Obstáculos: Pedra

### Sistema de XP e Níveis
- Barra de XP preenchida com o tempo de sobrevivência
- A cada nível a EXP necessária aumenta
- Nível alcançado é salvo ao morrer

### Loja Pós-Morte
- A cada morte o jogador ganha 1 compra disponível
- Itens disponíveis:
  - **Redução de Cooldown** — reduz 1s no cooldown da habilidade
  - **Vida Extra** — adiciona vida ao personagem
- Compras acumulam entre partidas e resetam ao fechar o jogo

### HUD
- Avatar e classe do personagem selecionado
- Corações de vida 
- Barra de XP (azul)
- Tempo sobrevivido
- Botão de pause 

### Menu Principal
- Seleção de personagem com animações (Idle (Personagens não selecionados) / Waving (Personagem selecionado))
- Tela de instruções
- Botão de sair
- **Botão PERSONALIZAR** (XP Week Halloween): o aluno escolhe a skin do personagem entre os assets da pasta `Assets/Resources/HalloweenAssets`

### Personalização de Skins (Halloween)
- As skins são texturas `.png` na pasta `Assets/Resources/HalloweenAssets`, no padrão `<classe>_<skin>.png` (ex.: `mage_abobora.png`; classes: `knight`, `barbarian`, `mage`)
- Skins incluídas: original, abóbora, fantasma, zumbi, vampiro e bruxa
- Para criar uma skin nova, basta colocar `<classe>_<nome>.png` na pasta: ela aparece sozinha no botão PERSONALIZAR do menu
- A escolha é salva em `PlayerPrefs` (`SkinEscolhida`) e aplicada ao personagem na partida (`SkinService`, `SeletorSkinUI`)
- Para regenerar o pacote de skins: `python3 Tools/gerar_skins_halloween.py`

---

## Tecnologias

- **Engine:** Unity 6.0 (URP)
- **Linguagem:** C#
- **Input System:** Unity New Input System
- **Persistência:** PlayerPrefs

---

## Equipe

| Nome | Perfil |
|---|---|
| **Suelle Maciel** | [(@SuelleMaciel)](https://github.com/SuelleMaciel) |
| **Thales Teles** | [(@ThalesTeles)](https://github.com/ThalesTeles) |
