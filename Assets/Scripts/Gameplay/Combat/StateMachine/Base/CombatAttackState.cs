using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Combat.StateMachine.Base
{
    /// <summary>
    /// Generic abstract base class for multi-step combo-based attack states.
    ///
    /// Responsibilities:
    /// - Manages combo step progression
    /// - Synchronizes hit frame + animation clip end + reaction completion
    /// - Handles mid-combo target death (force finish)
    /// - Ensures combo only advances when ALL conditions are met:
    ///     (1) Hit frame fired for this step
    ///     (2) Attack clip ended
    ///     (3) Target reaction ended
    ///
    /// This class does NOT:
    /// - Subscribe to EventHub (FSM is the only subscriber layer)
    /// - Resolve combat damage
    /// - Select targets
    /// - Control turn flow logic
    /// </summary>
    public abstract class ComboAttackState<TStateMachine> : BaseState
        where TStateMachine : class
    {
        protected readonly TStateMachine StateMachine;

        protected readonly int AttackVariant;
        protected readonly int TotalSteps;

        /// <summary>
        /// Damage type applied by this attack sequence. Carried from the
        /// puzzle tile that triggered the action (physical or magical) so
        /// subclasses can forward it into their hit payloads without
        /// duplicating a whole state per damage type.
        /// </summary>
        protected readonly DamageType DamageType;

        protected int CurrentStep;

        /// <summary>
        /// True after hit frame fired for current step.
        /// Prevents clip-end from advancing combo before hit actually happened.
        /// </summary>
        protected bool HitFrameFiredForStep;

        /// <summary>
        /// True when current step's animation clip ended.
        /// </summary>
        protected bool ClipEndedForStep;

        /// <summary>
        /// True when target hit reaction animation ended for this step.
        /// </summary>
        protected bool ReactionEndedForStep;

        /// <summary>
        /// True if sequence was force-finished due to target death.
        /// </summary>
        protected bool SequenceFinished;

        /// <summary>
        /// Stores the actual step index that triggered combat resolution.
        /// Protects against step mismatch when events are delayed.
        /// </summary>
        protected int LastHitStep;

        protected ComboAttackState(
            TStateMachine stateMachine,
            int attackVariant,
            int attackIntensity,
            DamageType damageType)
        {
            StateMachine = stateMachine;
            AttackVariant = attackVariant;
            TotalSteps = attackIntensity < 1 ? 1 : attackIntensity;
            DamageType = damageType;
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.CombatSystem, "Enter ComboAttackState");
            CurrentStep = 1;

            HitFrameFiredForStep = false;
            ClipEndedForStep = false;
            ReactionEndedForStep = false;
            SequenceFinished = false;
            LastHitStep = 0;

            if (!TryConsumeActionPoint())
            {
                NotifySequenceEnded();
                return;
            }

            PlayAttackAnimation();
        }

        public override void Exit()
        {
            // No-op. EventHub subscription handled at FSM layer.
        }

        // ------------------------------------------------------------------
        // Step synchronization
        // ------------------------------------------------------------------

        /// <summary>
        /// Called at authored hit frame of current step.
        /// This is the ONLY valid moment to raise combat hit.
        /// </summary>
        protected void HandleHitFrame(HitDirection direction)
        {
            if (SequenceFinished)
                return;

            LastHitStep = CurrentStep;
            HitFrameFiredForStep = true;

            GameLogger.Log(
                LogSourceType.CombatSystem,
                $"HitFrame Step={CurrentStep}/{TotalSteps}"
            );

            RaiseAttackHit(CurrentStep, TotalSteps, direction);
        }

        /// <summary>
        /// Called when the current attack clip animation ends.
        /// </summary>
        protected void HandleAttackClipEnded()
        {
            if (SequenceFinished)
                return;

            ClipEndedForStep = true;

            TryAdvanceIfStepComplete();
        }

        /// <summary>
        /// Called when target hit reaction animation ends.
        /// </summary>
        protected void HandleTargetReactionEnded()
        {
            GameLogger.Log(
                LogSourceType.CombatSystem,
                $"HandleTargetReactionEnded called | waiting={ReactionEndedForStep} clipEnded={ClipEndedForStep}"
            );

            if (SequenceFinished)
                return;

            ReactionEndedForStep = true;

            TryAdvanceIfStepComplete();
        }

        /// <summary>
        /// Advances combo if this step is fully complete.
        /// Completion requires:
        /// - Hit frame fired
        /// - Reaction ended
        /// - Clip ended
        /// </summary>
        private void TryAdvanceIfStepComplete()
        {
            if (SequenceFinished)
                return;

            if (!HitFrameFiredForStep)
                return;

            if (!ReactionEndedForStep)
                return;

            // Reset per-step flags
            HitFrameFiredForStep = false;
            ReactionEndedForStep = false;
            ClipEndedForStep = false;

            CurrentStep++;

            if (CurrentStep > TotalSteps)
            {
                // Final sequence completion handled by authored behaviour.
                return;
            }

            AdvanceCombo(CurrentStep);
        }

        // ------------------------------------------------------------------
        // Combat resolution
        // ------------------------------------------------------------------

        /// <summary>
        /// Called when combat resolution completes.
        /// Detects mid-combo target death and force-finishes sequence.
        /// </summary>
        protected void HandleCombatHitResolved(
            CombatHitResolvedPayload payload,
            int attackerId)
        {
            if (SequenceFinished)
                return;

            if (payload.AttackerId != attackerId)
                return;

            // IMPORTANT:
            // Compare with LastHitStep, not CurrentStep,
            // because CurrentStep may have already advanced.
            if (payload.StepIndex != LastHitStep)
                return;

            if (!payload.TargetIsAliveAfter)
            {
                SequenceFinished = true;
                PlayForcedFinish();
            }
        }

        // ------------------------------------------------------------------
        // Abstract API
        // ------------------------------------------------------------------

        protected abstract bool TryConsumeActionPoint();
        protected abstract void PlayAttackAnimation();
        protected abstract void AdvanceCombo(int step);
        protected abstract void PlayForcedFinish();
        protected abstract void RaiseAttackHit(int step, int totalSteps, HitDirection direction);
        protected abstract void NotifySequenceEnded();
    }
}