using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class ProceduralReefsGenerator : MyMonoBehaviour
{
                        public static   ProceduralReefsGenerator proceduralReefsGeneratorInstance;
                        public static   Transform   reefs;
                        public static   bool        isBusy;
                        private         Transform   _newReef;
    [SerializeField]    private         Material    reefsMaterial;

    private void Awake()
    {
        if (proceduralReefsGeneratorInstance != null)
            if (proceduralReefsGeneratorInstance != this)
                DestroyImmediate(gameObject);

        proceduralReefsGeneratorInstance = this;
    }

    private IEnumerator Start()
    {
        isBusy = true;

        yield return null;
        reefs = transform;
        reefs.name = "reefs";
        reefs.SetParent(transform);
        reefs.localPosition = vector3Zero;
        reefs.localRotation = quaternionIdentity;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                _newReef = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                _newReef.transform.position = Vector3.down * Random.Range(200f, 209f);
                _newReef.transform.position = _newReef.transform.position + Vector3.right * i * 90f;
                _newReef.transform.position = _newReef.transform.position + Vector3.forward * j * 90f;
                _newReef.transform.Rotate(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
                _newReef.transform.localScale = new Vector3(Random.Range(180, 270), Random.Range(20, 60f), Random.Range(180, 270));
                _newReef.GetComponent<MeshRenderer>().material = reefsMaterial;
                _newReef.name = "reef";
                _newReef.SetParent(reefs);
                _newReef.localPosition = (_newReef.localPosition + new Vector3(-450f, 0f, -450f));
                _newReef.gameObject.layer = gameObject.layer;
                _newReef.gameObject.AddComponent<Rigidbody>();
                _newReef.GetComponent<Rigidbody>().useGravity = false;
                _newReef.GetComponent<Rigidbody>().isKinematic = true;
                _newReef.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            yield return null;
        }

        yield return null;

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                _newReef = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                _newReef.transform.position = Vector3.down * Random.Range(200f, 209f);
                _newReef.transform.position = _newReef.transform.position + Vector3.right * i * 45f;
                _newReef.transform.position = _newReef.transform.position + Vector3.forward * j * 45f;
                _newReef.transform.Rotate(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
                _newReef.transform.localScale = new Vector3(Random.Range(30, 45), Random.Range(40, 120f), Random.Range(30, 45));
                _newReef.GetComponent<MeshRenderer>().material = reefsMaterial;
                _newReef.name = "reef";
                _newReef.SetParent(reefs);
                _newReef.localPosition = (_newReef.localPosition + new Vector3(-450f, 0f, -450f));
                _newReef.gameObject.layer = gameObject.layer;
                _newReef.gameObject.AddComponent<Rigidbody>();
                _newReef.GetComponent<Rigidbody>().useGravity = false;
                _newReef.GetComponent<Rigidbody>().isKinematic = true;
                _newReef.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            yield return null;
        }

        yield return null;

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                _newReef = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                _newReef.transform.position = Vector3.down * Random.Range(200f, 209f);
                _newReef.transform.position = _newReef.transform.position + Vector3.right * i * 45f;
                _newReef.transform.position = _newReef.transform.position + Vector3.forward * j * 45f;
                _newReef.transform.Rotate(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
                _newReef.transform.localScale = new Vector3(Random.Range(20, 30), Random.Range(80, 240f), Random.Range(20, 30));
                _newReef.GetComponent<MeshRenderer>().material = reefsMaterial;
                _newReef.name = "reef";
                _newReef.SetParent(reefs);
                _newReef.localPosition = (_newReef.localPosition + new Vector3(-450f, 0f, -450f));
                _newReef.gameObject.layer = gameObject.layer;
                _newReef.gameObject.AddComponent<Rigidbody>();
                _newReef.GetComponent<Rigidbody>().useGravity = false;
                _newReef.GetComponent<Rigidbody>().isKinematic = true;
                _newReef.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            yield return null;
        }

        yield return null;




        //Combining Reefs mesh into a single Mesh
        var unifiedMeshFilter = gameObject.AddComponent< MeshFilter >();
        var unifiedMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        var meshFilters = GetComponentsInChildren<MeshFilter>();
        var amountOfFilters = meshFilters.Length;
        var combine = new CombineInstance[amountOfFilters];


        for( int i = 0; i < amountOfFilters; i++ )
        {
            //Extracting Meshes
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            //Cleaning
            if (meshFilters[i] != unifiedMeshFilter)
            {
                DestroyImmediate(meshFilters[i].GetComponent<MeshRenderer>());
                DestroyImmediate(meshFilters[i]);
            }
        }

        //Combining
        unifiedMeshFilter.mesh = new Mesh();
        unifiedMeshFilter.mesh.CombineMeshes(combine);
        unifiedMeshRenderer.sharedMaterial = reefsMaterial;
        GC.Collect();

        isBusy = false;

        yield return null;
    }
}
