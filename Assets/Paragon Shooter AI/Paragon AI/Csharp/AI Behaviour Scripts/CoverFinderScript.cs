using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This script finds cover based on an agent's position, target position, and a key position in the level
 * First tries to find static cover, but if none can be found, dynamic cover is used
 * */

namespace ParagonAI
{
    public class CoverFinderScript : MonoBehaviour
    {

        //Cover Seek Methods
        public CoverSeekMethods currentCoverSeekMethod = CoverSeekMethods.WithinCombatRange;
        public enum CoverSeekMethods
        {
            RandomCover = 0, WithinCombatRange = 1, AdvanceTowardsTarget = 2,
        }

        //Distance Stuff	
        private Vector3 lastCoverPos;
        public float minDistBetweenLastCover = 20;
        private float minDistBetweenLastCoverSquared = 10;
        public float minCoverDistFromEnemy = 10;
        public float maxCoverDistFromEnemy = 50;
        //Use squared values for cheaper distance check via Vector3.SqrMagnitude
        private float maxCoverDistSqrd;
        private float minCoverDistSqrd;
        public float maxDistToCover = 9999;

        public float minDistToAdvance = 5;

        private ParagonAI.CoverNodeScript[] coverNodeScripts;

        //Dynamic cover
        public bool shouldUseDynamicCover = true;
        public bool useFirstDynamicCoverFound = false;
        public float dynamicCoverMaxDistFromMe = 60;
        //private var dynamicCoverMaxDistFromMeSqr : float;
        public float dynamicCoverNodeHeightOffset = 0.3f;
        public float dynamicCoverNodeFireOffset = 1.5f;
        public float dynamicCoverWidthNeededToHide = 1.5f;
        public float maxDistBehindDynamicCover = 5;

        public float defendingDist = 20;
        private float defendingDistSquared = 20;
        private Transform myTransform;

        [HideInInspector]
        public LayerMask layerMask;

        // Use this for initialization
        void Start()
        {
            myTransform = transform;
            //dynamicCoverMaxDistFromMeSqr = dynamicCoverMaxDistFromMe*dynamicCoverMaxDistFromMe;			
            maxCoverDistSqrd = maxCoverDistFromEnemy * maxCoverDistFromEnemy;
            minCoverDistSqrd = minCoverDistFromEnemy * minCoverDistFromEnemy;
            layerMask = ParagonAI.ControllerScript.currentController.GetLayerMask();
            defendingDistSquared = defendingDist * defendingDist;
            minDistToAdvance = minDistToAdvance * minDistToAdvance;
            if (ParagonAI.ControllerScript.currentController != null)
            {
                coverNodeScripts = ParagonAI.ControllerScript.currentController.GetCovers();
            }
            else
            {
                Debug.LogWarning("No Controller has been detected!  An AIControllerScript is required for the AI to work!  Please create a new gameObject and attach the Paragon AI ControllerScript to it!");
            }
        }

        //Utility Stuff	
        public void ResetLastCoverPos()
        {
            lastCoverPos = new Vector3(100000, 100000, 100000);
        }

        public ParagonAI.CoverData FindCover(Transform targetTransform)
        {
            return FindStaticCover(targetTransform, null);
        }

        public ParagonAI.CoverData FindCover(Transform targetTransform, Transform transformToDefend)
        {
            return FindStaticCover(targetTransform, transformToDefend);
        }

        // Static Cover
        ParagonAI.CoverData FindStaticCover(Transform targetTransform, Transform transformToDefend)
        {
            if (targetTransform && myTransform)
            {
                Vector3 targetTransformPos = targetTransform.position;

                //Closest Covers
                if (currentCoverSeekMethod == CoverSeekMethods.WithinCombatRange)
                {
                    return FindCoverWithinCombatRange(targetTransformPos, transformToDefend);
                }
                //Advance Towards Cover
                else if (currentCoverSeekMethod == CoverSeekMethods.AdvanceTowardsTarget)
                {
                    return FindCoverWithinCombatRange(targetTransformPos, transformToDefend);
                }
                //Random Cover
                else
                {
                    return FindRandomCover(targetTransformPos, transformToDefend);
                }

            }
            ParagonAI.CoverData bsData = new ParagonAI.CoverData();
            return bsData;
        }

        //Agent will try and find cover that is within a given range of the target.
        ParagonAI.CoverData FindCoverWithinCombatRange(Vector3 targetTransformPos, Transform transformToDefend)
        {
            int i = 0;
            Vector3 myPos = myTransform.position;
            ParagonAI.CoverNodeScript currentCoverNodeScript = null;
            float closestDistSquared = maxDistToCover;
            float nodeCheckingNowDistSquared;
            float distToTargetSquared;

            //We will take cover outside of the desired range if we can't find any within.
            bool foundCoverWithinAcceptableRange = false;
            for (i = 0; i < coverNodeScripts.Length; i++)
            {
                //Check if the node we are checking is occupied and within acceptable distances to key points
                if (!coverNodeScripts[i].isOccupied() && Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - lastCoverPos) > minDistBetweenLastCoverSquared &&
                  (!transformToDefend || Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - transformToDefend.position) < defendingDistSquared))
                {
                    distToTargetSquared = Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - targetTransformPos);
                    nodeCheckingNowDistSquared = Vector3.SqrMagnitude(myPos - coverNodeScripts[i].GetPosition());
                    //Check for line of sight
                    if (coverNodeScripts[i].ValidCoverCheck(targetTransformPos))
                    {
                        //Prefer nodes within othe agent's combat range
                        if (minCoverDistSqrd < distToTargetSquared && maxCoverDistSqrd > distToTargetSquared)
                        {
                            if (!foundCoverWithinAcceptableRange || (nodeCheckingNowDistSquared < closestDistSquared))
                            {
                                closestDistSquared = nodeCheckingNowDistSquared;
                                currentCoverNodeScript = coverNodeScripts[i];
                                foundCoverWithinAcceptableRange = true;
                            }
                        }
                        //Check if this is the closest so far 
                        else if (!foundCoverWithinAcceptableRange && nodeCheckingNowDistSquared < closestDistSquared)
                        {
                            closestDistSquared = nodeCheckingNowDistSquared;
                            currentCoverNodeScript = coverNodeScripts[i];
                        }
                    }
                }
            }

            //pass the data to the script that asked for cover
            if (currentCoverNodeScript != null)
            {
                lastCoverPos = currentCoverNodeScript.GetPosition();
                return new ParagonAI.CoverData(true, currentCoverNodeScript.GetPosition(), currentCoverNodeScript.GetSightNodePosition(), false, currentCoverNodeScript);
            }

            //Only bother with dynamic cover if we need it
            if (shouldUseDynamicCover)
            {
                return FindDynamicCover(targetTransformPos, transformToDefend);
            }

            return new ParagonAI.CoverData();
        }

        //The agent will find cover that is closer to their target each time they change cover locations
        ParagonAI.CoverData FindAdvancingCover(Vector3 targetTransformPos, Transform transformToDefend)
        {
            int i = 0;
            Vector3 myPos = myTransform.position;
            ParagonAI.CoverNodeScript currentCoverNodeScript = null;

            //Will find closest cover that is nearer than the last one we have if possible.
            //If not, we'll move to the target.
            Vector3 posToAdvanceTo;

            if (transformToDefend)
                posToAdvanceTo = transformToDefend.position;
            else
                posToAdvanceTo = targetTransformPos;

            float distBetweenMeAndTarget = Vector3.SqrMagnitude(myPos - posToAdvanceTo) - minDistToAdvance;
            float closestDistBetweenMeAndCover = distBetweenMeAndTarget;

            for (i = 0; i < coverNodeScripts.Length; i++)
            {
                if (!coverNodeScripts[i].isOccupied())
                {
                    float sqrDistBetweenNodeAndTargetPos = Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - posToAdvanceTo);
                    //Check if we'll be closer to target than we stand now
                    if (sqrDistBetweenNodeAndTargetPos < distBetweenMeAndTarget)
                    {
                        //Check if this node is closest to us
                        if (Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - myPos) < closestDistBetweenMeAndCover)
                        {
                            //Check if node is safe
                            if (coverNodeScripts[i].ValidCoverCheck(targetTransformPos))
                            {
                                closestDistBetweenMeAndCover = sqrDistBetweenNodeAndTargetPos;
                                currentCoverNodeScript = coverNodeScripts[i];
                            }
                        }
                    }
                }
            }
            if (currentCoverNodeScript != null)
            {
                lastCoverPos = currentCoverNodeScript.GetPosition();
                return new ParagonAI.CoverData(true, currentCoverNodeScript.GetPosition(), currentCoverNodeScript.GetSightNodePosition(), false, currentCoverNodeScript);
            }

            //Dynamic advancing cover is NOT supported

            return new ParagonAI.CoverData();
        }

        //The agent will use a random peice of cover that satisfies the line of sight requirements.  
        ParagonAI.CoverData FindRandomCover(Vector3 targetTransformPos, Transform transformToDefend)
        {
            int i = 0;
            ParagonAI.CoverNodeScript currentCoverNodeScript = null;
            List<ParagonAI.CoverNodeScript> availableCoverNodeScripts = new List<ParagonAI.CoverNodeScript>();

            //Fill a list with potential nodes
            for (i = 0; i < coverNodeScripts.Length; i++)
            {
                if (!coverNodeScripts[i].isOccupied())
                {
                    if (coverNodeScripts[i].ValidCoverCheck(targetTransformPos) && (!transformToDefend || Vector3.SqrMagnitude(coverNodeScripts[i].GetPosition() - transformToDefend.position) < defendingDistSquared))
                    {
                        availableCoverNodeScripts.Add(coverNodeScripts[i]);
                    }
                }
            }

            if (availableCoverNodeScripts.Count > 0)
            {
                //Pick a random node
                currentCoverNodeScript = availableCoverNodeScripts[Random.Range(0, availableCoverNodeScripts.Count)];
                lastCoverPos = currentCoverNodeScript.GetPosition();

                return new ParagonAI.CoverData(true, currentCoverNodeScript.GetPosition(), currentCoverNodeScript.GetSightNodePosition(), false, currentCoverNodeScript);
            }
            //Only bother with dynamic cover if we need it
            if (shouldUseDynamicCover)
            {
                return FindDynamicCover(targetTransformPos, transformToDefend);
            }

            return new ParagonAI.CoverData();
        }

        //Dynamic Cover
        ParagonAI.CoverData FindDynamicCover(Vector3 targetTransformPos, Transform transformToDefend)
        {
            Vector3[] verts = UnityEngine.AI.NavMesh.CalculateTriangulation().vertices;
            Vector3 myPos;
            if (!transformToDefend)
                myPos = myTransform.position;
            else
                myPos = transformToDefend.position;
            myPos.y += dynamicCoverNodeHeightOffset;
            var hideOffset = dynamicCoverNodeFireOffset - dynamicCoverNodeHeightOffset;
            //int nodesFound = 0;			
            Vector3 hidingPosCheckingNow;
            int x = 0;
            //int y;			
            float currDistTarget;

            Vector3 coverHidePos = Vector3.zero;
            Vector3 coverFirePos = Vector3.zero;

            float closestDistToMeSoFarSqr = dynamicCoverMaxDistFromMe * dynamicCoverMaxDistFromMe;
            float distBetweenMeAndCoverNow;

            bool shouldCont = true;

            //Use each vertex on the navmesh as a potential "firing position"
            //Then test whether we can hide from enemy fire by either crouching or moving off to the side (distance to move is hideOffset)
            //If we can see the enemy from the firing position and not see them from the hiding position, then it is a valid cover spot.

            for (int i = 0; i < verts.Length; i++)
            {
                //random value to make sure we don't take the same cover every time
                if (Random.value > 0.5 && Vector3.SqrMagnitude(verts[i] - myPos) > minDistBetweenLastCover)
                {
                    currDistTarget = Vector3.SqrMagnitude(verts[i] - targetTransformPos);
                    distBetweenMeAndCoverNow = Vector3.SqrMagnitude(verts[i] - myPos);

                    if (currDistTarget > minCoverDistSqrd && currDistTarget < maxCoverDistSqrd && distBetweenMeAndCoverNow < closestDistToMeSoFarSqr)
                    {
                        verts[i].y += dynamicCoverNodeFireOffset;

                        //If we can fire from here		
                        if (!Physics.Linecast(verts[i], targetTransformPos, layerMask))
                        {
                            verts[i].y -= hideOffset;
                            //Debug.Break();


                            if (Physics.Raycast(verts[i], targetTransformPos - verts[i], maxDistBehindDynamicCover, layerMask) && !ParagonAI.ControllerScript.currentController.isDynamicCoverSpotCurrentlyUsed(verts[i]))
                            {
                                shouldCont = true; 
                                										
                                //If chest high wall
                                hidingPosCheckingNow = verts[i];
                                //Check to make sure we have clear LoS between the firing position and the move position.
                                if (!Physics.Linecast(targetTransformPos, verts[i], layerMask) && Physics.Linecast(hidingPosCheckingNow, targetTransformPos, layerMask))
                                {
                                    closestDistToMeSoFarSqr = distBetweenMeAndCoverNow;
                                    coverHidePos = hidingPosCheckingNow;
                                    coverFirePos = verts[i];
                                    shouldCont = false;
                                    if (useFirstDynamicCoverFound)
                                    {
                                        i = verts.Length;
                                    }
                                }

                                //Check for side cover
                                if (shouldCont)
                                {
                                    for (x = -1; x <= 1; x += 2)
                                    {
                                        hidingPosCheckingNow = verts[i] + myTransform.right * x * dynamicCoverWidthNeededToHide;
                                        //If we're safe
                                        if (!Physics.Linecast(hidingPosCheckingNow, verts[i], layerMask) && Physics.Linecast(hidingPosCheckingNow, targetTransformPos, layerMask))
                                        {
                                            //lastCoverPos = hidingPosCheckingNow;
                                            //return new ParagonAI.CoverData(true, hidingPosCheckingNow, verts[i], true, null);
                                            closestDistToMeSoFarSqr = distBetweenMeAndCoverNow;
                                            coverHidePos = hidingPosCheckingNow;
                                            coverFirePos = verts[i];
                                            shouldCont = false;
                                            if (useFirstDynamicCoverFound)
                                            {
                                                i = verts.Length;
                                            }
                                        }

                                        hidingPosCheckingNow = verts[i] + myTransform.forward * x * dynamicCoverWidthNeededToHide;

                                        //If we're safe
                                        if (shouldCont && !Physics.Linecast(hidingPosCheckingNow, verts[i], layerMask) && Physics.Linecast(hidingPosCheckingNow, targetTransformPos, layerMask))
                                        {
                                            //return new ParagonAI.CoverData(true, hidingPosCheckingNow, verts[i], true, null);
                                            closestDistToMeSoFarSqr = distBetweenMeAndCoverNow;
                                            coverHidePos = hidingPosCheckingNow;
                                            coverFirePos = verts[i];
                                            if (useFirstDynamicCoverFound)
                                            {
                                                i = verts.Length;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            if (coverHidePos != Vector3.zero)
            {
                lastCoverPos = coverHidePos;
                return new ParagonAI.CoverData(true, coverHidePos, coverFirePos, true, null);
            }

            return new ParagonAI.CoverData();
        }
    }
}

namespace ParagonAI
{
    public class CoverData
    {
        public bool foundCover = false;
        public Vector3 hidingPosition;
        public Vector3 firingPosition;
        public bool isDynamicCover;
        public ParagonAI.CoverNodeScript coverNodeScript;

        public CoverData(bool f, Vector3 hp, Vector3 fP, bool d, ParagonAI.CoverNodeScript cns)
        {
            foundCover = f;
            hidingPosition = hp;
            firingPosition = fP;
            isDynamicCover = d;
            coverNodeScript = cns;
        }

        public CoverData()
        {
            foundCover = false;
        }
    }
}
