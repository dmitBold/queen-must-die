using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{   
    public enum Stats { 
        WealthStat, 
        SnakeStat,
        WaspStat,
        SpiderStat,
        PeopleStat,
        OrderStat
    }

    public HashSet<string> flags = new HashSet<string>();

    [SerializeField] int Wealth = 50;
    [SerializeField] int Snakes = 50;
    [SerializeField] int Wasps = 50;
    [SerializeField] int Spiders = 50;
    [SerializeField] int Order = 50;
    [SerializeField] int People = 50;


    /*public bool OpenVault_Wealth = false;
    public bool OpenVault_Faith = false;
    public bool OpenVault_Order = false;
    public bool OpenVault_People = false;*/

    public int CriticaltWealth = 0;
    public int CriticalSnakes = 0;
    public int CriticalSpiders = 0;
    public int CriticalWasps = 0;
    public int CriticalOrder = 0;
    public int CriticalPeople = 0;

    public bool IsWealthCritical = false;
    public bool IsSnakesCritical = false;
    public bool IsSpidersCritical = false;
    public bool IsWaspsCritical = false;
    public bool IsOrderCritical = false;
    public bool IsPeopleCritical = false;

    public int WeeksPassed = 0;
    //public int VaultOpenCount = 0;

    public event Action<Stats> OnStatChanged;
    public event Action<Stats> OnStatBecameCritical;

    HashSet<Stats> locked_stats = new();
    public event Action<Stats> OnStatLocked;

    public void LockStat(Stats stat)
    {
        if(locked_stats.Contains(stat)) return;

        locked_stats.Add(stat);
        SetStatToZero(stat);
        OnStatLocked?.Invoke(stat);
    }


    void SetStatToZero(Stats stat)
    {
        switch (stat)
        {
            case Stats.WealthStat: Wealth = 0; break;
            case Stats.SnakeStat: Snakes = 0; break;
            case Stats.OrderStat: Order = 0; break;
            case Stats.PeopleStat: People = 0; break;
            case Stats.SpiderStat: Spiders = 0; break;
            case Stats.WaspStat: Wasps = 0; break;
        }

        OnStatChanged?.Invoke(stat);
    }

    public bool IsStatLocked(Stats stat)
    {
        return locked_stats.Contains(stat);
    }


    public void ApplyToStat(int value, Stats stat)
    {
        if(locked_stats.Contains((Stats)stat)) return;
        switch (stat)
        {
            case Stats.WealthStat:
                Wealth += value;
                Wealth = Mathf.Clamp(Wealth, 0, 100);
                break;
            case Stats.SnakeStat:
                Snakes += value;
                Snakes = Mathf.Clamp(Snakes, 0, 100);
                break;
            case Stats.SpiderStat:
                Spiders += value;
                Spiders = Mathf.Clamp(Spiders, 0, 100);
                break;
            case Stats.WaspStat:
                Wasps += value;
                Wasps = Mathf.Clamp(Wasps, 0, 100);
                break;
            case Stats.OrderStat:
                Order += value;
                Order = Mathf.Clamp(Order, 0, 100);
                break;
            case Stats.PeopleStat:
                People += value;
                People = Mathf.Clamp(People, 0, 100);
                break;
        }
        OnStatChanged?.Invoke(stat);
        CheckCriticalState();

    }

    void CheckCriticalState()
    {
        if (Wealth == 0)
        {
            if (!IsWealthCritical)
            {
                CriticaltWealth += 1;
                IsWealthCritical = true;
                if (CriticaltWealth >= 2)
                    LockStat(Stats.WealthStat);
                OnStatBecameCritical?.Invoke(Stats.WealthStat);
            }
        }
        else { IsWealthCritical = false; }
        if (Snakes == 0)
        {
            if (!IsSnakesCritical)
            {
                CriticalSnakes += 1;
                IsSnakesCritical = true;
                if (CriticalSnakes >= 2)
                    LockStat(Stats.SnakeStat);
                OnStatBecameCritical?.Invoke(Stats.SnakeStat);
            }
        }
        else { IsSnakesCritical = false; }
        if (Spiders == 0)
        {
            if (!IsSpidersCritical)
            {
                CriticalSpiders += 1;
                IsSpidersCritical = true;
                if (CriticalSpiders >= 2)
                    LockStat(Stats.SpiderStat);
                OnStatBecameCritical?.Invoke(Stats.SpiderStat);
            }
        }
        else { IsSpidersCritical = false; }
        if (Wasps == 0)
        {
            if (!IsWaspsCritical)
            {
                CriticalWasps += 1;
                IsWaspsCritical = true;
                if (CriticalWasps >= 2)
                    LockStat(Stats.WaspStat);
                OnStatBecameCritical?.Invoke(Stats.WaspStat);
            }
        }
        else { IsWaspsCritical = false; }
        if (Order == 0)
        {
            if (!IsOrderCritical)
            {
                CriticalOrder += 1;
                IsOrderCritical = true;
                if (CriticalOrder >= 2)
                    LockStat(Stats.OrderStat);
                OnStatBecameCritical?.Invoke(Stats.OrderStat);
            }
        }
        else { IsOrderCritical = false; }
        if (People == 0)
        {
            if (!IsPeopleCritical)
            {
                CriticalPeople += 1;
                IsPeopleCritical = true;
                if (CriticalPeople >= 2)
                    LockStat(Stats.PeopleStat);
                OnStatBecameCritical?.Invoke(Stats.PeopleStat);
            }
        }
        else { IsPeopleCritical = false; }

    }

    public int GetStatValue(Stats stat)
    {
        switch (stat)
        {
            case Stats.WealthStat:
                return Wealth;
            case Stats.SnakeStat:
                return Snakes;
            case Stats.SpiderStat:
                return Spiders;
            case Stats.WaspStat:
                return Wasps;
            case Stats.OrderStat:
                return Order;
            case Stats.PeopleStat:
                return People;
            default:
                return -1;
        }
    }
    
    public void AddFlag(string flag)
    {
        flags.Add(flag);
    }

    public void RemoveFlag(string flag) 
    { 
        flags.Remove(flag); 
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public void ApplyEyePenalty()
    {
        //Stats eye_stat = (Stats)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Stats)).Length);

        List<Stats> availableStats = new List<Stats>();

        foreach (Stats stat in System.Enum.GetValues(typeof(Stats)))
        {
            if (GetStatValue(stat) > 0 && !IsStatLocked(stat))
            {
                availableStats.Add(stat);
            }
        }

        if (availableStats.Count == 0)
            return; // или GameOver / ничего не делать

        Stats eye_stat = availableStats[UnityEngine.Random.Range(0, availableStats.Count)];

        //Stats eye_stat = eye_stats[UnityEngine.Random.Range(0, eye_stats.Count)];
        //if (stat == null)
        //  return;

        //Stats stat = availableStats[UnityEngine.Random.Range(0, availableStats.Count)];

        ApplyToStat(-30, eye_stat);
    }
}