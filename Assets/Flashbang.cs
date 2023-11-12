using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// import Math;

public class Flashbang : MonoBehaviour
{
    [SerializeField] private GameObject myObj;

    public Rigidbody2D P1;
    public Rigidbody2D P2;
    private float a;
    private float b;
    private float d;
    private float ra;
    private float rb;
    private float ba;
    private float bb;
    // private float ravg;
    // private float bavg;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log(myObj.GetComponent<Renderer>().material.GetFloat("_alpha"));
    }

    // Update is called once per frame
    void Update() {
        d = System.Math.Min(System.Math.Min((P1.position - P2.position).magnitude, (P1.position - P2.position + new Vector2(0, -40)).magnitude), System.Math.Min((P1.position - P2.position + new Vector2(0, 40)).magnitude, 200));
        b = 0.75f * (float)System.Math.Exp(-0.5f * (float)d);
        if(P1.position[1] - P2.position[1] > 20) {
            rb = 1.0f;
        } else if(P1.position[1] - P2.position[1] < -20) {
            bb = 1.0f;
        } else {
            rb = 0.0f;
            ra = 0.0f;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(P1.position[1] > 0) {
                ra = 1.0f;
            } else {
                ba = 1.0f;
            }
            a = 1.0f;
        }
        if(a > 0) {
            a -= 0.1f;
        }
        if(ra > 0) {
            ra -= 0.1f;
        }
        if(rb > 0) {
            rb -= 0.1f;
        }
        if(ba > 0) {
            ba -= 0.1f;
        }
        if(bb > 0) {
            bb -= 0.1f;
        }
        myObj.GetComponent<Renderer>().material.SetFloat("_alpha", System.Math.Max(a, b));
        myObj.GetComponent<Renderer>().material.SetFloat("_Red", (ra + rb) / 2);
        myObj.GetComponent<Renderer>().material.SetFloat("_Blue", (ba + bb) / 2);
    }
}
