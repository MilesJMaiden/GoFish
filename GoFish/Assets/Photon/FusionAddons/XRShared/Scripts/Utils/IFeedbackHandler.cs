using Fusion.XR.Shared.Rig;

namespace Fusion.XR.Shared
{
    public interface IFeedbackHandler
    {
        public void PlayAudioFeeback(string audioType);
        public void PlayAudioAndHapticFeeback(string audioType);
        public void PlayAudioAndHapticFeeback(string audioType, float hapticAmplitude);
        public void PauseAudioFeeback();
        public void StopAudioFeeback();
        public void PlayHapticFeedback(float hapticAmplitude, HardwareHand hardwareHand);
        public void PlayHapticFeedback(HardwareHand hardwareHand = null);
    }
}
