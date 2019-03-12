using UnityEngine;

public class ArcadeScreen : MonoBehaviour
{
    public int Width = 240;
    public int Height = 320;


    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }


    void Update()//TODO 
    {
        var min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0));
        var max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));
        var pos = (min + max) * 0.5f;
        pos.z = 0;            

        this.transform.position = pos;
        this.transform.localScale = Vector3.one * (max.y / 40);
        Debug.Log(0.27f * max.y);

    }
}
