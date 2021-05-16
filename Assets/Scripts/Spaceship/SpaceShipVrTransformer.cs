using System;
using System.Collections;
using System.Collections.Generic;
using Spaceship;
using UnityEngine;

public class SpaceShipVrTransformer : MonoBehaviour
{
    public SpaceshipMover spaceshipMover;
    public Transform      spaceshipTransform;

    private float _curPos;

    public float curPos
    {
        get => _curPos;
        set => _curPos = Mathf.Clamp(value,-.3f,1);
    }

    private void Update()
    {
        if (spaceshipMover.inputVector.x > float.Epsilon )
            //nowForwardSpeed += forwardAcc * Time.fixedDeltaTime * input.z;
        {
            curPos = Mathf.Lerp(curPos, curPos +  spaceshipMover.inputVector.x,
                                   Time.deltaTime);

        }            
        else if(spaceshipMover.inputVector.x < -float.Epsilon)
        {
            curPos = Mathf.Lerp(curPos, curPos + spaceshipMover.inputVector.x,
                                   Time.deltaTime);

        }
        else
        {
            curPos = Mathf.Lerp(curPos, 0,
                                   Time.deltaTime);
        }           

        spaceshipTransform.localPosition = new Vector3(0, 0, curPos);
    }
}
