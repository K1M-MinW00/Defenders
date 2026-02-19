using System;
using UnityEngine;

public class UnitInstance : MonoBehaviour
{
	public UnitData Data { get; private set; }
	public int Star { get; private set; } = 1;

	public UnitStats CurrentStats { get; private set; }
	public float CurrentHp { get; private set; }
	public float CurrentEnergy { get; private set; }

	public event Action<UnitInstance> OnDataChanged;
	public event Action<UnitInstance> OnStarChanged;
	public event Action<UnitInstance> OnHpChanged;
	public event Action<UnitInstance> OnEnergyChanged;


	public void Initialize(UnitData data, int star = 1)
	{
		SetUnitData(data, star);
		CurrentHp = CurrentStats.maxHp;
		OnHpChanged?.Invoke(this);
	}

	public void SetUnitData(UnitData newData, int star = 1)
	{
		if (newData == null)
		{
			Debug.LogError("SetUnitData failed: newData is null.");
			return;
		}

		Data = newData;
		Star = Mathf.Max(1, star);

		RecalculateStats();

		CurrentHp = CurrentStats.maxHp;

		OnDataChanged?.Invoke(this);
		OnStarChanged?.Invoke(this);
		OnHpChanged?.Invoke(this);
	}

	public void ApplyStarUp(int newStar)
	{
		if (Data == null)
		{
			Debug.LogError("ApplyStarUp failed: Data is null.");
			return;
		}

		newStar = Mathf.Max(Star, newStar);
		
		Star = newStar;
		RecalculateStats();

		CurrentHp = CurrentStats.maxHp;

		OnStarChanged?.Invoke(this);
		OnHpChanged?.Invoke(this);
	}

	public void TakeDamage(float dmg)
	{
		if (dmg <= 0f) 
			return;
		CurrentHp = Mathf.Max(0f, CurrentHp - dmg);
		OnHpChanged?.Invoke(this);
	}

	private void RecalculateStats()
	{
		CurrentStats = Data.GetStats(Star);
	}
}
