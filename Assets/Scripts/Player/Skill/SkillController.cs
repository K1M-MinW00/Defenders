//using UnityEngine;

//public class UnitSkillController : MonoBehaviour
//{
//    private PlayerCharacter owner;
//    private UnitRuntime runtime;

//    private IActiveSkill activeSkill;
//    private IPassiveSkill passiveSkill;

//    public IActiveSkill ActiveSkill => activeSkill;
//    public IPassiveSkill PassiveSkill => passiveSkill;

//    public void Initialize(PlayerCharacter owner, UnitRuntime runtime)
//    {
//        this.owner = owner;
//        this.runtime = runtime;

//        DisposeSkills();

//        UnitDataSO unitData = runtime.Data;
//        if (unitData == null)
//            return;

//        if (unitData.ActiveSkill != null)
//        {
//            activeSkill = unitData.ActiveSkill.CreateRuntimeSkill();
//            // activeSkill.Initialize(owner, unitData.ActiveSkill, unitData.);
//        }

//    }

//    public bool CanUseActiveSkill()
//    {
//        if (activeSkill == null)
//            return false;

//        if (!runtime.IsEnergyFull)
//            return false;

//        return activeSkill.CanUseSkill();
//    }

//    public void BeginActiveSkill()
//    {
//        activeSkill?.BeginSkill();
//    }

//    public void EndActiveSkill()
//    {
//        activeSkill?.EndSkill();
//    }

//    public void CancelActiveSkill()
//    {
//        activeSkill?.CancelSkill();
//    }

//    public void DisposeSkills()
//    {
//        activeSkill?.CancelSkill();
//        passiveSkill?.Dispose();

//        activeSkill = null;
//        passiveSkill = null;
//    }

//    private void OnDestroy()
//    {
//        DisposeSkills();
//    }
//}