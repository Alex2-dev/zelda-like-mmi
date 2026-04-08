using System.Collections;
using UnityEngine;

/// <summary>
/// Gère la séquence musicale du boss :
/// 1. Joueur entre dans la zone → musique d'intro
/// 2. Intro terminée → musique de combat + boss spawn
/// 3. Boss mort → musique normale
///
/// SETUP :
/// - Ajouter sur le même GameObject que la SpawnZone du boss
/// - Assigner les 3 clips et le prefab boss dans l'Inspector
/// - NE PAS mettre de boss dans la scène — il sera spawné automatiquement
/// </summary>
[RequireComponent(typeof(SpawnZone))]
public class BossMusicTrigger : MonoBehaviour
{
    [Header("Musiques")]
    [SerializeField] AudioClip m_introMusic;
    [SerializeField] AudioClip m_fightMusic;
    [SerializeField] AudioClip m_normalMusic;

    [Header("Boss")]
    [SerializeField] GameObject m_bossPrefab;
    [Tooltip("Position de spawn du boss — laisse vide pour spawner au centre de la zone")]
    [SerializeField] Transform m_bossSpawnPoint;

    private bool m_triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_triggered) return;
        if (!other.CompareTag("Player")) return;

        m_triggered = true;
        StartCoroutine(BossSequence());
    }

    private IEnumerator BossSequence()
    {
        // 1. Joue l'intro
        AudioManager.instance?.PlayMusic(m_introMusic, loop: false);

        // Attend la fin de l'intro
        float introDuration = m_introMusic != null ? m_introMusic.length : 0f;
        yield return new WaitForSeconds(introDuration);

        // 2. Lance la musique de combat
        AudioManager.instance?.PlayMusic(m_fightMusic, loop: true);

        // 3. Spawn le boss
        if (m_bossPrefab != null)
        {
            Vector3 spawnPos = m_bossSpawnPoint != null
                ? m_bossSpawnPoint.position
                : transform.position;

            GameObject boss = Instantiate(m_bossPrefab, spawnPos, Quaternion.identity);

            // Passe la référence de zone au boss
            BossBehavior behavior = boss.GetComponent<BossBehavior>();
            if (behavior != null)
                behavior.m_bossZone = GetComponent<SpawnZone>();
        }
    }

    /// <summary>Appelée par BossBehavior à la mort du boss.</summary>
    public void OnBossDefeated()
    {
        AudioManager.instance?.PlayMusic(m_normalMusic, loop: true);
    }
}
