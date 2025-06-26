using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class KongzNFTSpawner : MonoBehaviour
{
    [Header("Kong Builder Integration")]
    public Transform spawnPoint; // Where to spawn the 3D Kong
    public bool spawnWithFingers = true;
    public bool combineMeshes = true;

    [Header("UI References")]
    public Button spawn3DButton;
    public TextMeshProUGUI statusText;

    [Header("Display Settings")]
    public float displayScale = 1f;
    public bool autoRotate = true;
    public float rotationSpeed = 30f;

    [Header("Currently Spawned")]
    private GameObject currentKong;
    private List<int> ownedTokenIds = new List<int>();
    private int currentIndex = 0;

    void Start()
{
    if (spawn3DButton != null)
    {
        spawn3DButton.onClick.AddListener(SpawnNext3DKong);
    }
    
    // Only use spawn point if one is assigned in Inspector
    if (spawnPoint == null)
    {
        Debug.LogWarning("[KongzSpawner] No spawn point set! Kong will spawn at (0,0,0)");
    }
    
    // Ensure KongBuilder is initialized
    var builder = KongBuilder.Instance;
    if (builder == null)
    {
        Debug.LogError("[KongzSpawner] KongBuilder not found! Make sure the Kong Builder SDK is imported.");
    }
    else
    {
        Debug.Log("[KongzSpawner] KongBuilder instance found and ready");
    }
}

    /// <summary>
    /// Called by KongzNFTChecker when NFTs are found
    /// </summary>
    public void SetOwnedTokenIds(List<string> tokenIds)
    {
        ownedTokenIds.Clear();
        foreach (string id in tokenIds)
        {
            if (int.TryParse(id, out int tokenId))
            {
                ownedTokenIds.Add(tokenId);
                Debug.Log($"[KongzSpawner] Added token ID: {tokenId}");
            }
        }

        Debug.Log($"[KongzSpawner] Set {ownedTokenIds.Count} owned token IDs: {string.Join(", ", ownedTokenIds)}");

        if (ownedTokenIds.Count > 0)
        {
            UpdateStatus($"Ready to spawn {ownedTokenIds.Count} Kongz");
            if (spawn3DButton != null)
            {
                spawn3DButton.interactable = true;
            }

            // REMOVED AUTO-SPAWN - User must click button
        }
        else
        {
            UpdateStatus("No Kongz to spawn");
            if (spawn3DButton != null)
            {
                spawn3DButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// Spawn the next Kong in the list
    /// </summary>
    public void SpawnNext3DKong()
    {
        Debug.Log($"[KongzSpawner] SpawnNext3DKong called");

        if (ownedTokenIds.Count == 0)
        {
            UpdateStatus("No token IDs available");
            Debug.LogWarning("[KongzSpawner] No token IDs available to spawn");
            return;
        }

        // Get first token ID for now (we can add cycling later)
        int tokenId = ownedTokenIds[0];

        Debug.Log($"[KongzSpawner] Selected token ID: {tokenId}");
        SpawnSpecificKong(tokenId);
    }

    /// <summary>
    /// Spawn a specific Kong by token ID
    /// </summary>
    public void SpawnSpecificKong(int tokenId)
    {
        Debug.Log($"[KongzSpawner] Starting spawn for Kong #{tokenId}");
        Debug.Log($"[KongzSpawner] Using KongBuilder SDK");
        Debug.Log($"[KongzSpawner] Spawn position: {spawnPoint.position}");
        Debug.Log($"[KongzSpawner] With fingers: {spawnWithFingers}, Combine meshes: {combineMeshes}");

        UpdateStatus($"Spawning KONGZ VX #{tokenId}...");

        // Remove previous Kong if exists
        if (currentKong != null)
        {
            Debug.Log("[KongzSpawner] Destroying previous Kong");
            Destroy(currentKong);
        }

        try
        {
            Debug.Log("[KongzSpawner] Calling KongBuilder.Instance.LoadKong()");

            // Call Kong Builder SDK - LoadKong returns the spawned GameObject
            GameObject spawnedKong = KongBuilder.Instance.LoadKong(
                tokenId,                    // The NFT token ID
                spawnPoint.position,        // Where to spawn
                spawnWithFingers,          // Include finger meshes
                combineMeshes              // Optimize by combining meshes
            );

            if (spawnedKong != null)
            {
                currentKong = spawnedKong;
                currentKong.name = $"KONGZ VX #{tokenId}";

                Debug.Log($"[KongzSpawner] Kong spawned successfully: {currentKong.name}");

                // Apply display settings
                currentKong.transform.localScale = Vector3.one * displayScale;
                Debug.Log($"[KongzSpawner] Applied scale: {displayScale}");

                // Add rotation if enabled
                if (autoRotate)
                {
                    AddRotation(spawnedKong);
                    Debug.Log("[KongzSpawner] Added auto-rotation");
                }

                // REMOVED: Camera positioning

                UpdateStatus($"Spawned KONGZ VX #{tokenId}");
                Debug.Log($"[KongzSpawner] Successfully completed spawning Kong #{tokenId}");
            }
            else
            {
                UpdateStatus($"Failed to spawn Kong #{tokenId} - Invalid ID or missing data");
                Debug.LogError($"[KongzSpawner] KongBuilder returned null for ID {tokenId}");
            }
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Error spawning Kong: {e.Message}");
            Debug.LogError($"[KongzSpawner] Exception while spawning Kong {tokenId}: {e}");
            Debug.LogError($"[KongzSpawner] Stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Add simple rotation to the spawned Kong
    /// </summary>
    private void AddRotation(GameObject kong)
    {
        var rotator = kong.AddComponent<SimpleRotate>();
        rotator.rotationSpeed = rotationSpeed;
    }

    /// <summary>
    /// Position camera to properly view the Kong
    /// </summary>
    private void PositionCameraForKong(GameObject kong)
    {
        Debug.Log("[KongzSpawner] Positioning camera for Kong");

        // Find the Kong's bounds
        Renderer[] renderers = kong.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            Debug.Log($"[KongzSpawner] Kong bounds: center={bounds.center}, size={bounds.size}");

            // Position camera to view the Kong
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                float distance = bounds.size.magnitude * 1.5f;
                Vector3 camPos = bounds.center + (Vector3.back * distance) + (Vector3.up * bounds.size.y * 0.2f);
                mainCam.transform.position = camPos;
                mainCam.transform.LookAt(bounds.center);

                Debug.Log($"[KongzSpawner] Camera positioned at: {camPos}");
            }
            else
            {
                Debug.LogWarning("[KongzSpawner] Main camera not found!");
            }
        }
        else
        {
            Debug.LogWarning("[KongzSpawner] No renderers found on Kong!");
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[KongzSpawner] Status: {message}");
    }
}

/// <summary>
/// Simple rotation component
/// </summary>
public class SimpleRotate : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}