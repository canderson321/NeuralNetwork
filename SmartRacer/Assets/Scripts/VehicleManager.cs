using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeuralNetwork;

public class VehicleManager : MonoBehaviour {

    public int NumIndividuals = 100;
    public float GenerationLifespan = 5;

    private GenePool pool;
    public GameObject CarPrototype;
    public List<VehicleDriver> Cars;

    private int PoolSize;

    private float time = 0;

    // Use this for initialization
    void Start () {
        Cars = new List<VehicleDriver>();
        pool = new GenePool();
        pool.PoolSize = NumIndividuals;
        pool.Initialize(new int[] { 17, 10, 10, 10, 2 });

        for (int i = 0; i < pool.PoolSize; i++)
        {
            VehicleDriver car = (Instantiate(CarPrototype) as GameObject).GetComponent<VehicleDriver>();
            car.network = pool.Individuals[i];
            Cars.Add(car);
        }

        GetComponent<TrackGenerator>().GenerateTrack();
        Cars[0].FollowedCar = true;
    }

    public void NextGen()
    {
        if (pool == null) return;
        pool.SortByFitness();
        Debug.Log("Generation " + pool.Generation + " " + pool.Individuals[0].fitness + " " + pool.Individuals[pool.Individuals.Count - 1].fitness);
        pool.NextGeneration();
        for (int i = 0; i < pool.PoolSize; i++)
        {
            Cars[i].network = pool.Individuals[i];

        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        time += Time.fixedDeltaTime;
        while (time > GenerationLifespan)
        {
            time -= GenerationLifespan;
            GetComponent<TrackGenerator>().GenerateTrack();
            NextGen();
        }
	}

    public void OnTimestepChange(Slider slider)
    {
        Time.timeScale = slider.value;
    }
    public void OnGenerationLifeChange(Slider slider)
    {
        GenerationLifespan = slider.value;
    }


    public void OnPoolSizeChange(Slider slider)
    {
        int newSize = Mathf.FloorToInt(slider.value);
        int diff = newSize - pool.PoolSize;
        pool.PoolSize = newSize;

        if (diff > 0)
        {
            Debug.Log("Adding " + diff);
            for (int i = 0; i < diff; i++)
            {
                VehicleDriver car = (Instantiate(CarPrototype) as GameObject).GetComponent<VehicleDriver>();
                Cars.Add(car);
            }
        }
        else if (diff < 0)
        {
            Debug.Log("Removing " + -diff);
            for (int i = 0; i < -diff; i++)
            {
                VehicleDriver car = Cars.Last();
                Destroy(car.gameObject);
                Cars.Remove(car);
            }
        }
    }
}
