using System.Collections.Generic;
using UnityEngine;
using Library;

[CreateAssetMenu(menuName = "GameSettings")]
public class Settings : ScriptableObject
{
    public static int DotSpawnCooldown { get => Instance.dotSpawnCooldown; }
    public static float MaxReachableDistanceBetweenDots { get => Instance.maxReachableDistanceBetweenDots; }
    public static float DotSpinSpeed { get => Instance.dotSpinSpeed; }
    public static int UnitMoveUpdatesCount { get => Instance.unitMoveUpdatesCount; }
    public static int UnitMoveCountKf { get => Instance.unitMoveCountKf; }
    public static float TurnLength { get => Instance.turnLength; }
    public static float MultiPlayerDelayTime { get => Instance.multiPlayerDelayTime; }

    [SerializeField]
    private Color playerColor;
    [SerializeField]
    private Color enemyColor;
    [SerializeField]
    private List<Icon> icons;
    [SerializeField]
    private int dotSpawnCooldown = 6;
    [SerializeField]
    private float maxReachableDistanceBetweenDots = 270f;
    [SerializeField]
    private float dotSpinSpeed = 300f;
    [SerializeField]
    private int unitMoveUpdatesCount = 30;
    [SerializeField]
    private int unitMoveCountKf = 4;
    [SerializeField]
    private float turnLength = 30f;
    [SerializeField]
    private float multiPlayerDelayTime = 1f;

    private static Settings _instance;

    private static Settings Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<Settings>("Options");

            return _instance;
        }
    }

    public static Color GetColor(Player player)
    {
        return player switch
        {
            Player.Green => Instance.playerColor,
            Player.Red => Instance.enemyColor,
            _ => Color.white,
        };
    }

    public static Sprite GetIcon(Type type)
    {
        foreach (Icon icon in Instance.icons)
            if (icon.type == type)
                return icon.sprite;

        return null;
    }
}
