using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paramètres spécifiques à un niveau.
///
/// SETUP :
/// - Créer un GameObject vide dans la scène (ex: "LevelSettings")
/// - Attacher ce script dessus
/// - Cocher "m_permanentIntegrity" pour que le joueur soit en intégrité maximale
///   pendant tout ce niveau
/// </summary>
public class LevelSettings : MonoBehaviour
{
    [Header("Intégrité permanente")]
    [Tooltip("Si coché, le joueur sera en intégrité maximale pendant tout ce niveau (barre pleine, effets actifs en permanence).")]
    public bool m_permanentIntegrity = false;
}
