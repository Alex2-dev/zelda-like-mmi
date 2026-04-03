# Design — UI Menu Responsive (PC)

**Date :** 2026-04-03  
**Scope :** MenuScene — adaptation automatique du menu à toutes les résolutions PC

---

## Problème

En plein écran (ex: 1920x1080), les éléments UI restent positionnés en pixels fixes issus de la résolution de l'éditeur (petite fenêtre). Les zones de clic ne correspondent plus aux visuels, rendant les boutons Manette/Controles/Son et Retour incliquables.

---

## Solution : Canvas Scaler + Anchors

### Canvas Scaler (sur chaque Canvas des scènes UI)

| Propriété | Valeur |
|---|---|
| UI Scale Mode | Scale With Screen Size |
| Reference Resolution | 1920 x 1080 |
| Screen Match Mode | Match Width Or Height |
| Match | 0.5 |

Scènes concernées : `MenuScene` (priorité), vérifier aussi `MainScene` si elle contient des Canvas.

### Anchors des panels

Chaque panel racine (`MainPanel`, `SlotPanel`, `SettingsPanel`) utilise **stretch/stretch** (anchors aux 4 coins, offsets à 0) pour occuper toute la zone du Canvas parent.

### Anchors des éléments internes

| Élément | Anchor preset |
|---|---|
| Boutons onglets (Manette, Controles, Son) | top-center |
| Zone de contenu des rebindings | center / stretch horizontal |
| Bouton Retour | bottom-left |
| Sliders Son | center / stretch horizontal |

---

## Hors scope

- Pas de support mobile/tablette
- Pas de layouts différents selon la résolution (pas de breakpoints)
- Pas de nouveau code C# — uniquement configuration Unity Editor
