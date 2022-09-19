using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSettings : MonoBehaviour
{
    [SerializeField]
    private bool ReUseCallBack = true;

    // Start is called before the first frame update
    void Start()
    {
        Physics.reuseCollisionCallbacks = ReUseCallBack;
    }

}
