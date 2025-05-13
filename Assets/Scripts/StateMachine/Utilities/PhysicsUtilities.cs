using System;
using UnityEngine;

namespace FrameLabs.Utilities
{
    /// <summary>
    /// Author: Thanh Hon
    /// Date: 2/07/2024
    /// Description: A utility class providing common physics methods.
    /// </summary>
    public sealed class PhysicsUtilities
    {
        public PhysicsUtilities() { }

        private static readonly Lazy<PhysicsUtilities> instance = new Lazy<PhysicsUtilities>();
        public static PhysicsUtilities Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Determines if the target GameObject is within a cone-shaped area in front of the origin GameObject.
        /// </summary>
        /// <param name="origin">The GameObject from which the cone originates.</param>
        /// <param name="target">The GameObject to check if it is within the cone.</param>
        /// <param name="angle">The angle of the cone in degrees.</param>
        /// <param name="distance">The maximum distance to check within the cone.</param>
        /// <param name="layerMask">Optional LayerMask to consider only specific layers in the check.</param>
        /// <returns>True if the target is within the cone and there is a direct line of sight; otherwise, false.</returns>
        public bool IsInCone(GameObject origin, GameObject target, float horizontalAngle, float verticalAngle, float distance, int layerMask = 0)
        {
            // Get the positions of the origin and target GameObjects
            Vector3 originPosition = origin.transform.position;
            Vector3 targetPosition = target.transform.position;

            // Calculate the vector from the origin to the target
            Vector3 directionToTarget = (targetPosition - originPosition).normalized;

            // Calculate half of the given angles to simplify the angle comparison
            float halfHorizontalAngle = horizontalAngle * 0.5f;
            float halfVerticalAngle = verticalAngle * 0.5f;

            // Calculate the angle between the forward direction of the origin and the direction to the target (horizontal)
            float horizontalAngleToTarget = Vector3.Angle(origin.transform.forward, directionToTarget);

            // Project the direction vector onto a plane that is perpendicular to the 'right' vector to get the vertical component
            Vector3 verticalDirection = Vector3.ProjectOnPlane(directionToTarget, origin.transform.right).normalized;
            float verticalAngleToTarget = Vector3.Angle(origin.transform.forward, verticalDirection);

            // Check if the target is within the horizontal and vertical angles
            if (horizontalAngleToTarget > halfHorizontalAngle || verticalAngleToTarget > halfVerticalAngle)
            {
                // Target is outside the cone angle
                return false;
            }

            // Define the radius for the sphere cast for accurate detection
            float sphereCastRadius = 0.5f;  // Example radius; adjust as necessary

            // Perform a sphere cast to check if there is any object in the direction to the target within the specified distance
            if (Physics.SphereCast(originPosition, sphereCastRadius, directionToTarget, out RaycastHit hit, distance, layerMask))
            {
                // If the sphere cast hits an object, check if it's the target
                return hit.transform.gameObject == target;
            }

            return false;  // No object detected in the cone
        }

        /// <summary>
        /// Determines if the specified GameObject is colliding with another specified GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to check for collisions.</param>
        /// <param name="hasCollidedWith">The GameObject to check if it has collided with the first object.</param>
        /// <param name="layerMask">Optional LayerMask to consider only specific layers in the collision check.</param>
        /// <returns>True if the specified GameObject is colliding with the other specified GameObject; otherwise, false.</returns>
        public bool IsColliding( GameObject obj, GameObject hasCollidedWith, int layerMask = 0 )
        {
            // Get the collider of the first GameObject
            Collider collider = obj.GetComponent<Collider>();

            // If the first GameObject does not have a collider, return false
            if( collider == null )
            {
                return false;
            }

            // Check for collision based on collider type
            switch( collider )
            {
                case BoxCollider box:
                    // Calculate the center of the box collider in world space
                    Vector3 boxCenter = box.transform.TransformPoint( box.center );
                    // Perform an OverlapBox check to find colliders within the box bounds
                    Collider[] boxColliders = Physics.OverlapBox( boxCenter, box.size / 2, box.transform.rotation, layerMask );
                    // Iterate through the colliders to see if any match the second GameObject
                    foreach( var col in boxColliders )
                    {
                        if( col.gameObject == hasCollidedWith )
                        {
                            return true;
                        }
                    }
                    break;

                case SphereCollider sphere:
                    // Calculate the center of the sphere collider in world space
                    Vector3 sphereCenter = sphere.transform.TransformPoint( sphere.center );
                    // Perform an OverlapSphere check to find colliders within the sphere bounds
                    Collider[] sphereColliders = Physics.OverlapSphere( sphereCenter, sphere.radius, layerMask );
                    // Iterate through the colliders to see if any match the second GameObject
                    foreach( var col in sphereColliders )
                    {
                        if( col.gameObject == hasCollidedWith )
                        {
                            return true;
                        }
                    }
                    break;

                case CapsuleCollider capsule:
                    // Calculate the center of the capsule collider in world space
                    Vector3 capsuleCenter = capsule.transform.TransformPoint( capsule.center );
                    float radius = capsule.radius;
                    float height = capsule.height / 2 - radius;
                    Vector3 direction = capsule.transform.up;
                    // Perform an OverlapCapsule check to find colliders within the capsule bounds
                    Collider[] capsuleColliders = Physics.OverlapCapsule( capsuleCenter + direction * height, capsuleCenter - direction * height, radius, layerMask );
                    // Iterate through the colliders to see if any match the second GameObject
                    foreach( var col in capsuleColliders )
                    {
                        if( col.gameObject == hasCollidedWith )
                        {
                            return true;
                        }
                    }
                    break;

                default:
                case MeshCollider:
                    Debug.Log( "Unsupported collider type." );
                    break;
            }

            return false;
        }

        // <summary>
        /// Casts a ray from the given origin in the specified direction and checks for collisions within the given distance and layer mask.
        /// </summary>
        /// <param name="origin">The GameObject from which the ray is cast.</param>
        /// <param name="direction">The direction of the ray in world space coordinates.</param>
        /// <param name="distance">The maximum distance the ray should check for collisions.</param>
        /// <param name="layerMask">The LayerMask used to filter which objects the ray can interact with.</param>
        /// <returns>Returns the GameObject that the ray hits, or null if nothing is hit.</returns>
        public GameObject CastRay(GameObject origin, Vector3 direction, float distance, LayerMask layerMask)
        {
            GameObject target = null;

            // Get the origin's position
            Vector3 originPosition = origin.transform.position;

            // Cast the ray from the origin in the specified direction
            if (Physics.Raycast(originPosition, direction, out RaycastHit hit, distance, layerMask))
            {
                // If it hits something, return the GameObject that was hit
                target = hit.transform.gameObject;
            }

            return target;
        }

        /// <summary>
        /// Determines if there is a direct line of sight between the origin GameObject and the target GameObject within a specified distance.
        /// </summary>
        /// <param name="origin">The GameObject from which the line of sight originates.</param>
        /// <param name="target">The GameObject to check if it is within direct line of sight.</param>
        /// <param name="distance">The maximum distance to check for a direct line of sight.</param>
        /// <param name="layerMask">Optional LayerMask to consider only specific layers in the check.</param>
        /// <returns>True if there is a direct line of sight to the target within the specified distance; otherwise, false.</returns>
        public bool DirectLine(GameObject origin, GameObject target, float distance, int layerMask = 0, int rayCount = 10, float coneAngle = 10f)
        {
            // Get the positions of the origin and target GameObjects
            Vector3 originPosition = origin.transform.position;
            Vector3 targetPosition = target.transform.position;

            // Calculate the normalized direction vector from the origin to the target
            Vector3 directionToTarget = (targetPosition - originPosition).normalized;

            // If a single central ray already hits the target directly, return true
            if (Physics.Raycast(originPosition, directionToTarget, out RaycastHit hit, distance, layerMask))
            {
                if (hit.transform.gameObject == target)
                {
                    return true;
                }
            }

            // Perform multiple rays within a cone around the central direction
            for (int i = 0; i < rayCount; i++)
            {
                // Randomly generate a direction within the cone
                Vector3 randomDirection = RandomConeDirection(directionToTarget, coneAngle);

                // Perform a raycast in the random direction
                if (Physics.Raycast(originPosition, randomDirection, out hit, distance, layerMask))
                {
                    // Check if the hit object is the target GameObject
                    if (hit.transform.gameObject == target)
                    {
                        return true; // Return true if any ray hits the target directly
                    }
                }
            }

            return false; // No ray hit the target
        }

        // Helper method to generate a random direction within a cone
        private Vector3 RandomConeDirection(Vector3 forward, float angle)
        {
            // Convert the angle to radians
            float angleInRadians = angle * Mathf.Deg2Rad;

            // Generate a random point on the surface of a unit sphere
            Vector3 randomPoint = UnityEngine.Random.insideUnitSphere;

            // Rotate the random point around the forward direction within the cone's angle
            Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, angle), forward);
            Vector3 randomDirection = rotation * randomPoint;

            // Ensure the random direction is within the angle of the cone
            if (Vector3.Angle(forward, randomDirection) > angle / 2)
            {
                randomDirection = Vector3.Slerp(randomDirection, forward, 0.5f);
            }

            return randomDirection.normalized;
        }


        

    }
}