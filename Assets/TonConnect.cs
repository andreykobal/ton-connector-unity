using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;



public class TonConnect : MonoBehaviour
{
    private const string walletUniversalUrl = "https://app.tonkeeper.com/ton-connect";
    private const string walletUniversalUrl2 = "https://app.tonsafe.net/ton-connect";
    private const string qrCodeApiUrl = "https://api.qrserver.com/v1/create-qr-code";

    private const string manifestUrl = "https://metaverse-course.ams3.cdn.digitaloceanspaces.com/tonconnect-manifest.json";

    public RawImage image1; 
    public RawImage image2;

    public static readonly UTF8Encoding Encoder = new UTF8Encoding();
    public static readonly byte[] Buffer = new byte[2048];
    public static readonly HttpClient HttpClient = new HttpClient();

    public TransactionSender sender;
    public string SenderId; 
    public Button sendTransactionButton;


    private void Start()
    {
        sendTransactionButton.onClick.AddListener(OnSendTransactionButtonClick);

        var connectRequest = new ConnectRequest
        {
            manifestUrl = manifestUrl,
            items = new ConnectItem[]
            {
                new ConnectItem
                {
                    type = "string",
                    name = "message",
                    value = "Hello, Tonkeeper!"
                }
            }
        };

        var connectRequestJson = JsonConvert.SerializeObject(connectRequest);

        Debug.Log(connectRequestJson);

        var id = GenerateRandomId();
        SenderId = id;
        Debug.LogError("ID: " + id);

        //Tonkeeper
        var url = $"{walletUniversalUrl}?v=2&id={id}&r={UnityWebRequest.EscapeURL(connectRequestJson)}&ret=none";
        //Tonsafe
        var url2 = $"{walletUniversalUrl2}?v=2&id={id}&r={UnityWebRequest.EscapeURL(connectRequestJson)}&ret=none";

        Debug.Log(url);
        Debug.Log(url2);

        // Display QR code
        StartCoroutine(LoadQrCode(url, image1));
        StartCoroutine(LoadQrCode(url2, image2));
        OpenSSEStream($"https://bridge.tonapi.io/bridge/events?client_id={id}");

    }

    void OnSendTransactionButtonClick() {
            Debug.Log("Send transaction button clicked!");
            sender.SendTransaction(SenderId);

    }

    private string GenerateRandomId()
    {
        var buffer = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(buffer);
        }
        return BitConverter.ToString(buffer).Replace("-", "");
    }

    private IEnumerator LoadQrCode(string url, RawImage image)
    {
        var request = UnityWebRequestTexture.GetTexture($"{qrCodeApiUrl}/?size=512&data={UnityWebRequest.EscapeURL(url)}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            // GetComponent<RawImage>().texture = texture;
            // // Display the texture on a UI element or in 3D space, etc.
            var rawImage = image;
            rawImage.texture = texture;

        }
        else
        {
            Debug.LogError(request.error);
        }
    }

        public static async Task OpenSSEStream(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "text/event-stream");
        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        using var stream = await response.Content.ReadAsStreamAsync();

        while (true)
        {
            var len = await stream.ReadAsync(Buffer, 0, Buffer.Length);
            if (len > 0)
            {
                var text = Encoder.GetString(Buffer, 0, len);
                Debug.Log(text);
            }
        }
    }
}


public class ConnectRequest {
    public string manifestUrl { get; set; }
    public ConnectItem[] items { get; set; }
}

public class ConnectItem {
    public string type { get; set; }
    public string name { get; set; }
    public string value { get; set; }
}