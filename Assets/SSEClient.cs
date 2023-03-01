using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SSEClient : MonoBehaviour
{
    public static readonly UTF8Encoding Encoder = new UTF8Encoding();
    public static readonly byte[] Buffer = new byte[2048];
    public static readonly HttpClient HttpClient = new HttpClient();

    // Start is called before the first frame update
    void Start()
    {
        OpenSSEStream("https://bridge.tonapi.io/bridge/events?client_id=1E876B4F501DCDC8270CE473FBEFC58598719F4C2CF26DD02C2E7B8BE40CDB72");
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
