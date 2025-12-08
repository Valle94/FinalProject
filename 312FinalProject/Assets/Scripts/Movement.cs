using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] LayerMask driveable;

    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;

    private int[] wheelsIsGrounded = new int[4];
    [HideInInspector] public bool isGrounded = false;
    private Vector2 movement;
    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio;
    private Vector3 startPos;
    private Quaternion startRotation;

    void Awake()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
    }

    void Start()
    {
        carRB = GetComponent<Rigidbody>();
        carRB.isKinematic = true;
    }

    void Update()
    {
        //Debug.Log(movement.x);
        //Debug.Log(movement.y);
    }

    void FixedUpdate()
    {
        if (!RaceManager.Instance.raceStarted)
        return;

        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Move();
    }

    public void OnMove(InputValue value)
    {
        if (RaceManager.Instance.raceStarted)
        {
            movement = value.Get<Vector2>();    
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    public void ResetPlayer()
    {
        transform.position = startPos;
        transform.rotation = startRotation;

        carRB.linearVelocity = Vector3.zero;
        carRB.angularVelocity = Vector3.zero;
    }

    public void StartPlayer()
    {
        carRB.isKinematic = false;
    }

    public void StopPlayer()
    {
        // Freeze physics so the car doesn't move or fall during menus
        carRB.linearVelocity = Vector3.zero;
        carRB.angularVelocity = Vector3.zero;
        carRB.isKinematic = true;
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    private void Move()
    {
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }

    private void Acceleration()
    {
        carRB.AddForceAtPosition(acceleration * movement.y * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        carRB.AddForceAtPosition(deceleration * movement.y * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        carRB.AddTorque(steerStrength * movement.x * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void Suspension()

    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;
            bool rayDidHit = Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, driveable);

            if (!rayDidHit)
            {
                wheelsIsGrounded[i] = 0;
                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
                continue;
            }

            wheelsIsGrounded[i] = 1;

            Vector3 springDir = rayPoints[i].up;   // direction of the spring

            // Effective suspension length (pivot to wheel contact)
            float currentLength = hit.distance - wheelRadius;

            // Extension relative to rest
            float displacement = restLength - currentLength;

            // Velocity along the spring direction
            float vel = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), springDir);

            // Hooke’s law + damping
            float force = (springStiffness * displacement) - (damperStiffness * vel);

            // Clamp so suspension doesn't "pull" downward
            if (force < 0) force = 0;

            carRB.AddForceAtPosition(springDir * force, rayPoints[i].position);

            Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
        }
    }

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

}
