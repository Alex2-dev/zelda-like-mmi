# Game Infrastructure Design
**Date:** 2026-04-02  
**Project:** Zelda-Like MMI  
**Scope:** Menu, Save System, Settings, Transitions

---

## 1. Architecture Globale

### Scènes
- `MenuScene` — menu principal, fond animé, paramètres, sélection de save
- `MainScene` — jeu existant (inchangé)

### Singletons DontDestroyOnLoad
| Singleton | Rôle |
|-----------|------|
| `GameManager` | État global : slot actif, boss tué, chargement de scène |
| `SaveManager` | Lecture/écriture des 3 slots JSON |
| `InputManager` | Rebinding touches + manette via New Input System |
| `AudioManager` | Existant — adapté pour volumes réglables |
| `TransitionManager` | Canvas de transition (fondu, texte, zoom) |

### Flux principal
```
MenuScene
  ├─ Jouer → Sélection slot → Animation "Entrée PC" → MainScene
  ├─ Paramètres → Touches / Manette / Son
  └─ Quitter → Application.Quit()

MainScene
  ├─ Boss tué → Save → Animation "Sortie PC" → MenuScene
  └─ Auto-save à l'entrée de nouvelles zones
```

---

## 2. Système de Save

### Stockage
- Format : JSON
- Chemin : `Application.persistentDataPath/save_slot_X.json` (X = 0, 1, 2)
- 3 slots indépendants

### Structure d'un slot
```json
{
  "slotIndex": 0,
  "playerPosition": { "x": 0.0, "y": 0.0 },
  "currentHP": 100,
  "maxHP": 100,
  "inventory": [
    { "itemId": "pistol", "quantity": 1 }
  ],
  "hotbar": ["pistol", null],
  "killedEnemyIds": ["enemy_01", "enemy_03"],
  "bossDefeated": false,
  "completedDialogs": ["npc_intro"],
  "openedDoors": ["door_01"],
  "playTime": 0
}
```

### Déclencheurs de sauvegarde
- Entrée dans une nouvelle zone (trigger collider)
- Mort du boss (avant animation de fin)

### Écran de sélection de slot
```
┌──────────────────────────────────┐
│  Slot 1 — 1h20  — Boss: Non     │
│  Slot 2 — VIDE                   │
│  Slot 3 — 0h45  — Boss: Oui     │
└──────────────────────────────────┘
```
- Clic sur un slot existant → charge la save → lance l'animation d'entrée
- Clic sur VIDE → crée une nouvelle save → lance l'animation d'entrée

---

## 3. Animations de Transition

### Entrée dans le PC (Menu → Jeu)
1. Bouton "Jouer" / slot sélectionné
2. Zoom caméra vers sprite "écran de PC" au centre du menu
3. Fondu noir progressif (`CanvasGroup.alpha` 0 → 1)
4. Texte "Connexion en cours..." lettre par lettre
5. `MainScene` se charge en arrière-plan (`LoadSceneAsync`)
6. Fondu disparaît → jeu démarre

### Sortie du PC (Boss tué → Menu)
1. Boss meurt → `GameManager.TriggerEndGame()`
2. Save automatique du slot actif
3. Fondu noir (`CanvasGroup.alpha` 0 → 1)
4. Texte "Déconnexion..." lettre par lettre
5. Dézoom depuis écran de PC
6. `MenuScene` rechargée → menu visible

### Implémentation
- `TransitionManager` : Canvas en DontDestroyOnLoad, `SortingOrder` max
- Fondus : Coroutines + `CanvasGroup.alpha`
- Zoom : `Camera.orthographicSize` lerp ou `transform.localScale`
- Texte lettre par lettre : Coroutine avec `WaitForSeconds(0.05f)` par caractère

### Fond animé du menu
- `RenderTexture` filmée par une caméra secondaire pointant sur une zone de décor animée
- Affiché en `RawImage` en fond du Canvas du menu

---

## 4. Paramètres

### Structure du menu Paramètres
3 onglets : **Touches** / **Manette** / **Son**

### Touches (Clavier) — toutes rebindables
| Action | Touche par défaut |
|--------|------------------|
| Haut | Z |
| Bas | S |
| Gauche | Q |
| Droite | D |
| Attaquer / Interagir | ESPACE |
| Inventaire | E |
| Courir | SHIFT |
| Se cacher | F |
| Carte | M |
| Hotbar 1 | 1 |
| Hotbar 2 | 2 |

### Manette (Xbox + PS)
- Stick gauche : mouvement (non rebindable)
- Tous les boutons d'action rebindables
- Détection automatique du type de manette → icônes adaptées (A/B/X/Y ou ✕/○/□/△)

### Son
- Slider Musique (0–100%) → `AudioManager.SetMusicVolume()`
- Slider Effets (0–100%) → `AudioManager.SetSoundVolume()`
- Sauvegardés dans `PlayerPrefs` (global, pas par slot)

### Implémentation
- Unity **New Input System** (`com.unity.inputsystem`)
- `InputActionAsset` centralisé avec toutes les actions
- Rebinding via `InputAction.PerformInteractiveRebinding()`
- Bindings sauvegardés dans `PlayerPrefs` en JSON (`InputActionAsset.SaveBindingOverridesAsJson`)

---

## 5. Nouveaux Fichiers à Créer

### Scripts
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Managers/SaveManager.cs`
- `Assets/Scripts/Managers/InputManager.cs`
- `Assets/Scripts/Managers/TransitionManager.cs`
- `Assets/Scripts/UI/MainMenuUI.cs`
- `Assets/Scripts/UI/SaveSlotUI.cs`
- `Assets/Scripts/UI/SettingsUI.cs`

### Scènes
- `Assets/_Scenes/MenuScene.unity`

### Assets
- `Assets/Input/GameInputActions.inputactions`
- `Assets/RenderTextures/MenuBackground.renderTexture`

---

## 6. Modifications des Fichiers Existants

| Fichier | Modification |
|---------|-------------|
| `AudioManager.cs` | Ajouter `SetMusicVolume()`, `SetSoundVolume()`, lecture depuis `PlayerPrefs` |
| `PlayerBehavior.cs` | Remplacer `Input.GetKey` par `InputManager` actions |
| `WeaponManager.cs` | Idem — utiliser les actions rebindables |
| `BossBehavior.cs` | Appeler `GameManager.TriggerEndGame()` à la mort phase 2 |
| `EnemyBehavior.cs` | Assigner un ID unique pour le tracking dans les saves |

---

## 7. Dépendances et Ordre d'Implémentation

1. **New Input System** — installer le package, créer `GameInputActions`
2. **GameManager + SaveManager** — fondation de tout
3. **MenuScene** — scène + UI de base + fond animé
4. **TransitionManager** — animations entrée/sortie PC
5. **Settings UI** — rebinding + sliders son
6. **Intégration MainScene** — connecter save, input, fin de jeu
