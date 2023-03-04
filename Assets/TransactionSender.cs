using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

public class TransactionSender : MonoBehaviour
{
    // Set the sender's Client ID
    //public string senderClientId = "";

    // Reference to the input field that contains the recipient's Client ID
    public UnityEngine.UI.InputField recipientClientIdInputField;

    // Set the transaction parameters
    public string to = "EQC-DCwSioXSekCytL9Cm3kopBZiQBPUKEgjiLfhC6qx7gRl";
    public string value = "1000000000000000000";
    public string gas = "200000";
    public string gasPrice = "1000000000";
    public string nonce = "0";

    public void SendTransaction(string senderClientId)
    {
        // Get the recipient's Client ID from the input field
        string recipientClientId = recipientClientIdInputField.text;

        // Encode the message as a JSON object
        string json = "{\n" +
            "  \"type\": \"Transaction\",\n" +
            "  \"data\": {\n" +
            "    \"to\": \"" + to + "\",\n" +
            "    \"value\": \"" + value + "\",\n" +
            "    \"gas\": \"" + gas + "\",\n" +
            "    \"gasPrice\": \"" + gasPrice + "\",\n" +
            "    \"nonce\": \"" + nonce + "\"\n" +
            "  }\n" +
            "}";

        // Encode the message as base64
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(json);
        string encodedMessage = Convert.ToBase64String(bytesToEncode);

        // Send the POST request to the Bridge server
        StartCoroutine(SendPostRequest(encodedMessage, senderClientId, recipientClientId));
    }

    IEnumerator SendPostRequest(string message, string senderId, string recipientId)
    {
        // Set the URL of the Bridge server
        string url = "https://bridge.tonapi.io/bridge/message";

        // Build the request parameters
        string parameters = "?client_id=" + senderId + "&to=" + recipientId + "&ttl=300&topic=sendTransaction";

        // Build the request headers
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");

        // Build the request body
        byte[] bodyRaw = Encoding.UTF8.GetBytes(message);

        // Send the POST request
        UnityWebRequest request = UnityWebRequest.Post(url + parameters, "");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.timeout = 10;
        foreach (KeyValuePair<string, string> header in headers)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("Transaction sent successfully!");
        }
    }
}
