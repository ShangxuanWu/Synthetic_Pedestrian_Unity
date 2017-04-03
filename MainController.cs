using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    private System.Random random; 

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 120;
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("MainController start()");
        random = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        if (Utils.shouldQuit())
        {
            return;
        }  

        string fname;

        switch (Utils.now_mode)
        {
            case Utils.states.NORMAL:
                ChangeBackgroundImage();
                PedestrianLoader model_loader = gameObject.GetComponent<PedestrianLoader>();

                // randomly set light and shadow position
                Light lightComp = GameObject.FindWithTag("Light_Brightness").GetComponent<Light>();
                lightComp.intensity = (float) (0.8 + 1.0 * random.NextDouble());
                Vector3 lightRotation = new Vector3(50, random.Next(-20, 20), 0);
                lightComp.transform.eulerAngles = lightRotation;

                // This is where images got saved
                fname = Utils.result_image_folder + Utils.now_image_num.ToString(Utils.saving_format) + ".png";
                Debug.Log(fname);   
                Application.CaptureScreenshot(fname);

                // save joints
                model_loader.SaveJointCoordinate();
                break;

            case Utils.states.SEG:
                model_loader = gameObject.GetComponent<PedestrianLoader>();
                ChangeBackgroundImageToBlack();
                fname = Utils.result_segmentation_folder + Utils.now_image_num.ToString(Utils.saving_format) + ".png";
                Debug.Log(fname);
                Application.CaptureScreenshot(fname);
                break;

            default:
                break;
        }

        // Make blur should go here
        Utils.update();
    }
        
    void ChangeBackgroundImage()
    {
        Debug.Log("MainController.ChangeBackgroundImage()");
        GameObject bgObj = GameObject.Find("BackgroundPlane");

        // set background
        var fn = Utils.background_fns[Utils.now_image_num % Utils.total_background_num];
        Debug.Log(String.Format("<color=blue>change background to {0}</color>", fn));

        var bytes = System.IO.File.ReadAllBytes(fn);
        var image = new Texture2D(1, 2);

        image.LoadImage(bytes);

        Texture tex = bgObj.GetComponent<Renderer>().material.mainTexture;
        if (tex != null)
            UnityEngine.Object.Destroy(tex);

        bgObj.GetComponent<Renderer>().material.mainTexture = image;

    }

    void ChangeBackgroundImageToBlack()
    {
        Debug.Log("MainController.ChangeBackgroundImageToBlack()");
        // destroy background image
        GameObject bgObj = GameObject.Find("BackgroundPlane");
        Destroy(bgObj.GetComponent<Renderer>().material.mainTexture);
        Debug.Log("Background Plane Texture Destroyed !");
    }
    /* 
    void MakeBlur(int idx, int w, float sigma)
    {
        Debug.Log("Blurring");

        var filepath = Utils.result_image_folder + idx.ToString(Utils.saving_format) + ".png";
        var bytes = System.IO.File.ReadAllBytes(filepath);
        var image = new Texture2D(1, 1);
        var image_blur = new Texture2D(1, 1);
        image.LoadImage(bytes);
        image_blur.LoadImage(bytes);



        Texture2D[] segmentation = new Texture2D[5];

        for (int i = 0; i < 5; i++)
        {
            filepath = Application.dataPath + "/../result/segmentation/" + idx.ToString("0000000") + "_" + i.ToString("00") + ".png";

            var bytes_segmentation = System.IO.File.ReadAllBytes(filepath);
            segmentation[i] = new Texture2D(1, 1);
            segmentation[i].LoadImage(bytes_segmentation);

        }


        int width = 368;
        int height = 368;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                bool bSeg = false;
                for (int i = 0; i < 5; i++)
                {
                    Color color = segmentation[i].GetPixel(x, y);
                    if (color.r == 1.0f && color.g == 1.0f && color.b == 1.0f)
                        bSeg = true;
                }

                Color mean_c = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                if (bSeg)
                {
                    // blur

                    float sum_weight = 0.0f;
                    for (int wx = -w; wx <= w; wx++)
                    {
                        for (int wy = -w; wy <= w; wy++)
                        {
                            int new_x = x + wx;
                            int new_y = y + wy;

                            float weight = 1.0f / Mathf.Sqrt(2.0f * Mathf.PI * sigma * sigma) * Mathf.Exp(-(wx * wx + wy * wy) / (2.0f * (sigma * sigma)));
                            Color c = image.GetPixel(new_x, new_y);
                            mean_c += c * weight;
                            sum_weight += weight;
                        }
                    }

                    mean_c /= sum_weight;
                    //Color image_c = image.GetPixel (x, y);
                    //mean_c.a = image_c.a;
                    image_blur.SetPixel(x, y, mean_c);
                }


            }
        }

        byte[] bytes_png = image_blur.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../result/image/" + idx.ToString("0000000") + "_blur.png", bytes_png);

        UnityEngine.Object.Destroy(image);
        UnityEngine.Object.Destroy(image_blur);

        for (int i = 0; i < 5; i++)
        {
            filepath = Application.dataPath + "/../result/segmentation/" + idx.ToString("0000000") + "_" + i.ToString("00") + ".png";

            UnityEngine.Object.Destroy(segmentation[i]);

        }

    }*/
}