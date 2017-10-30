using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Rifle : VRTK_InteractableObject
{

    public GameObject playerHipHolster;
    public GameObject bullet;
    public GameObject gunPoint;
    public GameObject hole;
    public GameObject muzzle;
    public float bulletSpeed = 1000f;
    private float timer;
    public float shootSpeed = 0.5f;


    // Update is called once per frame
    override protected void Update()
    {
        if (!IsGrabbed())
        {
            transform.position = playerHipHolster.transform.position;
            transform.rotation = playerHipHolster.transform.rotation;
        }
        if (IsUsing())
        {
            timer += Time.deltaTime;
            if (timer > shootSpeed)
            {
                RaycastHit hit;
                if (Physics.Raycast(bullet.transform.position, -bullet.transform.forward + (new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) / 50), out hit, 9999f))
                {
                    DrawLine(bullet.transform.position, hit.point, Color.white);
                    var holeInst = Instantiate(hole, hit.transform, true);
                    holeInst.transform.position = hit.point;
                    holeInst.transform.LookAt(hit.point + hit.normal);
                    holeInst.transform.SetGlobalScale( new Vector3(.5f, .5f, .5f));
                    if (hit.rigidbody != null)
                    {
                        if (hit.rigidbody.GetComponent<Character>() != null)
                        {
                            hit.rigidbody.GetComponent<Character>().Damage(4.0f);
                        }
                    }
                }
                else
                {
                    DrawLine(bullet.transform.position, bullet.transform.position + (-bullet.transform.forward + (new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) / 50))*100, Color.white);
                }
                var muzz = Instantiate(muzzle, gunPoint.transform);
                muzz.transform.localPosition = new Vector3(0.0f, 0.00624f, 0.0f);
                muzz.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f));
                muzz.transform.localScale = new Vector3(.015f, .015f, .015f);
                //muzz.transform.LookAt(muzz.transform.position + -transform.right);
                Destroy(muzz, shootSpeed);
                timer = 0.0f;
            }
        }
        base.Update();
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.01f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.005f;
        lr.endWidth = 0.005f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }

}
