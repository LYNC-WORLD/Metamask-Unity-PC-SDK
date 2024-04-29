using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MetamaskExample : MonoBehaviour
{
    public Button login, logout, sendTokenButton;
    public TMP_Text MetamaskWalletAddress;
    public Transform transactionResultsParent;
    public GameObject transactionResultHolder;
    [Space]
    [Header("Transaction details")]
    public Transaction SendTokenTrx;

    private void OnEnable()
    {
        LyncManager.onLyncReady += LyncReady;
    }

    private void Awake()
    {
        login.interactable = false;
        logout.interactable = false;
        sendTokenButton.interactable = false;
        Application.targetFrameRate = 30;
    }

    private void LyncReady(LyncManager Lync)
    {
        login.interactable = true;

        login.onClick.AddListener(() =>
        {
            Lync.WalletAuth.ConnectWallet((wallet) =>
            {
                OnWalletConnected(wallet);
            });
        });

        logout.onClick.AddListener(() =>
        {
            Lync.WalletAuth.Logout();
            login.interactable = true;
            logout.interactable = false;
            sendTokenButton.interactable = false;
        });

        sendTokenButton.onClick.AddListener(async () =>
        {
            TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(SendTokenTrx);
            if (txData.success)
                SuccessfullTransaction(txData.hash);
            else
                ErrorTransaction(txData.error);

            sendTokenButton.interactable = true;
        });

    }

    private void OnWalletConnected(AuthBase _authBase)
    {
        MetamaskWalletAddress.text = "Wallet address = " + _authBase.PublicAddress;

        login.interactable = false;
        logout.interactable = true;
        sendTokenButton.interactable = true;
    }

    private void SuccessfullTransaction(string hash)
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);

        go.transform.GetComponentInChildren<TMP_Text>().text = "<color=\"green\">Success - Click to check block explorer<color=\"green\">";
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener((eventData) => { Application.OpenURL("https://sepolia.etherscan.io/tx/" + hash); });
        trigger.triggers.Add(entry);
    }

    private void ErrorTransaction(string error, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);
        go.transform.GetComponentInChildren<TMP_Text>().text = txnTitle + " <color=\"red\">TXN ERROR:</color=\"red\"> " + error;
    }
}
