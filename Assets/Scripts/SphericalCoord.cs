//-----------------------------------------------------------------------
// <copyright file="SphericalCoord.cs" company="Quill18 Productions">
//     Copyright (c) Quill18 Productions. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalCoord 
{
    public SphericalCoord()
    {
        //SphericalCoord(0,0);
    }

    public SphericalCoord( float lat, float lon )
    {
        Latitude = lat;
        Longitude = lon;
    }

    /// <summary>
    /// Gets or sets the latitude. 0 the equator. -90 is the North Pole. +90 is the South Pole
    /// </summary>
    /// <value>The latitude.</value>
    public float Latitude { 
        get
        {
            return _Latitude;
        }
        set
        {
            // Deal with values that exceed +/- 360
            _Latitude = value % 360;

            // If we're above 180, then we are in the northern hemisphere
            // and we represent that by a negative value
            if(_Latitude > 180)
            {
                _Latitude = -(180 - (_Latitude - 180));
            }

        }
    }
    private float _Latitude;

    /// <summary>
    /// Gets or sets the longitude. 0 is left edge, 360 is right edge
    /// </summary>
    /// <value>The longitude.</value>
    public float Longitude 
    { 
        get
        {
            return _Longitude;
        }

        set
        {
            _Longitude = value % 360;

        }
    }
    private float _Longitude;


    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="SphericalCoord"/>.
    /// This string will be in the classic Earthican format where 0° latitude is equator
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="SphericalCoord"/>.</returns>
    public override string ToString()
    {
        string latString = string.Format("0°");

        if(Latitude < 0)
        {
            // North
            latString = string.Format("{0}° N", (-Latitude) );
        }
        else if(Latitude > 0)
        {
            // South
            latString = string.Format("{0}° S", (Latitude) );
        }

        string longString = string.Format("0°");

        if(Longitude <= 0.0001f)
        {
            // Do nothing
        }
        else if(Longitude <= 180)
        {
            longString = string.Format("{0}° E", (Longitude) );
        }
        else if(Longitude > 180)
        {
            longString = string.Format("{0}° W", (180 - (Longitude - 180)) );
        }


        return string.Format("{0}, {1}", latString, longString);
        //return string.Format("{0}, {1}", Latitude, Longitude);
    }
}
