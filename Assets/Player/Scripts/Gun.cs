using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player 
{
    public class Gun : MonoBehaviour
    {
        Engine Engine;
        float FireRateTimer;

        [SerializeField]
        GameObject LazerPrefab;

        [SerializeField]
        float FireRate = 0.3f;

        // Start is called before the first frame update
        void Start()
        {
            Engine = GetComponentInParent<Engine>();
        }

        // Update is called once per frame
        void Update()
        {

            if (FireRateTimer > 0)
            {
                FireRateTimer = (FireRateTimer - Time.deltaTime < 0) ? 0 : FireRateTimer - Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Mouse0) && FireRateTimer == 0)
            {
                Shoot();
            }
        }
        void Shoot()
        {
            FireRateTimer = FireRate;
            GameObject InstantiatedLazer = Instantiate(LazerPrefab, transform.position + Engine.Velocity, Quaternion.identity);
            InstantiatedLazer.transform.up = this.transform.up;
        }
    }
}