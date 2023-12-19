using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PoseManager : MonoBehaviour
{

    public InputActionReference joystickXY;
    public InputActionReference joystickZR;
    public GameObject sphere;
    public float speed = 1.0f;
    public bool handSelectable = false;
    public Transform root;
    public Transform _root;
    private Transform _mainCamera;

    void Start()
    {
        if (root == null)
        {
            root = transform;
        }
        // need to ensure this happens after tf init....
        _mainCamera = Camera.main.transform;

    }

    void Update()
    {
        while (root.parent != null)
        {
            root = root.parent;
            _root = root;
            Debug.Log("root frame: " + root);
        
        } 
        
        if (joystickXY.action.IsPressed() || joystickZR.action.IsPressed())
        {
            Move(joystickXY.action.ReadValue<Vector2>());
            OffsetRotate(joystickZR.action.ReadValue<Vector2>());
            sphere.SetActive(true);
        } else {
            sphere.SetActive(false);
        }
    }

    void Move(Vector2 input)
    {
        Vector3 move = new Vector3(input.x, 0, input.y);
        // move = root.TransformDirection(move);

        // get relative to the player's view point
        move = _mainCamera.TransformDirection(move);
        // take into account the gameobject's orientation
        // zero out the vertical component
        move.y = 0;
        // move the gameobject relative to the player regardless of gameobject orientation
        _root.Translate(move * speed * Time.deltaTime, Space.World);        
    }

    void OffsetRotate(Vector2 input)
    {
        // offset on the y axis based on forwards/back on second joystick
        Vector3 move = new Vector3(0, input.y, 0);

        // rotate on the x axis based on left/right on second joystick
        _root.Translate(move * speed * Time.deltaTime / 10);
        _root.Rotate(0, input.x * speed * Time.deltaTime * 20, 0);
    }

    public void ClickCb(SelectEnterEventArgs args)
    {
        if (_root == null || !handSelectable) return;

        Vector3 position;
        XRRayInteractor rayInteractor = (XRRayInteractor)args.interactor;
        rayInteractor.TryGetHitInfo(out position, out _, out _, out _);
        _root.position = position;

    }
}
