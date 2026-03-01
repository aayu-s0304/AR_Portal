using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlimeSpawner : MonoBehaviour
{
    public GameObject slimePrefab;
    public GameObject terrainPrefab;
    public Transform arCamera;

    private int slimeCount = 7;
    private GameObject spawnedTerrain;
    private List<GameObject> spawnedSlimes = new List<GameObject>();
    private float minDistance = 2f; // Minimum distance from player for slimes
    private float maxDistance = 7f; // Maximum distance from player for slimes
    private float moveSpeed = 1f; // Movement speed of the slimes

    void Update()
    {
        // Check if terrain is spawned
        if (spawnedTerrain == null)
        {
            spawnedTerrain = GameObject.FindWithTag("Terrain");
            if (spawnedTerrain != null)
            {
                AdjustPlayerHeight();
                StartCoroutine(SpawnSlimes());
            }
        }

        // Move the slimes based on player position
        MoveSlimes();
    }

    void AdjustPlayerHeight()
    {
        if (spawnedTerrain != null)
        {
            Terrain terrain = spawnedTerrain.GetComponent<Terrain>();
            if (terrain != null)
            {
                Vector3 playerPos = arCamera.position;
                float terrainHeight = terrain.SampleHeight(playerPos);
                arCamera.position = new Vector3(playerPos.x, terrainHeight + 1.0f, playerPos.z);
            }
        }
    }

    IEnumerator SpawnSlimes()
    {
        for (int i = 0; i < slimeCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition();
            GameObject slime = Instantiate(slimePrefab, spawnPos, slimePrefab.transform.rotation);
            spawnedSlimes.Add(slime);

            Rigidbody rb = slime.GetComponent<Rigidbody>();
            if (rb == null)
                rb = slime.AddComponent<Rigidbody>();

            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            StartCoroutine(SlimeJump(rb));
            yield return new WaitForSeconds(0.5f);
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 playerPos = arCamera.position;
        Vector3 direction = (Random.insideUnitSphere * 2f).normalized;
        direction.y = 0;

        Vector3 spawnPos = playerPos + direction * 2f; // Spawn slimes 2 meters away from player
        float terrainHeight = spawnedTerrain.GetComponent<Terrain>().SampleHeight(spawnPos);
        spawnPos.y = terrainHeight + 0.1f;

        return spawnPos;
    }

    IEnumerator SlimeJump(Rigidbody rb)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse); // Slime jump effect
        }
    }

    void MoveSlimes()
    {
        foreach (GameObject slime in spawnedSlimes)
        {
            if (slime != null)
            {
                // Calculate the distance between the slime and the player
                float distanceToPlayer = Vector3.Distance(slime.transform.position, arCamera.position);

                if (distanceToPlayer < minDistance)
                {
                    // If the slime is too close to the player, move it away
                    Vector3 directionAway = (slime.transform.position - arCamera.position).normalized;
                    directionAway.y = 0; // Keep slimes on the ground
                    slime.transform.position += directionAway * moveSpeed * Time.deltaTime;
                }
                else if (distanceToPlayer > maxDistance)
                {
                    // If the slime is too far from the player, move it towards the player
                    Vector3 directionTowardsPlayer = (arCamera.position - slime.transform.position).normalized;
                    directionTowardsPlayer.y = 0; // Keep slimes on the ground
                    slime.transform.position += directionTowardsPlayer * moveSpeed * Time.deltaTime;
                }
            }
        }
    }
}
