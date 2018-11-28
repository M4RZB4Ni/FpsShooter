using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ParagonAI.BaseScript))]
[CanEditMultipleObjects]
public class BaseScriptInspector : Editor
{
    SerializedObject baseScript;

    bool showScripts = false;
    //bool showGeneralBehaviours = true;
    bool showSearching = false;
    bool showCover = false;
    bool showDodge = false;
    bool showPatrolling = false;
    bool showWandering = false;
    bool showMelee = false;

    //scripts
    SerializedProperty gunScriptProp;
    SerializedProperty audioScriptProp;
    SerializedProperty headLookScriptProp;
    SerializedProperty animationScriptProp;
    SerializedProperty coverFinderScriptProp;
    SerializedProperty myTargetScriptProp;
    SerializedProperty timeUntilBodyIsDestroyedAfterDeathProp;

    //General Stuff
    SerializedProperty cycleTimeProp;
    SerializedProperty sprintSpeedProp;
    SerializedProperty maxSpeedProp;
    SerializedProperty alertSpeedProp;
    SerializedProperty idleSpeedProp;
    SerializedProperty myAITypeProp;
    SerializedProperty myIdleBehaviourProp;

    //Searching
    SerializedProperty radiusToCallOffSearchProp;

    //Cover
    SerializedProperty timeBetweenSafetyChecksProp;
    SerializedProperty maxTimeInCoverProp;
    SerializedProperty minTimeInCoverProp;

    //Dodging
    SerializedProperty dodgingSpeedProp;
    SerializedProperty dodgingTimeProp;
    SerializedProperty dodgingClearHeightProp;
    SerializedProperty timeBetweenLoSDodgesProp;
    SerializedProperty shouldTryAndDodgeProp;
    SerializedProperty minDistToDodgeProp;

    //Patrolling
    SerializedProperty closeEnoughToPatrolNodeDistProp;
    SerializedProperty patrolNodesProp;
    SerializedProperty shouldShowPatrolPathProp;

    //Wandering
    SerializedProperty wanderDiameterProp;
    SerializedProperty distToChooseNewWanderPointProp;

    //Key Transform
    SerializedProperty keyTransformProp;

    //Melee
    SerializedProperty canMeleeProp;
    SerializedProperty meleeDamageProp;
    SerializedProperty timeBetweenMeleesProp;
    SerializedProperty meleeRangeProp;
    SerializedProperty timeUntilMeleeDamageIsDealtProp;

    //Custom Behaviours
    //SerializedProperty idleBehaviourProp;
    //SerializedProperty combatBehaviourProp;

    SerializedProperty canUseDynamicObjectProp;
    SerializedProperty shouldRunFromGrenadesProp;

    	
	SerializedProperty canSprint;
	SerializedProperty distFromTargetToSprint;

    void OnEnable()
    {
        baseScript = new SerializedObject(target);

        // Setup the SerializedProperties.
        gunScriptProp = serializedObject.FindProperty("gunScript");
        audioScriptProp = serializedObject.FindProperty("audioScript");
        headLookScriptProp = serializedObject.FindProperty("headLookScript");
        animationScriptProp = serializedObject.FindProperty("animationScript");
        coverFinderScriptProp = serializedObject.FindProperty("coverFinderScript");

        timeUntilBodyIsDestroyedAfterDeathProp = serializedObject.FindProperty("timeUntilBodyIsDestroyedAfterDeath");

        shouldRunFromGrenadesProp = serializedObject.FindProperty("shouldRunFromGrenades");

        cycleTimeProp = serializedObject.FindProperty("cycleTime");
        sprintSpeedProp = serializedObject.FindProperty("sprintSpeed");
        maxSpeedProp = serializedObject.FindProperty("runSpeed");
        alertSpeedProp = serializedObject.FindProperty("alertSpeed");
        idleSpeedProp = serializedObject.FindProperty("idleSpeed");
        myAITypeProp = serializedObject.FindProperty("myAIType");
        myIdleBehaviourProp = serializedObject.FindProperty("myIdleBehaviour");
             
        canSprint = serializedObject.FindProperty("canSprint");
        distFromTargetToSprint = serializedObject.FindProperty("distFromTargetToSprint");      

        radiusToCallOffSearchProp = serializedObject.FindProperty("radiusToCallOffSearch");

        timeBetweenSafetyChecksProp = serializedObject.FindProperty("timeBetweenSafetyChecks");
        maxTimeInCoverProp = serializedObject.FindProperty("maxTimeInCover");
        minTimeInCoverProp = serializedObject.FindProperty("minTimeInCover");

        dodgingSpeedProp = serializedObject.FindProperty("dodgingSpeed");
        dodgingTimeProp = serializedObject.FindProperty("dodgingTime");
        dodgingClearHeightProp = serializedObject.FindProperty("dodgingClearHeight");
        timeBetweenLoSDodgesProp = serializedObject.FindProperty("timeBetweenLoSDodges");
        shouldTryAndDodgeProp = serializedObject.FindProperty("shouldTryAndDodge");
        minDistToDodgeProp = serializedObject.FindProperty("minDistToDodge");

        closeEnoughToPatrolNodeDistProp = serializedObject.FindProperty("closeEnoughToPatrolNodeDist");
        patrolNodesProp = serializedObject.FindProperty("patrolNodes");
        shouldShowPatrolPathProp = serializedObject.FindProperty("shouldShowPatrolPath");

        wanderDiameterProp = serializedObject.FindProperty("wanderDiameter");
        distToChooseNewWanderPointProp = serializedObject.FindProperty("distToChooseNewWanderPoint");

        keyTransformProp = serializedObject.FindProperty("keyTransform");

        canMeleeProp = serializedObject.FindProperty("canMelee");
        meleeDamageProp = serializedObject.FindProperty("meleeDamage");
        timeBetweenMeleesProp = serializedObject.FindProperty("timeBetweenMelees");
        meleeRangeProp = serializedObject.FindProperty("meleeRange");
        timeUntilMeleeDamageIsDealtProp = serializedObject.FindProperty("timeUntilMeleeDamageIsDealt");

        //idleBehaviourProp = serializedObject.FindProperty("idleBehaviour");
        //combatBehaviourProp = serializedObject.FindProperty("combatBehaviour");

        canUseDynamicObjectProp = serializedObject.FindProperty("canUseDynamicObject");
    }

    public override void OnInspectorGUI()
    {
        baseScript.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(myAITypeProp);
        EditorGUILayout.PropertyField(myIdleBehaviourProp);
        //EditorGUILayout.PropertyField(idleBehaviourProp, true);
        //EditorGUILayout.PropertyField(combatBehaviourProp, true);
        EditorGUILayout.PropertyField(keyTransformProp, true);
        //idleBehaviourProp.objectReferenceValue = EditorGUILayout.ObjectField("Custom Idle Behaviour", idleBehaviourProp.objectReferenceValue, typeof(ParagonAI.CustomAIBehaviour), true);
        //combatBehaviourProp.objectReferenceValue = EditorGUILayout.ObjectField("Custom Combat Behaviour", combatBehaviourProp.objectReferenceValue, typeof(ParagonAI.CustomAIBehaviour), true);
        //keyTransformProp.objectReferenceValue = EditorGUILayout.ObjectField("Key Transform", keyTransformProp.objectReferenceValue, typeof(Transform), true);
        canUseDynamicObjectProp.boolValue = EditorGUILayout.Toggle("Can use dynamic objects", canUseDynamicObjectProp.boolValue);
        shouldRunFromGrenadesProp.boolValue = EditorGUILayout.Toggle("Should run from grenades", shouldRunFromGrenadesProp.boolValue);
        EditorGUILayout.PropertyField(timeUntilBodyIsDestroyedAfterDeathProp);

        EditorGUILayout.Separator();
        cycleTimeProp.floatValue = EditorGUILayout.FloatField("Time Between AI Cycles", cycleTimeProp.floatValue);
        sprintSpeedProp.floatValue = EditorGUILayout.FloatField("Sprint Speed", sprintSpeedProp.floatValue);
        maxSpeedProp.floatValue = EditorGUILayout.FloatField("Run Speed", maxSpeedProp.floatValue);
        alertSpeedProp.floatValue = EditorGUILayout.FloatField("Alert Speed", alertSpeedProp.floatValue);
        idleSpeedProp.floatValue = EditorGUILayout.FloatField("Idle Speed", idleSpeedProp.floatValue);
        
        EditorGUILayout.PropertyField(canSprint, true);
        EditorGUILayout.PropertyField(distFromTargetToSprint, true);


        EditorGUILayout.Separator();
        showSearching = EditorGUILayout.Foldout(showSearching, "Show Searching Parameters");
        if (showSearching)
        {
            EditorGUI.indentLevel++;
            radiusToCallOffSearchProp.floatValue = EditorGUILayout.FloatField("Radius to call off search", radiusToCallOffSearchProp.floatValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showCover = EditorGUILayout.Foldout(showCover, "Show Cover Parameters");
        if (showCover)
        {
            EditorGUI.indentLevel++;
            timeBetweenSafetyChecksProp.floatValue = EditorGUILayout.FloatField("Time between cover safety checks", timeBetweenSafetyChecksProp.floatValue);
            maxTimeInCoverProp.floatValue = EditorGUILayout.FloatField("Maximum time in cover", maxTimeInCoverProp.floatValue);
            minTimeInCoverProp.floatValue = EditorGUILayout.FloatField("Minimum time in cover", minTimeInCoverProp.floatValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showDodge = EditorGUILayout.Foldout(showDodge, "Show Dodging Parameters");
        if (showDodge)
        {
            EditorGUI.indentLevel++;
            shouldTryAndDodgeProp.boolValue = EditorGUILayout.Toggle("Should dodge", shouldTryAndDodgeProp.boolValue);
            dodgingSpeedProp.floatValue = EditorGUILayout.FloatField("Dodging speed", dodgingSpeedProp.floatValue);
            dodgingTimeProp.floatValue = EditorGUILayout.FloatField("Dodging time", dodgingTimeProp.floatValue);
            dodgingClearHeightProp.floatValue = EditorGUILayout.FloatField("Dodging clear height", dodgingClearHeightProp.floatValue);
            timeBetweenLoSDodgesProp.floatValue = EditorGUILayout.FloatField("Time between dodges", timeBetweenLoSDodgesProp.floatValue);
            minDistToDodgeProp.floatValue = EditorGUILayout.FloatField("Minimum distance from target to dodge", minDistToDodgeProp.floatValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showPatrolling = EditorGUILayout.Foldout(showPatrolling, "Show Patroliing Parameters");
        if (showPatrolling)
        {
            EditorGUI.indentLevel++;
            closeEnoughToPatrolNodeDistProp.floatValue = EditorGUILayout.FloatField("Close enough to patrol node distance", closeEnoughToPatrolNodeDistProp.floatValue);
            //Draw Array
            DrawArray(patrolNodesProp);
            shouldShowPatrolPathProp.boolValue = EditorGUILayout.Toggle("Show patrol path", shouldShowPatrolPathProp.boolValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showWandering = EditorGUILayout.Foldout(showWandering, "Show Wander Parameters");
        if (showWandering)
        {
            EditorGUI.indentLevel++;
            wanderDiameterProp.floatValue = EditorGUILayout.FloatField("Wander Diameter", wanderDiameterProp.floatValue);
            distToChooseNewWanderPointProp.floatValue = EditorGUILayout.FloatField("Dist to choose new wander point", distToChooseNewWanderPointProp.floatValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showMelee = EditorGUILayout.Foldout(showMelee, "Show Melee Parameters");
        if (showMelee)
        {
            EditorGUI.indentLevel++;
            canMeleeProp.boolValue = EditorGUILayout.Toggle("Can melee", canMeleeProp.boolValue);
            meleeDamageProp.floatValue = EditorGUILayout.FloatField("Melee damage", meleeDamageProp.floatValue);
            timeBetweenMeleesProp.floatValue = EditorGUILayout.FloatField("Time between melee attacks", timeBetweenMeleesProp.floatValue);
            meleeRangeProp.floatValue = EditorGUILayout.FloatField("Melee range", meleeRangeProp.floatValue);
            timeUntilMeleeDamageIsDealtProp.floatValue = EditorGUILayout.FloatField("Time until damage is dealt", timeUntilMeleeDamageIsDealtProp.floatValue);
            EditorGUI.indentLevel--;
        }

        //EditorGUILayout.Separator();
        showScripts = EditorGUILayout.Foldout(showScripts, "Show linked componants");
        if (showScripts)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gunScriptProp, true);
            EditorGUILayout.PropertyField(audioScriptProp, true);
            EditorGUILayout.PropertyField(headLookScriptProp, true);
            EditorGUILayout.PropertyField(animationScriptProp, true);
            EditorGUILayout.PropertyField(coverFinderScriptProp, true);
            //gunScriptProp.objectReferenceValue = EditorGUILayout.ObjectField("Gun Script", gunScriptProp.objectReferenceValue, typeof(ParagonAI.GunScript), true);
            //audioScriptProp.objectReferenceValue = EditorGUILayout.ObjectField("Audio Script", audioScriptProp.objectReferenceValue, typeof(ParagonAI.SoundScript), true);
            //headLookScriptProp.objectReferenceValue = EditorGUILayout.ObjectField("Rotate To Aim Gun Script", headLookScriptProp.objectReferenceValue, typeof(ParagonAI.RotateToAimGunScript), true);
            //animationScriptProp.objectReferenceValue = EditorGUILayout.ObjectField("Animation Script", animationScriptProp.objectReferenceValue, typeof(ParagonAI.AnimationScript), true);
            //coverFinderScriptProp.objectReferenceValue = EditorGUILayout.ObjectField("Cover Finder Script", coverFinderScriptProp.objectReferenceValue, typeof(ParagonAI.CoverFinderScript), true);
            EditorGUI.indentLevel--;
        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    public void DrawArray(SerializedProperty prop)
    {
        //EditorGUIUtility.LookLikeControls();
        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prop, true);
        if(EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
