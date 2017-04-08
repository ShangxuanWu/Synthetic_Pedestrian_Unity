using System.IO;
using UnityEngine;

public static class Utils
{

    public enum states { NORMAL, SEG};
    public enum distance { NEAR, MIDDLE, FAR};

    // ********************************************* please just change this **********
    public static distance render_distance = distance.NEAR; // NEAR, MIDDLE, FAR
    // ********************************************************************************
    public static int w = 736;
    public static int h = 368;

    public static string background_folder = "./background/";
    public static string result_folder = "./result/";
    public static string result_image_folder = result_folder + "image/";
    public static string result_segmentation_folder = result_folder + "segmentation/";
    public static string result_joint_folder = result_folder + "joint/";
    public static string model_folder = "Assets/model/";

    public static string[] background_fns;
    public static string[] model_fns;
    public static int total_image_num = 11437; // total image numbers that you want to generate
    public static int now_image_num;
    public static int total_background_num;
    public static int total_model_num;
    public static states now_mode;  // 0 is for normal rendering, 1 is for segmentation

    public static int num_pedestrian_per_image = 5;
    public static int[] now_chosen_pedestrians;
    public static string saving_format = "00000";

    // just helpers
    private static FileInfo[] background_files;
    private static FileInfo[] model_files;

    private static System.Random random = new System.Random();

    static Utils()
    {
        Debug.Log("Utils() start !!!!!");

        now_image_num = 0;
        now_mode = states.NORMAL;

        // clean the result folder
        System.IO.DirectoryInfo di = new DirectoryInfo(result_image_folder);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        di = new DirectoryInfo(result_segmentation_folder);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        di = new DirectoryInfo(result_joint_folder);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        Debug.Log("Cleaned all the previous files!");

        // get background filenames and number

        DirectoryInfo d = new DirectoryInfo(background_folder);//Assuming Test is your Folder
        background_files = d.GetFiles("*.png"); //Getting Text files

        background_fns = new string[background_files.Length];

        for (int i = 0; i < background_files.Length; i++)
        {
            background_fns[i] = background_files[i].FullName;
        }

        total_background_num = background_fns.Length;
        Debug.Log("# background = "+total_background_num);

        // get model filenames and folder
        d = new DirectoryInfo(model_folder);//Assuming Test is your Folder
        model_files = d.GetFiles("*.fbx"); //Getting Text files

        model_fns = new string[model_files.Length];

        for (int i = 0; i < model_files.Length; i++)
        {
            model_fns[i] = model_folder +  model_files[i].Name;
        }
        total_model_num = model_fns.Length;
        Debug.Log("# models = " + total_model_num);
    }

    static public void update()
    {
        if(now_mode == states.NORMAL)
        {
            now_mode = states.SEG;
        }
        else
        {
            now_image_num += 1;
            now_mode = states.NORMAL;
        }
    }

    static public bool shouldQuit()
    {
        return now_image_num > total_image_num;
    }

    static public void generateRandomNumbers()
    {
        now_chosen_pedestrians = new int[num_pedestrian_per_image];
        for(int i = 0; i < num_pedestrian_per_image; i++)
        {
            now_chosen_pedestrians[i] = random.Next(0, total_model_num);
        }
    }
}