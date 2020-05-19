using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShilohGames
    {
    public class ShooterSample : MonoBehaviour
    {

        public enum WeaponTypes
        {
            BlueLasers = 1,
            RedLasers = 2,
            Plasma = 3
        }
        public WeaponTypes weaponType = WeaponTypes.BlueLasers;

        public float fireDelay = 0.05f;
        private float nextFire = 0.0f;



        void Start()
        {
            nextFire = Time.time + fireDelay;
        }


        void Update()
        {
            FireLaser();
        }


        private void FireLaser()
        {
            if (Time.time > nextFire) 
            {
                nextFire = Time.time + fireDelay;

                // This example randomly spins the game object
                // In a real game, the game object would be a space ship and would rotate based on player input or AI system
                transform.rotation = Random.rotation;

                switch (weaponType)
                {
                    case WeaponTypes.BlueLasers:
                        LasersBluePool.current.FireWeapon(transform.position, transform.rotation, transform.forward, gameObject);
                        break;
                    case WeaponTypes.RedLasers:
                        LasersRedPool.current.FireWeapon(transform.position, transform.rotation, transform.forward, gameObject);
                        break;
                    case WeaponTypes.Plasma:
                        PlasmaPool.current.FireWeapon(transform.position, transform.rotation, transform.forward, gameObject);
                        break;
                    default:
                        break;
                }

                

                // in a real game, you would play firing sounds and show firing effects here
            }
        }


    }

}