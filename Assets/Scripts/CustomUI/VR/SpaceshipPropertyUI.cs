using System;
using System.Collections;
using System.Collections.Generic;
using SpacePhysic;
using Spaceship;
using UnityEngine;
using UnityEngine.UI;

public class SpaceshipPropertyUI : MonoBehaviour
{
    public Transform      followTransform;
    public SpaceshipMover spaceshipMover;
    public AstralBody     spaceShipAstralBody;
    public Transform         faceTransform;
    public Text           speedText;
    public Text           forceText;

    private void Update()
    {
        this.transform.position = followTransform.position;
        this.transform.forward  = this.transform.position - faceTransform.position;
        speedText.text          = "速度："                   + spaceshipMover.mRigidbody.velocity.magnitude.ToString("f2") + "km/s";
        forceText.text          = "所受引力："                 + (spaceShipAstralBody.Force.magnitude * 100).ToString("f2")  + "×10³N";
    }
}
