using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CameraController : MonoBehaviour
{
    //public Define.Cameramode _mode = Define.Cameramode.QuarterView;
    //public Vector3 _delta=new Vector3(0.0f, 6.0f,0);
    //public Vector3 _gamma=new Vector3(1.0f, 1.0f, 0);
    //[SerializeField]
    //GameObject _player=null;
    void Start()
    {
        transform.position = new Vector3(0.0f, 16.3f, -58f);
    }
    /*
    void LateUpdate()
    {
        /*
        if(_mode == Define.Cameramode.QuarterView)
        {
            RaycastHit hit;
            if(Physics.Raycast(_player.transform.position, _delta, out hit, _delta.magnitude, LayerMask.GetMask("Wall")))
            {
                //Debug.Log("Hit on !");
                float dist = (hit.point - transform.position).magnitude * 0.8f;
                transform.position = _player.transform.position+ _delta.normalized * dist;
            }
            else
            {
                transform.position = _player.transform.position + _delta;
                transform.LookAt(_player.transform.position+_gamma);
            }

        }
        */
    /*
    }
    /*
    public void SetQuarterView(Vector3 delta)
    {
        _mode = Define.Cameramode.QuarterView;
        _delta = delta;
    }
    */
    
}
