using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//using UnityEditor.ShaderGraph;

public class EnemieSpawning : MonoBehaviour
{
    public List<int> spawnMulti;
    public List<GameObject> prefabs;
    [SerializeField] private Vector3 localOffset = Vector3.zero;

	[SerializeField] private Vector3 localRandomOffsetMin = Vector3.zero;
	[SerializeField] private Vector3 localRandomOffsetMax = Vector3.zero;
    [SerializeField] private SplinePathSet pathSet;
    enum Phase { Peace, Wave }
    Phase phase = Phase.Peace;
    public float startWait = 10f;
    public float waveTime = 60f;
    public float peaceTime = 30f;
    public float subWaves;
    public float tempWaveTime, tempPeaceTime, tempSubWaveTime;
    public int wave;
    private bool waving;
    public Slider slider;
    private List<GameObject> enemies = new List<GameObject>();

    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempPeaceTime = startWait;
    }

    // Update is called once per frame
    void Update()
    {
        enemies.RemoveAll(item => item == null);
        float interval = waveTime / subWaves;

        if (phase == Phase.Peace)
    {
        if(enemies.Count==0) tempPeaceTime -= Time.deltaTime;
        slider.value = Mathf.Clamp01(tempPeaceTime / peaceTime);
        slider.fillRect.gameObject.GetComponent<Image>().color = Color.green;

        if (tempPeaceTime <= 0f)
        {
            phase = Phase.Wave;
            wave += 1;
            tempWaveTime = waveTime;
            tempSubWaveTime = interval;
            audioSource.Play();
        }
    }
    else // Wave
    {
        tempWaveTime -= Time.deltaTime;
        slider.value = Mathf.Clamp01(tempWaveTime / waveTime);
        slider.fillRect.gameObject.GetComponent<Image>().color = Color.red;

        tempSubWaveTime += Time.deltaTime;

        while (tempSubWaveTime >= interval)
        {
            tempSubWaveTime -= interval;
            SpawnEnemy(Mathf.Clamp(wave, 0, prefabs.Count));
        }

        if (tempWaveTime <= 0f)
        {
            phase = Phase.Peace;
            tempPeaceTime = peaceTime;
            tempSubWaveTime = 0f;
        }
    }
    }

    public void SpawnEnemy(int index)
    {
        for(int enemy = 0; enemy < index; enemy ++){
            for(int i = 0; i < spawnMulti[enemy]; i++)
            {
                for(int j = 0; j < 10; j++)
                {
                    // Spawn amount
                    spawn(prefabs[enemy]);
                }
            }
        }
    }

    public void spawn(GameObject prefab)
    {
        Transform sp = transform;

		// Base
		Vector3 pos = sp.position;
		Quaternion rot = sp.rotation;

		// Configurable + random local-space offset
		Vector3 rand = new Vector3(
			UnityEngine.Random.Range(localRandomOffsetMin.x, localRandomOffsetMax.x),
			UnityEngine.Random.Range(localRandomOffsetMin.y, localRandomOffsetMax.y),
			UnityEngine.Random.Range(localRandomOffsetMin.z, localRandomOffsetMax.z)
		);

		Vector3 worldOffset = sp.TransformVector(localOffset + rand);
		pos += worldOffset;

		GameObject go = Instantiate(prefab, pos, rot);
        enemies.Add(go);

		var motor = go.GetComponent<SplineEnemyMotor>();
		if (motor)
		{
			motor.SetPathSet(pathSet);
		}
    }
}
