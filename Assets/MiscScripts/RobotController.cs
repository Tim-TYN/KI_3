using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    private readonly float maxPower = 100f;
    private readonly float MaxVelocity = 0.5f;
    private readonly float StandardTorque = 20f;
    private readonly float timeOut = 10f;
    private float currentTorque;
    private SensorSuite sensor;
    private Rigidbody rigid;
    private float startingRotation;
    private float desiredRotation;
    private Vector3 startingPosition;
    private float desiredDistance;
    private float timeOfStart;
    private float timeToEnd;
    private float desiredPower;
    private bool movementDesired = false;
    private bool rotationDesired = false;
    private bool needsToCrossThreshold = false;
    private float lastFrameRotation = 0;
    // Start is called before the first frame update
    void Start()
    {
        sensor = GetComponentInChildren<SensorSuite>();
        rigid = GetComponent<Rigidbody>();
        Rotate(-30);
        gameObject.layer = Physics.IgnoreRaycastLayer;
        ControllScript.GetInstance().RegisterRobot(this);
        ControllScript.GetInstance().notifyRobotReady();
    }

    // Update is called once per frame
    void Update()
    {
        if (rotationDesired){
            
            rigid.AddTorque(0, currentTorque, 0);
            float virtualRotation = transform.rotation.eulerAngles.y;
            float virtualDesiredRotation = desiredRotation;
            
            if (Mathf.Abs(lastFrameRotation - virtualRotation) > 50)
                needsToCrossThreshold = false;
            
            lastFrameRotation = virtualRotation;
            if (!needsToCrossThreshold && currentTorque > 0 && virtualRotation > virtualDesiredRotation)
            {
                rotationDesired = false;
                ControllScript.GetInstance().notifyRobotReady();
            }
            if (!needsToCrossThreshold && currentTorque < 0 && virtualRotation < virtualDesiredRotation)
            {
                rotationDesired = false;
                ControllScript.GetInstance().notifyRobotReady();
            }
        }

        if (!movementDesired)
            return;
        rigid.AddForce(transform.forward * desiredPower);
        if(rigid.velocity.magnitude> MaxVelocity)
        {
            rigid.velocity.Normalize();
            rigid.velocity *= MaxVelocity;
        }
        CheckIfMoveFinished();
    }


    void CheckIfMoveFinished()
    {
        if (Vector3.Distance(startingPosition, transform.position) >= desiredDistance || Time.time > timeToEnd||Time.time >timeOfStart+timeOut)
        {
            movementDesired = false;
            ControllScript.GetInstance().notifyRobotReady();
        }
    }
    /// <summary>
    /// Moves the bot forward (if power > 0) or backward (if power < 0)
    /// </summary>
    /// <param name="distance">distance the robot is supposed to cross</param>
    /// <param name="power">(optional) Determines whether the bot moves forward or backwards and how fast, remains the same as last use if not provided</param>
    public void Move(float distance, float power = 0)
    {
        if (!Mathf.Approximately(power, 0))
        {
            power = Mathf.Clamp(power, -maxPower, maxPower);
            desiredPower = power;
        }
        startingPosition = transform.position;
        desiredDistance = Mathf.Abs(distance);
        timeOfStart = Time.time;
        movementDesired = true;
        timeToEnd = float.PositiveInfinity;
    }
    /// <summary>
    /// Rotates the bot by degrees (negative for left, positive for right)
    /// </summary>
    /// <param name="degrees"></param>
    public void Rotate(float degrees)
    {
        rotationDesired = true;
        currentTorque = StandardTorque;
        startingRotation = transform.rotation.eulerAngles.y;
        desiredRotation = startingRotation + degrees;
        if (desiredRotation < 0)
        {
            desiredRotation = 360 + desiredRotation;
            needsToCrossThreshold = true;
        }
        if (desiredRotation > 360)
        {
            desiredRotation -= 360;
            needsToCrossThreshold = true;
        }
        if (degrees < 0)
            currentTorque *= -1;
        lastFrameRotation = startingRotation;
    }

    /// <summary>
    /// Performs a scan and returns the results
    /// </summary>
    /// <returns></returns>
    public float Scan()
    {
        return sensor.GetDistance();
    }
    void OnCollisionEnter(Collision col)
    {
        float ratioOfMovement = Vector3.Distance(transform.position, startingPosition) / desiredDistance;
        float elapsedTime = Time.time - timeOfStart;
        timeToEnd = timeOfStart + elapsedTime / ratioOfMovement;
    }
    public float CompareLocations(Vector3 position, Vector3 angles)
    {
        Debug.Log(
            "Position is off by: " + 
            Vector3.Distance(position, transform.position) + 
            "\nRotation is off by: " + 
            Vector3.Angle(angles, transform.rotation.eulerAngles)
            );
        
        return Vector3.Distance(position, transform.position);
    }
    public float MaxForward
    {
        get
        {
            return maxPower;
        }
    }
    public float MaxBackward
    {
        get
        {
            return -maxPower;
        }
    }
}
