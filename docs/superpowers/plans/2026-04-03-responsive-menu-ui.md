# Responsive Menu UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Faire en sorte que le menu s'adapte à toutes les résolutions PC en configurant le Canvas Scaler et les anchors des panels.

**Architecture:** Configuration Unity Editor pure — aucun code C#. Le Canvas Scaler passe en "Scale With Screen Size" (ref 1920x1080, match 0.5), les panels passent en stretch/stretch, et les éléments internes sont ancrés selon leur rôle (onglets en haut, contenu au centre, Retour en bas-gauche).

**Tech Stack:** Unity 2D, Canvas Scaler, RectTransform Anchors

---

## Fichiers

| Action | Fichier |
|---|---|
| Modifier (Editor) | `Assets/_Scenes/MenuScene.unity` |

---

## Task 1 : Canvas Scaler — MenuScene

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Ouvrir MenuScene**

File > Open Scene > `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 2 : Configurer le Canvas Scaler**

Dans la Hierarchy, sélectionner le GameObject **Canvas** racine :
1. Dans l'Inspector, trouver le composant **Canvas Scaler**
2. `UI Scale Mode` → **Scale With Screen Size**
3. `Reference Resolution` → **X: 1920 / Y: 1080**
4. `Screen Match Mode` → **Match Width Or Height**
5. `Match` → **0.5**

- [ ] **Step 3 : Vérifier**

Dans la Game view, changer la résolution (dropdown en haut de la Game view) entre `Free Aspect`, `1920x1080`, `1280x720`. Les éléments doivent se redimensionner proportionnellement sans déborder.

- [ ] **Step 4 : Sauvegarder et commit**

Ctrl+S, puis :
```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: configure Canvas Scaler with Scale With Screen Size (1920x1080)"
```

---

## Task 2 : Anchors — MainPanel

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Sélectionner MainPanel**

Dans la Hierarchy, sélectionner **MainPanel** (enfant du Canvas).

- [ ] **Step 2 : Appliquer stretch/stretch**

Dans l'Inspector, cliquer sur le carré **Anchor Presets** (en haut à gauche du RectTransform) :
- Maintenir **Alt** (pour aussi déplacer le pivot) + cliquer sur le preset **stretch/stretch** (coin bas-droit du sélecteur, icône 4 flèches)
- Left: 0 / Right: 0 / Top: 0 / Bottom: 0

- [ ] **Step 3 : Vérifier**

Changer la résolution dans la Game view — MainPanel doit couvrir tout le Canvas.

- [ ] **Step 4 : Sauvegarder et commit**

Ctrl+S, puis :
```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: set MainPanel anchors to stretch/stretch"
```

---

## Task 3 : Anchors — SlotPanel

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : Sélectionner SlotPanel**

Dans la Hierarchy, sélectionner **SlotPanel**.

- [ ] **Step 2 : Appliquer stretch/stretch**

Anchor Presets → Alt + **stretch/stretch** → Left: 0 / Right: 0 / Top: 0 / Bottom: 0

- [ ] **Step 3 : Vérifier**

SlotPanel doit couvrir tout le Canvas à toutes les résolutions.

- [ ] **Step 4 : Sauvegarder et commit**

Ctrl+S, puis :
```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: set SlotPanel anchors to stretch/stretch"
```

---

## Task 4 : Anchors — SettingsPanel + éléments internes

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity`

- [ ] **Step 1 : SettingsPanel — stretch/stretch**

Sélectionner **SettingsPanel** → Anchor Presets → Alt + **stretch/stretch** → offsets à 0.

- [ ] **Step 2 : Boutons onglets (Manette, Controles, Son)**

Pour chacun des 3 boutons d'onglet dans SettingsPanel :
1. Sélectionner le bouton
2. Anchor Presets → **top-center** (ou top-stretch si tu veux qu'ils s'étirent)
3. Ajuster `Pos Y` pour que les boutons restent proches du haut (ex: `-60`)

- [ ] **Step 3 : Zone de contenu des rebindings**

Sélectionner le panel/conteneur qui contient les labels et boutons "Attaquer / Button" :
1. Anchor Presets → **stretch/stretch** avec une marge haut (pour ne pas couvrir les onglets) et marge bas (pour le bouton Retour)
2. Top offset : `120` / Bottom offset : `80` / Left: `20` / Right: `20`

- [ ] **Step 4 : Bouton Retour**

Sélectionner le bouton **Retour** :
1. Anchor Presets → **bottom-left**
2. `Pos X`: `30` / `Pos Y`: `30` (marges depuis le coin bas-gauche)

- [ ] **Step 5 : Sliders Son (SonPanel)**

Pour chaque Slider dans le panel Son :
1. Anchor Presets → **middle-stretch** (s'étire horizontalement, centré verticalement)
2. Left: `40` / Right: `40`

- [ ] **Step 6 : Vérifier en Play Mode**

Appuyer sur Play. Tester à différentes résolutions dans la Game view :
- Les 3 boutons onglets sont cliquables
- Le bouton Retour est cliquable en bas à gauche
- Les labels/boutons de rebinding sont lisibles et dans leur zone
- Aucun élément ne déborde

- [ ] **Step 7 : Sauvegarder et commit**

Ctrl+S, puis :
```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: fix SettingsPanel internal anchors for responsive layout"
```

---

## Task 5 : Vérification finale multi-résolution

**Files:**
- Modify (Editor): `Assets/_Scenes/MenuScene.unity` (si ajustements nécessaires)

- [ ] **Step 1 : Tester les résolutions clés**

Dans la Game view, tester successivement :
- `1920x1080` (Full HD)
- `2560x1440` (2K)
- `1280x720` (HD)
- `Free Aspect` (fenêtre redimensionnable)

Pour chaque résolution vérifier :
- [ ] MainPanel : boutons Jouer / Paramètres / Quitter visibles et cliquables
- [ ] SettingsPanel : boutons Manette / Controles / Son cliquables
- [ ] SettingsPanel : bouton Retour cliquable
- [ ] SlotPanel : slots de sauvegarde visibles

- [ ] **Step 2 : Corriger si nécessaire**

Si un élément déborde ou est mal positionné : ajuster son anchor ou ses offsets.

- [ ] **Step 3 : Commit final**

```bash
git add Assets/_Scenes/MenuScene.unity
git commit -m "feat: responsive menu UI validated across PC resolutions"
```
