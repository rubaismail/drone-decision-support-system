using UnityEngine;

namespace Infrastructure.Drone
{
    public class DroneAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource motorAudio;

        public void StartMotor()
        {
            if (motorAudio != null && !motorAudio.isPlaying)
                motorAudio.Play();
        }

        public void StopMotor()
        {
            if (motorAudio != null && motorAudio.isPlaying)
                motorAudio.Stop();
        }
    }
}
