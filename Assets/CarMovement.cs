using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    //todo Разделить вычисление ускорения для центростремительной силы и для линейного ускорения

    public UDPSend udpSend;

    private enum SendThroughUDP { Yes, No }
        [SerializeField] SendThroughUDP useUDP = SendThroughUDP.Yes;

    private enum InteractiveSimulation { Yes, No }
        [SerializeField] InteractiveSimulation interactiveSimulation = InteractiveSimulation.No;

    private enum CreateTrajectoryPoints { Yes, No }
        [SerializeField] CreateTrajectoryPoints createTrajectoryPoints = CreateTrajectoryPoints.Yes;


    public Rigidbody rb;

    public GameObject trajectoryPoint;
    private GameObject trajectoryPointClone;

    
    float countdownTimer = 0f;
    
    [SerializeField] private float speed = 3.6f;
    [SerializeField] private float pullForce = 2000f;

    [SerializeField] private float pointAddFrequency = 0.5f;
    [SerializeField] private float durationOfStraightTrajectory = 3f;
    [SerializeField] private float radiusOfCurvature = 15.33f;
    [SerializeField] private float timestepOfSimulation = 0.001f;
    [SerializeField] private float durationOfExitStraight = 3f;

    private float durationOfCurve;
    private float rotationStepYaxis;
    

    private enum StateOfTurn { areTurnedLeft, areTurnedRight, areStandingStraight }

    StateOfTurn wheels = StateOfTurn.areStandingStraight;
    private enum StateOfAcceleration { isAccelerating, isDecelerating, constantSpeed, isIdling }

    StateOfAcceleration car = StateOfAcceleration.isIdling;


    private Vector3 acceleration;
    private Vector3 lastVelocityLocalCoordinates;
    private Vector3 velocityLocalCoordinates;

    private float lastVelocityZaxis = 0f;
    private float accelerationZaxis = 0f;
    private float currentVelocityZaxis = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        durationOfCurve = (3.14f * (radiusOfCurvature * 2)) / speed;
        rotationStepYaxis = 180 / ((durationOfCurve / 2) / timestepOfSimulation);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(createTrajectoryPoints == CreateTrajectoryPoints.Yes)
        {
            createTrajectoryPointClone();
        }
        
        moveCar();
        getAcceleration();

        if (useUDP == SendThroughUDP.Yes)
        {
            udpSend.sendDouble(getAcceleration().y);
        }
        
    }

    private void moveCar()
    {
        if(interactiveSimulation == InteractiveSimulation.Yes)
        {
            useButtonsToMoveCar();
        }

        else if (interactiveSimulation == InteractiveSimulation.No)
        {
            useConstantTrajectory();
        }
        
    }

    private void useButtonsToMoveCar()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddRelativeForce(Vector3.forward * pullForce);
            //print(rb.velocity.magnitude);
        }


        if (rb.velocity.magnitude > 0.001)
        {
            if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                wheels = StateOfTurn.areStandingStraight;

            } 
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                addLeftRotation();
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                addRightRotation();
            }
            else
            {
                wheels = StateOfTurn.areStandingStraight;
            }
        } 
        else
        {
            wheels = StateOfTurn.areStandingStraight;
        }
    }

    private Vector3 getAcceleration()
        
    {
        acceleration = new Vector3(0, 0, 0);

        acceleration += new Vector3(0f, 0f, getAccelerationZaxis());
        getStateOfCar();


        if (wheels == StateOfTurn.areTurnedLeft)
        {
            acceleration += new Vector3(0f, (float)(Math.Pow(speed, 2) / radiusOfCurvature), 0f);
        }

        if (wheels == StateOfTurn.areTurnedRight)
        {
            acceleration += new Vector3(0f, (float) -(Math.Pow(speed, 2) / radiusOfCurvature), 0f);
        }

        return acceleration;
    }

    private float getAccelerationZaxis()
    {
        currentVelocityZaxis = transform.InverseTransformDirection(rb.velocity).z;
        accelerationZaxis = (transform.InverseTransformDirection(rb.velocity).z - lastVelocityZaxis) / Time.deltaTime;
        lastVelocityZaxis = currentVelocityZaxis;

        return accelerationZaxis;
    }

    private void getStateOfCar()
    {
        if (accelerationZaxis > 0) car = StateOfAcceleration.isAccelerating;
        else if (accelerationZaxis < 0) car = StateOfAcceleration.isDecelerating;
        else if (accelerationZaxis < 0.1 && accelerationZaxis >= -0.1) car = StateOfAcceleration.constantSpeed; //todo проверить, почему заходит в этот цикл, когда машина стоит на месте

        if (rb.velocity.magnitude == 0) car = StateOfAcceleration.isIdling;
        
    }

    private void createTrajectoryPointClone()
    {
        countdownTimer -= Time.deltaTime;

        if (countdownTimer <= 0f)
        {
            trajectoryPointClone = Instantiate(trajectoryPoint,
                new Vector3(transform.position.x, -0.6f, transform.position.z), transform.rotation) as GameObject;
            countdownTimer = pointAddFrequency;
        }
    }

    private void useConstantTrajectory()
    {

        if (Time.time >= 0 && Time.time <= durationOfStraightTrajectory)
        {
            addConstantVelocity();
        }
        else if (Time.time > durationOfStraightTrajectory && Time.time <= durationOfStraightTrajectory + (durationOfCurve / 2))
        {
            addConstantVelocity();
            addLeftRotation();
        }
        else if (Time.time > durationOfStraightTrajectory + (durationOfCurve / 2) &&
                 Time.time < durationOfStraightTrajectory + durationOfCurve)
        {

            addConstantVelocity();

            addRightRotation();
        }
        else if (Time.time >= durationOfStraightTrajectory + durationOfCurve &&
                 Time.time <= durationOfStraightTrajectory + durationOfCurve + durationOfExitStraight)
        {
            wheels = StateOfTurn.areStandingStraight;
            addConstantVelocity();
        }
        
        else
        {
            stopCarMovement();
            //todo Сделать меню выхода или перезапуска игры
        }
    }

    private void stopCarMovement()
    {
        car = StateOfAcceleration.isIdling;
        wheels = StateOfTurn.areStandingStraight;
        rb.velocity = Vector3.forward * 0f;
    }

    private void addConstantVelocity()
    {
        car = StateOfAcceleration.constantSpeed;
        rb.velocity = transform.forward * speed;
    }

    private void addRightRotation()
    {
        wheels = StateOfTurn.areTurnedRight;
        transform.Rotate(0, rotationStepYaxis, 0, Space.Self);
    }

    private void addLeftRotation()
    {
        wheels = StateOfTurn.areTurnedLeft;
        transform.Rotate(0, -rotationStepYaxis, 0, Space.Self);
    }
}
