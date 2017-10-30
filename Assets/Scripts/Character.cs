using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
public class Character : MonoBehaviour {

    [Range(0.0f, 100.0f)]
    public float health = 100.0f;
    //[Range(0.0f, 100.0f)]
    //public float damageValue = 1.0f;

    //public Transform gunPoint;

    public float timer;
    public GameObject rotation;
    public GameObject bullet;
    public float bulletRate;

    public GameObject gunPoint;
    public GameObject hole;
    public GameObject muzzle;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
    }

    public void Damage(float damageValue)
    {
        health -= damageValue;
        if (health <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    public void Shoot(GameObject threat)
    {
        if (threat)
        {
            rotation.transform.LookAt(threat.transform);
            timer += Time.deltaTime;
            if (timer > bulletRate)
            {
                RaycastHit hit;
                if (Physics.Raycast(bullet.transform.position, -bullet.transform.forward + (new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) / 50), out hit, 9999f))
                {
                    Rifle.DrawLine(bullet.transform.position, hit.point, Color.white);
                    var holeInst = Instantiate(hole, hit.transform, true);
                    holeInst.transform.position = hit.point;
                    holeInst.transform.LookAt(hit.point + hit.normal);
                    holeInst.transform.SetGlobalScale(new Vector3(.5f, .5f, .5f));
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
                    Rifle.DrawLine(bullet.transform.position, bullet.transform.position + (-bullet.transform.forward + (new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f) / 50)) * 100, Color.white);
                }
                var muzz = Instantiate(muzzle, gunPoint.transform);
                muzz.transform.localPosition = new Vector3(0.0f, 0.00624f, 0.0f);
                muzz.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f));
                muzz.transform.localScale = new Vector3(.015f, .015f, .015f);
                //muzz.transform.LookAt(muzz.transform.position + -transform.right);
                Destroy(muzz, bulletRate);
                timer = 0.0f;
            }
        }
    }
}
