using UnityEngine;
using UnityEngine.Splines;
using TMPro;
using Oculus.Voice.Dictation;
using Meta.WitAi.Dictation;
using System;
using System.Linq;
using Meta.WitAi.TTS.Utilities;

public class GameLogic : MonoBehaviour
{

    string[] states_list;
    string[] input_strings;
    string[] tower_responses;

    string[] input_display_strings;
    int current_state;
    int number_of_states;
    [SerializeField] SplineAnimate splineAnimate;
    bool engine_started;
    bool power_on;
    [SerializeField] OVRHand leftHand;
    [SerializeField] OVRSkeleton ovrSkeleton;
    [SerializeField] OVRSkeleton rightHandSkeleton;
    bool is_pinched;
    float pinch_threshold = 0.5f; // Adjust this value as needed
    float pinch_distance = 0.1f; // Adjust this value as needed

    float splineSpeed = 0f;

    [SerializeField] TMP_Text debugText;
    [SerializeField] TMP_Text inputText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] TMP_Text callSignText;

    [SerializeField] GameObject RDO_trigger;
    [SerializeField] AppDictationExperience appDictationExperience;
    [SerializeField] DictationService dictationService;
    [SerializeField] Whisper whisper;
    [SerializeField] TTSSpeaker ttsService;
    Renderer RDO_trigger_renderer;

    [SerializeField] AudioSource engineAudioSource;
    [SerializeField] GameObject propeller;
    [SerializeField] GameObject propellerCenter;
    [SerializeField] GameObject BatteryMaster;
    [SerializeField] GameObject EngineKey;
    [SerializeField] GameObject BatteryMasterCenter;
    [SerializeField] GameObject CallSignBG;
    [SerializeField] GameObject KeyCenter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        states_list = new string[] {
            "initial", // 0
            "ready_initial_call1", // 1
            "ready_initial_call2", // 2
            "taxi_to_holding_point", // 3
            "ready_departure1", // 4
            "ready_departure2", // 5
            "lining_up", // 6
            "taking_off", // 7
            "crosswind_climbing", // 8
            "downwind1", // 9
            "downwind2", // 10
            "downwind3", // 11
            "base", // 12
            "final1", // 13
            "final2", // 14
            "final3", // 15
            "vacating", // 16
            "runway_vacated1", // 17
            "runway_vacated2", // 18
            "taxi_to_parking", // 19
            "complete" // 20
        };
        number_of_states = states_list.Length;
        current_state = 0; // Start with the initial state
        engine_started = false; // Assume the engine is started for the sake of this example
        power_on = false;

        input_strings = new string[]{
            "",
            "_ Tower Hotel bravo papa papa golf green parking vfr local flight information delta request taxi", // initial call
            "holding point alpha QNH 1012 hotel papa golf",
            "", // taxi to holding point
            "hotel papa golf via outer circuit ready for departure", // ready for departure
            "runway 06 cleared for takeoff hotel papa golf",
            "", // lining up
            "", // taking off
            "",
            "hotel papa golf outer downwind runway 06", // downwind
            "number _ hotel papa golf",
            "", // downwind call done
            "", //base
            "hotel papa golf final runway 06", // final
            "runway 06 cleared to land hotel papa golf",
            "", // final call done
            "", // vacating
            "hotel papa golf runway vacated via delta request taxi", // req taxi to parking
            "taxi to green parking hotel papa golf",
            "", // taxi to green parking
            "",
        };

        tower_responses = new string[]{
            "",
            "Hotel papa golf. taxi to holding point alpha. QNH one zero one two", // initial call
            "",
            "", // taxi to holding point
            "Hotel papa golf. wind. one six zero degrees. five knots. runway zero six. cleared for takeoff", // ready for departure
            "",
            "", // lining up
            "", // taking off
            "",
            "hotel papa golf. number one", // downwind
            "",
            "", // downwind call done
            "", //base
            "hotel papa golf. wind. calm. runway zero six. cleared to land", // final
            "",
            "", // final call done
            "", // vacating
            "hotel papa golf. taxi to green parking", // req taxi to parking
            "",
            "",
            "",
        };

        input_display_strings = new string[]{
            "",
            "Grenchen Tower Hotel bravo papa papa golf green parking vfr local flight information delta request taxi", // initial call
            "holding point alpha QNH 1012 hotel papa golf",
            "", // taxi to holding point
            "hotel papa golf via outer circuit ready for departure", // ready for departure
            "runway 06 cleared for takeoff hotel papa golf",
            "", // lining up
            "", // taking off
            "",
            "hotel papa golf outer downwind runway 06", // downwind
            "number one hotel papa golf",
            "", // downwind call done
            "", //base
            "hotel papa golf final runway 06", // final
            "runway 06 cleared to land hotel papa golf",
            "", // final call done
            "", // vacating
            "hotel papa golf runway vacated via delta request taxi", // req taxi to parking
            "taxi to green parking hotel papa golf",
            "", // taxi to green parking
            "",
        };

        Renderer renderer = RDO_trigger.GetComponent<Renderer>();
        renderer.material.color = Color.black; // Set the color to black

        RDO_trigger_renderer = RDO_trigger.GetComponent<Renderer>();

        // init dication
        dictationService.DictationEvents.OnFullTranscription.AddListener(OnFullTranscription);
        whisper.onTranscriptionFinished.AddListener(OnFullTranscription_Whisper);

        // splineAnimate.method = Spl
        splineAnimate.MaxSpeed = 0f;

        callSignText.text = "<mark=#000000FF><color=#FFFFFFFF>HB-PPG</color></mark>";

        // Set buttons
        BatteryMaster.transform.RotateAround(BatteryMasterCenter.transform.position, BatteryMasterCenter.transform.right, 30f);

        var CallSignBG_renderer = CallSignBG.GetComponent<Renderer>();
        CallSignBG_renderer.material.color = new Color(0, 0, 0, 1.0f); // Set the color to black with 100% transparency
    }

    void OnDestroy()
    {
        dictationService.DictationEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 thumbTip = Vector3.zero;
        Vector3 indexTip = Vector3.zero;

        try
        {
            thumbTip = ovrSkeleton.Bones[5].Transform.position;
            indexTip = ovrSkeleton.Bones[10].Transform.position;
        }
        catch (Exception e)
        {

        }

        switch (current_state)
        {
            case 0: // initial
                    // splineAnimate.MaxSpeed = 0f;

                Vector3 indexTip_right = Vector3.zero;

                try
                {
                    indexTip_right = rightHandSkeleton.Bones[10].Transform.position;
                }
                catch (Exception e)
                {

                }
                // RDO_trigger.transform.position = indexTip; // testing

                if (!power_on && Vector3.Distance(indexTip_right, BatteryMasterCenter.transform.position) < 0.03f)
                {
                    power_on = true;
                    BatteryMaster.transform.RotateAround(BatteryMasterCenter.transform.position, BatteryMasterCenter.transform.right, -30f);
                }

                if (power_on && !engine_started && (Vector3.Distance(indexTip, KeyCenter.transform.position) < 0.03f))
                {
                    inputText.text = "Key turned";
                    engine_started = true;
                }

                if (engine_started)
                {
                    current_state = 1; // Move to the next state
                    engineAudioSource.Play();
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("Very good. Now that the engine is started, contact the tower and request taxi clearance.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                }
                break;

            case 1:
                break;

            case 2:
                // splineAnimate.MaxSpeed = 3f;
                break;

            case 3: // ready for departure
                // splineAnimate.MaxSpeed = 10f;
                if (splineAnimate.StartOffset >= 0.008)
                {
                    splineSpeed = 0f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("Now that we are next to the runway, contact the tower to request take off. ");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state++;
                }

                break;

            case 6: // lining up
                // splineAnimate.MaxSpeed = 10f;
                if (splineAnimate.StartOffset >= 0.0105)
                {
                    splineSpeed = 35f;
                    current_state++;
                }
                break;

            case 7: // taking off
                // splineAnimate.MaxSpeed = 10f;
                if (splineAnimate.StartOffset >= 0.23)
                {
                    splineSpeed = 46f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("We are now entering the crosswind leg. Soon we will reach circuit altitude.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state++;
                }
                break;

            case 8: // crosswind climbing
                if (splineAnimate.StartOffset >= 0.337)
                {
                    splineSpeed = 46f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("We are now entering the downwind leg. Don't forget to report your position to the tower.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state++;
                }
                break;

            case 9: // downwind
                if (splineAnimate.StartOffset >= 0.653)
                {
                    splineSpeed = 60f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("You did not report your position to the tower. We are now already in the base leg. Please report your position in final.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state = 12; // skip to base
                }
                break;
            case 10: // downwind
                if (splineAnimate.StartOffset >= 0.653)
                {
                    splineSpeed = 60f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("You did not report your position to the tower. We are now already in the base leg. Please report your position in final.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state = 12; // skip to base
                }
                break;
            case 11: // downwind
                if (splineAnimate.StartOffset >= 0.653)
                {
                    splineSpeed = 60f;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("Good Job. you reported your position in the downwind. We are now already in the base leg. Please report your position in final.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    current_state = 12; // skip to base
                }
                break;
            case 12: // base
                if (splineAnimate.StartOffset >= 0.824)
                {
                    splineSpeed = 35f;
                    current_state++;
                }
                break;
            case 13: // final
                if (splineAnimate.StartOffset >= 0.974)
                {
                    current_state = 16; // skip to vacating
                    splineSpeed = 5f;
                }
                break;
            case 14: // final
                if (splineAnimate.StartOffset >= 0.974)
                {
                    current_state = 16; // skip to vacating
                    splineSpeed = 5f;
                }
                break;
            case 15: // final
                if (splineAnimate.StartOffset >= 0.974)
                {
                    current_state = 16;
                    splineSpeed = 5f;
                }
                break;
            case 16: // vacating
                if (splineAnimate.StartOffset >= 0.9783)
                {
                    current_state++;
                    ttsService.VoiceID = "WIT$CODY";
                    ttsService.Speak("Now request a taxi clearance to the parking.");
                    ttsService.VoiceID = "WIT$CHARLIE";
                    splineSpeed = 0f;
                }
                break;
            case 19: // taxi to parking
                if (splineAnimate.StartOffset >= 0.9999)
                {
                    current_state++;
                }
                break;

            case 20: // complete
                splineSpeed = 0f;
                ttsService.VoiceID = "WIT$CODY";
                ttsService.Speak("Congratulations. You have completed your first flight. ");
                ttsService.VoiceID = "WIT$CHARLIE";
                break;


        }


        if (!is_pinched && leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinch_threshold && Vector3.Distance((thumbTip + indexTip) / 2, RDO_trigger.transform.position) < pinch_distance)
        {
            is_pinched = true;
            // ttsService.Speak("button pressed"); // for debugging

            // for testing
            //current_state++;
            if (current_state >= number_of_states)
            {
                current_state = 0; // Stay in the last state
            }

            // dictationService.ActivateImmediately();
            whisper.BeginRecording();
        }
        else if (is_pinched && (leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) < pinch_threshold || Vector3.Distance((thumbTip + indexTip) / 2, RDO_trigger.transform.position) >= pinch_distance))
        {

            is_pinched = false;
            // dictationService.Deactivate();
            debugText.text = "dictation deactivated";
            whisper.EndRecording(saveWavLocally: false);
            debugText.text = "Recording ended and saved";
            whisper.UploadLastClip();
            debugText.text = "Clip uploaded";
        }

        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            is_pinched = true;

            // for testing
            //current_state++;
            if (current_state >= number_of_states)
            {
                current_state = 0; // Stay in the last state
            }

            dictationService.ActivateImmediately();
            whisper.BeginRecording();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            debugText.text = "Pinch released";
            is_pinched = false;
            dictationService.Deactivate();
            whisper.EndRecording();
            whisper.UploadLastClip();
        }
        */


        if (Vector3.Distance((thumbTip + indexTip) / 2, RDO_trigger.transform.position) < pinch_distance)
        {
            RDO_trigger_renderer.material.color = Color.red; // Set the color to green
        }
        else
        {
            RDO_trigger_renderer.material.color = Color.black; // Set the color to black
        }

        /*
        debugText.text = "Current State: " + states_list[current_state] + "\n" +
            "Pinch Strength: " + leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) + "\n" +
            "Max Speed: " + splineAnimate.MaxSpeed + "\n" +
            "index: " + (int)OVRSkeleton.BoneId.Hand_Thumb3 + " " + (int)OVRSkeleton.BoneId.Hand_Index3; // 5, 10
        */

        // update spline postion
        float offset = splineAnimate.StartOffset;
        float distance_to_move = splineSpeed * Time.deltaTime;
        distance_to_move = distance_to_move / splineAnimate.Container.Spline.GetLength();
        splineAnimate.StartOffset = offset + distance_to_move;

        statusText.text = "Current State: " + states_list[current_state] + "\n" + "StateNr: " + current_state + "\n";
        inputText.text = input_display_strings[current_state] + "\n";

        // audio pitch
        float pitch = (splineSpeed / 46f) * 0.875f + 0.625f;
        engineAudioSource.pitch = pitch;

        // propeller rotation
        if (engine_started)
        {
            float propellerspeed = ((splineSpeed / 46f) * 1200f + 1000f) * 60f;
            // propellerspeed = 90f;
            propeller.transform.RotateAround(propellerCenter.transform.position, propellerCenter.transform.right, propellerspeed * Time.deltaTime);
        }
    }

    private void OnFullTranscription(string transcription)
    {
        // Handle the full transcription result here
        // debugText.text = "Transcription: " + transcription;
    }

    private void OnFullTranscription_Whisper(string transcription)
    {
        // Handle the full transcription result here
        debugText.text = "Transcription: " + transcription;

        char[] separators = new char[] { ' ', ',', '.' };
        string[] words = transcription.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToUpper()).ToArray();

        if (compare(words))
        {
            debugText.text = "Correct transcription!";


            // Play Response
            if (tower_responses[current_state] != "")
            {
                ttsService.Speak(tower_responses[current_state]);
            }

            // increment state
            current_state++;
            if (current_state >= number_of_states)
            {
                current_state = 0; // Stay in the last state
            }

            switch (current_state)
            {
                case 3: // taxi to holding point
                    splineSpeed = 5f;
                    break;

                case 6: // lining up
                    splineSpeed = 5f;
                    break;

                case 11: // downwind3
                    splineSpeed = 100f;
                    break;

                case 19: // vacating
                    splineSpeed = 5f;
                    break;
            }
        }
        else
        {
            // debugText.text = "Incorrect transcription!";
            ttsService.Speak("I did not get that");
        }

    }

    private bool compare(string[] transcription)
    {
        string[] ground_truth = input_strings[current_state].Split(new char[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToUpper()).ToArray();

        if (transcription.Length < ground_truth.Length)
        {
            return false;
        }

        for (int i = 0; i < transcription.Length; i++)
        {
            if (ground_truth[i] == "_")
            {
                continue;
            }
            if (transcription[i] != ground_truth[i])
            {
                return false;
            }
        }

        return true;
    }
}
