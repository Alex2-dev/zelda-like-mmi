# Design — Animation de fond du menu principal

**Date :** 2026-04-03  
**Scope :** MenuScene — fond animé avec le personnage joueur assis devant un PC

---

## Objectif

Ajouter un fond animé à la `MenuScene` : le personnage principal apparaît dans une chambre 2D, assis devant un écran de PC, en idle en boucle. L'écran pulse légèrement pour simuler la lumière du moniteur.

---

## Structure de la scène

### Tilemap — Chambre/bureau
- Construite avec la tile palette existante du projet
- Représente une pièce simple : murs, sol, bureau
- Placée en arrière-plan (Sorting Layer : Background)

### GameObject "Player_Menu"
- `SpriteRenderer` utilisant le sprite sheet du personnage principal
- `Animator` avec un controller dédié `MenuCharacter.controller`
- Positionné devant le bureau, face caméra
- Aucun `Rigidbody`, aucun script de mouvement

### GameObject "PCScreen"
- `SpriteRenderer` avec un sprite simple (rectangle de couleur bleu/blanc)
- Positionné devant le personnage (légèrement au-dessus, simulant un écran)
- Script `PCScreenBlink.cs` attaché

### Caméra
- Camera fixe, pas de Cinemachine
- Cadre centré sur la scène de fond

---

## Animation du personnage

- Nouvel `AnimatorController` : `Assets/Animation/MenuCharacter.controller`
- Un seul état : **Idle** — joue `idle_main.anim` en boucle infinie
- Séparé de `Main_Character.controller` pour ne pas interférer avec le jeu

---

## Script PCScreenBlink.cs

```
Localisation : Assets/Scripts/UI/PCScreenBlink.cs
```

- Attaché au GameObject "PCScreen"
- Fait varier la couleur/alpha du `SpriteRenderer` en boucle via `Mathf.Sin`
- Simule la lumière pulsante d'un écran de moniteur
- Valeurs internes : vitesse de pulse, intensité min/max

---

## Assets existants réutilisés

| Asset | Usage |
|---|---|
| `Assets/Animation/idle_main.anim` | Animation idle du perso dans le menu |
| Sprite sheet du personnage principal | SpriteRenderer du Player_Menu |
| Tile palette existante | Construction du décor chambre/bureau |

---

## Hors scope

- Pas de Cinemachine
- Pas de mouvement de caméra
- Pas de nouveaux sprites à dessiner
- Pas de Timeline
