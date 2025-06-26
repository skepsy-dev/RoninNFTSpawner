using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoninTestManager : MonoBehaviour
{
    [Header("UI References")]
    public Button connectButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI addressText;

     void Awake()
    {
        // Preload the shader so KongBuilder can find it
        Shader.Find("Legacy Shaders/Diffuse");
    }

    void Start()
    {
        connectButton.onClick.AddListener(ConnectWallet);
        UpdateStatus("Ready to connect");
    }

    public void ConnectWallet()
    {
        UpdateStatus("Connecting...");

#if UNITY_WEBGL && !UNITY_EDITOR
        // Call JavaScript bridge
        RoninJSBridge.Instance.ConnectWallet();
#else
        UpdateStatus("WebGL build required");
#endif
    }

    public void OnWalletConnected(string address)
    {
        UpdateStatus("Connected!");
        addressText.text = $"Address: {address}";

        FindObjectOfType<KongzNFTChecker>()?.SetWalletAddress(address);
    }

    public void OnWalletError(string error)
    {
        UpdateStatus($"Error: {error}");
    }

    public void UpdateStatus(string message)
    {
        statusText.text = message;
        Debug.Log($"[Ronin] {message}");
    }
}