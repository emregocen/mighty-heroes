namespace MightyHeroes.Gameplay.Animation.Interfaces
{
    /// <summary>
    /// Implemented by BattleIntroController to receive taunt animation
    /// boundary signals from StateMachineBehaviours attached to the taunt clip.
    /// </summary>
    public interface IIntroSignalReceiver
    {
        void NotifyTauntStarted();
        void NotifyTauntEnded();
    }
}