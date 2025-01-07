using UnityEngine;

public enum EnemyType { Common, Medium, Elite, Boss }

[System.Serializable]
public class Enemy
{
    public string name;
    public EnemyType type;
    public int health;
    public int attack;
    public int defense;
    public int defenseScalingFactor;

    public Enemy(string name, EnemyType type, int health, int attack, int defense, int defenseScalingFactor)
    {
        this.name = name;
        this.type = type;
        this.health = health;
        this.attack = attack;
        this.defense = defense;
        this.defenseScalingFactor = defenseScalingFactor;
    }
}
