using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class TonKeeperController : MonoBehaviour
{
    // Wallet details
    private const string walletName = "Tonkeeper";
    private const string universalUrl = "https://app.tonkeeper.com/ton-connect";
    private const string bridgeUrl = "https://bridge.tonapi.io/bridge";
    private const string bridgeKey = "tonkeeper";

    // QR code generation variables
    private const int qrCodeSize = 256;
    private Texture2D qrCodeTexture;

    // Connection variables
    private int connectEventCounter = 0;
    private bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        // Prompt player to connect Tonkeeper wallet
        StartCoroutine(DisplayQrCode());
    }

    // Generate and display QR code
    IEnumerator DisplayQrCode()
    {
        // Generate universal link
        var universalLink = $"{universalUrl}?response_type=connect&bridge=js&key={bridgeKey}";

        // Generate QR code
        var url = $"https://api.qrserver.com/v1/create-qr-code/?size={qrCodeSize}x{qrCodeSize}&data={universalLink}";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            qrCodeTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
        else
        {
            Debug.LogError(request.error);
        }

        // Display QR code
        if (qrCodeTexture != null)
        {
            var qrCodeSprite = Sprite.Create(qrCodeTexture, new Rect(0, 0, qrCodeTexture.width, qrCodeTexture.height), Vector2.zero);
            var qrCodeObject = new GameObject("QRCode");
            var qrCodeRenderer = qrCodeObject.AddComponent<SpriteRenderer>();
            qrCodeRenderer.sprite = qrCodeSprite;
        }
    }

    // Handle Connect events from the Tonkeeper wallet
    public void OnConnectEvent(string eventData)
    {
        var connectEvent = JsonConvert.DeserializeObject<ConnectEventSuccess>(eventData);
        if (connectEvent.payload.device.name == walletName && connectEvent.payload.items.Length > 0)
        {
            isConnected = true;
            Debug.Log("Tonkeeper wallet connected");
        }
    }

    // Disconnect Tonkeeper wallet
    public void DisconnectTonkeeper()
    {
        if (isConnected)
        {
            var disconnectRequest = new DisconnectRequest
            {
                method = "disconnect",
                @params = new object[] { },
                id = connectEventCounter++
            };
            var disconnectRequestJson = JsonConvert.SerializeObject(disconnectRequest);

            StartCoroutine(PostRequest(bridgeUrl, disconnectRequestJson, (responseJson) =>
            {
                var disconnectResponse = JsonConvert.DeserializeObject<DisconnectResponse>(responseJson);
                if (disconnectResponse.error != null)
                {
                    Debug.LogError($"Error disconnecting Tonkeeper wallet: {disconnectResponse.error.message}");
                }
                else
                {
                    isConnected = false;
                    Debug.Log("Tonkeeper wallet disconnected");
                }
            }));
        }
    }

    // Send POST request
    IEnumerator PostRequest(string url, string json, System.Action<string> callback)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding(true).GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandlerBuffer)new DownloadHandlerBuffer();
request.SetRequestHeader("Content-Type", "application/json");
    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        callback(request.downloadHandler.text);
    }
    else
    {
        Debug.LogError(request.error);
    }
}
}

// Response classes
public class ConnectEventSuccess
{
public string @event;
public int id;
public ConnectPayload payload;
}

public class ConnectPayload
{
public ConnectItemReply[] items;
public DeviceInfo device;
}

public class ConnectItemReply
{
public string name;
public string version;
public string icon;
public string description;
public string url;
}

public class DeviceInfo
{
public string name;
public string platform;
public string version;
}

public class DisconnectRequest
{
public string method;
public object[] @params;
public int id;
}

public class DisconnectResponse
{
public DisconnectResponseError error;
}

public class DisconnectResponseError
{
public int code;
public string message;
}