using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private const string SfxVolumeKey = "TD_SFX_VOLUME";
        private const string SfxMutedKey = "TD_SFX_MUTED";

        private AudioSource audioSource;
        private float sfxVolume = 0.65f;
        private bool sfxMuted;

        public float SfxVolume => sfxVolume;
        public bool SfxMuted => sfxMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.65f);
            sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null || audioSource == null || sfxMuted)
                return;

            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
        }

        public void SetVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public void SetMuted(bool muted)
        {
            sfxMuted = muted;
            PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (Instance == null)
                return;

            Instance.Play(clip, volume);
        }

        public static void SetSfxVolume(float volume)
        {
            if (Instance == null)
                return;

            Instance.SetVolume(volume);
        }

        public static void SetSfxMuted(bool muted)
        {
            if (Instance == null)
                return;

            Instance.SetMuted(muted);
        }
    }
}
