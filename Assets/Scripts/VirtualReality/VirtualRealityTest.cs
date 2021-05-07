using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VirtualRealityTest : MonoBehaviour
{
    public int    imageWidth = 1024;
    public bool   saveAsJPEG = true;
    public Camera vrCamera;
    void Start()
    {
        CaptureVirtualReality();
    }

    [ContextMenu("Capture VR")]
    public void CaptureVirtualReality()
    {
        byte[] bytes = I360Render.Capture( imageWidth, saveAsJPEG , vrCamera);
        if( bytes != null )
        {
            string path = Path.Combine( Application.streamingAssetsPath, "360render" + ( saveAsJPEG ? ".jpeg" : ".png" ) );
            File.WriteAllBytes( path, bytes );
            Debug.Log( "360 render saved to " + path );
        }
    }

}
