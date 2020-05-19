using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace ShilohGames
{
    public class InstancePoolLasersBaseClass : MonoBehaviour
    {
        public Material WeaponMaterial;
        public Mesh WeaponMesh;

        private int pooledHighestIndex = 0;

        private Camera PlayerCamera = null;
        private bool boolReceiveShadows = false;
        private MaterialPropertyBlock materialProperty;
        private Matrix4x4[] MatrixArray;
        private int matrixSize = 500;


        public struct LaserProjectile
        {
            public Vector3 currentPosition;
            public Vector3 oldPosition;
            public Quaternion rotation;
            public Vector3 movementVector;
            public Vector3 directionVectorNormalized;
            public bool active;
            public GameObject FiredBy;
            public float timeToExpire;
        }


        public float life = 10.0f;
        public float velocity = 2000.0f;
        public float DamageToTarget = 15.0f;

        private LaserProjectile[] ProjectileArray;
        private int projectileArraySize = 15000;

        private float currentTime = 0.0f;
        private float cachedDeltaTime;

        private float lifeOriginal = 10.0f;
        private float velocityOriginal = 850.0f;

        private float distancePerFrame;
        private Vector3 newPosition = new Vector3();
        private float distanceToCheck;

        private int objectCount = 0;

        private bool renderEnabled = true;

        /*
        public enum ProjectileTypes
        {
            PlayerDualLasers = 1,
            HumanSingleMediumLaser = 2,
            HumanSingleFlakLaser = 3,
            PlasmaSingleSmall = 10,
            RedSingleMediumLaser = 20,
            ConcussionMissile = 30,
            Unknown = 255
        }

        public ProjectileTypes projectileType = ProjectileTypes.Unknown;
        */


        public enum ImpactTypeEnum
        {
            LaserImpact = 0,
            PlasmaImpact = 1
        }
        public ImpactTypeEnum impactType;


        private RaycastHit _hit = new RaycastHit();

        private GameObject objImpact;




        private void Awake()
        {
            objImpact = new GameObject();
        }



        void Start()
        {
            PlayerCamera = null;

            MatrixArray = new Matrix4x4[matrixSize];
            InitializeMatrixArray();

            ProjectileArray = new LaserProjectile[projectileArraySize];
            InitializeProjectileArray();

            materialProperty = new MaterialPropertyBlock();
            materialProperty.Clear();

            if (Application.isBatchMode)
            {
                renderEnabled = false;
            }
        }



        void InitializeMatrixArray()
        {
            for (int i = 0; i < matrixSize; i++)
            {
                MatrixArray[i] = new Matrix4x4();
                MatrixArray[i].SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
            }
        }



        void InitializeProjectileArray()
        {
            for (int i = 0; i < projectileArraySize; i++)
            {
                ProjectileArray[i].active = false;
                ProjectileArray[i].currentPosition = Vector3.zero;
                ProjectileArray[i].oldPosition = Vector3.zero;
                ProjectileArray[i].rotation = Quaternion.identity;
                ProjectileArray[i].movementVector = Vector3.zero;
                ProjectileArray[i].directionVectorNormalized = Vector3.zero;
                ProjectileArray[i].FiredBy = null;
                ProjectileArray[i].timeToExpire = 0.0f;
            }
        }



        public void FireWeapon(Vector3 weaponPosition, Quaternion weaponRotation, Vector3 weaponMovementVectorForward, GameObject weaponFiredBy)
        {
            int projectileNumber = 0;
            projectileNumber = FindNextInactiveProjectile();

            if (projectileNumber > pooledHighestIndex)
            {
                pooledHighestIndex = projectileNumber;
            }

            ProjectileArray[projectileNumber].active = true;

            ProjectileArray[projectileNumber].oldPosition = weaponPosition;
            ProjectileArray[projectileNumber].currentPosition = weaponPosition;
            ProjectileArray[projectileNumber].FiredBy = weaponFiredBy;
            ProjectileArray[projectileNumber].movementVector = weaponMovementVectorForward * velocity;
            ProjectileArray[projectileNumber].directionVectorNormalized = ProjectileArray[projectileNumber].movementVector.normalized;
            ProjectileArray[projectileNumber].rotation = weaponRotation;
            ProjectileArray[projectileNumber].timeToExpire = Time.time + life;
        }


        /*
        public void AddProjectileFromNetwork(Vector3 weaponPosition, Quaternion weaponRotation, Vector3 weaponMovementVectorForward, GameObject weaponFiredBy)
        {
            int projectileNumber = 0;
            projectileNumber = FindNextInactiveProjectile();

            if (projectileNumber > pooledHighestIndex)
            {
                pooledHighestIndex = projectileNumber;
            }

            ProjectileArray[projectileNumber].active = true;

            ProjectileArray[projectileNumber].oldPosition = weaponPosition;
            ProjectileArray[projectileNumber].currentPosition = weaponPosition;
            ProjectileArray[projectileNumber].FiredBy = weaponFiredBy;
            ProjectileArray[projectileNumber].movementVector = weaponMovementVectorForward * velocity;
            ProjectileArray[projectileNumber].directionVectorNormalized = ProjectileArray[projectileNumber].movementVector.normalized;
            ProjectileArray[projectileNumber].rotation = weaponRotation;
            ProjectileArray[projectileNumber].timeToExpire = Time.time + life;
        }
        */


        public void DisableProjectiles()
        {
            for (int i = 0; i <= pooledHighestIndex; i++)
            {
                ProjectileArray[i].active = false;
            }
        }



        private int FindNextInactiveProjectile()
        {
            int selectedItem = 0;
            for (int i = 0; i < projectileArraySize; i++)
            {
                if (ProjectileArray[i].active == false)
                {
                    selectedItem = i;
                    return selectedItem;
                }           
            }
            return selectedItem;
        }



        private void CountActiveObjects()
        {
            objectCount = 0;
            for (int i = 0; i <= pooledHighestIndex; i++)
            {
                if (ProjectileArray[i].active == true)
                {
                    objectCount++;
                }
            }
        }



        void MoveProjectiles()
        {
            bool hitDetected = false;

            currentTime = Time.time;
            cachedDeltaTime = Time.deltaTime;
            distancePerFrame = velocity * Time.deltaTime;
            distanceToCheck = distancePerFrame * 2.0f;

            for (int projectileNumber = 0; projectileNumber <= pooledHighestIndex; projectileNumber++)
            {
                if (ProjectileArray[projectileNumber].active == true)
                {
                    if (currentTime > ProjectileArray[projectileNumber].timeToExpire)
                    {
                        ProjectileArray[projectileNumber].active = false;
                    }
                    else
                    {               
                        newPosition = ProjectileArray[projectileNumber].currentPosition + (ProjectileArray[projectileNumber].movementVector * cachedDeltaTime);

                        if (distancePerFrame > 0)
                        {
                            hitDetected = Physics.Raycast(ProjectileArray[projectileNumber].oldPosition, ProjectileArray[projectileNumber].directionVectorNormalized, out _hit, distanceToCheck);

                            if (_hit.normal != Vector3.zero)
                            {
                                if (_hit.transform.gameObject != ProjectileArray[projectileNumber].FiredBy && !_hit.collider.isTrigger)
                                {
                                    Quaternion impactRotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);

                                    // This is where you can place code to enable impacts and apply damage to the health system on the target object
                                    // I use the impactType (ImpactTypeEnum) to set the type of impact for each type of projectile.
                                    // In the ImpactTypeEnum in this example, I only have LaserImpact and PlasmaImpact, but you can expand that to cover all of the types of impacts you have in your game.
                                    // I strongly recommend using an object pool or instancing pool for managing impacts instead of using Instantiate right here, especially if there are lots of impacts in your game.

                                    // If your game has visual shields around the ships, this is where you would trigger the impacts on your shield effects
                                    // Apply the shield effect at _hit.point if you have a visual shield system
                                                                        

                                    ProjectileArray[projectileNumber].active = false;
                                }
                            }
                        }

                        ProjectileArray[projectileNumber].oldPosition = ProjectileArray[projectileNumber].currentPosition;
                        ProjectileArray[projectileNumber].currentPosition = newPosition;
                    }
                }
            }
        }



        void RenderProjectiles()
        {
            int meshCount = 0;
            bool atLeastOneUnit = false;

            for (int projectileNumber = 0; projectileNumber <= pooledHighestIndex; projectileNumber++)
            {
                if (ProjectileArray[projectileNumber].active == true)
                {
                    MatrixArray[meshCount].SetTRS(ProjectileArray[projectileNumber].currentPosition, ProjectileArray[projectileNumber].rotation, Vector3.one);

                    meshCount++;
                    atLeastOneUnit = true;
                }

                if (meshCount == matrixSize)
                {
                    Graphics.DrawMeshInstanced(WeaponMesh, 0, WeaponMaterial, MatrixArray, meshCount, materialProperty, UnityEngine.Rendering.ShadowCastingMode.Off, boolReceiveShadows, 0, PlayerCamera);

                    meshCount = 0;
                    atLeastOneUnit = false;
                }
            }

            if (atLeastOneUnit)
            {
                Graphics.DrawMeshInstanced(WeaponMesh, 0, WeaponMaterial, MatrixArray, meshCount, materialProperty, UnityEngine.Rendering.ShadowCastingMode.Off, boolReceiveShadows, 0, PlayerCamera);
            }
        }



        void Update()
        {
            CountActiveObjects();

            if (objectCount > 0)
            {
                MoveProjectiles();

                if (renderEnabled)
                {
                    RenderProjectiles();
                }            
            }
        }



    }
}
