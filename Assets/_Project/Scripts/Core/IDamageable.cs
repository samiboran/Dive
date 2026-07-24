/// <summary>
/// Hasar alabilen tüm varlıkların (oyuncu, köpekbalığı, objeler)
/// implemente etmesi gereken ortak arayüz.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
