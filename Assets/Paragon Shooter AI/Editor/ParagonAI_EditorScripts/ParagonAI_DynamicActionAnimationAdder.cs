using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Animations;
using System.Collections.Generic;

public class ParagonAI_DynamicActionAnimationAdder : EditorWindow  {
	
	[MenuItem ("Assets/Create/Add A Paragon AI Dynamic Action")]		
    public static void  ShowWindow () 
	    {
	        EditorWindow.GetWindow(typeof(ParagonAI_DynamicActionAnimationAdder));      
	    }
	
	AnimatorController animationController = null;
	AnimationClip dynamicAnimation = null;
	string triggerName = "Dynamic Object";
	
	void OnGUI(){
        animationController = (AnimatorController)EditorGUILayout.ObjectField("Animation Controller", animationController, typeof(AnimatorController), true);
        dynamicAnimation = (AnimationClip)EditorGUILayout.ObjectField("Animation", dynamicAnimation, typeof(AnimationClip), true);	
        triggerName = EditorGUILayout.TextField("Trigger Name: ", triggerName);		
        
        if(animationController && dynamicAnimation)
        	{
				if(GUILayout.Button("Add Action"))
		        {
		            this.AddAnimation();
		            this.Close();
		        }
        	}
        else
        	{
        			EditorGUILayout.HelpBox("All fields must be filled before continuing!", MessageType.Warning);        	
        	}
	}
	
	void AddAnimation(){
        //Top Layer
        for (int i = 0; i < 2; i++)
        {
            //Create New State
            var stateMachine = animationController.layers[i].stateMachine;
            string stateName = triggerName + " State";

            animationController.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);

            AnimatorState newState = stateMachine.AddState(stateName);
            newState.motion = dynamicAnimation;

            //To		
            var anyToActionTransition = stateMachine.AddAnyStateTransition(newState);
            anyToActionTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0.25f, triggerName);

            //From
            newState.AddTransition(stateMachine.defaultState, true);
        }
	
	}
}

