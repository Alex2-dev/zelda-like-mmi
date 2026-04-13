using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gère la navigation manette dans les menus Unity UI sans dépendre
/// de la configuration d'InputSystemUIInputModule.
///
/// SETUP : Attacher sur l'objet InputManager dans chaque scène.
///
/// Contrôles :
///   Stick gauche / D-pad  → naviguer entre les boutons
///   Bouton Sud (A/Croix)  → confirmer (clic sur le bouton sélectionné)
///   Bouton Est  (B/Rond)  → annuler / retour (UIBackPressed)
/// </summary>
public class GamepadMenuNavigator : MonoBehaviour
{
    [Tooltip("Délai avant la première répétition de navigation (en secondes).")]
    public float m_firstDelay = 0.4f;

    [Tooltip("Délai entre les répétitions suivantes.")]
    public float m_repeatRate = 0.15f;

    private float m_nextNavTime = 0f;
    private bool  m_axisIdle   = true;

    void Update()
    {
        if (EventSystem.current == null) return;

        HandleNavigation();
        HandleSubmit();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void HandleNavigation()
    {
        Vector2 dir = GetNavInput();

        if (dir.magnitude < 0.5f)
        {
            m_axisIdle    = true;
            m_nextNavTime = 0f;
            return;
        }

        float now = Time.unscaledTime;

        if (m_axisIdle)
        {
            m_axisIdle    = false;
            m_nextNavTime = now + m_firstDelay;
            TryNavigate(dir);
        }
        else if (now >= m_nextNavTime)
        {
            m_nextNavTime = now + m_repeatRate;
            TryNavigate(dir);
        }
    }

    private void TryNavigate(Vector2 dir)
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        Selectable sel = selected?.GetComponent<Selectable>();
        if (sel == null) return;

        Selectable next = null;
        if      (dir.y >  0.5f) next = sel.FindSelectableOnUp();
        else if (dir.y < -0.5f) next = sel.FindSelectableOnDown();
        else if (dir.x < -0.5f) next = sel.FindSelectableOnLeft();
        else if (dir.x >  0.5f) next = sel.FindSelectableOnRight();

        if (next != null && next.interactable && next.gameObject.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(next.gameObject);
    }

    private Vector2 GetNavInput()
    {
        Vector2 best = Vector2.zero;

        // Stick gauche via InputManager
        if (InputManager.Instance != null)
        {
            Vector2 stick = InputManager.Instance.MoveInput;
            if (stick.magnitude > best.magnitude) best = stick;
        }

        // D-pad direct (Gamepad.current)
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 dpad = gamepad.dpad.ReadValue();
            if (dpad.magnitude > best.magnitude) best = dpad;
        }

        return best;
    }

    // ── Confirmer (A / Croix) ────────────────────────────────────────────────

    private void HandleSubmit()
    {
        // On n'agit que si un élément UI est sélectionné
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        // buttonSouth = A (Xbox) / Croix (PS) — wasPressedThisFrame pour éviter les répétitions
        if (!gamepad.buttonSouth.wasPressedThisFrame) return;

        // Slider : ajuster la valeur avec les axes plutôt que cliquer
        var slider = selected.GetComponent<Slider>();
        if (slider != null) return; // Les sliders se gèrent avec le stick, pas avec A

        var btn = selected.GetComponent<Button>();
        btn?.onClick.Invoke();
    }
}
