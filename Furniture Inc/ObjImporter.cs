/* This version of ObjImporter first reads through the entire file, getting a count of how large
 * the final arrays will be, and then uses standard arrays for everything (as opposed to ArrayLists
 * or any other fancy things). 
 */

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ObjImporter
    {
    private struct MeshStruct
        {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector2[] Uv;
        public int[] Triangles;
        public Vector3[] FaceData;
        public string FileName;
        }

    // Use this for initialization
    public static Mesh ImportFile(string filePath)
        {
        var newMesh = CreateMeshStruct(filePath);
        PopulateMeshStruct(ref newMesh);

        var newVerts = new Vector3[newMesh.FaceData.Length];
        var newUVs = new Vector2[newMesh.FaceData.Length];
        var newNormals = new Vector3[newMesh.FaceData.Length];
        var i = 0;
        /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
         * for the appropriate Unity mesh array.
         */
        foreach (var v in newMesh.FaceData)
            {
            newVerts[i] = newMesh.Vertices[(int)v.x - 1];
            if (v.y >= 1)
                newUVs[i] = newMesh.Uv[(int)v.y - 1];

            if (v.z >= 1)
                newNormals[i] = newMesh.Normals[(int)v.z - 1];
            i++;
            }

        var mesh = new Mesh
            {
            vertices = newVerts,
            uv = newUVs,
            normals = newNormals,
            triangles = newMesh.Triangles
            };

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
        }

    private static MeshStruct CreateMeshStruct(string filename)
        {
        var triangles = 0;
        var vertices = 0;
        var vt = 0;
        var vn = 0;
        var face = 0;
        var mesh = new MeshStruct {FileName = filename};
        var stream = File.OpenText(filename);
        var entireText = stream.ReadToEnd();
        stream.Close();
        using (var reader = new StringReader(entireText))
            {
            var currentText = reader.ReadLine();
            char[] splitIdentifier = { ' ' };
            string[] brokenString;
            while (currentText != null)
                {
                if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                    && !currentText.StartsWith("vn "))
                    {
                    currentText = reader.ReadLine();
                    if (currentText != null)
                        {
                        currentText = currentText.Replace("  ", " ");
                        }
                    }
                else
                    {
                    currentText = currentText.Trim();                           //Trim the current line
                    brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                    switch (brokenString[0])
                        {
                        case "v":
                            vertices++;
                            break;
                        case "vt":
                            vt++;
                            break;
                        case "vn":
                            vn++;
                            break;
                        case "f":
                            face = face + brokenString.Length - 1;
                            triangles = triangles + 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertice, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
                            break;
                        }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                        {
                        currentText = currentText.Replace("  ", " ");
                        }
                    }
                }
            }
        mesh.Triangles = new int[triangles];
        mesh.Vertices = new Vector3[vertices];
        mesh.Uv = new Vector2[vt];
        mesh.Normals = new Vector3[vn];
        mesh.FaceData = new Vector3[face];
        return mesh;
        }

    private static void PopulateMeshStruct(ref MeshStruct mesh)
        {
        var stream = File.OpenText(mesh.FileName);
        var entireText = stream.ReadToEnd();
        stream.Close();
        using (var reader = new StringReader(entireText))
            {
            var currentText = reader.ReadLine();

            char[] splitIdentifier = { ' ' };
            char[] splitIdentifier2 = { '/' };
            string[] brokenString;
            string[] brokenBrokenString;
            var f = 0;
            var f2 = 0;
            var v = 0;
            var vn = 0;
            var vt = 0;
            var vt1 = 0;
            var vt2 = 0;
            while (currentText != null)
                {
                if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                    !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                    !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                    !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                    {
                    currentText = reader.ReadLine();
                    if (currentText != null)
                        {
                        currentText = currentText.Replace("  ", " ");
                        }
                    }
                else
                    {
                    currentText = currentText.Trim();
                    brokenString = currentText.Split(splitIdentifier, 50);
                    switch (brokenString[0])
                        {
                        case "g":
                            break;
                        case "usemtl":
                            break;
                        case "usemap":
                            break;
                        case "mtllib":
                            break;
                        case "v":
                            mesh.Vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                     System.Convert.ToSingle(brokenString[3]));
                            v++;
                            break;
                        case "vt":
                            mesh.Uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt++;
                            break;
                        case "vt1":
                            mesh.Uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt1++;
                            break;
                        case "vt2":
                            mesh.Uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt2++;
                            break;
                        case "vn":
                            mesh.Normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                    System.Convert.ToSingle(brokenString[3]));
                            vn++;
                            break;
                        case "vc":
                            break;
                        case "f":

                            var j = 1;
                            var intArray = new List<int>();
                            while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                {
                                var temp = new Vector3();
                                brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                                temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
                                if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                    {
                                    if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                        {
                                        temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
                                        }
                                    temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
                                    }
                                j++;

                                mesh.FaceData[f2] = temp;
                                intArray.Add(f2);
                                f2++;
                                }
                            j = 1;
                            while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                                {
                                mesh.Triangles[f] = intArray[0];
                                f++;
                                mesh.Triangles[f] = intArray[j];
                                f++;
                                mesh.Triangles[f] = intArray[j + 1];
                                f++;

                                j++;
                                }
                            break;
                        }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                        {
                        currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.
                        }
                    }
                }
            }
        }
    }