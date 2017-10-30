using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderFactory : Singleton<OrderFactory> {

    public Dictionary<string, Execution> converter;

    public Order currentOrder;

    public Text OrderCreationText;

    void Awake()
    {
        converter = new Dictionary<string, Execution>();
        converter.Add("Attack", Order.Attack);
        converter.Add("Defend", Order.Defend);
        converter.Add("Move", Order.Goto);
        converter.Add("Cover", Order.Cover);
        converter.Add("Flank", Order.Flank);
        converter.Add("Stay", Order.Stay);

        OrderCreationText.text = "Select order to give";
    }

    public void StartOrderCreation(string orderString)
    {
        GesturesController.Instance.orderStarted = true;
        currentOrder = new Order();
        currentOrder.execution = converter[orderString];
        OrderCreationText.text = "Selected: " + orderString;
    }

    public void CompleteOrder(Vector3 position)
    {
        OrderCreationText.text = "Select order to give";
        GesturesController.Instance.orderStarted = false;
        currentOrder.waypoint = position;
        foreach (NewSquadMember member in SquadMaster.Instance.getSelectedMembers())
        {
            member.orders.Add(new Order(currentOrder));
            //member.orders.Sort();
        }
        //return new Order(currentOrder.objectPosition, currentOrder.waypoint, currentOrder.execution, currentOrder.priority);
    }

    public void CompleteOrder(GameObject Waypoint)
    {
        OrderCreationText.text = "Select order to give";
        GesturesController.Instance.orderStarted = false;
        currentOrder.objectPosition = Waypoint;
        foreach (NewSquadMember member in SquadMaster.Instance.getSelectedMembers())
        {
            member.orders.Add(currentOrder);
            //member.orders.Sort();
        }
        //return new Order(currentOrder.objectPosition, currentOrder.waypoint, currentOrder.execution, currentOrder.priority);
    }

    public void CompleteOrderOnOwnPosition()
    {
        OrderCreationText.text = "Select order to give";
        GesturesController.Instance.orderStarted = false;
        foreach (NewSquadMember member in SquadMaster.Instance.getSelectedMembers())
        {
            currentOrder.waypoint = member.transform.position;
            member.orders.Add(currentOrder);
            //member.orders.Sort();
        }
        //return new Order(currentOrder.objectPosition, currentOrder.waypoint, currentOrder.execution, currentOrder.priority);
    }
}
