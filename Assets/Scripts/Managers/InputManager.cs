using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton DontDestroyOnLoad.
/// Centralise toutes les entrées (clavier + manette) avec rebinding.
/// PlayerBehavior et WeaponManager lisent leurs inputs ici.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Actions
    private InputAction m_moveAction;
    private InputAction m_attackAction;
    private InputAction m_inventoryAction;
    private InputAction m_runAction;
    private InputAction m_hideAction;
    private InputAction m_dashAction;
    private InputAction m_alterEgoAction;
    private InputAction m_hotbar1Action;
    private InputAction m_hotbar2Action;
    private InputAction m_scrollAction;
    private InputAction m_pauseAction;
    private InputAction m_placeMessageAction;
    private InputAction m_uiBackAction;

    // Clé PlayerPrefs pour les overrides
    private const string BINDINGS_KEY = "InputBindings";

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetInstance() => Instance = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildActions();
        LoadBindings();
        EnableActions();
    }

    void OnDestroy()
    {
        DisableActions();
    }

    // ── Propriétés publiques ────────────────────────────────────────────────

    public Vector2 MoveInput        => m_moveAction.ReadValue<Vector2>();
    public bool    AttackHeld       => m_attackAction.IsPressed();
    public bool    AttackPressed    => m_attackAction.WasPressedThisFrame();
    public bool    InventoryPressed => m_inventoryAction.WasPressedThisFrame();
    public bool    RunHeld          => m_runAction.IsPressed();
    public bool    HidePressed      => m_hideAction.WasPressedThisFrame();
    public bool    DashPressed      => m_dashAction.WasPressedThisFrame();
    public bool    AlterEgoPressed  => m_alterEgoAction.WasPressedThisFrame();
    public bool    Hotbar1Pressed   => m_hotbar1Action.WasPressedThisFrame();
    public bool    Hotbar2Pressed   => m_hotbar2Action.WasPressedThisFrame();
    public float   ScrollInput      => m_scrollAction.ReadValue<float>();
    public bool    PausePressed        => m_pauseAction.WasPressedThisFrame();
    public bool    PlaceMessagePressed => m_placeMessageAction.WasPressedThisFrame();
    /// <summary>Bouton "retour/annuler" dans les menus (B sur Xbox, Rond sur PS).</summary>
    public bool    UIBackPressed       => m_uiBackAction.WasPressedThisFrame();

    // ── Rebinding ───────────────────────────────────────────────────────────

    public InputAction GetAction(string actionName)
    {
        return actionName switch
        {
            "Move"      => m_moveAction,
            "Attack"    => m_attackAction,
            "Inventory" => m_inventoryAction,
            "Run"       => m_runAction,
            "Hide"      => m_hideAction,
            "Dash"      => m_dashAction,
            "AlterEgo"  => m_alterEgoAction,
            "Hotbar1"   => m_hotbar1Action,
            "Hotbar2"   => m_hotbar2Action,
            _           => null
        };
    }

    public void StartRebinding(string actionName, int bindingIndex, System.Action onComplete)
    {
        InputAction action = GetAction(actionName);
        if (action == null) { onComplete?.Invoke(); return; }

        action.Disable();
        action.PerformInteractiveRebinding(bindingIndex)
              .WithCancelingThrough("<Keyboard>/escape")
              .OnComplete(op => { op.Dispose(); SaveBindings(); action.Enable(); onComplete?.Invoke(); })
              .Start();
    }

    public string GetBindingDisplayString(string actionName, int bindingIndex)
    {
        InputAction action = GetAction(actionName);
        return action?.GetBindingDisplayString(bindingIndex) ?? "?";
    }

    public void ResetAllBindings()
    {
        m_moveAction.RemoveAllBindingOverrides();
        m_attackAction.RemoveAllBindingOverrides();
        m_inventoryAction.RemoveAllBindingOverrides();
        m_runAction.RemoveAllBindingOverrides();
        m_hideAction.RemoveAllBindingOverrides();
        m_dashAction.RemoveAllBindingOverrides();
        m_alterEgoAction.RemoveAllBindingOverrides();
        m_hotbar1Action.RemoveAllBindingOverrides();
        m_hotbar2Action.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(BINDINGS_KEY);
    }

    // ── Interne ─────────────────────────────────────────────────────────────

    private void BuildActions()
    {
        // Mouvement — composite WASD + stick gauche manette
        m_moveAction = new InputAction("Move", InputActionType.Value);
        m_moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        m_moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        m_moveAction.AddBinding("<Gamepad>/leftStick");

        // Attaque / Interaction
        m_attackAction = new InputAction("Attack", InputActionType.Button);
        m_attackAction.AddBinding("<Keyboard>/space");
        m_attackAction.AddBinding("<Gamepad>/buttonSouth");

        // Inventaire
        m_inventoryAction = new InputAction("Inventory", InputActionType.Button);
        m_inventoryAction.AddBinding("<Keyboard>/e");
        m_inventoryAction.AddBinding("<Gamepad>/select");

        // Courir
        m_runAction = new InputAction("Run", InputActionType.Button);
        m_runAction.AddBinding("<Keyboard>/leftShift");
        m_runAction.AddBinding("<Gamepad>/leftShoulder");

        // Se cacher
        m_hideAction = new InputAction("Hide", InputActionType.Button);
        m_hideAction.AddBinding("<Keyboard>/f");
        m_hideAction.AddBinding("<Gamepad>/buttonEast");

        // Dash (alter ego uniquement — partage F avec Hide, contexte géré par AlterEgoManager)
        m_dashAction = new InputAction("Dash", InputActionType.Button);
        m_dashAction.AddBinding("<Keyboard>/f");
        m_dashAction.AddBinding("<Gamepad>/buttonEast");

        // Alter Ego — bascule la transformation
        m_alterEgoAction = new InputAction("AlterEgo", InputActionType.Button);
        m_alterEgoAction.AddBinding("<Keyboard>/q");
        m_alterEgoAction.AddBinding("<Gamepad>/rightStickPress");

        // Hotbar
        m_hotbar1Action = new InputAction("Hotbar1", InputActionType.Button);
        m_hotbar1Action.AddBinding("<Keyboard>/1");
        m_hotbar1Action.AddBinding("<Gamepad>/buttonWest");

        m_hotbar2Action = new InputAction("Hotbar2", InputActionType.Button);
        m_hotbar2Action.AddBinding("<Keyboard>/2");
        m_hotbar2Action.AddBinding("<Gamepad>/buttonNorth");

        // Molette
        m_scrollAction = new InputAction("Scroll", InputActionType.Value);
        m_scrollAction.AddBinding("<Mouse>/scroll/y");
        m_scrollAction.AddBinding("<Gamepad>/dpad/y");

        // Pause / Echap
        m_pauseAction = new InputAction("Pause", InputActionType.Button);
        m_pauseAction.AddBinding("<Keyboard>/escape");
        m_pauseAction.AddBinding("<Gamepad>/start");

        // Poser un message (style Dark Souls) — touche N
        m_placeMessageAction = new InputAction("PlaceMessage", InputActionType.Button);
        m_placeMessageAction.AddBinding("<Keyboard>/n");
        m_placeMessageAction.AddBinding("<Gamepad>/rightShoulder");

        // Retour/Annuler dans les menus — B (Xbox) / Rond (PS)
        m_uiBackAction = new InputAction("UIBack", InputActionType.Button);
        m_uiBackAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void EnableActions()
    {
        m_moveAction.Enable();
        m_attackAction.Enable();
        m_inventoryAction.Enable();
        m_runAction.Enable();
        m_hideAction.Enable();
        m_dashAction.Enable();
        m_alterEgoAction.Enable();
        m_hotbar1Action.Enable();
        m_hotbar2Action.Enable();
        m_scrollAction.Enable();
        m_pauseAction.Enable();
        m_placeMessageAction.Enable();
        m_uiBackAction.Enable();
    }

    private void DisableActions()
    {
        m_moveAction?.Disable();
        m_attackAction?.Disable();
        m_inventoryAction?.Disable();
        m_runAction?.Disable();
        m_hideAction?.Disable();
        m_dashAction?.Disable();
        m_alterEgoAction?.Disable();
        m_hotbar1Action?.Disable();
        m_hotbar2Action?.Disable();
        m_scrollAction?.Disable();
        m_pauseAction?.Disable();
        m_placeMessageAction?.Disable();
        m_uiBackAction?.Disable();
    }

    public void SaveBindings()
    {
        var data = new BindingSaveData
        {
            move      = m_moveAction.SaveBindingOverridesAsJson(),
            attack    = m_attackAction.SaveBindingOverridesAsJson(),
            inventory = m_inventoryAction.SaveBindingOverridesAsJson(),
            run       = m_runAction.SaveBindingOverridesAsJson(),
            hide      = m_hideAction.SaveBindingOverridesAsJson(),
            dash      = m_dashAction.SaveBindingOverridesAsJson(),
            alterEgo  = m_alterEgoAction.SaveBindingOverridesAsJson(),
            hotbar1   = m_hotbar1Action.SaveBindingOverridesAsJson(),
            hotbar2   = m_hotbar2Action.SaveBindingOverridesAsJson(),
        };
        PlayerPrefs.SetString(BINDINGS_KEY, JsonUtility.ToJson(data));
    }

    private void LoadBindings()
    {
        if (!PlayerPrefs.HasKey(BINDINGS_KEY)) return;
        var data = JsonUtility.FromJson<BindingSaveData>(PlayerPrefs.GetString(BINDINGS_KEY));
        if (data == null) return;
        m_moveAction.LoadBindingOverridesFromJson(data.move);
        m_attackAction.LoadBindingOverridesFromJson(data.attack);
        m_inventoryAction.LoadBindingOverridesFromJson(data.inventory);
        m_runAction.LoadBindingOverridesFromJson(data.run);
        m_hideAction.LoadBindingOverridesFromJson(data.hide);
        if (data.dash    != null) m_dashAction.LoadBindingOverridesFromJson(data.dash);
        if (data.alterEgo != null) m_alterEgoAction.LoadBindingOverridesFromJson(data.alterEgo);
        m_hotbar1Action.LoadBindingOverridesFromJson(data.hotbar1);
        m_hotbar2Action.LoadBindingOverridesFromJson(data.hotbar2);
    }

    [System.Serializable]
    private class BindingSaveData
    {
        public string move, attack, inventory, run, hide, dash, alterEgo, hotbar1, hotbar2;
    }
}
