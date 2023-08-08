using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace bemaker {
    public class MoveActuatorRC : Actuator
    {
        //forces applied on the x, y and z axes.    
        private float move, turn, jump, jumpForward, brake;
        public float moveAmount = 1;
        public float turnAmount = 1;
        public float jumpPower = 1;
        public float jumpForwardPower = 1;
        public float brakePower = 10;
        public float groundCheckDistance = 1f;
        public LayerMask groundMask;


        public bool showAgentArrow = true;
        public Vector3 arrowOffset = new Vector3(0, 0.5f, 0.1f); 

        public bool freezeXPos = false;
        public bool freezeYPos = false;
        public bool freezeZPos = false;
        public bool freezeXRot = true;
        public bool freezeYRot = false;
        public bool freezeZRot = true;


        private bool onGround = false;

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
                //Debug.Log(action[0] + ", " + action[1] + ", " + action[2] + ", " + action[3]);
                move = action[0];
                turn = action[1];
                //Debug.Log("move : " + move);
                //Debug.Log("turn : " + turn);
                if (action.Length >= 3)
                {
                    brake = action[2];
                    //Debug.Log("Break: " + brake);
                    if (action.Length >= 4)
                    {
                        jump = action[3];
                        if (action.Length >= 5)
                        {
                            jumpForward = action[4];
                        }
                    }
                }

                Rigidbody rBody = agent.body.GetComponent<Rigidbody>();
                Transform reference = agent.body.transform;

                if (rBody != null)
                {
                    onGround = Physics.Raycast(reference.position, Vector3.down, groundCheckDistance, groundMask);
                
                    if (onGround)
                    {
                        if (Mathf.Abs(turn) < 0.01f)
                        {
                            turn = 0;
                        }

                        Vector3 brkForce = Vector3.zero;
                        if (rBody.velocity.magnitude > 0)
                        {
                            brkForce = -rBody.velocity.normalized * brake * brakePower;
                            if (brkForce.magnitude > rBody.velocity.magnitude)
                            {
                                brkForce = -rBody.velocity;
                            }
                            rBody.AddForce(brkForce, ForceMode.Acceleration);
                        }

                        
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
                brake = 0;
            }
        }

        void Awake()
        {
            if (showAgentArrow)
            {
                GenerateArrow();
            }
        }

        public override void OnSetup(Agent agent)
        {
            shape = new int[1]{4};
            isContinuous = true;
            rangeMin = new float[]{0, -1, 0, 0};
			rangeMax = new float[]{1, 1, 1, 1};
            agent.AddResetListener(this);
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
        }

        public override void OnReset(Agent agent)
        {
            turn = 0;
            move = 0;
            jump = 0;
            jumpForward = 0;
            onGround = false;
        }

        private void GenerateArrow()
        {
            //Cria uma nova malha
            Mesh mesh = new Mesh();
            //Define os vértices da malha (a ponta da seta está na origem)
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(1.000000f, 2.004569f, -1.000000f),
                new Vector3(1.000000f, -1.000000f, -1.000000f),
                new Vector3(1.000000f, 2.004569f, 1.000000f),
                new Vector3(1.000000f, -1.000000f, 1.000000f),
                new Vector3(-1.000000f, 2.004569f, -1.000000f),
                new Vector3(-1.000000f, -1.000000f, -1.000000f),
                new Vector3(-1.000000f, 2.004569f, 1.000000f),
                new Vector3(-1.000000f, -1.000000f, 1.000000f),
                new Vector3(-0.000000f, 2.000354f, -1.921899f),
                new Vector3(0.374944f, 2.000354f, -1.884970f),
                new Vector3(0.735479f, 2.000354f, -1.775603f),
                new Vector3(1.067750f, 2.000354f, -1.598001f),
                new Vector3(1.358988f, 2.000354f, -1.358988f),
                new Vector3(1.598001f, 2.000354f, -1.067750f),
                new Vector3(1.775603f, 2.000354f, -0.735479f),
                new Vector3(1.884970f, 2.000354f, -0.374944f),
                new Vector3(1.921899f, 2.000354f, 0.000000f),
                new Vector3(1.884970f, 2.000354f, 0.374944f),
                new Vector3(1.775603f, 2.000354f, 0.735479f),
                new Vector3(1.598001f, 2.000354f, 1.067750f),
                new Vector3(1.358988f, 2.000354f, 1.358988f),
                new Vector3(1.067750f, 2.000354f, 1.598001f),
                new Vector3(0.735479f, 2.000354f, 1.775603f),
                new Vector3(0.374944f, 2.000354f, 1.884970f),
                new Vector3(-0.000000f, 2.000354f, 1.921899f),
                new Vector3(-0.374944f, 2.000354f, 1.884970f),
                new Vector3(-0.735479f, 2.000354f, 1.775603f),
                new Vector3(-1.067750f, 2.000354f, 1.598001f),
                new Vector3(-1.358988f, 2.000354f, 1.358988f),
                new Vector3(-1.598000f, 2.000354f, 1.067750f),
                new Vector3(-1.775603f, 2.000354f, 0.735479f),
                new Vector3(-1.884970f, 2.000354f, 0.374944f),
                new Vector3(-1.921899f, 2.000354f, -0.000000f),
                new Vector3(-1.884970f, 2.000354f, -0.374944f),
                new Vector3(-1.775603f, 2.000354f, -0.735479f),
                new Vector3(-1.598001f, 2.000354f, -1.067750f),
                new Vector3(-1.358988f, 2.000354f, -1.358988f),
                new Vector3(-1.067750f, 2.000354f, -1.598001f),
                new Vector3(-0.735479f, 2.000354f, -1.775603f),
                new Vector3(-0.374944f, 2.000354f, -1.884970f),
                new Vector3(-0.000000f, 5.844152f, -0.000000f)
            };
            //Define os índices dos triângulos da malha (sentido anti-horário)
            int[] triangles = new int[]
            {
                4, 2, 0, 2, 7, 3, 6, 5, 7, 1, 7, 5, 0, 3, 1, 4, 1, 5, 8, 40, 9, 9,
                40, 10, 10, 40, 11, 11, 40, 12, 12, 40, 13, 13, 40, 14, 14, 40, 15, 
                15, 40, 16, 16, 40, 17, 17, 40, 18, 18, 40, 19, 19, 40, 20, 20, 40, 
                21, 21, 40, 22, 22, 40, 23, 23, 40, 24, 24, 40, 25, 25, 40, 26, 26, 
                40, 27, 27, 40, 28, 28, 40, 29, 29, 40, 30, 30, 40, 31, 31, 40, 32, 
                32, 40, 33, 33, 40, 34, 34, 40, 35, 35, 40, 36, 36, 40, 37, 37, 40, 
                38, 23, 31, 39, 38, 40, 39, 39, 40, 8, 4, 6, 2, 2, 6, 7, 6, 4, 5, 1, 
                3, 7, 0, 2, 3, 4, 0, 1, 39, 8, 9, 9, 10, 11, 11, 12, 15, 12, 13, 15, 
                13, 14, 15, 15, 16, 17, 17, 18, 19, 19, 20, 23, 20, 21, 23, 21, 22, 
                23, 23, 24, 27, 24, 25, 27, 25, 26, 27, 27, 28, 31, 28, 29, 31, 29, 
                30, 31, 31, 32, 35, 32, 33, 35, 33, 34, 35, 35, 36, 39, 36, 37, 39, 
                37, 38, 39, 39, 9, 15, 9, 11, 15, 15, 17, 19, 15, 19, 39, 19, 23, 39, 
                23, 27, 31, 31, 35, 39
            };

            //Define as normais da malha (apontando para cima)
            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            //Atribui os dados à malha
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.RecalculateBounds(); //Recalcula os limites da malha

            //Cria um novo objeto na cena com a malha gerada e um material vermelho
            GameObject arrow = new GameObject("Arrow");
            arrow.AddComponent<MeshFilter>().mesh = mesh;
            arrow.AddComponent<MeshRenderer>().material.color = Color.red;
            arrow.transform.SetParent(transform);
            arrow.transform.position = transform.position;
            arrow.transform.Rotate(90, 0, 0);
            arrow.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            arrow.transform.position +=  arrowOffset;
        }
    }
}

