namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Abstraction over the camera shake implementation so CombatFeedbackPresenter
    /// stays plain C# and testable without Cinemachine.
    /// </summary>
    public interface ICameraShaker
    {
        /// <summary>
        /// Plays a single camera shake with the given impulse force.
        /// </summary>
        /// <param name="force">Impulse force. Zero or less plays nothing.</param>
        void Shake(float force);
    }
}
