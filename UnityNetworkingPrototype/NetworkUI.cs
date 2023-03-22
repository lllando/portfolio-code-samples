using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;
public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button serverButton; // server only. no client
    [SerializeField] private Button hostButton; // both client and server
    [SerializeField] private Button clientButton; // client only. no server

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text joinCodeText;

    public string JoinCode { get; private set; }

    private void Awake()
    {
        hostButton.gameObject.SetActive(true);
        clientButton.gameObject.SetActive(true);
        inputField.gameObject.SetActive(true);

        serverButton.onClick.AddListener(() => StartServer());
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    private async void StartHost()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);

        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(3);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = JoinCode;
        }
        catch
        {
            Debug.LogError("Relay get join code request failed");
            throw;
        }

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
    }

    private async void StartClient()
    {
        JoinAllocation allocation;

        try
        {
            Debug.Log(inputField.text);
            allocation = await RelayService.Instance.JoinAllocationAsync(inputField.text);
        }
        catch
        {
            Debug.LogError("Relay get join code request failed");
            throw;
        }

        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        var relayServerSata = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerSata);
        NetworkManager.Singleton.StartClient();
    }
}
