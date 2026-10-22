public interface IIncomingDamageModifier
{
    int Priority { get; }
    float ModifyIncomingDamage(DamageRequest request, float damage);
}
