using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GeneticMovementTcell : MonoBehaviour
{
    [Tooltip("How many cycles to contain")]
    [SerializeField]
    private int cycleLength = 10; // number of steps in cylcle
    [Tooltip("Scaler for the genetic speed")]
    [SerializeField]
    private float genSpeedScl = 0.5f; // sclr for the speed
    [Tooltip("Scaler for the target speed")]
    [SerializeField]
    private float targetSpeedScl = 1.5f; // sclr for the speed
    [HideInInspector]
    public Vector3[] geneticMovement; // list to hold vels in

    private Vector3 thisGenVel;// this steps velocity

    private KuramotoAffectedAgent sentinel; // sentinel obj

    private Rigidbody rb;// rigidbody

    private int step = 0;// to hold the steps number

    private float lastPhase = 0;// holds the last phase for a gate
   // [HideInInspector]
    public Vector3 target;// the target to aim for

    private TCellManager manager;//its manager
    [Tooltip("Shader to compare keys")]
    [SerializeField]
    private ComputeShader compare;

    private GeneticAntigenKey thisAnti;
    
    public bool notKeyed = true;

    private bool targeting = true;

    // Start is called before the first frame update
    void Start()
    {
        // gets the sentinels kurmto
        sentinel = GetComponent<KuramotoAffectedAgent>();
        // gets this rb
        rb = GetComponent<Rigidbody>();
        // sets it to a new vec3 list for vels
        geneticMovement = new Vector3[cycleLength];

        // set the vels of the list
        for(int i=0; i<cycleLength; i++)
        {
            // random vec
            geneticMovement[i] = Random.insideUnitSphere;

        }


        manager = GetComponentInParent<TCellManager>();
        if (notKeyed)
        {
            target = transform.parent.position;
        }
        
         thisAnti = gameObject.GetComponent<GeneticAntigenKey>();
        

    }
    
    // Update is called once per frame
    void Update()
    {
        // if phase is less than last phase (back to 0 from 1)
        if (sentinel.phase < lastPhase) { 
            step++;// add a step
            if (step >= cycleLength)// if greater than list length, back to 0
            {
                step = 0;
            }
        }

        thisGenVel = geneticMovement[step];

         // get vel from this steps genmov, mult by phase and scl
        Vector3 vel =   thisGenVel * genSpeedScl;
        if (targeting)
        {
            vel += Vector3.Normalize(target - transform.position) * targetSpeedScl;
        }

        vel *= sentinel.phase;
               
        // add the vel to the rb
        rb.AddForceAtPosition(vel * Time.deltaTime, transform.position + transform.up);

       // set last phase to phase
        lastPhase = sentinel.phase;
    }

    // reset randomizes the list of vels
    public void Reset()
    {
        for (int i = 0; i < geneticMovement.Length; i++)
        {
            geneticMovement[i] = Random.insideUnitSphere;
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Kill")
        {
            sentinel.dead = true;
        }
        else if (collision.gameObject.tag == "Player")//if hits player
        {
            // get keys from children
            GeneticAntigenKey[] Antigens = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>();

            // if there are any keys
            if (Antigens.Length > 0) // if it had antigens?
            {
                // get matches from children
                AntigenKeys[] results = Compare(Antigens);// gpu accelerated key compare

                // run over results
                for (int i = 1; i < results.Length; i++)
                {
                    // if it has a match
                    if (results[i].hit > 0)
                    {
                        // set movement gate to false
                        notKeyed = false;
                        // sett render value
                        GetComponent<Renderer>().material.SetFloat("KeyTrigger", 1);
                        GetComponent<KuramotoAffectedAgent>().played = 2;
                        // add fitness
                        Antigens[i - 1].antigen.fitness++;
                        // set the target from the origin
                        target = Antigens[i - 1].origin;
                        for (int j = 0; j < 3; j++)
                        {
                            // create a replica
                            GameObject replica = Instantiate(gameObject, transform.parent);
                            replica.GetComponent<GeneticMovementTcell>().notKeyed = false;
                            replica.GetComponent<GeneticMovementTcell>().target = target;
                            // add new tcell to manager
                            manager.AddTCell(replica);
                            
                        }
                    }
                }
            }


        }
        else if (collision.gameObject.tag == "Pathogen") // pathogen
        {
            // get keys from children
            GeneticAntigenKey[] Antigens = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>();

            // if there are any keys
            if (Antigens.Length > 0)
            {
                // get matches from children
                AntigenKeys[] results = Compare(Antigens);

                // run over results
                for (int i = 1; i < results.Length; i++)
                {
                    // if it has a connection
                    if (results[i].hit > 0)
                    {
                        // add fitness
                        Antigens[i - 1].antigen.fitness++;
                        collision.gameObject.GetComponent<KuramotoAffectedAgent>().dead = true;
                        //kills a pathogen
                    }
                }
            }
        }
        else if (collision.gameObject.tag == "Plastic")
        {
            Debug.Log("plasticTcellHit");

            target = collision.gameObject.GetComponent<GeneticMovementPlastic>().origin;
            notKeyed = false;
            GetComponent<Renderer>().material.SetFloat("KeyTrigger", 2);
            GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100);
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Lymphonde" && notKeyed)
        {

            target = transform.parent.position+(UnityEngine.Random.onUnitSphere * 100);

        }
        else if (other.gameObject.tag == "PathogenEmitter")
        {
            
            StartCoroutine(TargetTimeout(15));
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "LymphOuter" && notKeyed )
        {
            target = transform.parent.position;
        }
    }


    private IEnumerator TargetTimeout(float waitTime)
    {

        
        yield return new WaitForSeconds(waitTime);
        targeting = false;


    }



    /// <summary>
    ///  GPu accelerating bits
    /// </summary>
    private struct AntigenKeys
    {
        public int key1;
        public int key2;
        public int key3;
        public int key4;
        public int hit;

        public void SetupKey(int[] key)
        {
            key1 = key[0];
            key2 = key[1];
            key3 = key[2];
            key4 = key[3];
            hit = 0;
        }
    }

    private AntigenKeys[] Compare(GeneticAntigenKey[] antigens)
    {
        AntigenKeys[] keys = new AntigenKeys[antigens.Length + 1];
        
        
        
        
        keys[0].SetupKey(thisAnti.antigen.Key);

        for (int i=1; i<keys.Length; i++)
        {
            if (antigens[i - 1].antigen.Key != null)
            {
                keys[i].SetupKey(antigens[i - 1].antigen.Key);
            }
        }

        RenderTexture rt = new RenderTexture(1, 1, 0);
        rt.enableRandomWrite = true;
        RenderTexture.active = rt;

        ComputeBuffer keyBuffer = new ComputeBuffer(keys.Length, Marshal.SizeOf(typeof(AntigenKeys)));
        keyBuffer.SetData(keys);


        int UpdateBiome = compare.FindKernel("KeyCompare");
        //int UpdateSentinel = shader.FindKernel("SentinelUpdate");

        compare.SetTexture(UpdateBiome, "Result", rt);
        compare.SetBuffer(UpdateBiome, "antigenKeys", keyBuffer);

        compare.Dispatch(UpdateBiome, 1, 1, 1);

        keyBuffer.GetData(keys);

        keyBuffer.Dispose();

        return  keys;
    }


}
