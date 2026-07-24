// BU DOSYA DERLENMEZ — PlayerController.cs'e uygulanacak patch gösterimi.
// Mevcut HandleMovement() içindeki hız hesabına SpeedMultiplier eklenir.
//
// ── Değişiklik 1: alan + referans (class'ın başına) ──────────────────────
//
//     private PlayerControllerIntegration integration;
//
//     private void Awake()   // varsa mevcut Awake'e ekle
//     {
//         integration = GetComponent<PlayerControllerIntegration>();
//     }
//
// ── Değişiklik 2: HandleMovement() içinde hız hesabı ────────────────────
//
//   ÖNCE:
//     float speed = isSprinting ? swimSpeed * sprintMultiplier : swimSpeed;
//
//   SONRA:
//     float speed = isSprinting ? swimSpeed * sprintMultiplier : swimSpeed;
//     if (integration != null)
//         speed *= integration.SpeedMultiplier;
//
//   Aynı şekilde dikey hareket varsa:
//     float vSpeed = verticalSpeed;
//     if (integration != null)
//         vSpeed *= integration.SpeedMultiplier;
//
//   Ardından mevcut rb.AddForce(...) çağrıları bu speed/vSpeed ile devam eder.
//
// NOT: null-check bilinçli bırakıldı — Integration component'i sahnede
// yoksa oyun eskisi gibi çalışmaya devam eder (geri uyumluluk).
