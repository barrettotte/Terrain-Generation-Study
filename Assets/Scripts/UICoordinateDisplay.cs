//-----------------------------------------------------------------------
// <copyright file="UICoordinateDisplay.cs" company="Quill18 Productions">
//     Copyright (c) Quill18 Productions. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UICoordinateDisplay : MonoBehaviour {

	private Text text;
	public Transform TargetObject;
	public Transform ParentObject;

	void Start () {
        text = GetComponent<Text>();
    }


    void FixedUpdate () {
        string s = "";
        s += string.Format("TRANSFORM (SPACE):            {0}\n", TargetObject.position);
        SphericalCoord sphereCoord = CoordHelper.TransformToSphericalCoord( TargetObject.position, ParentObject.position );
        s += string.Format("SPHERICAL COORDINATES:   {0}\n", sphereCoord.ToString());
        Vector2 uvCoord = CoordHelper.SphericalToUV(sphereCoord);
        s += string.Format("UV COORDINATES:                    {0}\n", uvCoord.ToString());
        text.text = s;
    }

}
