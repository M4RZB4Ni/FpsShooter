using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Animations;
using System.Collections.Generic;


public class ParagonAICreationWizard : EditorWindow
{
    string agentName = "Paragon AI Agent";
    GameObject modelRootObject;
    Transform spineBoneToRotate;
    Transform eyeTransform;

    Transform targetObjParentTransform;
    int teamNumber = 1;
    int enemyTeamNumber = 0;

    //Transform weaponTransform;
    Transform bulletSpawnerTransform;
    GameObject bulletObject;
    AudioClip bulletSound;

    //Mandatory Animations:
    AnimatorController animationController;
    Avatar animationAvatar;

    [MenuItem("GameObject/3D Object/Paragon AI Agent")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ParagonAICreationWizard));
    }

    //Physics
    string hitBoxTag = "HitBox";
    float defaultHitMultiplier = 1;
    float headHitMultiplier = 10;

    private Vector2 scrollPos;



    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("[R] means that this is a requried field", EditorStyles.boldLabel);
        ///////////////////////////Base
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        agentName = EditorGUILayout.TextField("Agent Name: ", agentName);
        EditorGUILayout.Space();

        modelRootObject = (GameObject)EditorGUILayout.ObjectField("[R] Model Root Object", modelRootObject, typeof(GameObject), true);
        targetObjParentTransform = (Transform)EditorGUILayout.ObjectField("[R] Bone To Attach Target To", targetObjParentTransform, typeof(Transform), true);
        spineBoneToRotate = (Transform)EditorGUILayout.ObjectField("[R] Spine Bone To Rotate", spineBoneToRotate, typeof(Transform), true);
        eyeTransform = (Transform)EditorGUILayout.ObjectField("[R] Eye Transform", eyeTransform, typeof(Transform), true);

        EditorGUILayout.Space();
        teamNumber = EditorGUILayout.IntField("Team Number", teamNumber);
        enemyTeamNumber = EditorGUILayout.IntField("Enemy Team Number", enemyTeamNumber);

        ///////////////////////////Gun	
        EditorGUILayout.Space();
        GUILayout.Label("Gun", EditorStyles.boldLabel);
        //weaponTransform = (Transform)EditorGUILayout.ObjectField("Gun Transform", weaponTransform, typeof(Transform), true);
        bulletSpawnerTransform = (Transform)EditorGUILayout.ObjectField("Bullet Spawn", bulletSpawnerTransform, typeof(Transform), true);


        if (bulletSpawnerTransform)
        {
            bulletObject = (GameObject)EditorGUILayout.ObjectField("Bullet Object", bulletObject, typeof(GameObject), true);
            bulletSound = (AudioClip)EditorGUILayout.ObjectField("Bullet Sound", bulletSound, typeof(AudioClip), true);
            EditorGUILayout.Space();
        }


        ////////////////////////////Hitbox
        EditorGUILayout.Space();
        GUILayout.Label("Hitboxes", EditorStyles.boldLabel);
        hitBoxTag = EditorGUILayout.TextField("Hitbox Tag", hitBoxTag);
        defaultHitMultiplier = EditorGUILayout.FloatField("Normal Hitbox Multiplier", defaultHitMultiplier);
        headHitMultiplier = EditorGUILayout.FloatField("Head Hitbox Multiplier", headHitMultiplier);


        ///////////////////////////Animation
        EditorGUILayout.Space();
        GUILayout.Label("Animation Settings", EditorStyles.boldLabel);
        animationController = (AnimatorController)EditorGUILayout.ObjectField("[R] Animation Controller", animationController, typeof(AnimatorController), true);
        animationAvatar = (Avatar)EditorGUILayout.ObjectField("[R] Animation Avatar", animationAvatar, typeof(Avatar), true);

        EditorGUILayout.Space();

        bool canContinue = (modelRootObject && eyeTransform && (spineBoneToRotate || !bulletSpawnerTransform) && animationController && targetObjParentTransform && animationAvatar);

        if (canContinue)
        {
            GUI.enabled = true;
            if (!bulletSpawnerTransform)
                EditorGUILayout.HelpBox("Bullet Spawn empty!  Bullet Spawn must be filled if you want to automatically spawn your bullets in a specific location. (This is the trnaform that marks the position and direction the bullets spawn in)", MessageType.Warning);
            if (!bulletObject)
                EditorGUILayout.HelpBox("Bullet Object empty!  Bullet Object must be filled if you want to automatically allow your agent to fire. (This is the object that your agent will fire)", MessageType.Warning);
            //else if (!weaponTransform)
            //    EditorGUILayout.HelpBox("Gun Transform empty!  Gun Transform must be filled if you want to automatically create an enemy with a gun. (This is the object that contains the model of your gun)", MessageType.Warning);
        }
        else
        {
            string strToWrite = "";
            if (!modelRootObject)
                strToWrite = "'Base Object' cannot be empty! (This is the object that is the parent of all other objects in your agent's hierarchy.)";
            else if (!spineBoneToRotate)
                strToWrite = "'Spine Bone To Rotate' cannot be empty! (This is the transform that rotates in order to aim your agent's gun at their target.)";
            else if (!eyeTransform)
                strToWrite = "'Eye Transform' cannot be empty! (This is the transform that designates the direction your AI looks in, as well as the position to be used for line of sight checks.)";
            else if (!animationController)
                strToWrite = "'Animation Controller' cannot be empty! (This is the the animation controller that governs your agent's animations.  You can create one by going to Assets>Create>Paragon AI Animation Controller.)";
            else if (!animationAvatar)
                strToWrite = "'Animation Avatar' cannot be empty!";
            else if (!targetObjParentTransform)
                strToWrite = "'Bone To Attach Target To' cannot be empty! (This is the transform that other agents will aim at when targetting this agent.)";


            EditorGUILayout.HelpBox(strToWrite, MessageType.Error);
            GUI.enabled = false;
        }


        EditorGUILayout.Space();

        if (GUILayout.Button("Create New AI"))
        {
            this.CreateANewAI();
            this.Close();
        }

        EditorGUILayout.EndScrollView();
    }

    void CreateANewAI()
    {
        AttachAIComponents(modelRootObject, spineBoneToRotate, targetObjParentTransform, animationController, bulletSpawnerTransform, animationAvatar, eyeTransform);
        this.Close();
    }

    void AttachAIComponents(GameObject modelBase, Transform spineBoneTransform, Transform targetParentTrans, AnimatorController animController, Transform bulletSpawnTransform, Avatar ava, Transform eye)
    {
        //Create parent object and move it to the position of the model base
        GameObject parentObj = new GameObject();
        parentObj.name = agentName;
        parentObj.transform.position = modelBase.transform.position;

        //parent model base to parent object
        modelBase.transform.parent = parentObj.transform;

        parentObj.AddComponent(typeof(UnityEngine.AI.NavMeshAgent));
        parentObj.AddComponent(typeof(AudioSource));
        if (bulletSpawnerTransform)
            bulletSpawnerTransform.gameObject.AddComponent(typeof(AudioSource));

        parentObj.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = 30;
        parentObj.GetComponent<UnityEngine.AI.NavMeshAgent>().angularSpeed = 100000;
        parentObj.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 2;

        ParagonAI.BaseScript newBaseScript = (ParagonAI.BaseScript)parentObj.AddComponent(typeof(ParagonAI.BaseScript));
        ParagonAI.RotateToAimGunScript newRotateToAimGunScript = (ParagonAI.RotateToAimGunScript)parentObj.AddComponent(typeof(ParagonAI.RotateToAimGunScript));
        ParagonAI.HealthScript newHealthScript = (ParagonAI.HealthScript)parentObj.AddComponent(typeof(ParagonAI.HealthScript));
        ParagonAI.SoundScript newSoundScript = (ParagonAI.SoundScript)parentObj.AddComponent(typeof(ParagonAI.SoundScript));
        ParagonAI.CoverFinderScript newCoverFinderScript = (ParagonAI.CoverFinderScript)parentObj.AddComponent(typeof(ParagonAI.CoverFinderScript));
        ParagonAI.GunScript newGunScript = (ParagonAI.GunScript)parentObj.AddComponent(typeof(ParagonAI.GunScript));
        ParagonAI.AnimationScript newAnimationScript = (ParagonAI.AnimationScript)parentObj.AddComponent(typeof(ParagonAI.AnimationScript));


        //Attach Animator to to modelBase if one doesn't exist
        if (!modelBase.GetComponent<Animator>())
            modelBase.AddComponent(typeof(Animator));

        //Assign controller to animator
        modelBase.GetComponent<Animator>().runtimeAnimatorController = animController;
        modelBase.GetComponent<Animator>().avatar = ava;

        //Recursively Attach hitboxes to all colliders
        AttachHitboxes(modelBase.transform, newHealthScript);

        //Create target object
        //ParagonAI.TargetScript currTarScript = (ParagonAI.TargetScript) targetObjParentTransform.gameObject.AddComponent(typeof(ParagonAI.TargetScript));
        ParagonAI.TargetScript currTarScript = (ParagonAI.TargetScript)parentObj.AddComponent(typeof(ParagonAI.TargetScript));
        currTarScript.targetObjectTransform = targetObjParentTransform;
        currTarScript.myTeamID = teamNumber;
        currTarScript.alliedTeamsIDs = new int[1];
        currTarScript.alliedTeamsIDs[0] = teamNumber;
        currTarScript.enemyTeamsIDs = new int[1];
        currTarScript.enemyTeamsIDs[0] = enemyTeamNumber;
        currTarScript.myAIBaseScript = newBaseScript;
        currTarScript.eyeTransform = eye;

        //Set connections		
        newBaseScript.gunScript = newGunScript;
        newBaseScript.audioScript = newSoundScript;
        newBaseScript.headLookScript = newRotateToAimGunScript;
        newBaseScript.animationScript = newAnimationScript;
        newBaseScript.coverFinderScript = newCoverFinderScript;

        newRotateToAimGunScript.spineBone = spineBoneToRotate;
        newRotateToAimGunScript.bulletSpawnTransform = bulletSpawnTransform;
        //newRotateToAimGunScript.myBodyTransform = modelBase.transform;

        newHealthScript.rigidbodies = modelBase.GetComponentsInChildren<Rigidbody>();

        for (var r = 0; r < newHealthScript.rigidbodies.Length; r++)
        {
            newHealthScript.rigidbodies[r].isKinematic = true;
        }


        newHealthScript.collidersToEnable = modelBase.GetComponentsInChildren<Collider>();
        newHealthScript.gunScript = newGunScript;
        newHealthScript.rotateToAimGunScript = newRotateToAimGunScript;
        newHealthScript.animator = modelBase.GetComponent<Animator>();
        newHealthScript.myTargetScript = currTarScript;
        newHealthScript.myAIBaseScript = newBaseScript;
        newHealthScript.soundScript = newSoundScript;

        newGunScript.myAIBaseScript = newBaseScript;
        newGunScript.animationScript = newAnimationScript;
        newGunScript.bulletSpawn = bulletSpawnerTransform;
        newGunScript.bulletObject = bulletObject;
        newGunScript.bulletSound = bulletSound;

        newAnimationScript.myBaseScript = newBaseScript;
        newAnimationScript.rotateGunScript = newRotateToAimGunScript;
        newAnimationScript.myAIBodyTransform = modelBase.transform;
        newAnimationScript.animator = modelBase.GetComponent<Animator>();
        newAnimationScript.gunScript = newGunScript;
        newAnimationScript.turnSpeed = 7.0f;
    }

    void AttachHitboxes(Transform t, ParagonAI.HealthScript hs)
    {
        if (t.gameObject.GetComponent<Collider>())
        {
            ParagonAI.HitBox hitboxNow = t.gameObject.AddComponent<ParagonAI.HitBox>() as ParagonAI.HitBox;

            if (t.gameObject.GetComponent<SphereCollider>())
                hitboxNow.damageMultiplyer = headHitMultiplier;
            else
                hitboxNow.damageMultiplyer = defaultHitMultiplier;
            hitboxNow.myScript = hs;

            t.transform.tag = hitBoxTag;
        }

        for (int c = 0; c < t.childCount; c++)
        {
            AttachHitboxes(t.GetChild(c), hs);
        }
    }

}








