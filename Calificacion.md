# Calificación TP02 — Grande Carrera

---

## Calidad Técnica Transversal

### Bugs críticos.
- No se puede rejugar.
- **God Class**: `CarController` mezcla movimiento, cámaras, torreta, M1, M2, trayectoria, daño y gas (≈390 líneas, 7 responsabilidades).
- **NREs por falta de null-check** en `GetComponent` y `Singleton.Instance` en: `CarController.Awake`, `GameManager.Start`, `AudioManager.Start`, `UIGameHUD.Start`, `UIGameResult.Start`, `CheckpointFinish.Start`, `GameModeBootstrapper.Awake`, `FsmManager.Awake`, `CarController.HandleBombM2`, `FsmManager.ThrowWrench`.
- **Inconsistencia de nombre de escena**: `UIGameOver` carga `"MainMenu"`, todos los demás scripts cargan `"MainMenuScene"`. Bug de navegación.
- **Modificación de lista durante iteración** en `GasStation.RechargingGas` y `Workshop.RepairingCars`.
- **`Bullet.cs`** entero comentado, código muerto.
- **Sincronización de eventos `CarSpawner.OnCarSpawned`**: se dispara en `Start`, pero los suscriptores que se enganchan en `Start` pueden perdérselo (orden de ejecución no garantizado).

### Performance / GC.
- **`material.color`** en `Checkpoint.SetVisualState` y `CheckpointFinish.SetFinishLineColor`: instancia un material en cada cambio.
- **`Physics.OverlapSphere`** en `BombProjectile.Explode` aloca array por explosión — usar `OverlapSphereNonAlloc`.
- **`GameObject.Find("StartPosition")`** llamado 2 veces por respawn en `CheckpointSystem.GetStartPosition/Rotation`.
- **`AudioManager.Update`** consulta `isPlaying` cada frame — preferir corutina basada en `clip.length`.
- **`new Vector3(...)`** dentro de `Update` de `UIDamageText` — alloc por frame.
- **`Debug.Log` en hot paths** (HealthSystemV2.TakeDamage, Vehicle.TakeDamage, Checkpoint.SetVisualState, GameHUD.Start ×4).

### Diseño / Arquitectura.
- **Abuso de Singletons**: `PoolManager`, `CompetitionManager`, `EndlessModeManager`, `CompetitionScoreSystem`, `CheckpointSystem`. Acoplamiento global, difícil de testear.
- **`CompetitionManager` vs `EndlessModeManager`**: ~80% código duplicado. Falta clase base `RaceModeManager`.
- **`HealthSystem` vs `HealthSystemV2`**: clase duplicada; la V1 no se usa. Refactor inconcluso.
- **`GameModeManager.cs` define la clase `GameModeBootstrapper`** — viola la convención fuerte de Unity (no se puede agregar al GameObject desde Inspector).
- **`UIGameHUD.cs` define la clase `GameHUD`** — mismo problema.
- **Nomenclatura**: `IPooleable` (debería ser `IPoolable`), `enemyKillledSfx` (typo: tres L), método `Enemy()` ambiguo.
- **`UIGameOver`** declara `gameOverPanel` y `gameManager` pero **nunca los usa**.

---

## Calificación Final: **7**

### Justificación corta
El proyecto **funciona** y demuestra dominio sólido de las consignas centrales (físicas, cámaras, FSM, Object Pool propio, 2 modos de juego, sistema de score con eventos, ScriptableObjects para configuración). La arquitectura del **Pool** y del **GameEventSO** está bien por encima del promedio del nivel "Aprobado".

- **Faltan velocímetro.
- **POO incompleta**: la clase abstracta `Vehicle` está sin usar — los NPCs y el Player no comparten jerarquía.
- **Audio incompleto**: 3 volúmenes mal mapeados respecto a la consigna (Master/Sfx/Music ≠ Background/VFX/UI).
- **Bugs latentes serios**: NREs por falta de null-checks en múltiples managers, modificación de lista en coroutines (`GasStation`/`Workshop`), inconsistencia `"MainMenu"` vs `"MainMenuScene"`, texto de daño que no se resetea, `Vehicle.life` sin inicializar.
- **God Class `CarController`** y duplicación `CompetitionManager`/`EndlessModeManager` indican deuda técnica de diseño.

### A mejorar
- Agregar velocímetro real consumiendo `CarController.CurrentSpeed()`.
- Convertir `Vehicle` en `abstract class` y hacer que `FsmManager`/`CarController` heredan de ella.
- Borrar `HealthSystem` (V1) y `Bullet.cs` (vacío), renombrar archivos para que coincidan con sus clases (`UIGameHUD`/`GameModeManager`).
- Separar volúmenes Background/VFX/UI en `UIOptions` y `AudioManager`.
- Player como **objeto vacío** + `CarConfigurationSO` que instancie en runtime su prefab visual y registre el comportamiento.
