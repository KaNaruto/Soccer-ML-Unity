using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SoccerAgentGoals : Agent
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 100f;

    private Transform _redGoal;
    private Transform _blueGoal;

    private GameObject _ball;
    private BallManager _ballManager;


    private Vector3 _startingPosition;
    private Quaternion _startingRotation;


    private bool _goal;

    private void Start()
    {
        // Get the ball and goals in the environment
        var environment = transform.parent.parent;
        _ball = environment.Find("Ball").gameObject;
        _blueGoal = GameObject.Find("BlueGoal").transform;
        _redGoal = GameObject.Find("RedGoal").transform;


        var transform1 = transform;
        _startingPosition = transform1.position;
        _startingRotation = transform1.rotation;

        _ballManager = environment.GetComponentInChildren<BallManager>();
        if (_ballManager == null)
        {
            Debug.LogError("BallManager component not found in the environment!");
        }
        else
        {
            _ballManager.OnGoal += OnGoal;
        }

        GameManager.OnReset += OnReset;
    }

    private void OnDestroy()
    {
        GameManager.OnReset -= OnReset;
    }

    public override void OnEpisodeBegin()
    {
        ResetPositions();
        // Penalize if no goal was scored
        if (!_goal)
        {
            AddReward(-2f);
        }

        _goal = false;
    }

    private void FixedUpdate()
    {
        // Add small reward based on the distance to the ball
        float distanceToBall = Vector3.Distance(transform.position, _ball.transform.position);
        float distanceReward = 1f / (distanceToBall + 1f);
        AddReward(distanceReward * 0.03f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect relative positions and velocities of the agent, ball, and goals
        var position = transform.position;
        sensor.AddObservation(_ball.transform.position - position);
        sensor.AddObservation(GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(_ball.GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(_redGoal.position - position);
        sensor.AddObservation(_blueGoal.position - position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;

        // Move forward or backward based on actions
        if (discreteActions[0] == 0)
        {
            var transform1 = transform;
            transform1.position += transform1.forward * speed * Time.deltaTime;
        }
        else if (discreteActions[0] == 1)
        {
            var transform1 = transform;
            transform1.position -= transform1.forward * speed * Time.deltaTime;
        }

        // Rotate left or right
        if (discreteActions[1] == 1)
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }
        else if (discreteActions[1] == 2)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Shoot the ball if action is triggered
        if (discreteActions[2] == 1)
        {
            float shootPower = Mathf.Abs(actions.ContinuousActions[0]);
            Shoot(shootPower * 10);
        }
    }


    private void Shoot(float power)
    {
        // Check distance to the ball and shoot if within range
        if ((_ball.transform.position - transform.position).sqrMagnitude <= 5f)
        {
            _ballManager.ShootBall(gameObject, transform.position, power);
        }
    }


    // Add rewards or penalties based on which team scored
    private void OnGoal(bool team)
    {
        if (team)
        {
            Debug.Log("Red scored");
            if (transform.gameObject.CompareTag("RedPlayer"))
                AddReward(2f);
            else if (transform.gameObject.CompareTag("BluePlayer"))
                AddReward(-2f);
        }
        else
        {
            Debug.Log("Blue scored");
            if (transform.gameObject.CompareTag("RedPlayer"))
                AddReward(-2f);
            else if (transform.gameObject.CompareTag("BluePlayer"))
                AddReward(2f);
        }

        _goal = true;
        ResetPositions();
    }

    private void OnReset()
    {
        ResetPositions();
        EndEpisode();
    }

    private void ResetPositions()
    {
        var transform1 = transform;
        transform1.position = _startingPosition;
        transform1.rotation = _startingRotation;

        if (_ballManager != null)
        {
            _ballManager.ResetBall();
        }
    }

    // Rewards for passing, intercepting, losing the ball, or scoring an own goal
    public void Pass() => AddReward(0.3f);
    public void Intercept() => AddReward(0.3f);
    public void LossBall() => AddReward(-0.3f);
    public void OwnGoal() => AddReward(-2.5f);
}