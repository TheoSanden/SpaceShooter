using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health_UI : MonoBehaviour
{
    int healthPerPip = 0;
    int lastSavedHealth;
    [SerializeField]
    List<GameObject> healthPips = new List<GameObject>(),
                     healthDivisions = new List<GameObject>();
    [SerializeField] GameObject healthPip;
    [SerializeField] Health health;
    int maxHealth;
    private void Start()
    {
        Populate();
        health.OnHealthChange += UpdateHealth;
        lastSavedHealth = healthDivisions.Count - 1;
        UpdateHealth( health.GetMaxHealth);
    }
    private void Populate()
    {
        healthPerPip = healthPip.transform.childCount;
        int pipAmount = Mathf.CeilToInt(health.GetMaxHealth / healthPerPip);
        for (int i = 0; i < pipAmount; i++)
        {
            GameObject pip = GameObject.Instantiate(healthPip, this.transform);
            healthPips.Add(pip);
            foreach (Transform child in pip.transform)
            {
                healthDivisions.Add(child.gameObject);
            }
        }
    }
    private void UpdateHealth(int result)
    {
        //lost Life
        if (lastSavedHealth - result < 0)
        {
            for (int i = lastSavedHealth; i < result - 1; i++)
            {
                healthDivisions[healthDivisions.Count - i - 1].SetActive(true);
            }
        }//Gained Life
        else if (lastSavedHealth - result > 0)
        {
            for (int i = result; i < lastSavedHealth; i++)
            {
                healthDivisions[healthDivisions.Count - i - 1].SetActive(false);
            }
        }
        lastSavedHealth = result;
    }
}
