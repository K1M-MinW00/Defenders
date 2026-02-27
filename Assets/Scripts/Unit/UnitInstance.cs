using System;
using UnityEngine;

public class UnitInstance : MonoBehaviour, IDamageable
{
	public UnitData Data { get; private set; }
	public int Star { get; private set; } = 1;

	public UnitStats Stats { get; private set; }
	public float Hp { get; private set; }
	public float Energy { get; private set; }

	[SerializeField] private float energyPerSec = 10f;

	public bool IsAlive => Hp > 0f;
	public bool IsEnergyFull => Energy >= Stats.maxEnergy;
	public event Action<UnitInstance> OnStarChanged;
	public event Action<UnitInstance> OnHpChanged;
	public event Action<UnitInstance> OnEnergyChanged;

	public void Initialize(UnitData data, int star = 1)
	{
		Data = data;
		Star = star;

		RecalculateStats();
		ResetForPrepare();
	}

    private void Update()
    {
		if (!CanRegenEnergy() || IsEnergyFull)
			return;

        AddEnergy();
    }

    private void AddEnergy()
    {
		float before = Energy;
		Energy = Mathf.Clamp(Energy + energyPerSec * Time.deltaTime, 0f, Stats.maxEnergy);

		if(!Mathf.Approximately(before,Energy))
			OnEnergyChanged?.Invoke(this);

		if(IsEnergyFull)
		{
			Debug.Log($"Energy Full : {Data.displayName} (Star {Star})");
			Invoke("ResetEnergy",2f);
		}
    }

    private bool CanRegenEnergy()
    {
		return IsAlive && StageManager.Instance.CurrentState == StageState.Combat;
    }

    public void ApplyStarUp()
	{
		Star++;
		RecalculateStats();

		ResetForPrepare();
		OnStarChanged?.Invoke(this);
	}

	public void TakeDamage(float damage)
	{
		if (damage <= 0f) 
			return;

		if (!IsAlive)
			return;

		Hp = Mathf.Max(0f, Hp - damage);
		OnHpChanged?.Invoke(this);
	}

	public void HealToFull()
	{
		Hp = Stats.maxHp;
		OnHpChanged?.Invoke(this);
	}

	public void ResetEnergy()
	{
		Energy = 0f;
		OnEnergyChanged?.Invoke(this);
	}

	public void ResetForPrepare()
	{
		HealToFull();
		ResetEnergy();
	}

	private void RecalculateStats()
	{
		Stats = Data.GetStats(Star);
	}
}
