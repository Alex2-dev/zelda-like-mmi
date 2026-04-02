# Game Infrastructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter un menu principal, un système de save (3 slots), des paramètres (touches/manette/son), et des animations de transition "entrée/sortie du PC".

**Architecture:** Approche A — deux scènes (`MenuScene` + `MainScene`) reliées par des singletons `DontDestroyOnLoad` (`GameManager`, `SaveManager`, `InputManager`, `TransitionManager`). Le New Input System Unity gère le rebinding touches + manette.

**Tech Stack:** Unity New Input System (`com.unity.inputsystem`), `JsonUtility`, `PlayerPrefs`, `RenderTexture`, Coroutines, `SceneManager.LoadSceneAsync`

---

## Prérequis Manuel (Unity Editor)

Avant tout code :
- [ ] **Installer le package New Input System** : Window → Package Manager → chercher "Input System" → Install
- [ ] Quand Unity demande "Enable the new input system?" → cliquer **Yes** (redémarre l'éditeur)
- [ ] Dans Edit → Project Settings → Player → Other Settings → **Active Input Handling** = "Both" (garde la compatibilité avec l'ancien Input)
- [ ] Créer le dossier `Assets/Input/`
- [ ] Créer le dossier `Assets/Scripts/Managers/`
- [ ] Créer le dossier `Assets/Scripts/UI/`
- [ ] Créer le dossier `Assets/RenderTextures/`

---

## Task 1 : InputManager

**Files:**
- Create: `Assets/Scripts/Managers/InputManager.cs`

- [ ] **Créer InputManager.cs**

```csharp
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
    private InputAction m_mapAction;
    private InputAction m_hotbar1Action;
    private InputAction m_hotbar2Action;
    private InputAction m_scrollAction;
    private InputAction m_pauseAction;

    // Clé PlayerPrefs pour les overrides
    private const string BINDINGS_KEY = "InputBindings";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
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

    public Vector2 MoveInput     => m_moveAction.ReadValue<Vector2>();
    public bool    AttackHeld    => m_attackAction.IsPressed();
    public bool    AttackPressed => m_attackAction.WasPressedThisFrame();
    public bool    InventoryPressed => m_inventoryAction.WasPressedThisFrame();
    public bool    RunHeld       => m_runAction.IsPressed();
    public bool    HidePressed   => m_hideAction.WasPressedThisFrame();
    public bool    MapPressed    => m_mapAction.WasPressedThisFrame();
    public bool    Hotbar1Pressed => m_hotbar1Action.WasPressedThisFrame();
    public bool    Hotbar2Pressed => m_hotbar2Action.WasPressedThisFrame();
    public float   ScrollInput   => m_scrollAction.ReadValue<float>();
    public bool    PausePressed  => m_pauseAction.WasPressedThisFrame();

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
            "Map"       => m_mapAction,
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
        m_mapAction.RemoveAllBindingOverrides();
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
            .With("Up",    "<Keyboard>/z")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/q")
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
        m_attackAction.AddBinding("<Gamepad>/buttonSouth"); // A / Cross

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
        m_hideAction.AddBinding("<Gamepad>/buttonEast"); // B / Cercle

        // Carte
        m_mapAction = new InputAction("Map", InputActionType.Button);
        m_mapAction.AddBinding("<Keyboard>/m");
        m_mapAction.AddBinding("<Gamepad>/start");

        // Hotbar
        m_hotbar1Action = new InputAction("Hotbar1", InputActionType.Button);
        m_hotbar1Action.AddBinding("<Keyboard>/1");
        m_hotbar1Action.AddBinding("<Gamepad>/buttonWest"); // X / Carré

        m_hotbar2Action = new InputAction("Hotbar2", InputActionType.Button);
        m_hotbar2Action.AddBinding("<Keyboard>/2");
        m_hotbar2Action.AddBinding("<Gamepad>/buttonNorth"); // Y / Triangle

        // Molette
        m_scrollAction = new InputAction("Scroll", InputActionType.Value);
        m_scrollAction.AddBinding("<Mouse>/scroll/y");
        m_scrollAction.AddBinding("<Gamepad>/dpad/y");

        // Pause / Echap
        m_pauseAction = new InputAction("Pause", InputActionType.Button);
        m_pauseAction.AddBinding("<Keyboard>/escape");
        m_pauseAction.AddBinding("<Gamepad>/start");
    }

    private void EnableActions()
    {
        m_moveAction.Enable();
        m_attackAction.Enable();
        m_inventoryAction.Enable();
        m_runAction.Enable();
        m_hideAction.Enable();
        m_mapAction.Enable();
        m_hotbar1Action.Enable();
        m_hotbar2Action.Enable();
        m_scrollAction.Enable();
        m_pauseAction.Enable();
    }

    private void DisableActions()
    {
        m_moveAction?.Disable();
        m_attackAction?.Disable();
        m_inventoryAction?.Disable();
        m_runAction?.Disable();
        m_hideAction?.Disable();
        m_mapAction?.Disable();
        m_hotbar1Action?.Disable();
        m_hotbar2Action?.Disable();
        m_scrollAction?.Disable();
        m_pauseAction?.Disable();
    }

    private void SaveBindings()
    {
        var data = new BindingSaveData
        {
            move      = m_moveAction.SaveBindingOverridesAsJson(),
            attack    = m_attackAction.SaveBindingOverridesAsJson(),
            inventory = m_inventoryAction.SaveBindingOverridesAsJson(),
            run       = m_runAction.SaveBindingOverridesAsJson(),
            hide      = m_hideAction.SaveBindingOverridesAsJson(),
            map       = m_mapAction.SaveBindingOverridesAsJson(),
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
        m_mapAction.LoadBindingOverridesFromJson(data.map);
        m_hotbar1Action.LoadBindingOverridesFromJson(data.hotbar1);
        m_hotbar2Action.LoadBindingOverridesFromJson(data.hotbar2);
    }

    [System.Serializable]
    private class BindingSaveData
    {
        public string move, attack, inventory, run, hide, map, hotbar1, hotbar2;
    }
}
```

- [ ] **Vérifier compilation** : ouvrir Unity, s'assurer que la console n'affiche aucune erreur

- [ ] **Commit**
```bash
git add Assets/Scripts/Managers/InputManager.cs
git commit -m "feat: add InputManager with New Input System (keyboard + gamepad + rebinding)"
```

---

## Task 2 : Mettre à jour PlayerBehavior

**Files:**
- Modify: `Assets/Scripts/PlayerBehavior.cs`

- [ ] **Remplacer les appels Input legacy dans `Move()`**

Remplacer :
```csharp
float horizontalOffset = Input.GetAxis("Horizontal");
float verticalOffset = Input.GetAxis("Vertical");

float currentSpeed = m_speed;
if (m_playerStats != null && Input.GetKey(KeyCode.LeftShift))
{
    currentSpeed *= m_playerStats.m_runMultiplier;
}
```

Par :
```csharp
Vector2 moveInput = InputManager.Instance != null
    ? InputManager.Instance.MoveInput
    : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

float horizontalOffset = moveInput.x;
float verticalOffset   = moveInput.y;

float currentSpeed = m_speed;
bool running = InputManager.Instance != null
    ? InputManager.Instance.RunHeld
    : Input.GetKey(KeyCode.LeftShift);
if (m_playerStats != null && running)
    currentSpeed *= m_playerStats.m_runMultiplier;
```

- [ ] **Remplacer les appels Input legacy dans `Update()`**

Remplacer :
```csharp
if (Input.GetKeyDown(KeyCode.M))
    m_map.SetActive(!m_map.activeSelf);
```
Par :
```csharp
bool mapPressed = InputManager.Instance != null
    ? InputManager.Instance.MapPressed
    : Input.GetKeyDown(KeyCode.M);
if (mapPressed)
    m_map.SetActive(!m_map.activeSelf);
```

Remplacer :
```csharp
if (m_inventory != null && !InventoryUI.IsOpen)
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
        m_inventory.UseHotbarSlot(0);
    if (Input.GetKeyDown(KeyCode.Alpha2))
        m_inventory.UseHotbarSlot(1);

    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0)
        m_inventory.ScrollHotbar(scroll);
}
```
Par :
```csharp
if (m_inventory != null && !InventoryUI.IsOpen)
{
    bool h1 = InputManager.Instance != null ? InputManager.Instance.Hotbar1Pressed : Input.GetKeyDown(KeyCode.Alpha1);
    bool h2 = InputManager.Instance != null ? InputManager.Instance.Hotbar2Pressed : Input.GetKeyDown(KeyCode.Alpha2);
    float scroll = InputManager.Instance != null ? InputManager.Instance.ScrollInput : Input.GetAxis("Mouse ScrollWheel");

    if (h1) m_inventory.UseHotbarSlot(0);
    if (h2) m_inventory.UseHotbarSlot(1);
    if (scroll != 0) m_inventory.ScrollHotbar(scroll);
}
```

Remplacer :
```csharp
if (Input.GetKeyDown(KeyCode.Escape))
    Application.Quit();
```
Par :
```csharp
// Géré par GameManager via InputManager.PausePressed
```

Remplacer :
```csharp
if (Input.GetKeyDown(KeyCode.Space))
{
    if (m_closestNPCDialog != null)
        m_dialogDisplayer.SetDialog(m_closestNPCDialog.GetDialog());
    else if (m_weaponManager != null && m_weaponManager.HasWeapon() && !m_weaponManager.IsAutomatic())
        m_weaponManager.TryShoot(GetShootDirectionVector());
}
if (Input.GetKeyDown(KeyCode.F))
```
Par :
```csharp
bool attackPressed = InputManager.Instance != null ? InputManager.Instance.AttackPressed : Input.GetKeyDown(KeyCode.Space);
if (attackPressed)
{
    if (m_closestNPCDialog != null)
        m_dialogDisplayer.SetDialog(m_closestNPCDialog.GetDialog());
    else if (m_weaponManager != null && m_weaponManager.HasWeapon() && !m_weaponManager.IsAutomatic())
        m_weaponManager.TryShoot(GetShootDirectionVector());
}
bool hidePressed = InputManager.Instance != null ? InputManager.Instance.HidePressed : Input.GetKeyDown(KeyCode.F);
if (hidePressed)
```

- [ ] **Vérifier** : lancer MainScene en Play Mode, le joueur doit se déplacer et tirer normalement

- [ ] **Commit**
```bash
git add Assets/Scripts/PlayerBehavior.cs
git commit -m "feat: migrate PlayerBehavior to InputManager (legacy fallback kept)"
```

---

## Task 3 : Mettre à jour WeaponManager

**Files:**
- Modify: `Assets/Scripts/Weapons/WeaponManager.cs`

- [ ] **Remplacer `Input.GetKey(KeyCode.Space)` dans `Update()`**

Remplacer :
```csharp
if (m_canFire && m_equippedWeapon != null && m_equippedWeapon.m_isAutomatic && Input.GetKey(KeyCode.Space))
    TryShoot(m_currentDirection);
```
Par :
```csharp
bool fireHeld = InputManager.Instance != null ? InputManager.Instance.AttackHeld : Input.GetKey(KeyCode.Space);
if (m_canFire && m_equippedWeapon != null && m_equippedWeapon.m_isAutomatic && fireHeld)
    TryShoot(m_currentDirection);
```

- [ ] **Commit**
```bash
git add Assets/Scripts/Weapons/WeaponManager.cs
git commit -m "feat: migrate WeaponManager to InputManager"
```

---

## Task 4 : AudioManager — contrôle du volume

**Files:**
- Modify: `Assets/Scripts/AudioManager.cs`

- [ ] **Ajouter les méthodes de volume et chargement PlayerPrefs**

Remplacer tout le contenu de `AudioManager.cs` par :
```csharp
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public AudioSource m_soundStream;
    public AudioSource m_musicStream;

    private const string MUSIC_VOL_KEY  = "MusicVolume";
    private const string SOUND_VOL_KEY  = "SoundVolume";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ── Lecture ─────────────────────────────────────────────────────────────

    public void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        m_soundStream.pitch  = pitch;
        m_soundStream.volume = volume * GetSoundVolume();
        m_soundStream.clip   = clip;
        m_soundStream.Play();
    }

    public void StopSound() => m_soundStream.Stop();

    public void PlayMusic(AudioClip clip, bool loop, float volume = 1.0f, float pitch = 1.0f)
    {
        m_musicStream.pitch  = pitch;
        m_musicStream.volume = volume * GetMusicVolume();
        m_musicStream.loop   = loop;
        m_musicStream.clip   = clip;
        m_musicStream.Play();
    }

    public void StopMusic() => m_musicStream.Stop();

    // ── Volume ───────────────────────────────────────────────────────────────

    public float GetMusicVolume() => PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.8f);
    public float GetSoundVolume() => PlayerPrefs.GetFloat(SOUND_VOL_KEY, 1.0f);

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
        m_musicStream.volume = value;
    }

    public void SetSoundVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SOUND_VOL_KEY, value);
        m_soundStream.volume = value;
    }

    private void LoadVolumes()
    {
        m_musicStream.volume = GetMusicVolume();
        m_soundStream.volume = GetSoundVolume();
    }
}
```

- [ ] **Commit**
```bash
git add Assets/Scripts/AudioManager.cs
git commit -m "feat: add volume control to AudioManager with PlayerPrefs persistence"
```

---

## Task 5 : SaveManager

**Files:**
- Create: `Assets/Scripts/Managers/SaveManager.cs`

- [ ] **Créer SaveManager.cs**

```csharp
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int    slotIndex;
    public float  posX;
    public float  posY;
    public float  currentHP;
    public float  maxHP;
    public string inventoryJson;   // JSON sérialisé de l'inventaire
    public string hotbarJson;
    public string killedEnemyIds;  // IDs séparés par ","
    public bool   bossDefeated;
    public string completedDialogs;
    public string openedDoors;
    public float  playTime;
    public bool   isEmpty = true;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SLOT_COUNT = 3;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Chemins ─────────────────────────────────────────────────────────────

    private string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    // ── Lecture / Écriture ───────────────────────────────────────────────────

    public void Save(int slot, SaveData data)
    {
        data.slotIndex = slot;
        data.isEmpty   = false;
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
    }

    public SaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return new SaveData { slotIndex = slot, isEmpty = true };
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) ?? new SaveData { slotIndex = slot, isEmpty = true };
    }

    public void Delete(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool SlotExists(int slot) => File.Exists(GetPath(slot));

    public SaveData[] LoadAll()
    {
        var saves = new SaveData[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
            saves[i] = Load(i);
        return saves;
    }

    // ── Snapshot du joueur ───────────────────────────────────────────────────

    public SaveData TakeSnapshot(int slot, GameObject player)
    {
        var data = Load(slot); // garde l'existant pour les champs non-player
        data.slotIndex = slot;
        data.isEmpty   = false;

        // Position
        data.posX = player.transform.position.x;
        data.posY = player.transform.position.y;

        // HP
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            data.currentHP = stats.CurrentHealth;
            data.maxHP     = stats.MaxHealth;
        }

        // Temps de jeu
        data.playTime += Time.timeSinceLevelLoad;

        return data;
    }
}
```

- [ ] **Vérifier** : compiler dans Unity, aucune erreur

- [ ] **Commit**
```bash
git add Assets/Scripts/Managers/SaveManager.cs
git commit -m "feat: add SaveManager with 3-slot JSON persistence"
```

---

## Task 6 : GameManager

**Files:**
- Create: `Assets/Scripts/Managers/GameManager.cs`

- [ ] **Créer GameManager.cs**

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int  ActiveSlot   { get; private set; } = 0;
    public bool BossDefeated { get; private set; } = false;

    private const string MAIN_SCENE = "MainScene";
    private const string MENU_SCENE = "MenuScene";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Démarrer une partie ──────────────────────────────────────────────────

    public void StartGame(int slot)
    {
        ActiveSlot = slot;
        TransitionManager.Instance.PlayEnterPC(() =>
        {
            SceneManager.LoadScene(MAIN_SCENE);
        });
    }

    // ── Fin de jeu (boss tué) ────────────────────────────────────────────────

    public void TriggerEndGame()
    {
        BossDefeated = true;

        // Sauvegarder avant de partir
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && SaveManager.Instance != null)
        {
            SaveData data = SaveManager.Instance.TakeSnapshot(ActiveSlot, player);
            data.bossDefeated = true;
            SaveManager.Instance.Save(ActiveSlot, data);
        }

        TransitionManager.Instance.PlayExitPC(() =>
        {
            SceneManager.LoadScene(MENU_SCENE);
        });
    }

    // ── Retour menu (sans fin de jeu) ────────────────────────────────────────

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }

    // ── Quick Save ───────────────────────────────────────────────────────────

    public void QuickSave()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || SaveManager.Instance == null) return;
        SaveData data = SaveManager.Instance.TakeSnapshot(ActiveSlot, player);
        SaveManager.Instance.Save(ActiveSlot, data);
    }
}
```

- [ ] **Commit**
```bash
git add Assets/Scripts/Managers/GameManager.cs
git commit -m "feat: add GameManager singleton with scene transitions and quick save"
```

---

## Task 7 : TransitionManager

**Files:**
- Create: `Assets/Scripts/Managers/TransitionManager.cs`

- [ ] **Créer TransitionManager.cs**

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère les animations de transition "Entrée PC" et "Sortie PC".
/// Possède sa propre Canvas en DontDestroyOnLoad.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("UI Refs (assignées par code)")]
    private Canvas        m_canvas;
    private CanvasGroup   m_fadeGroup;
    private TextMeshProUGUI m_label;
    private RectTransform m_pcScreen;

    [Header("Timings")]
    public float m_fadeDuration   = 0.8f;
    public float m_zoomDuration   = 0.6f;
    public float m_typeSpeed      = 0.05f;  // secondes par lettre

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    // ── API publique ─────────────────────────────────────────────────────────

    public void PlayEnterPC(System.Action onComplete)
    {
        StartCoroutine(EnterPCRoutine(onComplete));
    }

    public void PlayExitPC(System.Action onComplete)
    {
        StartCoroutine(ExitPCRoutine(onComplete));
    }

    // ── Coroutines ───────────────────────────────────────────────────────────

    private IEnumerator EnterPCRoutine(System.Action onComplete)
    {
        m_canvas.gameObject.SetActive(true);

        // Zoom vers l'écran PC
        yield return ZoomIn();

        // Fondu noir
        yield return Fade(0f, 1f, m_fadeDuration);

        // Texte lettre par lettre
        yield return TypeText("Connexion en cours...");

        // Callback (charge la scène)
        onComplete?.Invoke();

        // Attendre que la scène soit chargée puis effacer
        yield return new WaitForSeconds(0.5f);
        yield return Fade(1f, 0f, m_fadeDuration);
        ClearText();
        m_canvas.gameObject.SetActive(false);
    }

    private IEnumerator ExitPCRoutine(System.Action onComplete)
    {
        m_canvas.gameObject.SetActive(true);

        // Fondu noir
        yield return Fade(0f, 1f, m_fadeDuration);

        // Texte lettre par lettre
        yield return TypeText("Déconnexion...");

        // Callback (charge le menu)
        onComplete?.Invoke();

        // Dézoom
        yield return new WaitForSeconds(0.5f);
        yield return ZoomOut();
        yield return Fade(1f, 0f, m_fadeDuration);
        ClearText();
        m_canvas.gameObject.SetActive(false);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            m_fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        m_fadeGroup.alpha = to;
    }

    private IEnumerator TypeText(string text)
    {
        m_label.text = "";
        foreach (char c in text)
        {
            m_label.text += c;
            yield return new WaitForSeconds(m_typeSpeed);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void ClearText() => m_label.text = "";

    private IEnumerator ZoomIn()
    {
        float t = 0f;
        Vector3 start = Vector3.one * 0.1f;
        Vector3 end   = Vector3.one;
        while (t < m_zoomDuration)
        {
            t += Time.deltaTime;
            m_pcScreen.localScale = Vector3.Lerp(start, end, t / m_zoomDuration);
            yield return null;
        }
        m_pcScreen.localScale = end;
    }

    private IEnumerator ZoomOut()
    {
        float t = 0f;
        Vector3 start = Vector3.one;
        Vector3 end   = Vector3.one * 0.1f;
        while (t < m_zoomDuration)
        {
            t += Time.deltaTime;
            m_pcScreen.localScale = Vector3.Lerp(start, end, t / m_zoomDuration);
            yield return null;
        }
        m_pcScreen.localScale = end;
    }

    // ── Création UI par code ─────────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas
        m_canvas = gameObject.AddComponent<Canvas>();
        m_canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        m_canvas.sortingOrder = 999;
        gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Fond noir (CanvasGroup pour le fade)
        var bg = new GameObject("Background");
        bg.transform.SetParent(transform, false);
        m_fadeGroup = bg.AddComponent<CanvasGroup>();
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

        // Écran PC (image simple blanc pour le zoom)
        var pc = new GameObject("PCScreen");
        pc.transform.SetParent(bg.transform, false);
        m_pcScreen = pc.AddComponent<RectTransform>();
        m_pcScreen.sizeDelta = new Vector2(200, 150);
        m_pcScreen.anchoredPosition = Vector2.zero;
        var pcImg = pc.AddComponent<Image>();
        pcImg.color = new Color(0.1f, 0.8f, 0.1f, 0.8f); // vert écran

        // Texte
        var textObj = new GameObject("Label");
        textObj.transform.SetParent(bg.transform, false);
        m_label = textObj.AddComponent<TextMeshProUGUI>();
        m_label.text      = "";
        m_label.fontSize  = 24;
        m_label.color     = new Color(0.1f, 1f, 0.1f); // vert terminal
        m_label.alignment = TextAlignmentOptions.Center;
        var labelRect = m_label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.1f, 0.4f);
        labelRect.anchorMax = new Vector2(0.9f, 0.6f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

        m_canvas.gameObject.SetActive(false);
    }
}
```

> **Note :** TransitionManager utilise `TextMeshPro`. S'assurer que le package TMP est installé (Window → Package Manager → TextMeshPro).

- [ ] **Commit**
```bash
git add Assets/Scripts/Managers/TransitionManager.cs
git commit -m "feat: add TransitionManager with enter/exit PC animations"
```

---

## Task 8 : Mettre à jour BossBehavior — fin de jeu

**Files:**
- Modify: `Assets/Scripts/Enemies/BossBehavior.cs`

- [ ] **Remplacer `Destroy(gameObject)` dans `OnBossDeath()` phase 2**

Remplacer :
```csharp
else if (m_phase == Phase.Phase2)
{
    // Vraiment mort
    Destroy(gameObject);
}
```
Par :
```csharp
else if (m_phase == Phase.Phase2)
{
    Destroy(gameObject);
    if (GameManager.Instance != null)
        GameManager.Instance.TriggerEndGame();
}
```

- [ ] **Commit**
```bash
git add Assets/Scripts/Enemies/BossBehavior.cs
git commit -m "feat: trigger end game sequence when boss phase 2 dies"
```

---

## Task 9 : Créer un GameObject "Managers" dans MainScene

**Étapes manuelles dans Unity Editor :**

- [ ] Ouvrir `MainScene`
- [ ] Créer un GameObject vide nommé `Managers`
- [ ] Ajouter les composants suivants sur `Managers` :
  - `GameManager`
  - `SaveManager`
  - `InputManager`
  - `TransitionManager`
  - `AudioManager` (si pas déjà dans la scène — le déplacer sur ce GO)
- [ ] Sauvegarder la scène (`Ctrl+S`)

- [ ] **Commit**
```bash
git add Assets/_Scenes/MainScene.unity
git commit -m "feat: add Managers GameObject to MainScene"
```

---

## Task 10 : Créer MenuScene

**Étapes manuelles dans Unity Editor :**

- [ ] File → New Scene → Basic 2D → sauvegarder sous `Assets/_Scenes/MenuScene.unity`
- [ ] File → Build Settings → Add Open Scenes (ajouter MenuScene ET MainScene, MenuScene en index 0)

**Créer MainMenuUI.cs :**

- [ ] **Créer `Assets/Scripts/UI/MainMenuUI.cs`**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller du menu principal.
/// Buttons : Jouer, Paramètres, Quitter.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject m_mainPanel;
    public GameObject m_slotPanel;
    public GameObject m_settingsPanel;

    void Start()
    {
        ShowMain();
    }

    public void OnPlayClicked()
    {
        m_mainPanel.SetActive(false);
        m_slotPanel.SetActive(true);
    }

    public void OnSettingsClicked()
    {
        m_mainPanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowMain()
    {
        m_mainPanel.SetActive(true);
        m_slotPanel.SetActive(false);
        m_settingsPanel.SetActive(false);
    }
}
```

**Setup manuel de la scène MenuScene :**

- [ ] Dans MenuScene, créer un Canvas (Screen Space — Overlay)
- [ ] Créer les panels enfants dans le Canvas :
  - `MainPanel` (3 boutons : "Jouer", "Paramètres", "Quitter")
  - `SlotPanel` (désactivé par défaut) — voir Task 11
  - `SettingsPanel` (désactivé par défaut) — voir Task 12
- [ ] Ajouter `MainMenuUI` sur le Canvas
- [ ] Lier les refs dans l'Inspector
- [ ] Lier les boutons aux méthodes `OnPlayClicked`, `OnSettingsClicked`, `OnQuitClicked`

- [ ] **Aussi créer un GameObject `Managers` dans MenuScene** avec les mêmes composants que MainScene (`GameManager`, `SaveManager`, `InputManager`, `TransitionManager`, `AudioManager`)

- [ ] **Commit**
```bash
git add Assets/_Scenes/MenuScene.unity Assets/Scripts/UI/MainMenuUI.cs
git commit -m "feat: create MenuScene with main menu UI"
```

---

## Task 11 : SaveSlotUI

**Files:**
- Create: `Assets/Scripts/UI/SaveSlotUI.cs`

- [ ] **Créer SaveSlotUI.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche les 3 slots de sauvegarde et gère la sélection.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("Slots (3 boutons)")]
    public Button[]        m_slotButtons;   // 3 boutons
    public TextMeshProUGUI[] m_slotLabels;  // texte de chaque bouton

    [Header("Bouton Retour")]
    public Button m_backButton;

    private MainMenuUI m_mainMenu;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();
        RefreshSlots();

        for (int i = 0; i < m_slotButtons.Length; i++)
        {
            int slot = i; // capture pour la lambda
            m_slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu.ShowMain());
    }

    private void RefreshSlots()
    {
        if (SaveManager.Instance == null) return;
        SaveData[] saves = SaveManager.Instance.LoadAll();

        for (int i = 0; i < saves.Length && i < m_slotLabels.Length; i++)
        {
            if (saves[i].isEmpty)
            {
                m_slotLabels[i].text = $"Slot {i + 1} — VIDE";
            }
            else
            {
                int minutes  = Mathf.FloorToInt(saves[i].playTime / 60f);
                int seconds  = Mathf.FloorToInt(saves[i].playTime % 60f);
                string boss  = saves[i].bossDefeated ? "Boss: Oui" : "Boss: Non";
                m_slotLabels[i].text = $"Slot {i + 1} — {minutes:00}:{seconds:00}  {boss}";
            }
        }
    }

    private void SelectSlot(int slot)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.StartGame(slot);
    }
}
```

**Setup manuel dans MenuScene :**

- [ ] Dans `SlotPanel`, créer 3 boutons (Slot1, Slot2, Slot3) + 1 bouton Retour
- [ ] Ajouter `SaveSlotUI` sur `SlotPanel`
- [ ] Lier les arrays `m_slotButtons` et `m_slotLabels` dans l'Inspector

- [ ] **Commit**
```bash
git add Assets/Scripts/UI/SaveSlotUI.cs
git commit -m "feat: add SaveSlotUI showing slot info (time, boss status)"
```

---

## Task 12 : SettingsUI

**Files:**
- Create: `Assets/Scripts/UI/SettingsUI.cs`

- [ ] **Créer SettingsUI.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 3 onglets : Touches / Manette / Son.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Onglets")]
    public GameObject m_touchesPanel;
    public GameObject m_manettPanel;
    public GameObject m_sonPanel;

    [Header("Son — Sliders")]
    public Slider m_musicSlider;
    public Slider m_soundSlider;

    [Header("Touches — Boutons de rebinding")]
    public Button[] m_rebindButtons;       // un par action (Move n'est pas rebindable ici)
    public TextMeshProUGUI[] m_bindLabels; // affiche la touche actuelle

    // Ordre des actions correspondant aux boutons
    private readonly string[] ACTION_NAMES = { "Attack", "Inventory", "Run", "Hide", "Map", "Hotbar1", "Hotbar2" };
    // Pour le clavier, binding index 0 ; pour la manette index varie selon la config

    [Header("Bouton Retour")]
    public Button m_backButton;

    private MainMenuUI m_mainMenu;
    private bool m_isRebinding = false;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();

        // Son
        if (m_musicSlider != null)
        {
            m_musicSlider.value = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 0.8f;
            m_musicSlider.onValueChanged.AddListener(v => AudioManager.instance?.SetMusicVolume(v));
        }
        if (m_soundSlider != null)
        {
            m_soundSlider.value = AudioManager.instance != null ? AudioManager.instance.GetSoundVolume() : 1f;
            m_soundSlider.onValueChanged.AddListener(v => AudioManager.instance?.SetSoundVolume(v));
        }

        // Touches
        RefreshBindLabels();
        for (int i = 0; i < m_rebindButtons.Length && i < ACTION_NAMES.Length; i++)
        {
            int idx = i;
            m_rebindButtons[i].onClick.AddListener(() => StartRebind(idx));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu.ShowMain());

        ShowTouches();
    }

    public void ShowTouches()
    {
        m_touchesPanel.SetActive(true);
        m_manettPanel.SetActive(false);
        m_sonPanel.SetActive(false);
    }

    public void ShowManette()
    {
        m_touchesPanel.SetActive(false);
        m_manettPanel.SetActive(true);
        m_sonPanel.SetActive(false);
    }

    public void ShowSon()
    {
        m_touchesPanel.SetActive(false);
        m_manettPanel.SetActive(false);
        m_sonPanel.SetActive(true);
    }

    private void StartRebind(int actionIndex)
    {
        if (m_isRebinding || InputManager.Instance == null) return;
        m_isRebinding = true;

        string actionName = ACTION_NAMES[actionIndex];
        m_bindLabels[actionIndex].text = "Appuie sur une touche...";

        InputManager.Instance.StartRebinding(actionName, 0, () =>
        {
            m_isRebinding = false;
            RefreshBindLabels();
        });
    }

    private void RefreshBindLabels()
    {
        if (InputManager.Instance == null) return;
        for (int i = 0; i < m_bindLabels.Length && i < ACTION_NAMES.Length; i++)
        {
            m_bindLabels[i].text = InputManager.Instance.GetBindingDisplayString(ACTION_NAMES[i], 0);
        }
    }
}
```

**Setup manuel dans MenuScene :**

- [ ] Dans `SettingsPanel`, créer 3 sous-panels : `TouchesPanel`, `ManettePanel`, `SonPanel`
- [ ] 3 boutons onglets qui appellent `ShowTouches()`, `ShowManette()`, `ShowSon()`
- [ ] Dans `SonPanel` : 2 Sliders (Musique, Effets) avec labels
- [ ] Dans `TouchesPanel` : 7 boutons de rebinding + labels (Attack, Inventory, Run, Hide, Map, Hotbar1, Hotbar2)
- [ ] Dans `ManettePanel` : texte descriptif des boutons manette (informatif, stick gauche = mouvement, etc.)
- [ ] Ajouter `SettingsUI` sur `SettingsPanel`, lier toutes les refs

- [ ] **Commit**
```bash
git add Assets/Scripts/UI/SettingsUI.cs
git commit -m "feat: add SettingsUI with 3 tabs (touches, manette, son) and rebinding"
```

---

## Task 13 : Fond animé du menu (RenderTexture)

**Étapes manuelles dans Unity Editor :**

- [ ] Dans `Assets/RenderTextures/`, créer une RenderTexture : clic droit → Create → Render Texture → nommer `MenuBackground`, résolution 1920×1080
- [ ] Dans `MenuScene`, créer un `GameObject` nommé `BackgroundCamera`
- [ ] Ajouter `Camera` sur ce GO, configurer :
  - `Output Texture` = `MenuBackground`
  - `Culling Mask` = layers à filmer (ex : Default)
  - `Depth` = -2 (derrière la caméra principale)
  - Position la caméra sur une zone du décor (ex: une zone tranquille de MainScene — ou créer une mini-zone décor dans MenuScene)
- [ ] Dans le Canvas de MenuScene, créer un `RawImage` en fond (premier enfant du Canvas)
  - `Texture` = `MenuBackground`
  - Rect = plein écran (anchor stretch)
  - `Color` = blanc avec alpha 180 (légèrement transparent pour voir derrière)

> **Alternative simple :** créer une petite zone de décor directement dans MenuScene avec quelques tuiles animées que la BackgroundCamera filme. Pas besoin d'utiliser MainScene.

- [ ] **Commit**
```bash
git add Assets/_Scenes/MenuScene.unity Assets/RenderTextures/
git commit -m "feat: add animated background to menu via RenderTexture"
```

---

## Task 14 : Zones de Save automatique dans MainScene

**Files:**
- Create: `Assets/Scripts/AutoSaveTrigger.cs`

- [ ] **Créer AutoSaveTrigger.cs**

```csharp
using UnityEngine;

/// <summary>
/// Placer ce script sur un trigger collider dans MainScene.
/// Déclenche une sauvegarde automatique quand le joueur entre dans la zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AutoSaveTrigger : MonoBehaviour
{
    private bool m_triggered = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_triggered) return;
        if (!other.CompareTag("Player")) return;

        m_triggered = true; // ne sauvegarde qu'une fois par zone

        if (GameManager.Instance != null)
            GameManager.Instance.QuickSave();

        Debug.Log("[AutoSave] Sauvegarde automatique déclenchée.");
    }
}
```

**Setup manuel dans MainScene :**

- [ ] Créer 2-3 GameObjects vides avec `BoxCollider2D` + `AutoSaveTrigger` aux points stratégiques du niveau (couloirs, avant le boss)
- [ ] Tagger ces objets en layer "Ignore Raycast" pour éviter les collisions physiques

- [ ] **Commit**
```bash
git add Assets/Scripts/AutoSaveTrigger.cs Assets/_Scenes/MainScene.unity
git commit -m "feat: add AutoSaveTrigger for automatic saves at zone entries"
```

---

## Task 15 : Build Settings + Tests finaux

**Build Settings :**

- [ ] File → Build Settings
- [ ] Vérifier l'ordre des scènes :
  - Index 0 : `MenuScene`
  - Index 1 : `MainScene`

**Tests manuels (Play Mode) :**

- [ ] **Test 1 — Menu** : Lancer `MenuScene` → les 3 boutons apparaissent, le fond animé est visible
- [ ] **Test 2 — Sélection slot** : Cliquer Jouer → les 3 slots s'affichent (VIDE au premier lancement)
- [ ] **Test 3 — Transition entrée** : Sélectionner un slot → l'animation "Connexion en cours..." joue → MainScene se charge
- [ ] **Test 4 — Inputs** : Dans MainScene, le joueur se déplace (ZQSD), tire (Espace), ouvre l'inventaire (E)
- [ ] **Test 5 — Manette** : Brancher une manette → le joueur se déplace au stick gauche, tire au bouton A/Cross
- [ ] **Test 6 — Paramètres** : Retourner au menu → Paramètres → modifier un slider son → volume change
- [ ] **Test 7 — Rebinding** : Paramètres → Touches → cliquer Attack → appuyer une touche → le label se met à jour
- [ ] **Test 8 — Save** : Traverser une zone AutoSave → tuer le boss → vérifier que `save_slot_X.json` existe dans `Application.persistentDataPath`
- [ ] **Test 9 — Transition sortie** : Tuer le boss → animation "Déconnexion..." joue → retour au menu → le slot affiche le bon temps de jeu
- [ ] **Test 10 — Reload save** : Relancer le jeu → Jouer → sélectionner le slot sauvegardé → le joueur démarre à la bonne position avec les bons HP

- [ ] **Commit final**
```bash
git add -A
git commit -m "feat: complete game infrastructure (menu, save, settings, transitions)"
```
