using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie1 : MonoBehaviour
{
    [Header("Zombie Health and Damage")]
    private float zombieHealth=100f;
    private float presentHealth;
    public float giveDamage=5f;

    [Header("Zombie Things")]
    public NavMeshAgent ZombieAgent;
    public Transform LookPoint;
    public Camera AttackingRaycastArea;
    public Transform playerBody;
    public LayerMask PlayerLayer;

    [Header("Zombie Guarding")]
    public GameObject[] walkPoints;
    int currentZombiePosition=0;
    public float zombieSpeed;
    float walkingpointRadius=2;

    [Header("Zombie Attacking")]
    public float TimeBtwAttack;
    bool previouslyAttack;

    [Header("Zombie Animation")]
    public Animator anim;

    [Header("Zombie States")]
    public float visionRadius;
    public float attackingRadius;
    public bool playerInvisionRadius;
    public bool playerInattackingRadius;

    private void Awake() {
        presentHealth=zombieHealth;
        ZombieAgent=GetComponent<NavMeshAgent>();
    }

    private void Update() {
        playerInvisionRadius=Physics.CheckSphere(transform.position,visionRadius,PlayerLayer);
        playerInattackingRadius=Physics.CheckSphere(transform.position,attackingRadius,PlayerLayer);

        if(!playerInvisionRadius && !playerInattackingRadius) Guard();
        if(playerInvisionRadius && !playerInattackingRadius) Pursueplayer();
        if(playerInvisionRadius && playerInattackingRadius) AttackPlayer();
    }

    private void Guard(){
        if(Vector3.Distance(walkPoints[currentZombiePosition].transform.position,transform.position)<walkingpointRadius){
            currentZombiePosition=Random.Range(0,walkPoints.Length);

            if(currentZombiePosition>=walkPoints.Length){
                currentZombiePosition=0;
            }
        }
        transform.position=Vector3.MoveTowards(transform.position,walkPoints[currentZombiePosition].transform.position,Time.deltaTime*zombieSpeed);

        //change zombie facing direction
        transform.LookAt(walkPoints[currentZombiePosition].transform.position);
    }

    private void Pursueplayer(){
        if(ZombieAgent.SetDestination(playerBody.position)){
            anim.SetBool("Walking",false);
            anim.SetBool("Running",true);
            anim.SetBool("Attacking",false);
            anim.SetBool("Died",false);
        }else{
            anim.SetBool("Walking",false);
            anim.SetBool("Running",false);
            anim.SetBool("Attacking",false);
            anim.SetBool("Died",true);
        }
    }

    private void AttackPlayer(){
        if(!previouslyAttack){
            RaycastHit hitInfo;
            if(Physics.Raycast(AttackingRaycastArea.transform.position,AttackingRaycastArea.transform.forward,out hitInfo,attackingRadius)){
                Debug.Log("Attacking"/*+hitInfo.transform.name*/);

                PlayerScript playerBody=hitInfo.transform.GetComponent<PlayerScript>();

                if(playerBody!=null){
                    playerBody.playerHitDamage(giveDamage);
                }

                anim.SetBool("Attacking",true);
                anim.SetBool("Walking",false);
                anim.SetBool("Running",false);
                anim.SetBool("Died",false);
            }

            previouslyAttack=true;
            Invoke(nameof(ActiveAttacking),TimeBtwAttack);
        }
    }

    private void ActiveAttacking(){
        previouslyAttack=false;
    }

    public void zombieHitDamage(float takeDamage){
        presentHealth-=takeDamage;
        if(presentHealth<=0){

            anim.SetBool("Walking",false);
            anim.SetBool("Running",false);
            anim.SetBool("Attacking",false);
            anim.SetBool("Died",true);

            zombieDie();
        }
    }

    private void zombieDie(){
        ZombieAgent.SetDestination(transform.position);
        zombieSpeed=0f;
        attackingRadius=0f;
        visionRadius=0f;
        playerInattackingRadius=false;
        playerInvisionRadius=false;
        Object.Destroy(gameObject,5.0f);
    }
}
