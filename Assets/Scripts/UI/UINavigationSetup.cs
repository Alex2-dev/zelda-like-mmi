using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Helpers statiques pour la navigation manette dans les menus.
/// Pas besoin d'attacher ce script à un GameObject.
/// </summary>
public static class UINavigationSetup
{
    /// <summary>
    /// Sélectionne le premier bouton actif et interactable dans le tableau fourni.
    /// Appeler après SetActive(true) sur le panel correspondant.
    /// </summary>
    public static void SelectFirst(Button[] buttons)
    {
        if (EventSystem.current == null) return;
        foreach (var btn in buttons)
        {
            if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable)
            {
                EventSystem.current.SetSelectedGameObject(btn.gameObject);
                return;
            }
        }
    }

    /// <summary>Sélectionne le premier bouton trouvé dans un panel (ordre hiérarchique).</summary>
    public static void SelectFirstInPanel(GameObject panel)
    {
        if (panel == null || EventSystem.current == null) return;
        var buttons = panel.GetComponentsInChildren<Button>(false);
        SelectFirst(buttons);
    }

    /// <summary>Retire la sélection courante (utile quand tous les menus se ferment).</summary>
    public static void Deselect()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
