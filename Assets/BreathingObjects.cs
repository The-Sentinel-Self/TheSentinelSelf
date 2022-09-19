using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingObjects : MonoBehaviour
{
    [SerializeField]
    private Renderer[] BreathingMaterials;

    [SerializeField]
    private string id;

    private KuramotoAffecterAgent phaseFocus;

    private float phaseSmoothed = 0;

    // Update is called once per frame
    void Update()
    {

        
        if (phaseFocus != null) {
            
            //phaseSmoothed += (phaseFocus.phase - phaseSmoothed) * 0.3f;

            foreach (Renderer breath in BreathingMaterials)
            {
                breath.material.SetFloat(id, phaseFocus.phase);
            }
        }
    }
    public void SetFocus(Transform focus)
    {
        phaseFocus = focus.GetComponent<KuramotoAffecterAgent>();
    }
}
