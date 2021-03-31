using System.Collections.Generic;
using System.Linq;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField]                     private CameraController _mainCameraController;
    [FormerlySerializedAs("quizEditor")] public  QuizBase         quizBase;
    public                                       bool             isQuizEditMode;
    public                                       Camera           mainCamera;
    public                                       List<GameObject> meshList;
    public                                       GravityTracing   orbit;

    public static GameManager GetGameManager { get; private set; }

    private void Awake()
    {
        GetGameManager = this;
    }


    public CameraController GetMainCameraController()
    {
        return _mainCameraController;
    }

    public Mesh GetMeshAndMaterialsFromList(int index,ref List<Material> materials)
    {
        materials = meshList[index].GetComponent<Renderer>().sharedMaterials.ToList();
        return meshList[index].GetComponent<MeshFilter>().sharedMesh;
    }

}