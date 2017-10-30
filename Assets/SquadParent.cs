using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadParent : MonoBehaviour {

    public List<NewSquadMember> squadList;
    public string orderString;

	// Use this for initialization
	void Awake () {
        squadList.AddRange(transform.GetComponentsInChildren<NewSquadMember>());
	}
	
    void Start()
    {
        StartCoroutine(LateStart(1.0f));
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SquadMaster.Instance.mergeSquads(squadList);
        SquadMaster.Instance.updateSelected(squadList);
        OrderFactory.Instance.StartOrderCreation(orderString);
        OrderFactory.Instance.CompleteOrder(transform.position);
        SquadMaster.Instance.updateSelected(new List<int>());
        yield return null;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
