using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraUpdate : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;//相机渲染的UI
    public WebCamTexture webcamTexture;
 
    void Start()
    {
        //Application.targetFrameRate = 30;
        //webcamTexture = new WebCamTexture(640, 640, 10);
        //rawImage.texture = webcamTexture;
        //webcamTexture.Play();
        StartCoroutine("OpenCamera");
    }
 
    /// <summary>
    /// 打开摄像机
    /// </summary>
    public IEnumerator OpenCamera()
    {
        // 申请摄像头权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
            }
 
            //打开渲染图
            if (rawImage != null)
            {
                rawImage.gameObject.SetActive(true);
            }

            // 监控第一次授权，是否获得到设备（因为很可能第一次授权了，但是获得不到设备，这里这样避免）
            // 多次 都没有获得设备，可能就是真没有摄像头，结束获取 camera
            int i = 0;
            while (WebCamTexture.devices.Length <= 0 && 1 < 300)
            {
                yield return new WaitForEndOfFrame();
                i++;
            }
            WebCamDevice[] devices = WebCamTexture.devices;//获取可用设备
            if (WebCamTexture.devices.Length <= 0)
            {
                Debug.LogError("没有摄像头设备，请检查");
            }
            else
            {
                string devicename = devices[0].name;

                //Resolution[] resolutions = devices[0].availableResolutions;
                //if (resolutions != null)
                //{
                    //int best = 0;
                    //for (int j = 0; j < resolutions.Length; j++)
                    //{
                        //Debug.Log("Resoltions for Camera " + $" Width: {resolutions[j].width}, Height: {resolutions[j].height}, Refresh Rate: {resolutions[j].refreshRateRatio}");
                        //if (resolutions[j].width < resolutions[best].width && resolutions[j].width <= 640)
                        //{
                            //best = j;
                        //}
                    //}
                    //webcamTexture = new WebCamTexture(devicename, resolutions[best].width, resolutions[best].height);
                    //Debug.Log(resolutions[best].width + "x" + resolutions[best].height);
                    //rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(320, resolutions[best].height * 320 / resolutions[best].width);  
                //}
                //else
                //{
                    webcamTexture = new WebCamTexture(devicename, 720, 1080, 30)
                    {
                        wrapMode = TextureWrapMode.Mirror
                    };
                //}



                // 渲染到 UI 或者 游戏物体上
                if (rawImage != null)
                {
                    rawImage.texture = webcamTexture;
                }
                webcamTexture.Play();
            }
        }
        else {
            Debug.LogError("未获得读取摄像头权限");
        }

    }

    private void OnApplicationPause(bool pause)
    {
        // 应用暂停的时候暂停camera，继续的时候继续使用
        if (webcamTexture !=null)
        {
            if (pause)
            {
                webcamTexture.Pause();
            }
            else
            {
                webcamTexture.Play();
            }
        }
        
    }
    
    private void OnDestroy()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}
