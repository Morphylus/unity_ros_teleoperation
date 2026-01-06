using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Summon : MonoBehaviour
{
    public InputActionReference summonAction;
    public InputActionReference handActiveAction;
    public InputActionReference palmRotationAction;
    public InputActionReference palmPositionAction;
    
    public bool active;
    public bool reversed = false;
    public bool useHand = true;
    public float speed = 1f;
    public Vector3 offset = new Vector3(0, 0.5f, 0);
    
    public MenuManager menuManager; // CHANGED: Replaced menuCanvas and showMenuButton with menuManager reference
    
    // REMOVED: Start() method - no longer needed
    
    void Update()
    {
        // CHANGED: Keyboard shortcuts now delegate to MenuManager
        if (menuManager != null)
        {
            if (Input.GetKeyDown(KeyCode.H))  // Press H to hide
            {
                menuManager.HideAllMenus();
            }
            if (Input.GetKeyDown(KeyCode.S))  // Press S to show
            {
                menuManager.ShowMenuCanvas();
            }
        }
        
        // REMOVED: Early return that prevented hand tracking when hidden
        // Now hand tracking works even when menu is hidden
        
        float angle;
        Vector3 palm;
        
        // check if the right hand is tracked, if not use controller input only
        if (useHand && handActiveAction.action.ReadValue<float>() > .5f)
        {
            angle = 180 - Quaternion.Angle(palmRotationAction.action.ReadValue<Quaternion>(), Quaternion.Euler(Vector3.up));
            palm = palmPositionAction.action.ReadValue<Vector3>();
            palm += offset;
        }
        else
        {
            angle = 180;
            palm = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }
        
        Vector3 diff = palm - transform.position;
        
        // apply offset to palm
        if ((angle < 5 || summonAction.action.ReadValue<float>() > 0.5f) && diff.magnitude > 0.15f)
        {
            transform.position += diff * Time.deltaTime * speed / diff.magnitude;
            
            // rotate over time to face forward
            if (reversed)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(palm - Camera.main.transform.position + offset / 2), 180);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Camera.main.transform.position - palm + offset / 2), 360 * Time.deltaTime);
            }
        }
    }
    
    // CHANGED: These methods now delegate to MenuManager
    public void ShowMenu()
    {
        if (menuManager != null)
        {
            menuManager.ShowMenuCanvas();
        }
    }
    
    public void HideMenu()
    {
        if (menuManager != null)
        {
            menuManager.HideAllMenus();
        }
    }
}