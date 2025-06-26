using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class MoralisNFTResponse
{
    public string status;
    public int page;
    public int page_size;
    public string cursor;
    public MoralisNFTResult[] result;
}

[System.Serializable]
public class MoralisNFTResult
{
    public string token_id;
    public string token_address;
    public string contract_type;
    public string owner_of;
    public string name;
    public string symbol;
    public string amount;
    public NormalizedMetadata normalized_metadata;
}

[System.Serializable]
public class NormalizedMetadata
{
    public string name;
    public string description;
    public string image;
    public string animation_url;
}

public class KongzNFTChecker : MonoBehaviour
{
    [Header("UI References")]
    public Button checkNFTsButton;
    public TextMeshProUGUI statusText;
    public RawImage nftImageDisplay; // Optional: to show NFT image
    
    [Header("Wallet Address")]
    public string walletAddress = "";
    
    // Moralis API Configuration - UPDATED API KEY
    private const string MORALIS_API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJub25jZSI6IjViYjVhNWU2LTY5M2QtNDYxYS04N2U4LTAzNmRjNmI5MjdmOSIsIm9yZ0lkIjoiMzIwNzQyIiwidXNlcklkIjoiMzI5NzQ4IiwidHlwZUlkIjoiZjdkZWM5OGUtNGYwNy00MmI2LTk2ZTItMDdjYzk5NTVhM2IwIiwidHlwZSI6IlBST0pFQ1QiLCJpYXQiOjE3NDgzMDk4MjIsImV4cCI6NDkwNDA2OTgyMn0.wIFS_YlAn_Ro5vbwY5TRrQaw7Gq_yyqeDXZgjVulEnU";
    private const string MORALIS_API_URL = "https://deep-index.moralis.io/api/v2.2";
    private const string KONGZ_VX_CONTRACT = "0x241a81fc0d6692707dad2b5025a3a7cf2cf25acf";
    
    [Header("3D Spawner Reference")]
    public KongzNFTSpawner kongSpawner; // Optional: reference to 3D spawner
    
    private List<MoralisNFTResult> currentNFTs = new List<MoralisNFTResult>();
    
    void Start()
    {
        if (checkNFTsButton != null)
        {
            checkNFTsButton.onClick.AddListener(CheckNFTs);
        }
    }
    
    public void SetWalletAddress(string address)
    {
        walletAddress = address;
        Debug.Log($"[KongzChecker] Wallet address set to: {address}");
    }
    
    public void CheckNFTs()
    {
        if (string.IsNullOrEmpty(walletAddress))
        {
            UpdateStatus("No wallet address set");
            return;
        }
        
        UpdateStatus("Fetching KONGZ VX NFTs...");
        StartCoroutine(FetchNFTsCoroutine());
    }
    
    private IEnumerator FetchNFTsCoroutine()
    {
        // Build the API endpoint URL
        string endpoint = $"{MORALIS_API_URL}/{walletAddress}/nft";
        string queryParams = $"?chain=ronin&format=decimal&token_addresses={KONGZ_VX_CONTRACT}&normalizeMetadata=true&media_items=true";
        string fullUrl = endpoint + queryParams;
        
        Debug.Log($"[KongzChecker] Fetching NFTs from: {fullUrl}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            // Use the correct header format
            request.SetRequestHeader("x-api-key", MORALIS_API_KEY);
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[KongzChecker] Response: {jsonResponse}");
                    
                    MoralisNFTResponse response = JsonUtility.FromJson<MoralisNFTResponse>(jsonResponse);
                    
                    currentNFTs.Clear();
                    List<string> tokenIds = new List<string>();
                    
                    if (response.result != null)
                    {
                        foreach (var nft in response.result)
                        {
                            if (!string.IsNullOrEmpty(nft.token_id))
                            {
                                tokenIds.Add(nft.token_id);
                                currentNFTs.Add(nft);
                            }
                        }
                    }
                    
                    // Display results
                    DisplayResults(tokenIds);
                    
                    // Send token IDs to spawner if connected
                    if (kongSpawner != null)
                    {
                        kongSpawner.SetOwnedTokenIds(tokenIds);
                    }
                    
                    // Load first NFT image if available
                    if (currentNFTs.Count > 0 && nftImageDisplay != null)
                    {
                        LoadNFTImage(0);
                    }
                }
                catch (System.Exception e)
                {
                    UpdateStatus($"Parse error: {e.Message}");
                    Debug.LogError($"[KongzChecker] Parse error: {e.Message}");
                }
            }
            else
            {
                UpdateStatus($"Request failed: {request.error}");
                Debug.LogError($"[KongzChecker] Request failed: {request.error}");
            }
        }
    }
    
    private void DisplayResults(List<string> tokenIds)
    {
        if (tokenIds.Count == 0)
        {
            UpdateStatus("No KONGZ VX NFTs found");
        }
        else
        {
            string result = $"Found {tokenIds.Count} KONGZ VX NFTs:\n";
            
            // Display all token IDs with names
            for (int i = 0; i < tokenIds.Count && i < 10; i++)
            {
                var nft = currentNFTs[i];
                string name = nft.normalized_metadata?.name ?? $"KONGZ VX #{tokenIds[i]}";
                result += $"{name}\n";
            }
            
            if (tokenIds.Count > 10)
            {
                result += $"... and {tokenIds.Count - 10} more";
            }
            
            UpdateStatus(result);
        }
        
        // Log all token IDs to console
        Debug.Log($"[KongzChecker] Total KONGZ VX NFTs: {tokenIds.Count}");
        foreach (var nft in currentNFTs)
        {
            Debug.Log($"[KongzChecker] Token ID: {nft.token_id} - Name: {nft.normalized_metadata?.name}");
            if (nft.normalized_metadata != null && !string.IsNullOrEmpty(nft.normalized_metadata.image))
            {
                Debug.Log($"[KongzChecker] Image URL: {nft.normalized_metadata.image}");
            }
        }
    }
    
    private void LoadNFTImage(int index)
    {
        if (index >= 0 && index < currentNFTs.Count)
        {
            var nft = currentNFTs[index];
            if (nft.normalized_metadata != null && !string.IsNullOrEmpty(nft.normalized_metadata.image))
            {
                StartCoroutine(LoadImageFromURL(nft.normalized_metadata.image));
            }
        }
    }
    
    private IEnumerator LoadImageFromURL(string imageUrl)
    {
        Debug.Log($"[KongzChecker] Loading image from: {imageUrl}");
        
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (nftImageDisplay != null)
                {
                    nftImageDisplay.texture = texture;
                    nftImageDisplay.gameObject.SetActive(true);
                }
                Debug.Log($"[KongzChecker] Image loaded successfully");
            }
            else
            {
                Debug.LogError($"[KongzChecker] Failed to load image: {request.error}");
            }
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[KongzChecker] Status: {message}");
    }
}