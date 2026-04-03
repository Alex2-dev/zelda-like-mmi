# Pause Menu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter un menu pause (Échap) dans MainScene avec Continuer, Paramètres, Sauvegarder et Quitter.

**Architecture:** Un script `PauseMenuUI.cs` détecte Échap via InputManager, gère `Time.timeScale`, et switche entre PausePanel et SettingsPanel. Le SettingsUI existant est réutilisé tel quel pour les paramètres. Le Canvas pause est dédié (sorting order 100, Scale With Screen Size).

**Tech Stack:** Unity 2D, Canvas UI, Time.timeScale, C#, TextMeshPro, GameManager.QuickSave()

---

## Fichiers

| Action | Fichier | Responsabilité |
|---|---|---|
| Créer | `Assets/Scripts/UI/PauseMenuUI.cs` | Logique pause : toggle, timeScale, boutons |
| Modifier (Editor) | `Assets/_Scenes/MainScene.unity` | Canvas PauseCanvas + PausePanel + SettingsPanel |

---

## Task 1 : Script PauseMenuUI

**Files:**
- Create: `Assets/Scripts/UI/PauseMenuUI.cs`

- [ ] **Step 1 : Écrire le script**

```csharp
// Assets/Scripts/UI/PauseMenuUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère le menu pause (Échap) dans MainScene.
/// Toggle PausePanel / SettingsPanel, gère Time.timeScale.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject m_pausePanel;
    public GameObject m_settingsPanel;

    [Header("Bouton Sauvegarder — label")]
    public TextMeshProUGUI m_saveLabel;

    private bool m_isPaused = false;

    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.PausePressed)
            TogglePause();
    }

    // ── API publique ────────────────────────────────────────────────────────

    public void TogglePause()
    {
        if (m_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        m_isPaused = true;
        Time.timeScale = 0f;
        m_pausePanel.SetActive(true);
        m_settingsPanel.SetActive(false);
    }

    public void Resume()
    {
        m_isPaused = false;
        Time.timeScale = 1f;
        m_pausePanel.SetActive(false);
        m_settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        m_pausePanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        m_settingsPanel.SetActive(false);
        m_pausePanel.SetActive(true);
    }

    public void SaveGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.QuickSave();
        if (m_saveLabel != null)
            StartCoroutine(SaveFeedback());
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        m_isPaused = false;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
    }

    // ── Interne ─────────────────────────────────────────────────────────────

    private IEnumerator SaveFeedback()
    {
        m_saveLabel.text = "Sauvegardé !";
        yield return new WaitForSecondsRealtime(1.5f);
        m_saveLabel.text = "Sauvegarder";
    }
}
```

> **Note :** `WaitForSecondsRealtime` est utilisé (pas `WaitForSeconds`) car `Time.timeScale = 0` pendant la pause — `WaitForSeconds` ne s'écoulerait jamais.

- [ ] **Step 2 : Vérifier la compilation**

Dans Unity Console (Window > General > Console) — aucune erreur liée à `PauseMenuUI`.

- [ ] **Step 3 : Commit**

```bash
git add Assets/Scripts/UI/PauseMenuUI.cs
git commit -m "feat: add PauseMenuUI script with toggle, save, settings and quit"
```

---

## Task 2 : Canvas PauseCanvas dans MainScene

**Files:**
- Modify (Editor): `Assets/_Scenes/MainScene.unity`

- [ ] **Step 1 : Ouvrir MainScene**

File > Open Scene > `Assets/_Scenes/MainScene.unity`

- [ ] **Step 2 : Créer le Canvas**

Hierarchy → clic droit → **UI > Canvas** → renommer `PauseCanvas` :
- Inspector → **Canvas** : Render Mode = Screen Space Overlay, Sort Order = **100**
- Inspector → **Canvas Scaler** : UI Scale Mode = Scale With Screen Size, Reference Resolution = 1920x1080, Match = 0.5

- [ ] **Step 3 : Créer PausePanel**

Sous PauseCanvas → clic droit → **UI > Panel** → renommer `PausePanel` :
- RectTransform : Anchor Presets → Alt + **middle-center**
- Width : 400 / Height : 300
- Image color : noir semi-transparent (R:0 G:0 B:0 A:180)

Ajouter 4 boutons enfants (UI > Button - TextMeshPro) nommés :
- `BtnContinuer` — texte "Continuer"
- `BtnParametres` — texte "Paramètres"
- `BtnSauvegarder` — texte "Sauvegarder"
- `BtnQuitter` — texte "Quitter"

Disposer verticalement avec un **Vertical Layout Group** sur PausePanel :
- Add Component → Vertical Layout Group
- Spacing : 10, Child Alignment : Middle Center, Child Force Expand Width : ✓

- [ ] **Step 4 : Créer SettingsPanel**

Sous PauseCanvas → clic droit → **UI > Panel** → renommer `SettingsPanel` :
- RectTransform : Anchor Presets → Alt + **stretch/stretch**, offsets à 0
- Add Component → **SettingsUI**
- Assigner toutes les références `SettingsUI` dans l'Inspector (même setup que dans MenuScene : m_touchesPanel, m_manettPanel, m_sonPanel, m_musicSlider, m_soundSlider, m_rebindButtons, m_bindLabels, m_backButton, m_saveButton, m_saveLabel)
- Désactiver par défaut : décocher la checkbox du GameObject

- [ ] **Step 5 : Créer le GameObject PauseManager**

Hierarchy → clic droit → **Create Empty** → renommer `PauseManager` :
- Add Component → **PauseMenuUI**
- Assigner dans l'Inspector :
  - `m_pausePanel` → PausePanel
  - `m_settingsPanel` → SettingsPanel
  - `m_saveLabel` → TextMeshPro du BtnSauvegarder

- [ ] **Step 6 : Brancher les boutons OnClick**

Sélectionner **BtnContinuer** → Inspector → OnClick (+) :
- Object : PauseManager / Fonction : PauseMenuUI.Resume

Sélectionner **BtnParametres** → OnClick (+) :
- Object : PauseManager / Fonction : PauseMenuUI.OpenSettings

Sélectionner **BtnSauvegarder** → OnClick (+) :
- Object : PauseManager / Fonction : PauseMenuUI.SaveGame

Sélectionner **BtnQuitter** → OnClick (+) :
- Object : PauseManager / Fonction : PauseMenuUI.QuitToMenu

Sélectionner le bouton **Retour** dans SettingsPanel → OnClick (+) :
- Object : PauseManager / Fonction : PauseMenuUI.BackFromSettings

- [ ] **Step 7 : Désactiver PausePanel par défaut**

Sélectionner PausePanel → décocher la checkbox dans l'Inspector (désactivé au départ).

- [ ] **Step 8 : Sauvegarder et commit**

Ctrl+S, puis :

```bash
git add Assets/_Scenes/MainScene.unity
git commit -m "feat: add PauseCanvas with PausePanel and SettingsPanel to MainScene"
```

---

## Task 3 : Vérification finale

**Files:**
- Modify (Editor): `Assets/_Scenes/MainScene.unity` (si ajustements)

- [ ] **Step 1 : Tester en Play Mode**

Lancer MainScene en Play Mode et vérifier :
- [ ] Appuyer Échap → PausePanel s'ouvre, jeu gelé
- [ ] Appuyer Échap à nouveau → menu se ferme, jeu reprend
- [ ] Cliquer Continuer → menu se ferme, jeu reprend
- [ ] Cliquer Paramètres → SettingsPanel s'ouvre
- [ ] Modifier une touche dans Settings → label se met à jour
- [ ] Cliquer Retour → retour à PausePanel
- [ ] Cliquer Sauvegarder → texte passe à "Sauvegardé !" puis revient
- [ ] Cliquer Quitter → retour au MenuScene

- [ ] **Step 2 : Vérifier Time.timeScale**

Pendant la pause, ouvrir Window > Analysis > Profiler ou simplement observer que les ennemis/animations sont gelés.

- [ ] **Step 3 : Commit final**

```bash
git add Assets/_Scenes/MainScene.unity
git commit -m "feat: pause menu fully wired and tested in MainScene"
```
