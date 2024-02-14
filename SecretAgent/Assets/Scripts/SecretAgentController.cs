using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SecretAgentController : Agent
{
    //SecretAgent variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    [SerializeField] private float pushForce = 10f;

    Material envMaterial;
    public GameObject env;

    public GameObject civilian;
    public AgentController classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }


    public override void OnEpisodeBegin()
    {
        //Secret Agent
        Vector3 spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.9f, Random.Range(-4f, 4f));

        bool distanceGood = classObject.CheckOverlap(civilian.transform.localPosition, spawnLocation, 1f);

        int maxAttempts = 100;
        int attempt = 0;

        while (!distanceGood && attempt < maxAttempts)
        {
            spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.9f, Random.Range(-4f, 4f));
            distanceGood = classObject.CheckOverlap(civilian.transform.localPosition, spawnLocation, 1f);
            attempt++;
        }

        if (!distanceGood)
        {
            Debug.LogError("Failed to find a suitable spawn location after " + maxAttempts + " attempts.");
            // Handle the situation where a suitable spawn location could not be found
        }

        transform.localPosition = spawnLocation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

        /*
        Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        velocity = velocity.normalized * Time.deltaTime * moveSpeed;

        transform.localPosition += velocity;
        */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Agent")
        {
            Vector3 direction = other.transform.position - transform.position;
            direction.y = 0f; // Ensure only horizontal force is applied
            direction.Normalize();
            rb.AddForce(direction * pushForce, ForceMode.Impulse);
        }
        if (other.gameObject.tag == "Wall")
        {
            envMaterial.color = Color.black;
            AddReward(-15f);
            EndEpisode();
            classObject.EndEpisode();
        }
    }
}

