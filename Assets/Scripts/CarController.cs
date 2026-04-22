using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private float inputAcceleration;
    [SerializeField] private float inputDirection;
    [SerializeField] private float inputBreak;
    [SerializeField] private CarConfigurationSO carConfigurationSO;

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

    private HealthSystem healthSystem;
    private GasSystem gasSystem;
    private Rigidbody rb;
    public static event Action<float, Transform> onPlayerCrashed;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        gasSystem = GetComponent<GasSystem>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        inputAcceleration = Input.GetAxis("Vertical") * carConfigurationSO.MotorForce;
        inputDirection = Input.GetAxis("Horizontal") * carConfigurationSO.DirectionForce;
        inputBreak = Input.GetAxisRaw("Break") * carConfigurationSO.BreakForce;
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (thirdPersonCamera.gameObject.activeSelf)
            {
                thirdPersonCamera.gameObject.SetActive(false);
                firstPersonCamera.gameObject.SetActive(true);
            }
            if (firstPersonCamera.gameObject.activeSelf)
            {
                thirdPersonCamera.gameObject.SetActive(true);
                firstPersonCamera.gameObject.SetActive(false);
            }
        }
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
