using UnityEngine;
using System.Collections;



/*
 * This script lets users manually mark cover positions.  
 * */


namespace ParagonAI
{
    public class CoverNodeScript : MonoBehaviour
    {

        public Vector3 SightNodeOffSet = new Vector3(0, 1, 0);

        private Vector3 myPosition;
        private Vector3 sightNodePosition;
        public float nodeRadiusVisualization = 0.1f;

        public bool alwaysDisplay = true;

        public LayerMask layerMask;

        public bool isActive = true;

        private bool occupied = false;

        void Start()
        {
            SetPositions();
        }


        void SetPositions()
        {
            myPosition = transform.position;

            sightNodePosition = transform.position;

            sightNodePosition += (transform.forward * SightNodeOffSet.x);
            sightNodePosition += (transform.up * SightNodeOffSet.y);
            sightNodePosition += (transform.right * SightNodeOffSet.z);
        }

        public bool ValidCoverCheck(Vector3 targetPos)
        {
            //Check to see if this cover node is safe
            if (isActive)
            {
                if (Physics.Linecast(myPosition, targetPos, layerMask))
                {
                    //Check to see if this cover node has LOS to target from firingPos
                    if (!Physics.Linecast(sightNodePosition, targetPos, layerMask))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        public bool CheckForSafety(Vector3 targetPos)
        {
            //Debug.DrawLine(myPosition, targetPos, Color.green);
            //Debug.Break();
            return (Physics.Linecast(myPosition, targetPos, layerMask));
        }



        void OnDrawGizmosSelected()
        {
            if (!alwaysDisplay)
            {
                SetPositions();

                if (occupied)
                    Gizmos.color = Color.yellow;
                else if (isActive)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawSphere(myPosition, nodeRadiusVisualization);
                Gizmos.DrawWireSphere(sightNodePosition, nodeRadiusVisualization * 2);
            }
        }

        void OnDrawGizmos()
        {
            if (alwaysDisplay)
            {
                SetPositions();

                if (occupied)
                    Gizmos.color = Color.yellow;
                else if (isActive)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawSphere(myPosition, nodeRadiusVisualization);
                Gizmos.DrawWireSphere(sightNodePosition, nodeRadiusVisualization * 2);
            }
        }

        public Vector3 GetSightNodePosition()
        {
            return sightNodePosition;
        }

        public Vector3 GetPosition()
        {
            return myPosition;
        }

        public void ActivateNode(float t)
        {
            StartCoroutine(EnableThisNode(t));
        }

        IEnumerator EnableThisNode(float t)
        {
            yield return new WaitForSeconds(t);
            isActive = true;
        }

        public void DeActivateNode()
        {
            isActive = false;
        }

        public bool isOccupied()
        {
            return occupied;
        }

        public void setOccupied(bool b)
        {
            occupied = b;
        }

    }
}
