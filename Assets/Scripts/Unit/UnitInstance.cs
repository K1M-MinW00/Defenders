using System;
using UnityEngine;

public class UnitInstance : MonoBehaviour, IDamageable
{
	public UnitDataSO Data { get; private set; }
	public int Star { get; private set; } = 1;

	public UnitStats Stats { get; private set; }
	public float Hp { get; private set; }
	public float Mp { get; private set; }

	[SerializeField] private float energyPerSec = 10f;

	public bool IsAlive => Hp > 0f;
	public bool IsEnergyFull => Mp >= Stats.maxMp;

	public event Action<UnitInstance> OnStarChanged;
	public event Action<UnitInstance> OnHpChanged;
	public event Action<UnitInstance> OnEnergyChanged;

	public void Initialize(UnitDataSO data, int star = 1)
	{
		Data = data;
		Star = star;

		RecalculateStats();
		ResetForPrepare();
		OnStarChanged?.Invoke(this);
	}

    private void Update()
    {
		if (!CanRegenEnergy() || IsEnergyFull)
			return;

        RegenMp();
    }

    private void RegenMp()
    {
		float before = Mp;
		Mp = Mathf.Clamp(Mp + energyPerSec * Time.deltaTime, 0f, Stats.maxMp);

		if(!Mathf.Approximately(before,Mp))
			OnEnergyChanged?.Invoke(this);

		if(IsEnergyFull)
		{
			Debug.Log($"Energy Full : {Data.DisplayName} (Star {Star})");
			//TODO 스킬사용 후 에너지 리셋
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
		if (damage <= 0f || !IsAlive) 
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
		Mp = 0f;
		OnEnergyChanged?.Invoke(this);
	}

	public void ResetForPrepare()
	{
		HealToFull();
		ResetEnergy();
	}

	private void RecalculateStats()
	{
		if (Data == null)
			return;

		Stats = Data.BaseStats;
		// TODO : 합성 스탯 업그레이드

	}
}
