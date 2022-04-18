using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CREDIT: Largely based on work by Tvtig https://github.com/Tvtig/UnityLightsaber/blob/master/Assets/Scripts
//PLANNED FEATURES/CHANGES:
//      -System for culling bifurcated objects which are too small
//      -Measures for preventing bifurcated halves from exploding away from each other with great velocity
//      -Alternative for CreateNewMeshObject which actually starts with a duplicate of the original (with all attached components and values)
//      -Better naming system (first bifurcation changes name to "name_bifurcated", following ones add "x2" to the end)
//      -Solve problems with vertex normals (add shared vertices to multiple tris for smooth surfaces, do not weld vertices in identical positions (separate smoothing groups))
//      -Functionality for a multi-plane bifurcation which just deletes the stuff in between the two planes (should be relatively straightforward)

namespace Assets.Scripts.Deformation
{
    /// <summary>
    /// Handles the process of slicing a mesh into two separate halves.
    /// </summary>
    static class Bifurcator
    {
        //PUBLIC METHODS:
        /// <summary>
        /// Cuts target Deformable object into two separate halves using given plane.
        /// </summary>
        /// <param name="target">Deformable attached to object being cut</param>
        /// <param name="cuttingPlane">Plane used to determine placement of cut</param>
        /// <returns></returns>
        public static GameObject[] Bifurcate(Deformable target, Plane cuttingPlane)
        {
            BifurcationData data = new BifurcationData(target.meshFilter.mesh, cuttingPlane); //Cut the entire mesh of given deformable using given plane
            GameObject positiveObject = CreateNewMeshObject(target.gameObject, data.posMesh); //Create object with positive mesh properties
            GameObject negativeObject = CreateNewMeshObject(target.gameObject, data.negMesh); //Create object with negative mesh properties
            UnityEngine.Object.Destroy(target.gameObject);                                    //Destroy original object (shaky about doing this in a static class)
            return new GameObject[] { positiveObject, negativeObject };                       //Return bifurcated objects
        }

        //PRIVATE METHODS:
        private static GameObject CreateNewMeshObject(GameObject original, Mesh mesh)
        {
            //Function: Generates a new gameObject with components from original and given mesh

            //Initialization:
            GameObject newObject = new GameObject(); //Create new gameobject

            //Add components:
            newObject.AddComponent<MeshFilter>();   //Add mesh filter
            newObject.AddComponent<MeshRenderer>(); //Add mesh renderer
            newObject.AddComponent<MeshCollider>(); //Add mesh collider
            newObject.AddComponent<Rigidbody>();    //Add rigidbody
            //newObject.AddComponent<Deformable>();   //Add deformable script

            //Set transform properties:
            newObject.transform.position = original.transform.position;     //Set position
            newObject.transform.rotation = original.transform.rotation;     //Set rotation
            newObject.transform.localScale = original.transform.localScale; //Set scale

            //Set component properties:
            newObject.GetComponent<MeshFilter>().mesh = mesh;         //Set object shape
            newObject.GetComponent<MeshCollider>().sharedMesh = mesh; //Set physics shape (NOTE: Change from sharedMesh to mesh?)
            newObject.GetComponent<MeshCollider>().convex = true;     //Make collisions convex
            newObject.GetComponent<MeshRenderer>().materials = original.GetComponent<MeshRenderer>().materials; //Match material from original object

            //Cleanup:
            newObject.tag = original.tag;   //Match tag from original object
            newObject.name = original.name; //Match name from original object
            return newObject;               //Return generated gameObject
        }

        private class BifurcationData
        {
            //Description: Simulates bifurcation of a mesh and stores resulting meshes

            //Classes, Structs & Enums:
            private enum MeshSide { Positive, Negative } //Enum for easily differentiating sides of a mesh

            //Input Components:
            private readonly Mesh mesh;                      //Mesh being bifurcated
            private readonly Plane plane;                    //Cutter plane for bifurcating mesh
            private readonly List<Vector3> pointsAlongPlane; //List of points where plane intersects a side

            //Simulated Mesh Components:
            private List<Vector3> posVerts, negVerts; //Containers for vertices added to each mesh side
            private List<int> posTris, negTris;       //Containers for triangles added to each mesh side
            private List<Vector2> posUVs, negUVs;     //Containers for UVs added to each mesh side
            private List<Vector3> posNorms, negNorms; //Containers for normals added to each mesh side

            //Output Vars:
            public readonly Mesh posMesh; //Mesh created on positive side of cutting plane
            public readonly Mesh negMesh; //Mesh created on negative side of cutting plane

            //CONSTRUCTORS:
            public BifurcationData(Mesh targetMesh, Plane cuttingTool)
            {
                //Constructor: Fully computes bifurcation and stores resulting meshes

                //Initialize lists:
                pointsAlongPlane = new List<Vector3>(); //Initialize list of plane intersection points
                posVerts = new List<Vector3>();         //Initialize list of positive vertices
                posTris = new List<int>();              //Initailize list of positive triangles
                posUVs = new List<Vector2>();           //Initialize list of positive UVs
                posNorms = new List<Vector3>();         //Initialize list of positive normals
                negVerts = new List<Vector3>();         //Initialize list of negative vertices
                negTris = new List<int>();              //Initailize list of negative triangles
                negUVs = new List<Vector2>();           //Initialize list of negative UVs
                negNorms = new List<Vector3>();         //Initialize list of negative normals

                //Store inputs:
                mesh = targetMesh;   //Store mesh to bifurcate
                plane = cuttingTool; //Store cutting tool

                //Perform bifurcation:
                for (int t = 0; t < mesh.triangles.Length; t += 3) //Iterate through each triangle (trio of vertices) in original mesh
                {
                    //Initialize containers:
                    Vector3[] verts = new Vector3[3]; //Initialize container to store values of found vertices
                    int[] vertIndexes = new int[3];   //Initialize container to store values of found vertex indexes
                    Vector2[] uvs = new Vector2[3];   //Initialize container to store uvs at given vertices
                    Vector3[] norms = new Vector3[3]; //Initialize container to store normals at given vertices
                    bool[] sides = new bool[3];       //Initialize container to store found sides

                    //Get data from triangle vertices:
                    for (int v = 0; v < 3; v += 1) //Iterate three times through group (once for each vertex in tri)
                    {
                        verts[v] = mesh.vertices[mesh.triangles[t + v]];         //Get vertex position from this point of the triangle
                        vertIndexes[v] = Array.IndexOf(mesh.vertices, verts[v]); //Get the index of the vertex that was just found
                        uvs[v] = mesh.uv[vertIndexes[v]];                        //Use vertex index to get according uv position
                        norms[v] = mesh.normals[vertIndexes[v]];                 //Use vertex index to get according normal
                        sides[v] = plane.GetSide(verts[v]);                      //Determine which side of cutting plane vertex lies on
                    }

                    //Divide data amongst sides:
                    if (sides[0] == sides[1] && sides[1] == sides[2]) //All vertices are on the same side of the cutting plane, triangle is not bifurcated
                    {
                        MeshSide targetSide = (sides[0]) ? MeshSide.Positive : MeshSide.Negative; //Determine which side to send triangle to
                        AddPartsToSide(targetSide, verts, uvs, norms, false);                     //Add entire triangle to found side
                    }
                    else //Triangle is cut by plane and components are being split between meshes
                    {
                        //Initialize data for two points where cutting plane intersects this triangle:
                        Vector3 intersectPoint1, intersectPoint2; //Initialize intersection points
                        Vector2 intersectUV1, intersectUV2;       //Initialize UVs for intersection points

                        //Determine which side to send vertices to:
                        MeshSide side1 = (sides[0]) ? MeshSide.Positive : MeshSide.Negative; //Determine which mesh first vertex should go into
                        MeshSide side2 = (sides[0]) ? MeshSide.Negative : MeshSide.Positive; //Set other side to negative of first side

                        //Determine which vertex is alone:
                        if (sides[0] == sides[1]) //Vertexes 1 and 2 are on the same side, so vertex 3 is alone
                        {
                            //Find intersection points and UVs:
                            intersectPoint1 = GetRayPlaneIntersection(verts[1], verts[2], uvs[1], uvs[2], out intersectUV1); //Get data for first intersection point
                            intersectPoint2 = GetRayPlaneIntersection(verts[2], verts[0], uvs[2], uvs[0], out intersectUV2); //Get data for second intersection point

                            //Add tris to their respective sides (with no normals):
                            AddPartsToSide(side1, new Vector3[] { verts[0], verts[1], intersectPoint1 }, new Vector2[] { uvs[0], uvs[1], intersectUV1 }, new Vector3[0], false);              //Add first triangle to side 1
                            AddPartsToSide(side1, new Vector3[] { verts[0], intersectPoint1, intersectPoint2 }, new Vector2[] { uvs[0], intersectUV1, intersectUV2 }, new Vector3[0], false); //Add second triangle to side 1
                            AddPartsToSide(side2, new Vector3[] { intersectPoint1, verts[2], intersectPoint2 }, new Vector2[] { intersectUV1, uvs[2], intersectUV2 }, new Vector3[0], false); //Add third triangle to side 2
                        }
                        else if (sides[0] == sides[2]) //Vertexes 1 and 3 are on the same side, so vertex 2 is alone
                        {
                            //Find intersection points and UVs:
                            intersectPoint1 = GetRayPlaneIntersection(verts[0], verts[1], uvs[0], uvs[1], out intersectUV1); //Get data for first intersection point
                            intersectPoint2 = GetRayPlaneIntersection(verts[1], verts[2], uvs[1], uvs[2], out intersectUV2); //Get data for second intersection point

                            //Add tris to their respective sides (with no normals):
                            AddPartsToSide(side1, new Vector3[] { verts[0], intersectPoint1, verts[2] }, new Vector2[] { uvs[0], intersectUV1, uvs[2] }, new Vector3[0], false);              //Add first triangle to side 1
                            AddPartsToSide(side1, new Vector3[] { intersectPoint1, intersectPoint2, verts[2] }, new Vector2[] { intersectUV1, intersectUV2, uvs[2] }, new Vector3[0], false); //Add second triangle to side 1
                            AddPartsToSide(side2, new Vector3[] { intersectPoint1, verts[1], intersectPoint2 }, new Vector2[] { intersectUV1, uvs[1], intersectUV2 }, new Vector3[0], false); //Add third triangle to side 2
                        }
                        else //Vertex 1 must be alone, all other possibilities are exhausted
                        {
                            //Find intersection points and UVs:
                            intersectPoint1 = GetRayPlaneIntersection(verts[0], verts[1], uvs[0], uvs[1], out intersectUV1); //Get data for first intersection point
                            intersectPoint2 = GetRayPlaneIntersection(verts[0], verts[2], uvs[0], uvs[2], out intersectUV2); //Get data for second intersection point

                            //Add tris to their respective sides (with no normals):
                            AddPartsToSide(side1, new Vector3[] { verts[0], intersectPoint1, intersectPoint2 }, new Vector2[] { uvs[0], intersectUV1, intersectUV2 }, new Vector3[0], false); //Add first triangle to side 1
                            AddPartsToSide(side2, new Vector3[] { intersectPoint1, verts[1], verts[2] }, new Vector2[] { intersectUV1, uvs[1], uvs[2] }, new Vector3[0], false);              //Add second triangle to side 2
                            AddPartsToSide(side2, new Vector3[] { intersectPoint1, verts[2], intersectPoint2 }, new Vector2[] { intersectUV1, uvs[2], intersectUV2 }, new Vector3[0], false); //Add third triangle to side 2
                        }

                        //Add to list of points along plane so that new polys can be created to cover up hole:
                        pointsAlongPlane.Add(intersectPoint1); //Add first intersection point to list of points along plane
                        pointsAlongPlane.Add(intersectPoint2); //Add second intersection point to list of points along plane
                    }
                }

                //Postprocessing:
                CloseMeshGaps(); //Generate planes to close up holes in generated meshes

                //Generate final output meshes:
                posMesh = new Mesh //Generate positive mesh
                {
                    vertices = posVerts.ToArray(), //Set vertices
                    triangles = posTris.ToArray(), //Set triangles
                    normals = posNorms.ToArray(),  //Set normals
                    uv = posUVs.ToArray()          //Set UVs
                };
                negMesh = new Mesh //Generate negative mesh
                {
                    vertices = negVerts.ToArray(), //Set vertices
                    triangles = negTris.ToArray(), //Set triangles
                    normals = negNorms.ToArray(),  //Set normals
                    uv = negUVs.ToArray()          //Set UVs
                };
            }

            //COMPUTATION METHODS:
            private void AddPartsToSide(MeshSide side, Vector3[] vertices, Vector2[] UVs, Vector3[] normals, bool addFirst)
            {
                //Function: Overrides AddPartsToSide to determine which side parts need to be added to

                if (side == MeshSide.Positive) //Parts are getting added to positive mesh
                {
                    AddPartsToSide(ref posVerts, ref posTris, ref posNorms, ref posUVs, vertices, UVs, new List<Vector3>(normals), addFirst); //Add parts to positive side of mesh
                }
                else //Parts are getting added to negative mesh
                {
                    AddPartsToSide(ref negVerts, ref negTris, ref negNorms, ref negUVs, vertices, UVs, new List<Vector3>(normals), addFirst); //Add parts to negative side of mesh
                }
            }
            private void AddPartsToSide(ref List<Vector3> sideVerts, ref List<int> sideTris, ref List<Vector3> sideNorms, ref List<Vector2> sideUVs, Vector3[] vertices, Vector2[] UVs, List<Vector3> normals, bool addFirst)
            {
                //Function: Adds given elements to one output mesh or the other depending on side
                //NOTE: Normals are stored in list so that size can be used as an indicator normals are missing and need to be generated

                for (int i = 0; i < 3; i++) //Iterate through parts of added triangle
                {
                    //Initialization:
                    if (i == 0 && addFirst) ShiftTriangleIndeces(ref sideTris); //Pop this triangle in bottom of tri array if indicated (and at first index)

                    //Add new vertices to side:
                    if (normals.Count == i) //Normal is missing for this vertex
                    {
                        //Re-order vertex list so that sides are in correct order duing normal computation:
                        List<Vector3> orderedVertices = new List<Vector3>(vertices); //Temporarily put vertices in list
                        for (int n = i; n > 0; n--) //Perform operation once for each position past first that current vertex is in (max iterations should be 2)
                        {
                            orderedVertices.Add(orderedVertices[0]); //Take first vertex and add it to end of list
                            orderedVertices.RemoveAt(3);             //Remove duplicate vertex from beginning of list
                        }
                        normals.Add(GenerateNormal(orderedVertices.ToArray())); //Generate a normal if normal data is missing (using vertices in corrected order if needed), then add to normal list
                    }
                    if (addFirst) //Elements need to be added to beginning of arrays
                    {
                        sideVerts.Insert(i, vertices[i]); //Insert vertex into side mesh
                        sideUVs.Insert(i, UVs[i]);        //Insert UV into side mesh
                        sideNorms.Insert(i, normals[i]);  //Insert normal into side mesh
                        sideTris.Insert(i, i);            //Insert triangle into side mesh
                    }
                    else //Elements are added to end of arrays by default
                    {
                        sideVerts.Add(vertices[i]);                   //Add vertex to mesh
                        sideUVs.Add(UVs[i]);                          //Add UV to mesh
                        sideNorms.Add(normals[i]);                    //Add normal to mesh
                        sideTris.Add(sideVerts.IndexOf(vertices[i])); //Find and add triangle to mesh
                    }
                }
            }
            private void CloseMeshGaps()
            {
                //Function: Closes open ends of generated meshes by making new polygons (in two duplicate planes with flipped normals, one for each mesh)

                //Initialization:
                Vector3 midPoint = Vector3.zero; //Initialize vector for storing midpoint of open gap
                float furthestDistance = 0f;     //Initialize reuseable container to store distance between a vertex and a midpoint

                //Find midpoint of plane:
                if (pointsAlongPlane.Count > 0) //Ensure there is at least one point along the cutting plane (should be unnecessary)
                {
                    Vector3 furthestPoint = Vector3.zero; //Initialize vector for storing current furthest point from first point
                    foreach (Vector3 currentPoint in pointsAlongPlane) //Iterate through list of vertices on cutting plane
                    {
                        float currentDistance = Vector3.Distance(pointsAlongPlane[0], currentPoint); //Get distance between first and current points on plane
                        if (currentDistance > furthestDistance) //New distance is greater than previous record
                        {
                            furthestDistance = currentDistance; //Record new furthest distance
                            furthestPoint = currentPoint;       //Record new furthest point
                        }
                    }
                    midPoint = Vector3.Lerp(pointsAlongPlane[0], furthestPoint, 0.5f); //Midpoint is halfway down the line between the two furthest points on plane
                }

                //Generate new triangles:
                for (int i = 0; i < pointsAlongPlane.Count; i += 2) //Iterate through each adjacent vertex pair
                {
                    //Initialization:
                    Vector3 vertA = pointsAlongPlane[i];     //Get position of first vertex
                    Vector3 vertB = pointsAlongPlane[i + 1]; //Get position of second vertex

                    //Get normal for all points on plane:
                    Vector3 normal = GenerateNormal(new Vector3[] { midPoint, vertB, vertA }); //Generate normal for the new polygon
                    normal.Normalize();                                                        //Normalize normal vector

                    //Add triangle to both meshes:
                    float direction = Vector3.Dot(normal, plane.normal); //Get variable for determining whether or not normal is aligned with cutting plane
                    if (direction > 0) //Plane is aligned with normal
                    {
                        AddPartsToSide(MeshSide.Positive, new Vector3[] { midPoint, vertA, vertB }, new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }, new Vector3[] { -normal, -normal, -normal }, true);
                        AddPartsToSide(MeshSide.Negative, new Vector3[] { midPoint, vertB, vertA }, new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }, new Vector3[] { normal, normal, normal }, true);
                    }
                    else //Normal is inverted relative to plane
                    {
                        AddPartsToSide(MeshSide.Positive, new Vector3[] { midPoint, vertB, vertA }, new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }, new Vector3[] { normal, normal, normal }, true);
                        AddPartsToSide(MeshSide.Negative, new Vector3[] { midPoint, vertA, vertB }, new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }, new Vector3[] { -normal, -normal, -normal }, true);
                    }
                }
            }

            //UTILITY METHODS:
            private void ShiftTriangleIndeces(ref List<int> triangles)
            {
                //Function: Shifts all triangle indexes upward in array to make room for a new triangle in front

                for (int i = 0; i < triangles.Count; i += 3) //Iterate through each triangle-vertex group
                {
                    for (int n = 0; n < 3; n++) //Iterate three times, once for each vertex in triangle group
                    {
                        triangles[i + n] += 3; //Move vertex (in current position) three positions up (to next triangle group)
                    }
                }
            }
            private Vector3 GenerateNormal(Vector3[] vertices)
            {
                //Function: Returns the normal of the plane defined by the three given vertices

                Vector3 sideA = vertices[1] - vertices[0]; //Get a vector representing the line between first two vertices
                Vector3 sideB = vertices[2] - vertices[0]; //Get a vector representing the line between first and last vertices
                return Vector3.Cross(sideA, sideB);        //Return the vector facing perpendicular to the two known lines
            }
            private Vector3 GetRayPlaneIntersection(Vector3 vertex1, Vector3 vertex2, Vector2 UV1, Vector2 UV2, out Vector2 intersectUV)
            {
                //Function: Returns the point on the cutting plane intersected by the line between two vertices (as well as its interpolated UV)

                Ray ray = new Ray(vertex1, (vertex2 - vertex1));     //Get a ray starting at vertex 1 and pointing toward vertex 2
                plane.Raycast(ray, out float distFromPlane);         //Find distance between vertex 1 and point of intersection with cutting plane (used to interpolate UVs)
                intersectUV = Vector2.Lerp(UV1, UV2, distFromPlane); //Interpolate new UV based on found distance from plane (interpolant) and known vertex UVs
                return ray.GetPoint(distFromPlane);                  //Return actual point of intersection
            }
        }
    }
}