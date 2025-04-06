using UnityEngine;

public class MusicSyncedLight : MonoBehaviour
{
    public AudioSource musicSource;
    public float bpm; // Beats per minute of the track
    private Light spotLight;
    private float beatInterval;
    private float nextBeatTime;
    private int beatsCount = 0; // Counter for the number of beats
    private bool isLightOn = true; // State of the light
    private int state = 0; // 0 for "three beats" state, 1 for "one beat" state
    private bool flickerOn = true; // Flickering state
    private float nextFlickerTime = 0; // Time for next flicker

    void Start()
    {
        spotLight = GetComponent<Light>();
        beatInterval = 60f / bpm; // Calculate the interval between beats in seconds
        nextBeatTime = beatInterval; // Initialize nextBeatTime
    }

    void Update()
    {
        if (musicSource.isPlaying)
        {
            // Handle beat synchronization
            if (musicSource.time >= nextBeatTime)
            {
                beatsCount++;
                nextBeatTime += beatInterval;

                // Toggle light based on beats
                HandleLightToggle();
            }

            // Handle flickering effect
            if (flickerOn && Time.time >= nextFlickerTime)
            {
                FlickerLight();
            }
        }
    }

    void HandleLightToggle()
    {
        // "Three beats" state
        if (state == 0 && beatsCount >= 3)
        {
            spotLight.enabled = false;
            flickerOn = false;
            beatsCount = 0;
            state = 1;
        }
        // "One beat" state
        else if (state == 1 && beatsCount >= 1)
        {
            spotLight.enabled = true;
            flickerOn = true; // Enable flickering
            nextFlickerTime = Time.time; // Reset flicker timing
            beatsCount = 0;
            state = 0;
        }
    }

    void FlickerLight()
    {
        // Randomly change light intensity
        spotLight.intensity = Random.Range(2f, 10f);

        // Set time for next flicker with random short interval
        nextFlickerTime = Time.time + Random.Range(0.05f, 0.2f);
    }
}
