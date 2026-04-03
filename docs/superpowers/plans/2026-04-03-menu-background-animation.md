# Menu Background Animation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter un fond animé à la MenuScene — le personnage principal en idle devant un écran de PC pulsant, construit sur tilemap 2D.

**Architecture:** Un AnimatorController dédié joue `idle_main.anim` en boucle sur un GameObject "Player_Menu" placé devant un décor tilemap. Un script `PCScreenBlink.cs` anime la couleur d'un SpriteRenderer simulant un écran de moniteur. La caméra du menu reste fixe.

**Tech Stack:** Unity 2D, Tilemap, Animator, SpriteRenderer, C#

---

## Fichiers

| Action | Fichier | Responsabilité |
|---|---|---|
| Créer (Editor) | `Assets/Animation/MenuCharacter.controller` | AnimatorController idle-only pour le menu |
| Créer | `Assets/Scripts/UI/PCScreenBlink.cs` | Pulse la couleur de l'écran PC |
| Modifier (Editor) | `Assets/_Scenes/MenuScene.unity` | Tilemap décor + GameObjects Player_Menu + PCScreen |

---

## Task 1 : Script PCScreenBlink

**Files:**
- Create: `Assets/Scripts/UI/PCScreenBlink.cs`

- [ ] **Step 1 : Écrire le script**

```csharp
// Assets/Scripts/UI/PCScreenBlink.cs
using UnityEngine;

public class PCScreenBlink : MonoBehaviour
{
    SpriteRenderer m_renderer;
    float m_speed = 1.2f;
    float m_minBrightness = 0.7f;
    float m_maxBrightness = 1.0f;

    void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * m_speed) + 1f) / 2f;
        float brightness = Mathf.Lerp(m_minBrightness, m_maxBrightness, t);
        Color c = m_renderer.color;
        c.r = brightness;
        c.g = brightness;
        c.b = brightness;
        m_renderer.color = c;
    }
}
```

- [ ] **Step 2 : Vérifier la compilation**

Dans Unity, ouvrir la console (Window > General > Console). S'assurer qu'il n'y a aucune erreur de compilation liée à `PCScreenBlink`.

- [ ] **Step 3 : Commit**

```bash
git add Assets/Scripts/UI/PCScreenBlink.cs
git commit -m "feat: add PCScreenBlink script for menu PC screen pulse effect"
```

---

## Task 2 : AnimatorController dédié au menu

**Files:**
- Create (Editor): `Assets/Animation/MenuCharacter.controller`

- [ ] **Step 1 : Créer le controller dans Unity Editor**

Dans le Project panel :
1. Clic droit sur `Assets/Animation/`
2. Create > Animator Controller
3. Nommer : `MenuCharacter`

- [ ] **Step 2 : Configurer le controller**

Double-cliquer sur `MenuCharacter.controller` pour ouvrir l'Animator window :
1. Clic droit dans la grille > Create State > Empty
2. Nommer l'état : `Idle`
3. Sélectionner l'état `Idle`, dans l'Inspector :
   - Motion : assigner `idle_main` (glisser depuis `Assets/Animation/idle_main.anim`)
   - Speed : 1
4. Clic droit sur l'état `Idle` > Set as Layer Default State (flèche orange doit pointer dessus)
5. Vérifier que Loop Time est coché sur le clip `idle_main` (sélectionner le clip, Inspector > Loop Time ✓)

- [ ] **Step 3 : Commit**

```bash
git add Assets/Animation/MenuCharacter.controller Assets/Animation/MenuCharacter.controller.meta
git commit -m "feat: add MenuCharacter animator controller with idle loop"
```

---

## Task 3 : Décor Tilemap dans MenuScene

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Ouvrir MenuScene**

File > Open Scene > `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 2 : Créer la Tilemap de fond**

Dans la Hierarchy :
1. Clic droit > 2D Object > Tilemap > Rectangular
2. Renommer le Grid : `BackgroundGrid`
3. Renommer la Tilemap enfant : `RoomTilemap`
4. Sur le composant `Tilemap Renderer` de `RoomTilemap` :
   - Sorting Layer : `Default` (ou en créer un `Background` si il existe)
   - Order in Layer : `-10`

- [ ] **Step 3 : Peindre le décor**

Ouvrir Window > 2D > Tile Palette :
1. Sélectionner la tile palette existante du projet
2. Peindre une pièce simple : sol (environ 10x6 tuiles), murs sur les bords, un bureau (quelques tuiles) centré en bas de la pièce
3. Positionner la scène pour que la caméra principale la cadre correctement

- [ ] **Step 4 : Sauvegarder**

Ctrl+S pour sauvegarder la scène.

- [ ] **Step 5 : Commit**

```bash
git add Assets/_Scenes/MenuScene.unity Assets/_Scenes/MenuScene.unity.meta
git commit -m "feat: add room tilemap background to MenuScene"
```

---

## Task 4 : GameObject Player_Menu

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Créer le GameObject**

Dans la Hierarchy de MenuScene :
1. Clic droit > Create Empty
2. Renommer : `Player_Menu`
3. Position : placer devant le bureau peint à la Task 3 (ex: `x: 0, y: -1, z: 0`)

- [ ] **Step 2 : Ajouter les composants**

Sélectionner `Player_Menu`, dans l'Inspector :
1. Add Component > Sprite Renderer
   - Sprite : assigner le sprite idle face caméra du personnage principal (même sprite sheet que le jeu, frame idle front)
   - Sorting Layer : `Default`
   - Order in Layer : `0`
2. Add Component > Animator
   - Controller : assigner `MenuCharacter` (glisser depuis `Assets/Animation/MenuCharacter.controller`)

- [ ] **Step 3 : Vérifier en Play Mode**

Appuyer sur Play. Le personnage doit jouer l'animation idle en boucle sans bouger.

- [ ] **Step 4 : Sauvegarder et commit**

Ctrl+S, puis :

```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: add Player_Menu GameObject with idle animation in MenuScene"
```

---

## Task 5 : GameObject PCScreen

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Créer le GameObject**

Dans la Hierarchy :
1. Clic droit > 2D Object > Sprite
2. Renommer : `PCScreen`
3. Positionner au-dessus/devant le bureau, légèrement devant le perso (ex: `x: 0.5, y: 0, z: 0`)

- [ ] **Step 2 : Configurer le SpriteRenderer**

Sélectionner `PCScreen`, dans l'Inspector :
1. Sprite : assigner un sprite carré blanc (Unity built-in `Sprites/Square`, ou créer un sprite blanc simple)
2. Color : `R: 0.6, G: 0.8, B: 1.0, A: 1` (teinte bleu écran)
3. Scale : ajuster pour ressembler à un petit écran (ex: `x: 0.8, y: 0.6, z: 1`)
4. Sorting Layer : `Default`
5. Order in Layer : `1` (au-dessus du perso si superposé, sinon `-1`)

- [ ] **Step 3 : Attacher PCScreenBlink**

Add Component > Scripts > PCScreenBlink

- [ ] **Step 4 : Vérifier en Play Mode**

Appuyer sur Play. L'écran doit pulser légèrement (luminosité qui oscille). Le personnage doit être en idle derrière/à côté.

- [ ] **Step 5 : Ajuster les positions si besoin**

Sortir du Play Mode, ajuster `Player_Menu` et `PCScreen` pour que la composition soit lisible avec les boutons du menu UI superposés.

- [ ] **Step 6 : Sauvegarder et commit**

Ctrl+S, puis :

```bash
git add Assets/_Scenes/MenuScene.unity Assets/Scripts/UI/PCScreenBlink.cs.meta
git commit -m "feat: add PCScreen with blink effect to MenuScene background"
```

---

## Task 6 : Vérification finale

- [ ] **Step 1 : Test complet en Play Mode**

Lancer MenuScene en Play Mode et vérifier :
- [ ] Le fond tilemap est visible derrière les panneaux du menu
- [ ] Le personnage joue l'animation idle en boucle
- [ ] L'écran PC pulse doucement
- [ ] Les boutons du menu (MainPanel) sont lisibles par-dessus le fond
- [ ] Aucune erreur dans la Console Unity

- [ ] **Step 2 : Ajuster l'Order in Layer si UI masquée**

Si le fond cache les éléments UI : sur le Canvas du menu, s'assurer que le Render Mode est `Screen Space - Overlay` (il passera toujours au-dessus de la scène 2D).

- [ ] **Step 3 : Commit final**

```bash
git add -A
git commit -m "feat: complete menu background animation with idle character and PC screen"
```
