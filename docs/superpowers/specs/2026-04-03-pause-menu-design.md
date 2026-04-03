# Design — Menu Pause (Échap)

**Date :** 2026-04-03  
**Scope :** MainScene — menu pause avec Continuer, Paramètres, Sauvegarder, Quitter

---

## Objectif

Appuyer sur Échap pendant le jeu ouvre un menu pause qui gèle le jeu (`Time.timeScale = 0`) et propose 4 actions. Appuyer à nouveau sur Échap ferme le menu.

---

## Structure de la scène (MainScene)

### Canvas "PauseCanvas"
- Render Mode : Screen Space Overlay
- Sorting Order : 100 (au-dessus du jeu)
- Canvas Scaler : Scale With Screen Size, 1920x1080, Match 0.5

### PausePanel
- Panel semi-transparent centré
- 4 boutons verticaux : Continuer / Paramètres / Sauvegarder / Quitter
- Désactivé par défaut

### SettingsPanel
- Même structure que le SettingsPanel du MenuScene (Touches/Manette/Son)
- Composant `SettingsUI.cs` attaché
- Bouton Retour → revient à PausePanel
- Désactivé par défaut

---

## Script PauseMenuUI.cs

**Localisation :** `Assets/Scripts/UI/PauseMenuUI.cs`

### Références Inspector
- `m_pausePanel` : GameObject
- `m_settingsPanel` : GameObject
- `m_saveLabel` : TextMeshProUGUI (texte du bouton Sauvegarder)

### Comportement

| Action | Résultat |
|---|---|
| Échap (jeu actif) | Ouvre PausePanel, `Time.timeScale = 0` |
| Échap (pause active) | Ferme menu, `Time.timeScale = 1` |
| Continuer | Ferme menu, `Time.timeScale = 1` |
| Paramètres | Cache PausePanel, affiche SettingsPanel |
| Sauvegarder | `GameManager.Instance.QuickSave()` + feedback "Sauvegardé !" 1.5s |
| Quitter | `Time.timeScale = 1` → `GameManager.Instance.ReturnToMenu()` |
| Retour (dans Settings) | Cache SettingsPanel, affiche PausePanel |

### Gestion Time.timeScale
- Ouverture pause : `Time.timeScale = 0`
- Fermeture pause : `Time.timeScale = 1`
- Toujours remettre à 1 avant de changer de scène

---

## Hors scope

- Pas de pause automatique (alt-tab, focus perdu)
- Pas d'animation d'ouverture/fermeture du panel
- Pas de sauvegarde automatique à la fermeture du menu
