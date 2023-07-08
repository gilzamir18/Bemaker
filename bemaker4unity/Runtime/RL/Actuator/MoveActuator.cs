using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace bemaker {
    public class MoveActuator : Actuator
    {
        //forces applied on the x, y and z axes.    
        private float move, turn, jump, jumpForward;
        public float moveAmount = 1;
        public float turnAmount = 1;
        public float jumpPower = 1;
        public float jumpForwardPower = 1;
        public bool showAgentArrow = true;
        public Vector3 arrowOffset = new Vector3(0, 0, 0); 

        public bool freezeXPos = false;
        public bool freezeYPos = false;
        public bool freezeZPos = false;
        public bool freezeXRot = true;
        public bool freezeYRot = false;
        public bool freezeZRot = true;


        private bool onGround = false;

        void Awake()
        {
            if (showAgentArrow)
            {
                GenerateArrow();
            }
        }

        public bool OnGround
        {
            get
            {
                return onGround;
            }
        }

        public override void Act()
        {
            if (agent != null && !agent.Done)
            {
                float[] action = agent.GetActionArgAsFloatArray();
                move = action[0];
                turn = action[1];
                jump = action[2];
                jumpForward = action[3];

                Rigidbody rBody = agent.GetComponent<Rigidbody>();
                Transform reference = agent.gameObject.transform;
                if (rBody != null)
                {
                    if (Mathf.Abs(rBody.velocity.y) > 0.001)
                    {
                        onGround = false;
                    }
                    else
                    {
                        onGround = true;
                    }
                    if (onGround)
                    {
                        if (Mathf.Abs(turn) < 0.01f)
                        {
                            turn = 0;
                        }

                        //Quaternion deltaRotation = Quaternion.Euler(reference.up * turn * turnAmount);
                        //rBody.MoveRotation(rBody.rotation * deltaRotation);
                        
                        rBody.angularVelocity = Vector3.zero;
                        rBody.AddTorque(reference.up * turn * turnAmount);

                        rBody.AddForce(
                                        (jump * jumpPower * reference.up +
                                        move * moveAmount * reference.forward + 
                                        (
                                            jumpPower * jumpForward * reference.up + 
                                            jumpForward * jumpForwardPower * 
                                            reference.forward )   
                                        )

                                      );
                    }
                }
                move = 0;
                turn = 0;
                jump = 0;
                jumpForward = 0;
            }
        }

        public override void OnSetup(Agent agent)
        {
            shape = new int[1]{4};
            isContinuous = true;
            rangeMin = new float[]{0, -1, 0, 0};
			rangeMax = new float[]{1, 1, 1, 1};
            var rb = agent.gameObject.GetComponent<Rigidbody>();
            if (freezeXPos)
            {
                rb.constraints |= RigidbodyConstraints.FreezePositionZ;
            }

            if (freezeYPos)
            {
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
            }

            if (freezeZPos)
            {
                rb.constraints |= RigidbodyConstraints.FreezePositionZ;
            }

            if (freezeXRot)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            }

            if (freezeYRot)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotationY;
            }
            
            if (freezeZRot)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
            }

            agent.AddResetListener(this);
        }

        private void GenerateArrow()
        {
            //Cria uma nova malha
            Mesh mesh = new Mesh();
            //Define os vértices da malha (a ponta da seta está na origem)
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(0, 0, 0), //ponta da seta
                new Vector3(-0.5f, 0, -1), //lado esquerdo da ponta
                new Vector3(0.5f, 0, -1), //lado direito da ponta
                new Vector3(-0.25f, 0, -1), //lado esquerdo do corpo
                new Vector3(0.25f, 0, -1), //lado direito do corpo
                new Vector3(-0.25f, 0, -2), //lado esquerdo da cauda
                new Vector3(0.25f, 0, -2), //lado direito da cauda
                new Vector3(-0.5f, 0, -2), //pena esquerda
                new Vector3(0.5f, 0, -2) //pena direita
            };
            //Define os índices dos triângulos da malha (sentido anti-horário)
            int[] triangles = new int[]
            {
                1, 0, 2, //ponta da seta
                3, 1, 4, //parte superior do corpo
                2, 4, 1, //parte inferior do corpo
                5, 3, 6, //parte superior da cauda
                4, 6, 3, //parte inferior da cauda
                7, 5, 6, //pena esquerda
                8, 6, 5 //pena direita
            };
            //Define as normais da malha (apontando para cima)
            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }
            //Define as coordenadas UV da malha (opcional)
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0.5f, 1), //ponta da seta
                new Vector2(0.25f, 0.75f), //lado esquerdo da ponta
                new Vector2(0.75f, 0.75f), //lado direito da ponta
                new Vector2(0.375f, 0.75f), //lado esquerdo do corpo
                new Vector2(0.625f, 0.75f), //lado direito do corpo
                new Vector2(0.375f, 0.5f), //lado esquerdo da cauda
                new Vector2(0.625f, 0.5f), //lado direito da cauda
                new Vector2(0.25f, 0.5f), //pena esquerda
                new Vector2(0.75f, 0.5f) //pena direita
            };
            //Atribui os dados à malha
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.RecalculateBounds(); //Recalcula os limites da malha

            //Cria um novo objeto na cena com a malha gerada e um material vermelho
            GameObject arrow = new GameObject("Arrow");
            arrow.AddComponent<MeshFilter>().mesh = mesh;
            arrow.AddComponent<MeshRenderer>().material.color = Color.red;
            arrow.transform.SetParent(transform);
            arrow.transform.position = transform.position;
            arrow.transform.rotation = transform.rotation;
            arrow.transform.position +=  arrowOffset;
        }

        public override void OnReset(Agent agent)
        {
            turn = 0;
            move = 0;
            jump = 0;
            jumpForward = 0;
            onGround = false;
        }
    }
}
