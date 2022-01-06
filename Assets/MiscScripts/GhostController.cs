using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{

    void Start()
    {
        ControllScript.GetInstance().RegisterGhost(this);
    }

    void OnDestroy()
    {
        try
        {
            ControllScript.GetInstance().DeRegisterGhost(this);
        }
        catch (System.Exception e)
        {

        }
    }
    public float GetDistance()
    {
        RaycastHit hit;
        float distance = float.PositiveInfinity;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distance))
        {
            distance = Vector3.Distance(hit.point, transform.position);
        }
        return distance;
    }

    public void Move(float distance)
    {
        transform.Translate(transform.forward * distance);
    }

    public void Rotate(float angle)
    {
        transform.Rotate(0, angle, 0);
    }

    public void SetPosition(Vector3 position, Quaternion rotation){
        transform.SetPositionAndRotation(position, rotation);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public Vector3 GetRotation()
    {
        return transform.rotation.eulerAngles;
    }

    public Quaternion GetQuaternion(){
        return transform.rotation;
    }
}
