using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.GraphicsBuffer;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private float inputAcceleration;
    [SerializeField] private float inputDirection;
    [SerializeField] private float inputBreak;
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    [SerializeField] private TurretDataSO turretDataSO;
    [SerializeField] private Transform car;
    [SerializeField] private Workshop workshop;
    [SerializeField] private GasStation gasStation;

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

    private float yawThird;
    private float pitchThird;
    private float yawFirst;
    private float pitchFirst;
    private Camera activeCam;
    private Vector3 targetPoint;

    private HealthSystem healthSystem;
    private GasSystem gasSystem;
    private Rigidbody rb;

    public static event Action<float, Transform> onPlayerCrashed;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        gasSystem = GetComponent<GasSystem>();
        rb = GetComponent<Rigidbody>();
        gasStation.onGasStationEntered += GasStation_onGasStationEntered;
        workshop.onWorkshopEntered += Workshop_onWorkshopEntered;
    }

    void Update()
    {
        inputAcceleration = Input.GetAxis("Vertical") * carConfigurationSO.MotorForce;
        inputDirection = Input.GetAxis("Horizontal") * carConfigurationSO.DirectionForce;
        inputBreak = Input.GetAxisRaw("Break") * carConfigurationSO.BreakForce;
        activeCam = firstPersonCamera.gameObject.activeSelf ? firstPersonCamera : thirdPersonCamera;

        SwitchPerspective();
        CameraRotate();
        Shoot();
        RotateTurret();

        if (inputAcceleration != 0)
        {
            gasSystem.ConsumeGas(carConfigurationSO.GasConsumedBySecond * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (inputAcceleration > 200)

        //Aceleraci�n
        frontRight.motorTorque = inputAcceleration;
        frontLeft.motorTorque = inputAcceleration;
        backRight.motorTorque = inputAcceleration;
        backLeft.motorTorque = inputAcceleration;

        //Frenado
        frontRight.brakeTorque = inputBreak;
        frontLeft.brakeTorque = inputBreak;
        backRight.brakeTorque = inputBreak;
        backLeft.brakeTorque = inputBreak;

        //Direcci�n
        frontRight.steerAngle = inputDirection;
        frontLeft.steerAngle = inputDirection;

        //Sincronizaci�n visual
        SyncWheel(frontRight, visualFrontRight);
        SyncWheel(frontLeft, visualFrontLeft);
        SyncWheel(backRight, visualBackRight);
        SyncWheel(backLeft, visualBackLeft);
    }

    private void OnDestroy()
    {
        gasStation.onGasStationEntered -= GasStation_onGasStationEntered;
        workshop.onWorkshopEntered -= Workshop_onWorkshopEntered;
    }

    private void GasStation_onGasStationEntered(float gasRecovered)
    {
        gasSystem.RecoverGas(gasRecovered);
    }

    private void Workshop_onWorkshopEntered(float healthRecovered)
    {
        healthSystem.Heal(healthRecovered);
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
    }

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

    private void RotateTurret()
    {
        Ray ray = new Ray(shootPoint.position, activeCam.transform.forward);
        RaycastHit hit;

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
            Vector3 direction = (targetPoint - shootPoint.position).normalized;

            Ray ray = new Ray(shootPoint.position, direction);

            RaycastHit hit;
            Vector3 endPoint;

            laserLine.SetPosition(0, shootPoint.position);

            if (Physics.Raycast(ray, out hit, turretDataSO.M1ShootRange, hitLayers))
            {
                Debug.Log("Impacto en: " + hit.collider.name);
                endPoint = hit.point;

                HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
                if (targetHealth != null)
                {
                    targetHealth.DoDamage(turretDataSO.M1Damage);
                }
            }
            else
            {
                endPoint = shootPoint.position + direction * turretDataSO.M1ShootRange;
            }

            laserLine.SetPosition(1, endPoint);
            StartCoroutine(ShootEffectSequence());
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
            healthSystem.DoDamage(carConfigurationSO.CrashDamage1);
            return;
        }
        else if (impactSpeed <= carConfigurationSO.SecondSpeedThreshold)
        {
            onPlayerCrashed?.Invoke(carConfigurationSO.CrashDamage2, healthPoint);
            healthSystem.DoDamage(carConfigurationSO.CrashDamage2);
            return;
        }
        else
        {
            onPlayerCrashed?.Invoke(carConfigurationSO.CrashDamage3, healthPoint);
            healthSystem.DoDamage(carConfigurationSO.CrashDamage3);
        }
    }

    public float CurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
    }
}
