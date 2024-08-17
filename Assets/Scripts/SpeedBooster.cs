using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BoostCycle
{
    Idle = 0,
    Held = 1,
    Success = 2,
    Stalled = 3,
    NotRunning = 4
}

public class SpeedBooster : MonoBehaviour
{
    public Slider RowSlider;

    private PlayerMovement PlayerMovement;

    public float DefaultSpeed = 10f;
    public float MaxSpeed = 30f;

    [Header("Boost Values")]
    public Vector2 BoostPressWindow = new Vector2(0.2f, 0.4f);
    public Vector2 BoostReleaseWindow = new Vector2(0.8f, 1.0f);
    public Color BoostFailed = Color.red;
    public Color BoostSuccess = Color.blue;
    public Color BoostInProgress = Color.yellow;
    public Color BoostOff = Color.grey;

    //in seconds
    public float BoostChangeDuration = 1.0f;
    public Ease BoostEase = Ease.Linear;

    //moves from 0-1
    //must be released from 
    private float BoostMeterValue = 0;
    private BoostCycle BoostCycle = BoostCycle.Idle;
    Tweener BoostMeterTweener;


    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement = GetComponent<PlayerMovement>();

        BoostCycle = BoostCycle.NotRunning;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoostCycle(Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0));

        float NewSpeed = EvaluateBoostInput(Input.GetMouseButton(0), Input.GetMouseButtonUp(0));

        PlayerMovement.SetMaxSpeed(NewSpeed);

        if (RowSlider != null)
        {
            RowSlider.value = BoostMeterValue;
        }
    }

    private void StartCycle()
    {
        BoostMeterTweener = DOTween.To(() => BoostMeterValue, x => BoostMeterValue = x, 1.0f, BoostChangeDuration).SetLoops(-1);
        BoostMeterTweener.OnStepComplete(OnBoostMeterComplete);
        BoostMeterTweener.SetEase(BoostEase);

        BoostCycle = BoostCycle.Idle;

    }

    private void StopCycle()
    {
        BoostMeterTweener.Kill();
        BoostMeterTweener = null;

        BoostCycle = BoostCycle.NotRunning;
    }

    private void UpdateBoostCycle(bool isPressingMouseButton, bool isReleasingMouseButton)
    {
        //Debug.Log("Cycle: " + BoostCycle);

        //if boost cycle isn't running then start it when button is pressed
        if (BoostCycle == BoostCycle.NotRunning)
        {
            if (isPressingMouseButton)
            {
                //start boost at minimum press window
                StartCycle();
                //Debug.Log("Starting");
                BoostMeterValue = BoostPressWindow.x;
            }
        }

        //button is NOT held and player is starting hold
        if (BoostCycle != BoostCycle.Held && isPressingMouseButton)
        {
            //player pressing hold at proper time
            if (BoostMeterValue >= BoostPressWindow.x && BoostMeterValue <= BoostPressWindow.y)
            {
                //boost started
                //Debug.Log("Boost Started");
                BoostCycle = BoostCycle.Held;
            }

            //player pressing button at wrong time
            else
            {
                //boost set back to idle
                //Debug.Log("Boost Start Failed");
                BoostCycle = BoostCycle.Idle;
            }
        }

        //button IS held and player is releasing
        else if (BoostCycle == BoostCycle.Held && isReleasingMouseButton)
        {
            //player releasing hold at proper time
            if (BoostMeterValue >= BoostReleaseWindow.x && BoostMeterValue <= BoostReleaseWindow.y)
            {
                //Release successful, keep speed
                //Debug.Log("Boost Release Success");

                BoostCycle = BoostCycle.Success;
            }

            //player releasing hold at wrong time
            else
            {
                //Debug.Log("Boost End Failed");
                BoostCycle = BoostCycle.Idle;
            }
        }

        //check if held is held too long
        else if (BoostCycle == BoostCycle.Held)
        {
            if (BoostMeterValue >= BoostReleaseWindow.y)
            {
                //boost held too long
                //Debug.Log("Boost Held too long");
                BoostCycle = BoostCycle.Idle;
            }
        }

        //check when success will expire
        else if (BoostCycle == BoostCycle.Success)
        {
            if (BoostMeterValue >= BoostPressWindow.y && BoostMeterValue <= BoostReleaseWindow.x)
            {
                //boost held too long
                //Debug.Log("Success Expired");
                BoostCycle = BoostCycle.Idle;
            }
        }
    }

    private float EvaluateBoostInput(bool isHoldingMouseButton, bool isReleasingMouseButton)
    {       
        
        //boost cycle was done well, we can chill
        if (BoostCycle == BoostCycle.Held)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostInProgress;
            }
            return MaxSpeed;
        }

        else if (BoostCycle == BoostCycle.Success)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostSuccess;
            }
            return MaxSpeed;
        }
        else if (BoostCycle == BoostCycle.NotRunning)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostOff;
            }
            return DefaultSpeed;
        }

        //player borked it, back to basics
        if (RowSlider != null)
        {
            Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
            Handle.GetComponent<Image>().color = BoostFailed;
        }

        return DefaultSpeed;
    }

    private void OnBoostMeterComplete()
    {
        BoostMeterValue = 0;
        BoostMeterTweener.ChangeStartValue(0.0f);

        //if boost cycle is idle when we finish, stop the cycle
        if (BoostCycle == BoostCycle.Idle)
        {
            StopCycle();
        }
    }
}
