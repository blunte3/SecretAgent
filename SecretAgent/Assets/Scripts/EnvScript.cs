using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvScript : MonoBehaviour
{
    //pellet variables
    [SerializeField] private Transform target;
    public int pelletCount;
    public GameObject food;
    //store pellets in this list
     [SerializeField] public List<GameObject> spawnedPelletsList = new List<GameObject>();
    public List<float> distanceList = new List<float>();
    public List<float> badDistanceList = new List<float>();

    //agent tracking
    public GameObject agent;
    [SerializeField] public List<GameObject> spawnedAgentsList = new List<GameObject>();

    public override void Initialize(){
        //initialize 
        
    }
    private void AddAgent(){
        if(!agent.IsSecret && agent != null){
            spawnedAgentsList.Add(agent);
        }
    }

    //loop through the list of Agents, checking all their hunger
    //if hunger <= 0, delete them from list and delete the object

    private void CheckAgentHunger(){
        for(i = 0; i < spawnedAgentsList.Count; i++){
            if(spawnedAgentsList[i].GetComponent<currentHunger>() <= 0){
                Destroy(spawnedAgentsList[i]);
                spawnedAgentsList.RemoveAt(i);
            }
        }
    }

    private void CreatePellet()
    {
        distanceList.Clear();
        badDistanceList.Clear();

        if(spawnedPelletsList.Count != 0)
        {
            RemovePellet(spawnedPelletsList);
        }

        for(int i = 0; i < pelletCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;

            //Spawning pellet
            GameObject newPellet = Instantiate(food);
            //Make pellet child of the environment
            newPellet.transform.parent = environmentLocation;
            //Give random spawn location
            Vector3 pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));

            if(spawnedPelletsList.Count != 0)
            {
                for(int k = 0; k < spawnedPelletsList.Count; k++)
                {
                    if(counter < 10)
                    {
                        distanceGood = CheckOverlap(pelletLocation, spawnedPelletsList[k].transform.localPosition, 5f);
                        if(distanceGood == false)
                        {
                            pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));
                            k--;
                            alreadyDecremented = true;
                        }

                        distanceGood = CheckOverlap(pelletLocation, transform.localPosition, 5f);
                        if (distanceGood == false)
                        {
                            pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));
                            if(alreadyDecremented == false)
                            {
                                k--;
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletsList.Count;
                    }
                }
            }

            //Spawn in new location
            newPellet.transform.localPosition = pelletLocation;
            //Add to list
            spawnedPelletsList.Add(newPellet);
        }
    }

    public override void OnEpisodeBegin(){
        AddAgent();
        CreatePellet();
        CheckAgentHunger();
    }
        // Update is called once per frame
    void Update()
    {
        
    }
}
