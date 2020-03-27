using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBasedTransformReplicator : ReplicatiorBase
{
    public GameObject CoordinateBaseObject;

    Vector3 prepos;

    private void Start()
    {
        CoordinateBaseObject = GameObject.Find("COBase");


        if (CoordinateBaseObject == null)
        {
            CoordinateBaseObject = GameObject.FindGameObjectWithTag("COBaseTag");
        }


    }

    public override byte[] GetReplicationData()
    {
        if (CoordinateBaseObject == null)
            return null;

        Vector3 vec = (transform.position - CoordinateBaseObject.transform.position);
        float   x = Vector3.Dot(vec, CoordinateBaseObject.transform.right),
                y = Vector3.Dot(vec, CoordinateBaseObject.transform.up),
                z = Vector3.Dot(vec, CoordinateBaseObject.transform.forward);
        Vector3 Degradedvec = new Vector3(x, y, z);
        float yRot = Quaternion.LookRotation(transform.position, CoordinateBaseObject.transform.position).eulerAngles.y;
        prepos = transform.position;
        return NetworkManagerBase.encoding.GetBytes(Serializer.Vector3ToString(Degradedvec, 3) + "," + yRot);

    }

    public override byte[] GetAutonomousData()
    {
        return GetReplicationData();
    }

    public override void ReceiveReplicationData(byte[] repdata)
    {
        if (CoordinateBaseObject == null)
            return;

        string[] s = NetworkManagerBase.encoding.GetString(repdata).Split(',');
        Vector3 vec = Serializer.StringToVector3(s[0], s[1], s[2]);
        Vector3 diff_X = vec.x * CoordinateBaseObject.transform.right,
                diff_Y = vec.y * CoordinateBaseObject.transform.up,
                diff_Z = vec.z * CoordinateBaseObject.transform.forward;
        transform.position = CoordinateBaseObject.transform.position + diff_X + diff_Y + diff_Z;
        transform.eulerAngles = new Vector3(0, float.Parse(s[3]), 0);
    }

    public override void ReceiveAutonomousData(byte[] autodata)
    {
        ReceiveReplicationData(autodata);
    }

    public override bool DoesServerNeedReplication()
    {
        return prepos != transform.position&&IsAutonomousObject();
    }

    public override bool DoesClientNeedReplication(ClientDataContainer client)
    {
        return prepos != transform.position&&!IsAutonomousObject();
    }
}
