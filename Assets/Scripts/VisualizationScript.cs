using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class VisualizationScript : MonoBehaviour
{
    BarcodeBehaviour mBarcodeBehaviour;

    public TMP_Text apiSensorText;
    public TMP_InputField remarksInputField;

    public Canvas scanPrompt;
    public Canvas interactionCanvas;
    public Canvas currentCanvas;
    public Canvas fullScreenCanvas;

    public Button pastDataBtn;
    public Button pastDataAsTextBtn;
    public Button pastDataAsImgBtn;
    public Button currentDataBtn;
    public Button getRemarksBtn;
    public Button sendRemarkBtn;
    public Button fullScreenBtn;
    public Button closeScreenBtn;
    
    public RawImage _rawImageReceiver;

    private string SensorName = "";
    private readonly string staticApiUrlSensor = "http://127.0.0.1:5000/api/sensors/";
    private readonly string staticApiUrlImage = "http://127.0.0.1:5000/api/image";
    void Start()
    {
        remarksInputField.gameObject.SetActive(false);
        sendRemarkBtn.gameObject.SetActive(false);
        pastDataAsTextBtn.gameObject.SetActive(false);
        pastDataAsImgBtn.gameObject.SetActive(false);
        _rawImageReceiver.gameObject.SetActive(false);
        interactionCanvas.enabled = false;
        fullScreenCanvas.enabled = false;
        apiSensorText.text = "";
        
        mBarcodeBehaviour = GetComponent<BarcodeBehaviour>();
        if (mBarcodeBehaviour != null && mBarcodeBehaviour.InstanceData != null)
        {
            
            SensorName = mBarcodeBehaviour.InstanceData.Text;
            StartCoroutine(GetData(staticApiUrlSensor + SensorName + "/latest"));

            currentDataBtn.onClick.AddListener(delegate { StartCoroutine(GetData(staticApiUrlSensor + SensorName + "/latest")); });

            pastDataBtn.onClick.AddListener(delegate { StartCoroutine(PastDataGraph(staticApiUrlImage)); });
            pastDataAsImgBtn.onClick.AddListener(delegate { StartCoroutine(PastDataGraph(staticApiUrlImage)); });
            pastDataAsTextBtn.onClick.AddListener(delegate { StartCoroutine(PastDataText(staticApiUrlSensor + SensorName + "/past")); });

            sendRemarkBtn.onClick.AddListener(delegate { StartCoroutine(SendNewRemark(staticApiUrlSensor + SensorName + "/remarks")); });
            getRemarksBtn.onClick.AddListener(delegate { StartCoroutine(GetAllRemark(staticApiUrlSensor + SensorName + "/remarks")); });

            fullScreenBtn.onClick.AddListener(delegate { OpenFullScreen(SensorName); });
            closeScreenBtn.onClick.AddListener(delegate { CloseFullScreen(); });
        }

    }

    void Update()
    {
        mBarcodeBehaviour = GetComponent<BarcodeBehaviour>();
        if (mBarcodeBehaviour != null && mBarcodeBehaviour.InstanceData != null)
        {
            scanPrompt.enabled = false;
            interactionCanvas.enabled = true;
        }
        else
        {
            scanPrompt.enabled = true;
            interactionCanvas.enabled = false;
        }

        if(remarksInputField.text == "")
        {
            sendRemarkBtn.interactable = false;
        }
        else
        {
            sendRemarkBtn.interactable = true;
        }

        if (fullScreenCanvas.enabled == true)
        {
            scanPrompt.enabled = false;
            interactionCanvas.enabled = false;
        }
    }

    //Event handler for the past current route
    IEnumerator GetData(string URL)
    {
        apiSensorText.text = "Loading..";
        pastDataAsImgBtn.gameObject.SetActive(false);
        pastDataAsTextBtn.gameObject.SetActive(false);
        apiSensorText.gameObject.SetActive(true);
        _rawImageReceiver.gameObject.SetActive(false);
        pastDataBtn.gameObject.SetActive(true);
        getRemarksBtn.gameObject.SetActive(true);
        sendRemarkBtn.gameObject.SetActive(false);
        remarksInputField.gameObject.SetActive(false);

        using UnityWebRequest request = UnityWebRequest.Get(URL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (request.error == "HTTP/1.1 404 Not Found")
            {
                apiSensorText.text = "Wrong or non existant item";
            }
            else {
                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(json);
                //yield return new WaitForSeconds(1);
                apiSensorText.text = data["sensorName"] + "\n\n" + "Temperature: " + data["value"] + "\n\nTime: " + data["createdAt"];
            }

        }
    }

    //Event handlers for the past data route
    IEnumerator PastDataText(string URL)
    {
        apiSensorText.text = "Loading..";
        apiSensorText.gameObject.SetActive(true);
        _rawImageReceiver.gameObject.SetActive(false);
        pastDataAsTextBtn.gameObject.SetActive(true);
        pastDataAsImgBtn.gameObject.SetActive(true);
        pastDataBtn.gameObject.SetActive(false);
        getRemarksBtn.gameObject.SetActive(false);
        sendRemarkBtn.gameObject.SetActive(false);

        using UnityWebRequest request = UnityWebRequest.Get(URL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (request.error == "HTTP/1.1 404 Not Found")
            {
                apiSensorText.text = "Wrong or non existant item";
            }
            else
            {

                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(json);
                
                //yield return new WaitForSeconds(1);
                apiSensorText.text = "";

                if (data.Count < 5)
                {
                    for (int i = data.Count - 1; i > 0; i--)
                    {
                        apiSensorText.text = apiSensorText.text + "Temperature: " + data[i]["value"] + "\nTime: " + data[i]["createdAt"] + "\n\n";
                    }
                }
                else
                {
                    for (int i = data.Count - 1; i > data.Count - 6; i--)
                    {
                        apiSensorText.text = apiSensorText.text + "Temperature: " + data[i]["value"] + "\nTime: " + data[i]["createdAt"] + "\n\n";
                    }
                }
            }

        }
    }

    IEnumerator PastDataGraph(string URL)
    {
        _rawImageReceiver.gameObject.SetActive(true);
        pastDataBtn.gameObject.SetActive(false);
        pastDataAsTextBtn.gameObject.SetActive(true);
        pastDataAsImgBtn.gameObject.SetActive(true);
        getRemarksBtn.gameObject.SetActive(false);
        sendRemarkBtn.gameObject.SetActive(false);
        apiSensorText.gameObject.SetActive(false);

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(URL);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            _rawImageReceiver.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    //Event handlers for the remarks route
    IEnumerator SendNewRemark(string URL)
    {
        apiSensorText.text = "Loading..";
        WWWForm form = new WWWForm();
        form.AddField("remark", remarksInputField.text);
        using UnityWebRequest request = UnityWebRequest.Post(URL, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
            string json = request.downloadHandler.text;
            SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(json);
            Debug.Log(data[0]);
            remarksInputField.text = "";
            StartCoroutine(GetAllRemark(URL));
        }
    }

    IEnumerator GetAllRemark(string URL)
    {
        apiSensorText.text = "Loading..";
        apiSensorText.gameObject.SetActive(true);
        _rawImageReceiver.gameObject.SetActive(false);
        getRemarksBtn.gameObject.SetActive(true);
        sendRemarkBtn.gameObject.SetActive(true);
        remarksInputField.gameObject.SetActive(true);
        pastDataBtn.gameObject.SetActive(false);

        using UnityWebRequest request = UnityWebRequest.Get(URL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(json);

            
            //yield return new WaitForSeconds(1);
            apiSensorText.text = "";
            if(data.Count < 5)
            {
                for (int i = data.Count - 1; i > 0; i--)
                {
                    apiSensorText.text = apiSensorText.text + "Remark: " + data[i]["remark"] + "\nTime: " + data[i]["createdAt"] + "\n\n";
                }
            }
            else
            {
                for (int i = data.Count - 1; i > data.Count - 6; i--)
                {
                    apiSensorText.text = apiSensorText.text + "Remark: " + data[i]["remark"] + "\nTime: " + data[i]["createdAt"] + "\n\n";
                }
            }
            
        }
    }

    void OpenFullScreen(string SensorName)
    {
        fullScreenCanvas.enabled = true;
        Debug.Log(SensorName);
    }

    public void CloseFullScreen()
    {
        Debug.Log("Close");
        fullScreenCanvas.enabled = false;
    }
}