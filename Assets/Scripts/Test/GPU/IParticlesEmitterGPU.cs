namespace Eden.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class IParticleEmitterGPU : IParticleEmitter
    {
        private bool lock_MaxParticulesNB = false;
        private int MaxParticulesNB;
        public int get_MaxParticulesNB { get { return MaxParticulesNB; } }

        // Compute Shader for spawning and update

        public ComputeShader CSParticlesUpdate;
        protected int kernel_PU;
        protected uint CS_PU_gx, CS_PU_gy, CS_PU_gz;

        public ComputeShader CSParticlesSpawn;
        protected int kernel_PS;
        protected uint CS_PS_gx, CS_PS_gy, CS_PS_gz;

        protected ComputeShader CSParticlesDelete;
        protected int kernel_PD;
        protected uint CS_PD_gx, CS_PD_gy, CS_PD_gz;

        // Data and Buffer for particles
        protected ParticuleShaderGPU[] particleData;
        protected ComputeBuffer particlesBuffer;


        // args and buffer for indirect draw
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        protected ComputeBuffer argsBuffer;

        // Number of particule alive
        protected int[] alive = new int[] { 0 };
        protected ComputeBuffer counterBuffer; // counter Alive
        protected ComputeBuffer sumBuffer; // sum counter Alive



        protected Mesh mesh;
        // Material define by the user (shared)
        // Copy of the Material (private/unique)
        public Material material;
        // Bound of the particule of oculusion
        protected Bounds bounds;

        public override int ParticleCount { get { return alive[0]; } }

        /**
         * Initialize the maximal number of particles that an emitter can produce.
         */
        protected void InitializeSize(int MaxParticulesNB) { if (!lock_MaxParticulesNB) this.MaxParticulesNB = MaxParticulesNB; }


        /// <summary>
        /// Initialize the Buffers.  <br></br>
        /// The Max size must have been set before.
        /// </summary>
        protected void InitializeBuffers()
        {
            if (MaxParticulesNB <= 0)
                Debug.LogError("\"InitializeSize\" must be called before \"InitializeBuffers\" with positif value (MaxParticulesNB = " + MaxParticulesNB + ")");
            lock_MaxParticulesNB = true;

            particlesBuffer = new ComputeBuffer(MaxParticulesNB, ParticuleShaderGPU.Size());
            particleData = new ParticuleShaderGPU[MaxParticulesNB];
            particlesBuffer.SetData(particleData);

            {
                // A ComputeShader can have multiple kernel. So before using a specific ComputeShader, we must find it's kernel ID
                kernel_PU = CSParticlesUpdate.FindKernel("CS_ParticleUpdate");
                // Then, we get the size of the ThreadGroupe to Dispatch later with the good number of ThreadGroupe for all the object
                CSParticlesUpdate.GetKernelThreadGroupSizes(kernel_PU, out CS_PU_gx, out CS_PU_gy, out CS_PU_gz);
                // we tell the compute shader to which variable to bind the buffer
                CSParticlesUpdate.SetBuffer(kernel_PU, "_Particules", particlesBuffer);

                // COUNTER :
                // 
                // 1 per GPU thread => sizeGroupe times nb Calls
                int size = (int)(CS_PU_gx * CS_PU_gy * CS_PU_gz) * Mathf.CeilToInt(MaxParticulesNB / (float)CS_PU_gx);
                counterBuffer = new ComputeBuffer(size, sizeof(int), ComputeBufferType.Counter);
                sumBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
                counterBuffer.SetCounterValue(0);
                CSParticlesUpdate.SetBuffer(kernel_PU, "counterBufferAlive", counterBuffer);
            }

            {
                kernel_PS = CSParticlesSpawn.FindKernel("CS_ParticleSpawn");
                CSParticlesSpawn.GetKernelThreadGroupSizes(kernel_PS, out CS_PS_gx, out CS_PS_gy, out CS_PS_gz);
                CSParticlesSpawn.SetBuffer(kernel_PS, "_Particules", particlesBuffer);
            }

            {
                kernel_PD = CSParticlesDelete.FindKernel("CS_ParticleDelete");
                CSParticlesDelete.GetKernelThreadGroupSizes(kernel_PD, out CS_PD_gx, out CS_PD_gy, out CS_PD_gz);
                CSParticlesDelete.SetBuffer(kernel_PD, "_Particules", particlesBuffer);
            }



            material.SetBuffer("_Particules", particlesBuffer);
        }


        /// <summary>
        /// We Instantiate the Shader and ComputeShader to create independent copy. <br></br>
        /// If we don't do this, the data of the buffer will be the same for all object that will use the same Shaders. <br></br>
        /// </summary>
        protected void CopyNewParam()
        {
            material = Instantiate(material);
            CSParticlesUpdate = Instantiate(CSParticlesUpdate);
            CSParticlesSpawn = Instantiate(CSParticlesSpawn);

            CSParticlesDelete = Instantiate(AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Test/Shaders/CSParticleDelete50.compute"));
            if (CSParticlesDelete == null)
                Debug.LogError("error load CSParticlesDelete");
        }


        /// <summary>
        /// Extract the data of a SpriteRenderer to create a mesh. <br></br>
        /// The SpriteRenderer doesn't have the scale value, So we scale the mesh with the Scale of the gameObject that containe the SpriteRenderer.
        /// </summary>
        /// <param name="m_SpriteRenderer">Reference to a SpriteRenderer. </param>
        /// <param name="_particlePrefab" >Use to scale the Mesh if not null </param>
        /// <returns>A Mesh with the same shape as the Sprite </returns>
        public static Mesh SpriteToMesh(ref SpriteRenderer m_SpriteRenderer, GameObject _particlePrefab)
        {
            Mesh mesh = new Mesh();

            if (_particlePrefab != null)
                mesh.vertices = Array.ConvertAll(m_SpriteRenderer.sprite.vertices, i => Vector3.Scale(i, _particlePrefab.transform.localScale));
            else
                mesh.vertices = Array.ConvertAll(m_SpriteRenderer.sprite.vertices, i => new Vector3(i.x, i.y));

            mesh.uv = m_SpriteRenderer.sprite.uv;
            mesh.triangles = Array.ConvertAll(m_SpriteRenderer.sprite.triangles, i => (int)i);

            return mesh;
        }


        /// <summary>
        /// Initialize the mesh that will be draw and the args Buffer with the data of the mesh
        /// </summary>
        /// <param name="_particlePrefab">GameObject with a SpriteRenderer</param>
        protected void InitializeMesh(ref GameObject _particlePrefab)
        {
            if (MaxParticulesNB <= 0)
                Debug.LogError("\"InitializeSize\" must be called before \"InitializeMesh\" with positif value (MaxParticulesNB = " + MaxParticulesNB + ")");

            lock_MaxParticulesNB = true;
            SpriteRenderer m_SpriteRenderer = _particlePrefab.GetComponent<SpriteRenderer>();
            if (m_SpriteRenderer == null)
                Debug.LogError("The GameObject doesn't have a Component SpriteRenderer");

            mesh = SpriteToMesh(ref m_SpriteRenderer, _particlePrefab);
            bounds = m_SpriteRenderer.bounds;
            material.SetTexture("_MainTex", m_SpriteRenderer.sprite.texture);

            // Argument buffer used by DrawMeshInstancedIndirect.
            // Arguments for drawing mesh.
            // 0 == number of triangle indices
            args[0] = (uint)mesh.GetIndexCount(0);
            // 1 == population
            args[1] = (uint)MaxParticulesNB;
            // others are only relevant if drawing submeshes.
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);
        }


        /// <summary>
        /// if the buffer is still active, release the buffer and set null.
        /// </summary>
        /// <param name="b">a Buffer</param>
        public void DisableBuffer(ref ComputeBuffer b) { if (b != null) b.Release(); b = null; }


        private void OnDestroy()
        {
            DisableBuffer(ref particlesBuffer);
            DisableBuffer(ref argsBuffer);
            DisableBuffer(ref counterBuffer);
            DisableBuffer(ref sumBuffer);

            Destroy(material);
            Destroy(CSParticlesUpdate);
            Destroy(CSParticlesSpawn);
            Destroy(CSParticlesDelete);
            Destroy(mesh);
        }


        /// <summary>
        /// Delete 50% of the particles (set _lifespan to 0)
        /// </summary>
        public override void Delete50()
        {
            InitRandomCS(ref CSParticlesDelete);

            CSParticlesDelete.Dispatch(kernel_PD, Mathf.CeilToInt(MaxParticulesNB / (float)CS_PD_gx), 1, 1);
        }


        /// <summary>
        /// Init random variables of the ComputeShader. <br></br>
        /// ComputeShader require "time" and "randSeed" variables
        /// </summary>
        /// <param name="cs">ComputeShader that have random variables</param>
        protected void InitRandomCS(ref ComputeShader cs)
        {
            cs.SetFloat("time", Time.time);
            cs.SetFloat("randSeed", UnityEngine.Random.Range(-20, 20));
        }


        protected void drawParticles() { Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer); }


        protected void Dispatch_CSParticlesSpawn() { Dispatch_CSParticlesSpawn(MaxParticulesNB); }


        protected void Dispatch_CSParticlesSpawn(int nb_object) { CSParticlesSpawn.Dispatch(kernel_PS, Mathf.CeilToInt(nb_object / (float)CS_PS_gx), 1, 1); }


        protected void Dispatch_CSParticlesUpdate() { CSParticlesUpdate.Dispatch(kernel_PU, Mathf.CeilToInt(MaxParticulesNB / (float)CS_PU_gx), 1, 1); }


        protected void ResetCountBuffer() { counterBuffer.SetCounterValue(0); }


        /// <summary>
        /// Request Async count of Alive particles
        /// </summary>
        protected void AsyncCountAlive()
        {
            ComputeBuffer.CopyCount(counterBuffer, sumBuffer, 0);
            AsyncGPUReadback.Request(sumBuffer, r1 => OnCounterAdd(r1));
        }


        private void OnCounterAdd(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || // Something wrong happened
            !Application.isPlaying) // Callback happened in edit mode afterwards
                return;

            alive[0] = request.GetData<int>()[0];
        }
    }
}