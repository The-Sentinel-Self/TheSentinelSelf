using System.Collections;
using System.Collections.Generic;
using Script.CameraSystem;
using UnityEngine;

public class GeneticMovementPathogen : GeneticMovement
{
    [SerializeField]
    private int maxKeys = 10;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kill"))
        {
            agent.kuramoto.dead = true;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            int numkeys = collision.gameObject.GetComponentsInChildren<GeneticAntigenKey>().Length;
            if (numkeys < collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>().NumKeysToCollect+maxKeys)// if less than max num, pick up key
            {
                Quaternion rot = Quaternion.LookRotation(collision.transform.position, transform.up);

                GameObject newObj = transform.GetChild(0).gameObject;

                newObj = Instantiate(newObj, collision.GetContact(0).point, rot, collision.transform);
                newObj.GetComponent<Digestion>().enabled = true;
                collision.gameObject.GetComponent<GeneticMovementSentinel>().keys++;
                collision.gameObject.GetComponent<GeneticMovementSentinel>().digestAntigens.Add(newObj.GetComponent<GeneticAntigenKey>());
                agent.kuramoto.dead = true;
                
                // TODO: @Neander: This is where the sentinel took an antigen
                CameraBrain.Instance.RegisterEvent(new WorldEvent(WorldEvents.SentinelAteAntigen, transform, new EventData(numkeys, collision.gameObject.GetComponentInChildren<GeneticMovementSentinel>().NumKeysToCollect+maxKeys)));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("PathogenEmitter"))
            transform.position = transform.parent.position+ Random.insideUnitSphere;
    }
}
