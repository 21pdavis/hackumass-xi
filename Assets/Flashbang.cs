using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashbang : MonoBehaviour
{
    [SerializeField] private GameObject myObj;
    private float a;
    // Start is called before the first frame update
    void Start()
    {
        // myObj.GetComponent<Material>().shader.alpha = 30;
        // Renderer rend = myObj.getComponent<Renderer>();
        // Material mat = rend.getComponent<Material>();
        // mat.a = 0;
        // rend.material = mat;
        // myObj.renderer.material = color;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            a = 10;
            Flash();
        }
        // GetComponent<Renderer>().material.color.a=Time.time-Mathf.Floor(Time.time);
    }

    // void FixedUpdate() {
    //     float a = 
    //     if(a == 1) {

    //     }
    // }

    void Flash() {
        for(int i = 0; i < 10; i++) {

        }
    }
}
