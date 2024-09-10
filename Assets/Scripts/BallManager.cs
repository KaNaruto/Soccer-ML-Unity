using System;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public event Action<bool> OnGoal;
    private Vector3 startingPosition;
    private GameObject lastOwner;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
    }


    public void ShootBall(GameObject player, Vector3 actionPosition, float power)
    {
        // Calculate shooting direction and apply force to the ball
        Vector3 direction = (transform.position - actionPosition).normalized;
        _rigidbody.AddForce(direction * power, ForceMode.Impulse);

        // Check for pass or intercept based on last owner of the ball
        if (lastOwner != null && lastOwner != player)
        {
            // Check if the pass was to a teammate
            bool isTeammate = lastOwner.CompareTag(player.tag);
            if (isTeammate)
            {
                player.GetComponent<SoccerAgentGoals>().Pass();
                lastOwner.GetComponent<SoccerAgentGoals>().Pass();
            }
            else
            {
                 player.GetComponent<SoccerAgentGoals>().Intercept();
                lastOwner.GetComponent<SoccerAgentGoals>().LossBall();
            }
        }

        // Update lastOwner to the current player
        lastOwner = player;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect goal and invoke appropriate event
        if (other.gameObject.CompareTag("RedGoal") || other.gameObject.CompareTag("BlueGoal"))
        {
            if (other.name == "BlueGoal")
            {
                
                if (lastOwner != null && lastOwner.CompareTag("BluePlayer"))
                    lastOwner.GetComponent<SoccerAgentGoals>().OwnGoal(); 

                OnGoal?.Invoke(true);
            }
            else
            {
                
                if (lastOwner != null && lastOwner.CompareTag("RedPlayer"))
                    lastOwner.GetComponent<SoccerAgentGoals>().OwnGoal(); 
                OnGoal?.Invoke(false);
            }
        }
    }


    // Reset the ball position and velocity
    public void ResetBall()
    {
        _rigidbody.position = startingPosition;
        _rigidbody.velocity = Vector3.zero; 
        _rigidbody.angularVelocity = Vector3.zero; 
        lastOwner = null;
    }
}