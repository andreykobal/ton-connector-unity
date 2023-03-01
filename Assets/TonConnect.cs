using System;
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

    private void Start()
    {
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

        var id = GenerateRandomId();

        //Tonkeeper
        var url = $"{walletUniversalUrl}?v=2&id={id}&r={UnityWebRequest.EscapeURL(connectRequestJson)}&ret=none";
        //Tonsafe
        var url2 = $"{walletUniversalUrl2}?v=2&id={id}&r={UnityWebRequest.EscapeURL(connectRequestJson)}&ret=none";

        Debug.Log(url);
        Debug.Log(url2);

        // Display QR code
        StartCoroutine(LoadQrCode(url, image1));
        StartCoroutine(LoadQrCode(url2, image2));
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