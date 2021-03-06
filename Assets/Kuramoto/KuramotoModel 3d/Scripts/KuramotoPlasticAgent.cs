using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuramotoPlasticAgent : MonoBehaviour
{
    private const float CIRCLE_IN_RADIAN = 2f * Mathf.PI; //2* pi


    private Renderer rendr;
    [Tooltip("colour 1 to lerp between")]
    [SerializeField]
    private Color col0;// phase col1
    [Tooltip("colour 2 to lerp between")]
    [SerializeField]
    private Color col1; // phase col2



    [Header("Debugging Atrributes (just for looking)")]
    public float speed; // driving force for the phase
    public float speedBPM;
    public float phase; // holds the phase position
    public float coherenceRadius; //holds the phase distance to 0,0
    public float couplingRange = 1; // holds the distance to the coupling range
    public float noiseScl = 1; // scales the noise added
    public float coupling = 0.5f; // scales the coupling effect
    public float speedVariation = 0.1f; // variation to randomise speed
   




    public void Setup(Vector2 noiseRange, Vector2 couplingRanges, Vector2 SpeedRange, Vector2 couplingScl, float thisSpeedVariation = 0.1f)
    {
        speedBPM = UnityEngine.Random.Range(SpeedRange.x, SpeedRange.y);
        speed = speedBPM/60;
        phase = speed * UnityEngine.Random.Range(1f - thisSpeedVariation, 1f + thisSpeedVariation);
        noiseScl = UnityEngine.Random.Range(noiseRange.x, noiseRange.y);
        coupling = UnityEngine.Random.Range(couplingScl.x, couplingScl.y);
        couplingRange = UnityEngine.Random.Range(couplingRanges.x, couplingRanges.y);
  
    }

    // Start is called before the first frame update
    void Start()
    {
        rendr = GetComponentInChildren<Renderer>();
        
        Setup(new Vector2(0.01f, 0.1f), new Vector2(20, 30), new Vector2(50, 60), new Vector2(4, 20));
    }

    // Update is called once per frame
    void Update()
    {
       
        


       
        rendr.material.color = Color.Lerp(col0, col1, phase);

    }

}
