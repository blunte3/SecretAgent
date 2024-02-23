using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

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

    // Hunger level variables
    public float maxHunger = 50f;
    public float hungerDecreaseRate = 2f;
    public float hungerIncreaseAmount = 5f;
    private float currentHunger;

    // UI variables
    public Slider hungerSlider;

    //Agent Variables
    public AgentController agentController;

    public bool IsSecret(){
        return true;
    }
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

        // Initialize hunger level
        currentHunger = maxHunger;
        UpdateHungerUI();
    }

    private void Update()
    {
        // Decrease hunger level over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        UpdateHungerUI();

        // Check if the agent is too hungry
        if (currentHunger <= 0f)
        {
            envMaterial.color = Color.magenta;
            AddReward(-50f);
            classObject.AddReward(30f);
            EndEpisode();
            classObject.EndEpisode();
        }
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
        if (other.gameObject.tag == "Pellet")
        {
            //Remove from list
            agentController.RemovePellet(other.gameObject);
            Destroy(other.gameObject);

            // Increase hunger level when eating a pellet
            currentHunger += hungerIncreaseAmount;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger); // Ensure hunger level doesn't exceed max
            UpdateHungerUI();
        }
        if (other.gameObject.tag == "Agent") //knockback physics for agent collisions
        {
            Vector3 direction = other.transform.position - transform.position;
            direction.y = 0f; // Ensure only horizontal force is applied
            direction.Normalize();
            rb.AddForce(direction * pushForce, ForceMode.Impulse);

            AddReward(2f);//makes collisions more likely
        }
        if (other.gameObject.tag == "Wall")
        {
            envMaterial.color = Color.black;
            AddReward(-50f);
            EndEpisode();
            classObject.EndEpisode();
        }
    }

    private void UpdateHungerUI()
    {
        // Update UI element to reflect hunger level
        hungerSlider.value = currentHunger / maxHunger;
    }
}

