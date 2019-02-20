// 游戏状态
public enum GameStatus
{
    // 游戏初始化
    Init = 0,
    // 装弹
    Reloading,
    // 瞄准
    Aiming,
    // 发射
    Launching,
}

// 砖块类型
public enum BlockType
{
    Normal = 0,
    Expansion,
    Explode,
    Frozen,
}
