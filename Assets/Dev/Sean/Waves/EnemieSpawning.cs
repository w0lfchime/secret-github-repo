using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
//using UnityEditor.ShaderGraph;

public class EnemieSpawning : MonoBehaviour
{
    public List<int> spawnMulti;
    public List<GameObject> prefabs;
	[SerializeField] private Vector2 spawnRadiusMinMax;
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
    public TMP_Text text;

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
        foreach(GameObject enemy in enemies)
            {
                enemy.transform.position+=Vector3.up*Time.deltaTime*40;
                if(enemy.transform.position.y > 30)
                {
                    Destroy(enemy);
                }
            }
        tempPeaceTime -= Time.deltaTime;
        slider.value = Mathf.Clamp01(tempPeaceTime / peaceTime);
        slider.fillRect.gameObject.GetComponent<Image>().color = Color.green;
        text.text = "PEACE";

        if (tempPeaceTime <= 0f)
        {
            phase = Phase.Wave;
            wave += 1;
            tempWaveTime = waveTime;
            tempSubWaveTime = interval;
        }
    }
    else // Wave
    {
        tempWaveTime -= Time.deltaTime;
        slider.value = Mathf.Clamp01(tempWaveTime / waveTime);
        slider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        text.text = "DANGER";

        tempSubWaveTime += Time.deltaTime;

        if (tempSubWaveTime >= interval)
        {
            tempSubWaveTime = 0;
            SpawnEnemy(Mathf.Clamp(wave, 0, prefabs.Count));
            audioSource.Play();
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
                for(int j = 0; j < 5/Mathf.Abs(index-i); j++)
                {
                    // Spawn amount
                    spawn(prefabs[enemy]);
                }
            }
        }
    }

    public void spawn(GameObject prefab)
    {
		Vector3 randDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 pos = randDirection * Random.Range(spawnRadiusMinMax.x, spawnRadiusMinMax.y);
		pos += transform.position;

		GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        enemies.Add(go);
    }
}
