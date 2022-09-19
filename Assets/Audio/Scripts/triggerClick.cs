using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Import freya; a maths toolset that includes a scaling method we use on line 42
using Freya;

public class triggerClick : MonoBehaviour
{
    // create script variable for collecting our agent info
    private KuramotoAffectedAgent kuramotoAffectedAgent;
    // create variable to hold our audio source
    private AudioSource audioSource;
    // weird check that sam doesnt like
    private bool isPlaying = false;
    // create variable for our threshold phase
    private float randThreshold;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // set script variable equal to the script component on this game object
        kuramotoAffectedAgent = GetComponent<KuramotoAffectedAgent>();
        
        // set audio source variable equal to the audio source component on this game object
        audioSource = GetComponent<AudioSource>();


    }

    // Update is called once per frame
    void Update()
    {

        // create a random threshold value
        randThreshold = UnityEngine.Random.Range(0.5f, 0.9f);

        // condition based on agent phase compared to threshold
        if (kuramotoAffectedAgent.phase > randThreshold && isPlaying == false)
        {
    
            // set pitch of the sample equal to a scaled (0-1) value of the agent's bpm (Pathogen Emitter manager sets this range = 90-100)
            audioSource.pitch = 1 + Mathfs.Remap(90f, 100f, 0f, 1f, kuramotoAffectedAgent.speedBPM);
            audioSource.Play();           
            isPlaying = true;
            //Debug.Log("Played file");
        }
        else if (kuramotoAffectedAgent.phase < randThreshold)
        {         
            audioSource.Stop();
            isPlaying = false;
        }

    }

}
