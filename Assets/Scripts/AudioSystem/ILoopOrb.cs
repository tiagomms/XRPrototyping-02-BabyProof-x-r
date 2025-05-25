namespace XRLoopPedal.AudioSystem
{
    // Interface for orb control
    public interface ILoopOrb
    {
        void SetState(LoopOrbState newState);
        void PlayStep(int index);
        void ResetLoop();
    }
}