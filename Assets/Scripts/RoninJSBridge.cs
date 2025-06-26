using UnityEngine;
using System.Runtime.InteropServices;

public class RoninJSBridge : MonoBehaviour
{
    private static RoninJSBridge _instance;
    public static RoninJSBridge Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("RoninJSBridge");
                _instance = go.AddComponent<RoninJSBridge>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [DllImport("__Internal")]
    private static extern bool DetectRoninWallet();

    [DllImport("__Internal")]
    private static extern void ConnectRoninWallet();

    [DllImport("__Internal")]
    private static extern string GetRoninAddress();

    private RoninTestManager testManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectWallet()
    {
        // Find the test manager if we don't have it
        if (testManager == null)
            testManager = FindObjectOfType<RoninTestManager>();

        #if UNITY_WEBGL && !UNITY_EDITOR
        if (DetectRoninWallet())
        {
            Debug.Log("Ronin wallet detected, connecting...");
            ConnectRoninWallet();
        }
        else
        {
            Debug.Log("No Ronin wallet detected");
            testManager?.OnWalletError("Ronin wallet extension not found. Please install it.");
        }
        #else
        Debug.Log("Not in WebGL build");
        #endif
    }

    // Called from JavaScript
    public void OnWalletConnectionSuccess(string address)
    {
        Debug.Log($"Wallet connected successfully: {address}");
        testManager?.OnWalletConnected(address);
    }

    // Called from JavaScript
    public void OnWalletConnectionFailed(string error)
    {
        Debug.Log($"Wallet connection failed: {error}");
        testManager?.OnWalletError(error);
    }

    // Called from JavaScript
    public void OnWalletConnectionRejected()
    {
        Debug.Log("User rejected the connection");
        testManager?.OnWalletError("Connection rejected by user");
    }
}