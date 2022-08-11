using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathogenManager : MonoBehaviour
{
    [Tooltip("The gameobject for each agent in this manager")]
    [SerializeField]
    private GameObject sentinel; // holds the sentinel prefab
    [Tooltip("Number of the agents to be produced by this manager")]
    [Range(1, 3000)]
    [SerializeField]
    public int nSentinels = 10; // number of them to be made

    [Tooltip("radius to be spawned in from this obects transform")]
    [Range(0.1f, 1000f)]
    [SerializeField]
    private float spawnArea = 1.0f; // area to spawn in 
    [Tooltip("Kuramoto speed, measured in bpm, x=min y=max")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(90, 100); // variation of speed for them to have
    [Tooltip("Kuramoto, range for the max distance for the effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingRange = new Vector2(1, 10); // coupling range to have
    [Tooltip("Kuramoto, range for noise effect, x=min y=max")]
    [SerializeField]
    private Vector2 noiseSclRange = new Vector2(0.01f, 0.5f); // noise Scl to have
    [Tooltip("Kuramoto, range for the strength of the coupling effect, x=min y=max")]
    [SerializeField]
    private Vector2 couplingSclRange = new Vector2(0.2f, 10f); // coupling scl
    [Tooltip("Kuramoto, range for the scaling the clustering/attraction effect, x=min y=max")]
    [SerializeField]
    private Vector2 attractionSclRange = new Vector2(0.2f, 1f); // coupling scl

    [HideInInspector]
    public GameObject[] sentinels; //list to hold the sentinels
    [HideInInspector]
    public GPUData[] GPUStruct; // list of struct ot hold data, maybe for gpu acceleration

    private List<Genetics.GenVel> GenVelLib; // lib to hold the gene move data
   
    private List<Genetics.GenKurmto> GenKurLib; // lib to hold gene kurmto data
    [Tooltip("Max age the agents will reach")]
    [SerializeField]
    private float MaxAge = 1000; // age limit to kill sentinels

    [SerializeField]
    private float speedScl = 3f;


    public int RealNumPathogens = 0;
    [SerializeField]
    private float emitionTimer = 1.0f;
    private float timeGate = 0;

    // struct to hold data maybe for gpu acceleration
    public struct GPUData
    {
        public int age;
        public int connections;
        public int played;
        public float speed;
        public float phase;
        public float cohPhi;
        public float coherenceRadius;
        public float couplingRange;
        public float noiseScl;
        public float coupling;
        public float attractionScl;
        public float fittness;
        public Vector3 vel;
        public Vector3 pos;

        
        public void SetFromKuramoto(KuramotoAffectedAgent kuramoto)
        {
            speed = kuramoto.speed;
            phase = kuramoto.phase;
            cohPhi = kuramoto.cohPhi;
            coherenceRadius = kuramoto.coherenceRadius;
            couplingRange = kuramoto.couplingRange;
            noiseScl = kuramoto.noiseScl;
            coupling = kuramoto.coupling;
            connections = kuramoto.Connections;
            attractionScl = kuramoto.attractionSclr;
            age = 0;
            fittness = 0;
        }

        public void SetPos(Vector3 Pos)
        {
            pos=Pos;
            age = 0;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // create list to hold object
        sentinels = new GameObject[nSentinels];
        // create list to hold data structs
        GPUStruct = new GPUData[nSentinels];
        // create the two lib lists
        GenKurLib = new List<Genetics.GenKurmto>();
        GenVelLib = new List<Genetics.GenVel>();

        // loop over the nsentinels
        for(int i=0; i<10; i++)
        {

            AddPathogen(i);

        }


    }

    private void AddPathogen(int i)
    {
        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        // instantiate a new sentinel as child and at pos
        GameObject thisSentinel = Instantiate(sentinel, pos, Quaternion.identity, this.transform);

        // get its kurmto component
        KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
        kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

        // add the object to the list
        sentinels[i] = thisSentinel;

        // set data in the struct
        GPUStruct[i].SetFromKuramoto(kuramoto);
        GPUStruct[i].pos = sentinels[i].transform.position;

        RealNumPathogens++;
    }

    private void DuplicatePathogen(int i, GameObject pathogen)
    {
        

        // instantiate a new sentinel as child and at pos
        GameObject thisSentinel = Instantiate(pathogen, pathogen.transform.position, Quaternion.identity, this.transform);

        // get its kurmto component
        KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
        kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

        // add the object to the list
        sentinels[i] = thisSentinel;

        // set data in the struct
        GPUStruct[i].SetFromKuramoto(kuramoto);
        GPUStruct[i].pos = sentinels[i].transform.position;

        RealNumPathogens++;
    }

    private void Update()
    {
        if(Time.time > emitionTimer + timeGate && RealNumPathogens < nSentinels)
        {

            timeGate = Time.time;
            AddPathogen(RealNumPathogens);


        }

        // loop over the n sentinels
        for (int i = 0; i < RealNumPathogens; i++)
        {
            // get the kurmto
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            
            // if older than age 
            if (kuramoto.dead || GPUStruct[i].age > MaxAge) {
                if (GPUStruct[i].age > MaxAge && RealNumPathogens < nSentinels)
                {
                    DuplicatePathogen(RealNumPathogens, sentinels[i]);
                }
                // add data to lib
                Genetics.GenKurmto genKurm = new Genetics.GenKurmto(kuramoto.speedBPM, kuramoto.noiseScl, kuramoto.coupling, kuramoto.couplingRange, kuramoto.attractionSclr, kuramoto.fitness);
                GenKurLib.Add(genKurm);
                Genetics.GenVel vels = new Genetics.GenVel(sentinels[i].GetComponent<GeneticMovementPathogen>().geneticMovement, kuramoto.fitness);
                GenVelLib.Add(vels);
                // reset its values
                ResetSentinel(i);
                
                GPUStruct[i].SetFromKuramoto(kuramoto);
                GPUStruct[i].pos = sentinels[i].transform.position;

               
            }
            else
            {
                kuramoto.fitness = GPUStruct[i].fittness;
                kuramoto.age = GPUStruct[i].age;
                kuramoto.phase = GPUStruct[i].phase;
                kuramoto.Connections = GPUStruct[i].connections;
                kuramoto.played = GPUStruct[i].played;
                sentinels[i].GetComponent<Rigidbody>().AddForceAtPosition( GPUStruct[i].vel* Time.deltaTime * speedScl, sentinels[i].transform.position + sentinels[i].transform.up);
                
                GPUStruct[i].pos = sentinels[i].transform.position;

            }

          
        }

        // if the lib is greater than ...
        if (GenVelLib.Count > 1000)
        {
            // negative selection
            GenVelLib = Genetics.NegativeSelection(GenVelLib);
            GenKurLib = Genetics.NegativeSelection(GenKurLib);

        }

    }

    public void RemovePathogen(int i)
    {
        

            for (int p = i + 1; p <= RealNumPathogens; p++)
            {
                GPUStruct[p - (i + 1)] = GPUStruct[p];
                sentinels[p - (i + 1)] = sentinels[p];

            }

        
            RealNumPathogens--;
    }
   
    public void ResetSentinel(int i)
    {

        // get i sentinel
        GameObject thisSentinel = sentinels[i];



        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnArea;

        thisSentinel.transform.position = pos;

        // if lib less than 500
        if (GenKurLib.Count < 500)
        {
            // add random new sentinel
            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.Setup(noiseSclRange, couplingRange, speedRange, couplingSclRange, attractionSclRange, 0.2f);// setup its setting to randomize them

            GeneticMovementPathogen genVel = thisSentinel.GetComponent<GeneticMovementPathogen>();
            genVel.Reset();
        }
        else
        {
            // add random sentinel from lib
            int rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData1 = GenKurLib[rand];
            rand = UnityEngine.Random.Range(0, GenKurLib.Count);
            Genetics.GenKurmto kurData2 = GenKurLib[rand];

            float[] Settings = kurData1.BlendAttributes(kurData2.Settings);

            KuramotoAffectedAgent kuramoto = thisSentinel.GetComponent<KuramotoAffectedAgent>();
            kuramoto.SetupData(Settings);

            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel1 = GenVelLib[rand];
            rand = UnityEngine.Random.Range(0, GenVelLib.Count);
            Genetics.GenVel genVel2 = GenVelLib[rand];

            Vector3[] Vels = genVel2.BlendAttributes(genVel1.Vels);

            GeneticMovementPathogen genMov = thisSentinel.GetComponent<GeneticMovementPathogen>();
            genMov.Reset();
            genMov.geneticMovement = genVel1.BlendAttributes(Vels);


        }
        
    }


    // functions bellow are for ui to change settings in debug
    public void setRange(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            kuramoto.couplingRange = range;

        }
    }

    public void setCoupling(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            kuramoto.coupling = range;

        }
    }

    public void setNoise(float range)
    {
        for (int i = 0; i < nSentinels; i++)
        {
            KuramotoAffectedAgent kuramoto = sentinels[i].GetComponent<KuramotoAffectedAgent>();
            kuramoto.noiseScl = range;

        }
    }

}
