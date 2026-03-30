/* Author : Raphaël Marczak - 2018/2020, for MIAMI Teaching (IUT Tarbes) and MMI Teaching (IUT Bordeaux Montaigne)
 * 
 * This work is licensed under the CC0 License. 
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents the cardinal directions (South, North, West, East)
public enum CardinalDirections { CARDINAL_S, CARDINAL_N, CARDINAL_W, CARDINAL_E };
public enum HideType { NONE, CLOSET, WATER, BUSH }


public class PlayerBehavior : MonoBehaviour
{
    [Header("Interaction")]
    public Collider2D m_hideInteractionCollider;   // le collider du joueur qui sert pour les cachettes

    public float m_speed = 1f; // Speed of the player when he moves
    private CardinalDirections m_direction; // Current facing direction of the player

    public Sprite m_frontSprite = null;
    public Sprite m_leftSprite = null;
    public Sprite m_rightSprite = null;
    public Sprite m_backSprite = null;

    public GameObject m_fireBall = null; // Object the player can shoot

    public GameObject m_map = null;
    public DialogManager m_dialogDisplayer;

    [Tooltip("Référence vers le script PlayerStats (sur ce même GameObject)")]
    public PlayerStats m_playerStats;

    [Tooltip("Référence vers le script Inventory (sur ce même GameObject)")]
    public Inventory m_inventory;

    [Tooltip("Référence vers AlterEgoManager (sur ce même GameObject)")]
    public AlterEgoManager m_alterEgoManager;

    [Tooltip("Référence vers le script WeaponManager (sur ce même GameObject)")]
    public WeaponManager m_weaponManager;

    private Dialog m_closestNPCDialog;

        // --- AJOUT POUR LA CACHE ---
    private bool m_isHidden = false;              // indique si le joueur est caché ou non
    private HideType m_currentHideType = HideType.NONE;  // type de cachette actuelle
    private Collider2D m_currentHideZone = null;  // référence au trigger de cachette dans lequel on se trouve

    Rigidbody2D m_rb2D;
    SpriteRenderer m_renderer;
    Animator m_animator;

    private bool m_isMoving = false;

    void Awake()
    {
        m_rb2D = gameObject.GetComponent<Rigidbody2D>();
        m_renderer = gameObject.GetComponent<SpriteRenderer>();
        m_animator = gameObject.GetComponent<Animator>();

        m_closestNPCDialog = null;
    }

    // This update is called at a very precise and constant FPS, and
    // must be used for physics modification
    // (i.e. anything related with a RigidBody)
    void FixedUpdate()
    {
        // If a dialog is on screen, the player should not be updated
        // If the map is displayed, the player should not be updated
        if (m_dialogDisplayer.IsOnScreen() || m_map.activeSelf)
        {
            return;
        }

        if (m_isHidden)
        {
            m_rb2D.velocity = Vector2.zero;   // sécurité : annule les vitesses résiduelles
            return;
        }
        if (m_alterEgoManager != null && m_alterEgoManager.IsDashing)
            return;
        // Moves the player regarding the inputs
        Move();
    }

    private void Move()
    {
        float horizontalOffset = Input.GetAxis("Horizontal");
        float verticalOffset = Input.GetAxis("Vertical");

        // Course avec Shift uniquement quand l'intégrité est au maximum
        float currentSpeed = m_speed;
        if (m_playerStats != null
            && m_playerStats.IsAtFullIntegrity()
            && Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= m_playerStats.m_runMultiplier;
        }

        // Translates the player to a new position, at a given speed.
        Vector2 newPos = new Vector2(transform.position.x + horizontalOffset * currentSpeed,
                                     transform.position.y + verticalOffset * currentSpeed);
        m_rb2D.MovePosition(newPos);

        // Computes the player main direction (North, Sound, East, West)
        if (Mathf.Abs(horizontalOffset) > Mathf.Abs(verticalOffset))
        {
            if (horizontalOffset > 0)
            {
                m_direction = CardinalDirections.CARDINAL_E;
            }
            else
            {
                m_direction = CardinalDirections.CARDINAL_W;
            }
        }
        else if (Mathf.Abs(horizontalOffset) < Mathf.Abs(verticalOffset))
        {
            if (verticalOffset > 0)
            {
                m_direction = CardinalDirections.CARDINAL_N;
            }
            else
            {
                m_direction = CardinalDirections.CARDINAL_S;
            }
        }

        m_isMoving = (Mathf.Abs(horizontalOffset) > 0.01f || Mathf.Abs(verticalOffset) > 0.01f);

        if (m_weaponManager != null)
            m_weaponManager.SetDirection(GetShootDirectionVector());
    }

    public Vector2 GetShootDirectionVector()
    {
        switch (m_direction)
        {
            case CardinalDirections.CARDINAL_N: return Vector2.up;
            case CardinalDirections.CARDINAL_S: return Vector2.down;
            case CardinalDirections.CARDINAL_E: return Vector2.right;
            case CardinalDirections.CARDINAL_W: return Vector2.left;
            default: return Vector2.up;
        }
    }


    // This update is called at the FPS which can be fluctuating
    // and should be called for any regular actions not based on
    // physics (i.e. everything not related to RigidBody)
    private void Update()
    {

        // If the player presses M, the map will be activated if not on screen
        // or desactivated if already on screen
        if (Input.GetKeyDown(KeyCode.M))
        {
            m_map.SetActive(!m_map.activeSelf);
        }

        // Inventaire : & (Alpha1) = slot 1, é (Alpha2) = slot 2, molette = cycle
        if (m_inventory != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                m_inventory.UseHotbarSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                m_inventory.UseHotbarSlot(1);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
                m_inventory.ScrollHotbar(scroll);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        bool isBlocked = m_dialogDisplayer.IsOnScreen() || m_map.activeSelf;
        if (m_weaponManager != null)
            m_weaponManager.m_canFire = !isBlocked;

        // If a dialog is on screen, the player should not be updated
        // If the map is displayed, the player should not be updated
        if (isBlocked)
        {
            return;
        }

        if (m_alterEgoManager == null || !m_alterEgoManager.IsAlterEgo)
            ChangeSpriteToMatchDirection();

        // If the player presses SPACE, then two solution
        // - If there is a dialog ready to be displayed (i.e. the player is closed to a NPC)
        //   then a dialog is set to the dialogManager
        // - If not, then the player will shoot a fireball
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_closestNPCDialog != null)
            {
                m_dialogDisplayer.SetDialog(m_closestNPCDialog.GetDialog());
            }
            else if (m_weaponManager != null && m_weaponManager.HasWeapon() && !m_weaponManager.IsAutomatic())
            {
                m_weaponManager.TryShoot(GetShootDirectionVector());
            }
            else if (m_weaponManager == null || !m_weaponManager.HasWeapon())
            {
                ShootFireball();
            }
            // si automatique : géré par WeaponManager.Update()
        }
            // Si le joueur appuie sur F :
        if (Input.GetKeyDown(KeyCode.F))
        {
            // En alter ego, F est géré par AlterEgoManager (dash)
            if (m_alterEgoManager != null && m_alterEgoManager.IsAlterEgo)
                return;
            if (m_isHidden)
            {
                // s'il est déjà caché, on le fait sortir
                UnhidePlayer();
            }
            else
            {
                // s'il ne l'est pas, on essaie de se cacher dans la zone actuelle
                TryHide();
            }
        }
    }

    // Changes the player animation regarding its direction
    // Uses Animator if available (parameters: "Direction" int + "IsMoving" bool),
    // otherwise falls back to direct sprite swap.
    private void ChangeSpriteToMatchDirection()
    {
        if (m_animator != null)
        {
            // Direction: 0=S, 1=N, 2=W, 3=E  (matches CardinalDirections enum order)
            m_animator.SetInteger("Direction", (int)m_direction);
            m_animator.SetBool("IsMoving", m_isMoving);
        }
        else
        {
            // Fallback: swap sprite directly (no animation)
            if (m_direction == CardinalDirections.CARDINAL_N)
                m_renderer.sprite = m_backSprite;
            else if (m_direction == CardinalDirections.CARDINAL_S)
                m_renderer.sprite = m_frontSprite;
            else if (m_direction == CardinalDirections.CARDINAL_E)
                m_renderer.sprite = m_rightSprite;
            else if (m_direction == CardinalDirections.CARDINAL_W)
                m_renderer.sprite = m_leftSprite;
        }
    }

    // Creates a fireball, and launches it
    private void ShootFireball()
    {
        GameObject newFireball = Instantiate(m_fireBall, this.transform) as GameObject;

        FireBehavior fireBallBehavior = newFireball.GetComponent<FireBehavior>();

        if (fireBallBehavior != null)
        {
            // Lauches the fireball upward
            // (Vector2 represents a direction in x and y ;
            // so Vector2(0f, 1f) is a direction of 0 in x and 1 in y (up)
            fireBallBehavior.Launch(new Vector2(0f, 1f));
        }
    }


    // This is automatically called by Unity when the gameObject (here the player)
    // enters a trigger zone. Here, two solutions
    // - the player is in an NPC zone, then he grabs the dialog information ready to be
    //   displayed when SPACE will be pressed
    // - the player is in an instantDialog zone, then he grabs the dialog information and
    //   displays it instantaneously
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "NPC")
        {
            m_closestNPCDialog = collision.GetComponent<Dialog>();
        }
        else if (collision.tag == "InstantDialog")
        {
            Dialog instantDialog = collision.GetComponent<Dialog>();
            if (instantDialog != null)
            {
                m_dialogDisplayer.SetDialog(instantDialog.GetDialog());
            }
        }
        // else if (collision.CompareTag("Hide_Closet"))
        // {
        //     m_currentHideZone = collision;
        //     m_currentHideType = HideType.CLOSET;
        // }
        // else if (collision.CompareTag("Hide_Water"))
        // {
        //     m_currentHideZone = collision;
        //     m_currentHideType = HideType.WATER;
        // }
        else if (collision.CompareTag("Hide_Bush"))
        {
            m_currentHideZone = collision;
            m_currentHideType = HideType.BUSH;
        }
    }


    // This is automatically called by Unity when the gameObject (here the player)
    // leaves a trigger zone. Here, two solutions
    // - the player was in an NPC zone, then the dialog information is removed
    // - the player was in an instantDialog zone, then the instantDialog is destroyed
    //   (as it has been displayed, and must only be displayed once)
private void OnTriggerExit2D(Collider2D collision)
{
    if (collision.tag == "NPC")
    {
        m_closestNPCDialog = null;
    }
    else if (collision.tag == "InstantDialog")
    {
        Destroy(collision.gameObject);
    }
    // --- AJOUT POUR LA CACHE ---
    else if (collision == m_currentHideZone)
    {
        // On quitte la zone de cachette actuelle
        m_currentHideZone = null;
        m_currentHideType = HideType.NONE;

        // Si on sort en étant caché, on force le Unhide
        if (m_isHidden)
        {
            UnhidePlayer();
        }
    }
    // --- FIN AJOUT ---
}

    private void TryHide()
    {
        // Si le joueur n'est pas dans une zone de cachette, on ne fait rien
        if (m_currentHideZone == null || m_currentHideType == HideType.NONE)
            return;

        // Ici tu peux personnaliser selon le type de cachette
        switch (m_currentHideType)
        {
            case HideType.CLOSET:
                // Exemple : dans un placard, tu peux centrer le joueur sur la position du placard
                // transform.position = m_currentHideZone.transform.position;
                break;

            case HideType.WATER:
                // Exemple : dans l'eau : certains effets, changement de sprite, etc.
                break;

            case HideType.BUSH:
                // Exemple : dans un buisson : juste alpha + layer différente
                break;
        }

        // On marque le joueur comme "caché"
        m_isHidden = true;

        // On arrête tout mouvement
        m_rb2D.velocity = Vector2.zero;

        // Optionnel : changer de layer pour que les ennemis ignorent le joueur
        // gameObject.layer = LayerMask.NameToLayer("HiddenPlayer");

        // Optionnel : changer l'alpha pour voir visuellement qu'il est "fondu" dans le décor
        Color c = m_renderer.color;
        c.a = 0.3f;   // 30% opaque
        m_renderer.color = c;
    }

    private void UnhidePlayer()
    {
        // On remet l'état normal
        m_isHidden = false;

        // Remet la layer normale
        // gameObject.layer = LayerMask.NameToLayer("Player");

        // Rétablit l'alpha du sprite
        Color c = m_renderer.color;
        c.a = 1f;
        m_renderer.color = c;
    }

}
