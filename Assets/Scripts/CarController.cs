using System;
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
    [SerializeField] private Transform car;

    [Header("Cameras: ")]
    [SerializeField] private Camera thirdPersonCamera;
    [SerializeField] private Camera firstPersonCamera;

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

    float yaw;
    float pitch;

    private HealthSystem healthSystem;
    private GasSystem gasSystem;
    private Rigidbody rb;
    public static event Action<float, Transform> onPlayerCrashed;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        gasSystem = GetComponent<GasSystem>();
        rb = GetComponent<Rigidbody>();
        GasStation.onGasStationEntered += GasStation_onGasStationEntered;
        Workshop.onWorkshopEntered += Workshop_onWorkshopEntered;
    }

    void Update()
    {
        inputAcceleration = Input.GetAxis("Vertical") * carConfigurationSO.MotorForce;
        inputDirection = Input.GetAxis("Horizontal") * carConfigurationSO.DirectionForce;
        inputBreak = Input.GetAxisRaw("Break") * carConfigurationSO.BreakForce;

        yaw += Input.GetAxis("Mouse X") * carConfigurationSO.MouseSens;
        pitch -= Input.GetAxis("Mouse Y") * carConfigurationSO.MouseSens;

        pitch = Mathf.Clamp(pitch, -20f, 60f);

        SwitchPerspective();
        CameraRotate();

        if (inputAcceleration != 0)
        {
            gasSystem.ConsumeGas(carConfigurationSO.GasConsumedBySecond * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (inputAcceleration > 200)

        //Aceleración
        frontRight.motorTorque = inputAcceleration;
        frontLeft.motorTorque = inputAcceleration;
        backRight.motorTorque = inputAcceleration;
        backLeft.motorTorque = inputAcceleration;

        //Frenado
        frontRight.brakeTorque = inputBreak;
        frontLeft.brakeTorque = inputBreak;
        backRight.brakeTorque = inputBreak;
        backLeft.brakeTorque = inputBreak;

        //Dirección
        frontRight.steerAngle = inputDirection;
        frontLeft.steerAngle = inputDirection;

        //Sincronización visual
        SyncWheel(frontRight, visualFrontRight);
        SyncWheel(frontLeft, visualFrontLeft);
        SyncWheel(backRight, visualBackRight);
        SyncWheel(backLeft, visualBackLeft);
    }

    private void OnDestroy()
    {
        GasStation.onGasStationEntered -= GasStation_onGasStationEntered;
        Workshop.onWorkshopEntered -= Workshop_onWorkshopEntered;
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
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 targetPosition = car.transform.position;

        Vector3 position = targetPosition - rotation * Vector3.forward * carConfigurationSO.ThirdPersonCameraDistance;

        thirdPersonCamera.transform.position = position;
        thirdPersonCamera.transform.LookAt(targetPosition);

        // First person camera
        yaw += Input.GetAxis("Mouse X") * carConfigurationSO.MouseSens;
        pitch -= Input.GetAxis("Mouse Y") * carConfigurationSO.MouseSens;

        pitch = Mathf.Clamp(pitch, -30f, 60f);

        yaw = Mathf.Clamp(yaw, -90f, 90f);

        firstPersonCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 angle = new Vector3(carConfigurationSO.MouseSens * (Input.GetAxis("Mouse Y") * -1), carConfigurationSO.MouseSens * Input.GetAxis("Mouse X"));
        firstPersonCamera.transform.Rotate(angle);
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
