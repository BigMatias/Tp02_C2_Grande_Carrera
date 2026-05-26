using System;
using System.Collections;
using UnityEngine;

// Suggestion: Alta - Esta clase es una "God Class" que viola el principio de Responsabilidad Única (SRP).
public class CarController : MonoBehaviour
{
    [Header("References")]
    // Warning: Media - Estas variables son estado interno (no input del usuario en el Inspector); porque [SerializeField]. Nunca más.
    [SerializeField] private float inputAcceleration;
    [SerializeField] private float inputDirection;
    [SerializeField] private float inputBreak;
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    [SerializeField] private TurretDataSO turretDataSO;
    [SerializeField] private Transform car;
    [SerializeField] private Transform centerOfMass;

    [Header("Turret: ")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform turretBase;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private LayerMask hitLayers;

    [Header("Cameras: ")]
    [SerializeField] private Camera thirdPersonCamera;
    [SerializeField] private Camera firstPersonCamera;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 45f;

    [Header("Wheels: ")]
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider backRight;
    [SerializeField] private WheelCollider backLeft;

    [Header("Visual: ")]
    [SerializeField] private Transform visualFrontRight;
    [SerializeField] private Transform visualFrontLeft;
    [SerializeField] private Transform visualBackRight;
    [SerializeField] private Transform visualBackLeft;

    [Header("HealthSystem")]
    [SerializeField] private Transform healthPoint;

    [Header("Secondary Fire (Bomb)")]
    [SerializeField] private float m2Cooldown = 20f;
    [SerializeField] private float timeToHit = 1.5f;
    [SerializeField] private KeyCode toggleTrajectoryKey = KeyCode.T;

    [Header("Trajectory Cheat")]
    [SerializeField] private LineRenderer trajectoryLine; 
    [SerializeField] private int trajectoryResolution = 30; 

    private float m2CooldownTimer = 0f;
    private bool showTrajectory = true;
    private float yawThird;
    private float pitchThird;
    private float yawFirst;
    private float pitchFirst;
    private Camera activeCam;
    private Vector3 targetPoint;

    private HealthSystemV2 healthSystemV2;
    private GasSystem gasSystem;
    private Rigidbody rb;

    public event Action<float, Transform> onPlayerCrashed;
    public event Action onPlayerHurt;
    public event Action onPlayerDied;
    public event Action onPlayerShootM1;
    public event Action onPlayerShootM2;

    private void Awake()
    {
        // Bug: Alta - No se valida que GetComponent<HealthSystemV2>() devuelva no-null. Usar Require Component
        healthSystemV2 = GetComponent<HealthSystemV2>();
        gasSystem = GetComponent<GasSystem>();
        rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }

        healthSystemV2.onDie += HealthSystemV2_onDie;
    }

    void Update()
    {
        inputAcceleration = Input.GetAxis("Vertical") * carConfigurationSO.MotorForce;
        inputDirection = Input.GetAxis("Horizontal") * carConfigurationSO.DirectionForce;
        inputBreak = Input.GetAxisRaw("Break") * carConfigurationSO.BreakForce;
        // Warning: Media - activeCam se recalcula cada frame leyendo gameObject.activeSelf. Mejor cachearlo en SwitchPerspective() cuando realmente cambia la cámara.
        activeCam = firstPersonCamera.gameObject.activeSelf ? firstPersonCamera : thirdPersonCamera;

        SwitchPerspective();
        CameraRotate();

        Shoot();
        RotateTurret();

        HandleBombM2();
        HandleTrajectoryCheat();

        if (inputAcceleration != 0)
        {
            gasSystem.ConsumeGas(carConfigurationSO.GasConsumedBySecond * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Aceleración 
        backRight.motorTorque = inputAcceleration;
        backLeft.motorTorque = inputAcceleration;

        // Freno 
        frontRight.brakeTorque = inputBreak;
        frontLeft.brakeTorque = inputBreak;
        backRight.brakeTorque = inputBreak;
        backLeft.brakeTorque = inputBreak;

        // Dirección 
        frontRight.steerAngle = inputDirection;
        frontLeft.steerAngle = inputDirection;

        // Sincronización visual
        SyncWheel(frontRight, visualFrontRight);
        SyncWheel(frontLeft, visualFrontLeft);
        SyncWheel(backRight, visualBackRight);
        SyncWheel(backLeft, visualBackLeft);
    }

    private void OnDestroy()
    {
        healthSystemV2.onDie -= HealthSystemV2_onDie;
    }

    private void HealthSystemV2_onDie()
    {
        onPlayerDied?.Invoke();
    }

    private void SyncWheel(WheelCollider wheel, Transform visual)
    {
        wheel.GetWorldPose(out var pos, out var rot);
        visual.rotation = rot;
        visual.position = pos;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == (int)Layers.Obstacles)
        {
            CrashedWithObstacle(other.relativeVelocity.magnitude);
        }
        else
        {
            IDamageable hs = other.gameObject.GetComponent<IDamageable>();
            if (hs != null)
            {
                hs.TakeDamage(carConfigurationSO.EnemyCollideDamage);
            }
        }

    }

    // Suggestion: Baja - La tecla V está hardcodeada. 
    private void SwitchPerspective()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (thirdPersonCamera.gameObject.activeSelf)
            {
                thirdPersonCamera.gameObject.SetActive(false);
                firstPersonCamera.gameObject.SetActive(true);
            }
            else if (firstPersonCamera.gameObject.activeSelf)
            {
                thirdPersonCamera.gameObject.SetActive(true);
                firstPersonCamera.gameObject.SetActive(false);
            }
        }
    }

    private void CameraRotate()
    {
        float mouseX = Input.GetAxis("Mouse X") * carConfigurationSO.MouseSens;
        float mouseY = Input.GetAxis("Mouse Y") * carConfigurationSO.MouseSens;

        yawThird += mouseX;
        pitchThird -= mouseY;
        pitchThird = Mathf.Clamp(pitchThird, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(pitchThird, yawThird, 0);

        Vector3 targetPosition = turret.transform.position;
        Vector3 position = targetPosition - rotation * Vector3.forward * carConfigurationSO.ThirdPersonCameraDistance;

        thirdPersonCamera.transform.position = position;
        thirdPersonCamera.transform.LookAt(targetPosition);

        yawFirst += mouseX;
        pitchFirst -= mouseY;

        pitchFirst = Mathf.Clamp(pitchFirst, -30f, 60f);
        yawFirst = Mathf.Clamp(yawFirst, -90f, 90f);

        firstPersonCamera.transform.localRotation = Quaternion.Euler(pitchFirst, yawFirst, 0);
    }

    // Bug: Media - Si ambas cámaras están desactivadas al iniciar la escena, activeCam queda null
    private void RotateTurret()
    {
        Ray ray = new Ray(shootPoint.position, activeCam.transform.forward);
        RaycastHit hit;

        // Warning: Baja - El rango 100f está hardcodeado. Debería leerse de TurretDataSO 
        if (Physics.Raycast(ray, out hit, 100f))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * 100f;

        Vector3 flatDirection = targetPoint - turretBase.position;
        flatDirection.y = 0;

        if (flatDirection != Vector3.zero)
        {
            Quaternion baseRot = Quaternion.LookRotation(flatDirection) * Quaternion.Euler(0, 90f, 0);

            turretBase.rotation = Quaternion.Lerp(
                turretBase.rotation,
                baseRot,
                Time.deltaTime * rotationSpeed
            );
        }

        Vector3 localTarget = turretBase.InverseTransformPoint(targetPoint);

        float pitch = Mathf.Atan2(localTarget.y, localTarget.x) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion pitchRot = Quaternion.Euler(0f, 0f, -pitch + 180f);

        turret.localRotation = Quaternion.Slerp(
         turret.localRotation,
        pitchRot,
        Time.deltaTime * rotationSpeed
        );
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(activeCam.transform.position, activeCam.transform.forward);
            RaycastHit hit;
            Vector3 endPoint;

            laserLine.SetPosition(0, shootPoint.position);

            if (Physics.Raycast(ray, out hit, turretDataSO.M1ShootRange, hitLayers))
            {
                // Warning: Baja - Debug.Log dentro del flujo de gameplay. 
                Debug.Log("Impacto en: " + hit.collider.name);
                endPoint = hit.point;

                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null)
                    target.TakeDamage(turretDataSO.M1Damage);
            }
            else
            {
                endPoint = ray.origin + ray.direction * turretDataSO.M1ShootRange;
            }

            laserLine.SetPosition(1, endPoint);
            onPlayerShootM1?.Invoke();
            StartCoroutine(ShootEffectSequence());
        }
    }

    private void HandleBombM2()
    {
        if (m2CooldownTimer > 0)
        {
            m2CooldownTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1) && m2CooldownTimer <= 0)
        {
            m2CooldownTimer = m2Cooldown;

            BombProjectile bomb = PoolManager.Instance.Get<BombProjectile>();

            if (bomb != null)
            {
                bomb.transform.position = shootPoint.position;
                bomb.transform.rotation = shootPoint.rotation;

                Vector3 launchVelocity = CalculateLaunchVelocity(shootPoint.position, targetPoint, timeToHit);

                bomb.Launch(launchVelocity);

                onPlayerShootM2?.Invoke();
            }
        }
    }

    private void HandleTrajectoryCheat()
    {
        if (Input.GetKeyDown(toggleTrajectoryKey))
        {
            showTrajectory = !showTrajectory;
            trajectoryLine.enabled = showTrajectory;
        }

        if (!showTrajectory || m2CooldownTimer > 0)
        {
            if (trajectoryLine.enabled) trajectoryLine.enabled = false;
            return;
        }

        if (!trajectoryLine.enabled) trajectoryLine.enabled = true;
        DrawTrajectory();
    }


    private Vector3 CalculateLaunchVelocity(Vector3 origin, Vector3 target, float flightTime)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float sY = distance.y;
        float sXZ = distanceXZ.magnitude;
        float Vxz = sXZ / flightTime;

        float Vy = (sY / flightTime) + (0.5f * Mathf.Abs(Physics.gravity.y) * flightTime);

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    private void DrawTrajectory()
    {
        Vector3 velocity = CalculateLaunchVelocity(shootPoint.position, targetPoint, timeToHit);
        trajectoryLine.positionCount = trajectoryResolution;

        float timeStep = timeToHit / (trajectoryResolution - 1);

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i * timeStep;

            Vector3 posXZ = shootPoint.position + new Vector3(velocity.x, 0, velocity.z) * t;
            float posY = shootPoint.position.y + (velocity.y * t) - (0.5f * Mathf.Abs(Physics.gravity.y) * t * t);

            Vector3 pointPosition = new Vector3(posXZ.x, posY, posXZ.z);
            trajectoryLine.SetPosition(i, pointPosition);
        }
    }
    private IEnumerator ShootEffectSequence()
    {
        laserLine.enabled = true;
        yield return new WaitForSeconds(turretDataSO.LaserDuration);
        laserLine.enabled = false;
    }

    private void CrashedWithObstacle(float impactSpeed)
    {

        if (impactSpeed < 2f) return;
        if (impactSpeed <= carConfigurationSO.FirstSpeedThreshold)
        {
            onPlayerCrashed?.Invoke(carConfigurationSO.CrashDamage1, healthPoint);
            healthSystemV2.TakeDamage(carConfigurationSO.CrashDamage1);
            return;
        }
        else if (impactSpeed <= carConfigurationSO.SecondSpeedThreshold)
        {
            onPlayerCrashed?.Invoke(carConfigurationSO.CrashDamage2, healthPoint);
            healthSystemV2.TakeDamage(carConfigurationSO.CrashDamage2);
            return;
        }
        else
        {
            onPlayerCrashed?.Invoke(carConfigurationSO.CrashDamage3, healthPoint);
            healthSystemV2.TakeDamage(carConfigurationSO.CrashDamage3);
        }
    }

    public float CurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
    }
}
