using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class PedestrianLoader : MonoBehaviour
{
    // this class is for loading 3D pedestrian models
    [SerializeField]
    private Material WhiteMat;

    private int x_min;
    private int x_max;
    private int z_min;
    private int z_max;
    private float y;

    private float scale;

    private GameObject[] all_pedestrian_models;
    private GameObject[] all_pedestrian_models_white;

    private System.Random random = new System.Random();

    // Use this for initialization
    void Start()
    {
        switch (Utils.render_distance)
        {
            case Utils.distance.NEAR:
                x_min = -5;
                x_max = 5;
                z_min = -14;
                z_max = -9;
                y = -1;
                scale = 1.5f;
                Utils.num_pedestrian_per_image = 5;
                break;
            case Utils.distance.MIDDLE:
                x_min = -7;
                x_max = 7;
                z_min = -9;
                z_max = -3;
                y = -1;
                scale = 1.2f;
                Utils.num_pedestrian_per_image = 10;
                break;
            case Utils.distance.FAR:
                x_min = -9;
                x_max = 9;
                z_min = -4;
                z_max = -2;
                y = -1.0f;
                scale = 1.0f;
                Utils.num_pedestrian_per_image = 15;
                break;
        }
        
        GameObject mainObj = GameObject.FindGameObjectWithTag("MainObject");
        MainController mainCtrl = (MainController)mainObj.GetComponent<MainController>();

        // load original models
        Debug.Log("<color=blue>Loading models</color>");
        all_pedestrian_models = new GameObject[Utils.total_model_num];

        for (int i = 0; i < Utils.total_model_num; i++)
        {
            string fn = Utils.model_fns[i];

            all_pedestrian_models[i] = AssetDatabase.LoadAssetAtPath(fn, typeof(GameObject)) as GameObject;
            
            SkinnedMeshRenderer[] renderers = all_pedestrian_models[i].GetComponentsInChildren<SkinnedMeshRenderer>();

            // change shader
            Shader shader1 = Shader.Find("Mobile/Bumped Diffuse");

            foreach (SkinnedMeshRenderer r in renderers) {
                Material mat = r.sharedMaterials [0];
                mat.SetFloat ("_Mode", 0.0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.SetFloat("_Glossiness", 0.0f);
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        } 

        // put all pedestrians to the scene
        for(int i = 0; i < Utils.total_model_num; i++)
        {
            all_pedestrian_models[i] = Instantiate(all_pedestrian_models[i]) as GameObject;
        }

        // load white models
        all_pedestrian_models_white = new GameObject[Utils.total_model_num];

        for (int i = 0; i < Utils.total_model_num; i++)
        {
            string fn = Utils.model_fns[i];
            all_pedestrian_models_white[i] = AssetDatabase.LoadAssetAtPath(fn, typeof(GameObject)) as GameObject;
            SkinnedMeshRenderer[] renderers = all_pedestrian_models_white[i].GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer r in renderers)
            {
                r.material = WhiteMat;
            }
        }

        // put all pedestrians to the scene
        for (int i = 0; i < Utils.total_model_num; i++)
        {
            all_pedestrian_models_white[i] = Instantiate(all_pedestrian_models_white[i]) as GameObject;
        }

        // change size 

        Vector3 this_distance_size = new Vector3(scale, scale, scale);
        for (int i = 0; i < Utils.total_model_num; i++)
        {
            all_pedestrian_models[i].transform.localScale = this_distance_size;
            all_pedestrian_models_white[i].transform.localScale = this_distance_size;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Utils.shouldQuit())
        {
            return;
        }
        Debug.Log("PedestrianLoader.Update() !");    

        // disable all
        for (int i = 0; i < Utils.total_model_num; i++)
        {
            all_pedestrian_models[i].SetActive(false);
            all_pedestrian_models_white[i].SetActive(false);
        }

        switch (Utils.now_mode)
        {
            case Utils.states.NORMAL:

                // randomly pick several pedestrians
                Utils.generateRandomNumbers();

                // change all the positions and rotations
                for (int i = 0; i < Utils.total_model_num; i++)
                {
                    // change rotations
                    Quaternion this_ped_rot = Quaternion.AngleAxis(random.Next(0, 360), Vector3.up);
                    all_pedestrian_models[i].transform.rotation = this_ped_rot;
                    all_pedestrian_models_white[i].transform.rotation = this_ped_rot;
                    // change positions
                    int now_x_10x = random.Next(x_min * 10, x_max * 10);
                    float now_x = (float)now_x_10x / 10;

                    int now_z_10x = random.Next(z_min * 10, z_max * 10);
                    float now_z = (float)now_z_10x / 10;

                    Vector3 this_ped_pos = new Vector3(now_x, y, now_z);
                    all_pedestrian_models[i].transform.position = this_ped_pos;
                    all_pedestrian_models_white[i].transform.position = this_ped_pos;
                }
                                
                // enable the five
                for (int i = 0; i < Utils.num_pedestrian_per_image; i++)
                {
                    all_pedestrian_models[Utils.now_chosen_pedestrians[i]].SetActive(true);
                }
                
                break;

            case Utils.states.SEG:
                // enable the five
                for (int i = 0; i < Utils.num_pedestrian_per_image; i++)
                {
                    all_pedestrian_models_white[Utils.now_chosen_pedestrians[i]].SetActive(true);
                }
                break;

            default:
                break;
        }     
    }
    
    public void SaveJointCoordinate()
    {
        Debug.Log("SaveJointCoordinate()");
        //Utils.now_image_num += 1;

        // get main object
        GameObject mainObj = GameObject.FindGameObjectWithTag("MainObject");
        MainController mainCtrl = (MainController)mainObj.GetComponent<MainController>();

        string fname = string.Format(Utils.result_joint_folder + Utils.now_image_num.ToString(Utils.saving_format) + ".txt");
        
        StreamWriter writer = new StreamWriter(fname);

        for (int i = 0; i < Utils.num_pedestrian_per_image; i++)
        {
            GetJointCoordinate(writer, all_pedestrian_models[Utils.now_chosen_pedestrians[i]]);
        }
        writer.Close();
    }
    
    void GetJointCoordinate(StreamWriter writer, GameObject go)
    {
        /*		// joint set #1 (before Feb.7.2017)
		// keyword of model from Mixamo
		// " " : top of head
		// "Hips" : center
		// "Neck" : lower neck
		// "Head" : upper neck
		// "RightToeBase" : tip of right foot
		// "LeftToeBase" : tip of left foot
		// "LeftLeg" : left knee
		// "RightLeg" : right knee
		// "Belly" : around belly button
		// "LeftHand" : left hand
		// "RightHand" : rigtht hand
		int nJoint = 11;
		string[] jointName = new string[nJoint];
		jointName[0] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End";	// top of head
		jointName[1] = "mixamorig:Hips";		// center
		jointName[2] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck";		// lower neck
		jointName[3] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head";		// upper neck
		jointName[4] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot/mixamorig:RightToeBase";// tip of right foot
		jointName[5] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot/mixamorig:LeftToeBase";	// tip of left foot
		jointName[6] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg";		// left knee
		jointName[7] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg";	// right knee
		jointName[8] = "mixamorig:Hips/mixamorig:Spine";		// around belly button
		jointName[9] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand";	// left hand
		jointName[10] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand";	// right hand
		*/

        // joint set #2 (changed in Feb.7.2017) according to Leeds dataset (www.comp.leads.ac.ur/mat4saj/lsp.html)
        // keyword of model from Mixamo
        // 1. Right ankle
        // 2. Right knee
        // 3. Right hip
        // 4. Left hip
        // 5. Left knee
        // 6. Left ankle
        // 7. Right wrist 
        // 8. Right elbow
        // 9. Right shoulder
        // 10. Left shoulder
        // 11. Left elbow
        // 12. Left wrist
        // 13. Neck
        // 14. Head top
        /*int nJoint = 14;
        string[] jointName = new string[nJoint];
        jointName[0] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot";
        jointName[1] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg";
        jointName[2] = "mixamorig:Hips/mixamorig:RightUpLeg";
        jointName[3] = "mixamorig:Hips/mixamorig:LeftUpLeg";
        jointName[4] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg";
        jointName[5] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot";
        jointName[6] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand";
        jointName[7] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm";
        jointName[8] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm";
        jointName[9] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm";
        jointName[10] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm";
        jointName[11] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand";
        jointName[12] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck";      // lower neck
        jointName[13] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"; // top of head*/

		// joint set #3 MPII setting
		// keyword of model from Mixamo
		/*{0,  "Head"},
		{1,  "Neck"},
		{2,  "RShoulder"},
		{3,  "RElbow"},
		{4,  "RWrist"},
		{5,  "LShoulder"},
		{6,  "LElbow"},
		{7,  "LWrist"},
		{8,  "RHip"},
		{9,  "RKnee"},
		{10, "RAnkle"},
		{11, "LHip"},
		{12, "LKnee"},
		{13, "LAnkle"},
		{14, "Chest"},
		{15, "Bkg"}*/

		int nJoint = 15;
        string[] jointName = new string[nJoint];
		jointName[0] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"; // Head
		jointName[1] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck"; // Neck
		jointName[2] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm"; // Right Shoulder
		jointName[3] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm";  // Right Elbow
		jointName[4] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand"; // Right Wrist
		jointName[5] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm"; // Left Shoulder
		jointName[6] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm"; // Left Elbow
		jointName[7] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand"; // Left Wrist
		jointName[8] = "mixamorig:Hips/mixamorig:RightUpLeg"; // Right Hip
		jointName[9] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg"; // Right Knee
		jointName[10] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot"; // Right ankle
		jointName[11] = "mixamorig:Hips/mixamorig:LeftUpLeg"; // Left Hip
		jointName[12] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg";      // Left Knee
		jointName[13] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot"; // Left Ankle
		jointName[14] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2"; // Chest

		// joint set #4 MSCOCO dataset
		/* {0,  "Nose"},
		{1,  "Neck"},
		{2,  "RShoulder"},
		{3,  "RElbow"},
		{4,  "RWrist"},
		{5,  "LShoulder"},
		{6,  "LElbow"},
		{7,  "LWrist"},
		{8,  "RHip"},
		{9,  "RKnee"},
		{10, "RAnkle"},
		{11, "LHip"},
		{12, "LKnee"},
		{13, "LAnkle"},
		{14, "REye"},
		{15, "LEye"},
		{16, "REar"},
		{17, "LEar"},
		{18, "Bkg"},


		int nJoint = 18;
		string[] jointName = new string[nJoint];
		jointName[0] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot";
		jointName[1] = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg";
		jointName[2] = "mixamorig:Hips/mixamorig:RightUpLeg";
		jointName[3] = "mixamorig:Hips/mixamorig:LeftUpLeg";
		jointName[4] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg";
		jointName[5] = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot";
		jointName[6] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand";
		jointName[7] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm";
		jointName[8] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm";
		jointName[9] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm";
		jointName[10] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm";
		jointName[11] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand";
		jointName[12] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck";      // lower neck
		jointName[13] = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"; // top of head
		jointName[14] = ; // right eye
		jointName[15] = ; // left eye
		jointName[16] = ; // right ear
		jointName[17] = ; // left ear*/


        Vector3[] jointPosition = new Vector3[nJoint];

        Camera cam = Camera.main;

        Vector3 screenPos;
        string output = "";

        for (int i = 0; i < nJoint; i++)
        {

            GameObject goPart = go.transform.Find(jointName[i]).gameObject;
            Vector3 position = goPart.transform.position;

            Vector3 viewPos = cam.WorldToViewportPoint(position);
            screenPos.x = viewPos.x * Utils.w;
            screenPos.y = (1 - viewPos.y) * Utils.h;
            int x = (int)screenPos.x; // FIXME!!!
            int y = (int)screenPos.y;
            output += x.ToString() + " " + y.ToString() + " ";
        }

        writer.WriteLine(output);
    }
}
