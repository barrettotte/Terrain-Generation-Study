using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordHelper {


    public static SphericalCoord TransformToSphericalCoord(Vector3 targetPos, Vector3 parentPos){
        Vector3 dirToTarget = targetPos - parentPos;

        if( dirToTarget.sqrMagnitude == 0){
            return new SphericalCoord(0,0);
        }

        Quaternion quatToTarget = Quaternion.LookRotation(dirToTarget);
        SphericalCoord coord = new SphericalCoord();
        float lat = quatToTarget.eulerAngles.x;
        float lon = 360 - quatToTarget.eulerAngles.y;
	
        coord.Latitude = lat;
        coord.Longitude = lon;

        return coord;
    }

    public static Quaternion SphericalToRotation(SphericalCoord sphereCoord){
        return Quaternion.Euler( -sphereCoord.Latitude, sphereCoord.Longitude, 0 );
    }

    public static SphericalCoord RotationToSpherical(Quaternion rotation){
        return new SphericalCoord( rotation.eulerAngles.x, rotation.eulerAngles.y );
    }

    public static Vector2 RotationToUV(Quaternion rotation){
        return SphericalToUV(RotationToSpherical(rotation));
    }

    public static Vector2 SphericalToUV(SphericalCoord sphereCoord){
        Vector2 uv = new Vector2(
            (sphereCoord.Longitude / 360f),
            (sphereCoord.Latitude + 90) / 180f
        );
        return uv;
    }

    public static SphericalCoord UVToSpherical(Vector2 uv){
        return new SphericalCoord(-(uv.y - 0.5f) * 180f, 360f * uv.x);
    }

    public static Quaternion UVToRotation(Vector2 uv){
        return Quaternion.Euler(-(uv.y - 0.5f) * 180f, 360f * uv.x , 0);
    }

}
