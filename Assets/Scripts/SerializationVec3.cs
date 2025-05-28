using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class SerializationVec3 : MonoBehaviour
{
    public static readonly byte[] memVector3 = new byte[3 * 4];
    public static short SerializeVector2(StreamBuffer outStream, object customobject)
    {
        Vector2 vo = (Vector2)customobject;
        lock (memVector3)
        {
            byte[] bytes = memVector3;
            int index = 0;
            Protocol.Serialize(vo.x, bytes, ref index);
            Protocol.Serialize(vo.y, bytes, ref index);
            outStream.Write(bytes, 0, 3 * 4);
        }

        return 3 * 4;
    }

    public static object DeserializeVector2(StreamBuffer inStream, short length)
    {
        Vector2 vo = new Vector2();
        lock (memVector3)
        {
            inStream.Read(memVector3, 0, 3 * 4);
            int index = 0;
            Protocol.Deserialize(out vo.x, memVector3, ref index);
            Protocol.Deserialize(out vo.y, memVector3, ref index);
        }

        return vo;
    }
}
