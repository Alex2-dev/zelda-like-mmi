using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton (par scène) qui gère les messages style Dark Souls :
///   - Au démarrage : récupère les messages du serveur et les instancie dans le monde
///   - Quand le joueur appuie sur N : ouvre l'UI de rédaction
///   - À l'envoi : POST vers le serveur, puis spawn local immédiat
/// </summary>
public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    private const string INSERT_URL = "https://dev.alex2-server.fr/zeldalike-msg/insertMessage.php";
    private const string GET_URL    = "https://dev.alex2-server.fr/zeldalike-msg/getMessage.php";

    [Header("Préfabriqué")]
    [Tooltip("Préfab du message posé dans le monde (MessageObject + SpriteRenderer + CircleCollider2D)")]
    public GameObject m_messagePrefab;

    [Header("Interface")]
    public MessageUI m_messageUI;

    // ── Cycle de vie ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(FetchMessages());
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>Ouvre le panneau de rédaction à la position donnée (position du joueur).</summary>
    public void OpenComposeUI(Vector3 playerPos)
    {
        if (m_messageUI != null)
            m_messageUI.OpenCompose(playerPos);
    }

    /// <summary>Affiche le panneau de lecture avec le texte d'un message existant.</summary>
    public void ShowReadUI(string text)
    {
        if (m_messageUI != null)
            m_messageUI.OpenRead(text);
    }

    /// <summary>Envoie un message au serveur, puis le place dans le monde.</summary>
    public void SendMessage(string text, Vector3 pos)
    {
        StartCoroutine(PostMessage(text, pos));
    }

    // ── Réseau ────────────────────────────────────────────────────────────────

    private IEnumerator FetchMessages()
    {
        string scene = SceneManager.GetActiveScene().name;
        string url   = GET_URL + "?world=" + UnityWebRequest.EscapeURL(scene);

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[MessageManager] GET échoué : " + req.error);
                yield break;
            }

            string raw = req.downloadHandler.text;
            Debug.Log("[MessageManager] Réponse serveur : " + raw);

            if (string.IsNullOrWhiteSpace(raw)) yield break;

            // Format reçu : "content,x,y,z,world;content,x,y,z,world;"
            string currentScene = scene;
            string[] entries = raw.Split(';');
            foreach (string entry in entries)
            {
                string trimmed = entry.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Les 4 derniers champs sont x,y,z,world — le reste est le content
                // On split depuis la fin pour gérer les virgules dans le contenu
                string[] parts = trimmed.Split(',');
                if (parts.Length < 5) continue;

                string world   = parts[parts.Length - 1];
                if (world != currentScene) continue;   // filtre par scène côté client

                float z = 0f, y = 0f, x = 0f;
                float.TryParse(parts[parts.Length - 2], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out z);
                float.TryParse(parts[parts.Length - 3], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out y);
                float.TryParse(parts[parts.Length - 4], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out x);

                // Tout ce qui précède x,y,z,world est le contenu
                string content = string.Join(",", parts, 0, parts.Length - 4);

                SpawnMessageObject(new MessageData { message = content, x = x, y = y, z = z, scene = world });
            }
        }
    }

    private IEnumerator PostMessage(string text, Vector3 pos)
    {
        string scene = SceneManager.GetActiveScene().name;
        var ic = System.Globalization.CultureInfo.InvariantCulture;

        string body = "content=" + UnityWebRequest.EscapeURL(text)
                    + "&x="     + pos.x.ToString(ic)
                    + "&y="     + pos.y.ToString(ic)
                    + "&z="     + pos.z.ToString(ic)
                    + "&world=" + UnityWebRequest.EscapeURL(scene);

        Debug.Log("[MessageManager] POST body : " + body);

        var req = new UnityWebRequest(INSERT_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("[MessageManager] POST échoué : " + req.error);
            yield break;
        }

        Debug.Log("[MessageManager] POST réponse : " + req.downloadHandler.text);

        // Spawn local dès confirmation serveur
        SpawnMessageObject(new MessageData { message = text, x = pos.x, y = pos.y, z = pos.z, scene = scene });
    }

    // ── Instanciation ─────────────────────────────────────────────────────────

    private void SpawnMessageObject(MessageData data)
    {
        if (m_messagePrefab == null)
        {
            Debug.LogWarning("[MessageManager] m_messagePrefab non assigné !");
            return;
        }

        var go = Instantiate(m_messagePrefab, new Vector3(data.x, data.y, data.z), Quaternion.identity, transform);
        go.GetComponent<MessageObject>()?.Init(data.message);
    }

    // ── Modèles de données ────────────────────────────────────────────────────

    [System.Serializable]
    public class MessageData
    {
        public int    id;
        public string message;
        public float  x;
        public float  y;
        public float  z;
        public string scene;
    }

    [System.Serializable]
    private class MessageListWrapper
    {
        public MessageData[] items;
    }
}
