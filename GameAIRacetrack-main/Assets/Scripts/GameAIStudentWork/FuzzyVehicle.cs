// compile_check
// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;
using Tochas.FuzzyLogic.Utils;
using static Tochas.FuzzyLogic.FuzzyCrossfade;
using static Tochas.FuzzyLogic.FuzzyDiscreteSet;
using static Tochas.FuzzyLogic.FuzzyVisualize;

namespace GameAI
{

    public class FuzzyVehicle : AIVehicle
    {

        // TODO create some Fuzzy Set enumeration types, and member variables for:
        // Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
        // Rule Sets for each output.
        // Also, create some methods to instantiate each of the member variables
        //   -->  [implemented] below (original stub only declared FzInputSpeed/FzOutputThrottle/FzOutputWheel)

        enum FzOutputThrottle { Brake, Coast, Accelerate }
        enum FzOutputWheel { HardLeft, Left, Straight, Right, HardRight }

        enum FzInputSpeed { Slow, Medium, Fast, VeryFast }

        // Signed angle (deg) from vehicle forward to a lookahead point on the track centerline.
        // Positive means the aim point is to the right of the nose.
        enum FzInputSteerError { HardLeft, Left, Straight, Right, HardRight }

        // Accumulated road bend (deg, always >= 0) over a speed-scaled window ahead of the car:
        // the sum of |tangent direction changes| between successive samples, so S-curves
        // register as curvy instead of cancelling out to "straight".
        enum FzInputBend { Straight, Mild, Sharp }

        FuzzySet<FzInputSpeed> fzSpeedSet;
        FuzzySet<FzInputSteerError> fzSteerErrorSet;
        FuzzySet<FzInputBend> fzBendSet;

        FuzzySet<FzOutputThrottle> fzThrottleSet;
        FuzzyRuleSet<FzOutputThrottle> fzThrottleRuleSet;

        FuzzySet<FzOutputWheel> fzWheelSet;
        FuzzyRuleSet<FzOutputWheel> fzWheelRuleSet;

        FuzzyValueSet fzInputValueSet = new FuzzyValueSet();

        // These are used for debugging (see ApplyFuzzyRules() call
        // in Update()
        FuzzyValueSet mergedThrottle = new FuzzyValueSet();
        FuzzyValueSet mergedWheel = new FuzzyValueSet();


        // Lookahead tuning: distances (m) grow with speed (m/s) so the car aims and
        // anticipates farther ahead the faster it travels.
        const float AimBaseDist = 5f;
        const float AimSpeedGain = 0.7f;

        const float BendBaseDist = 10f;
        const float BendSpeedGain = 2.0f;
        const int BendSamples = 5;      // tangent samples spread across the bend window


        private FuzzySet<FzInputSpeed> GetSpeedSet()
        {
            FuzzySet<FzInputSpeed> set = null;

            // TODO: Define this fuzzy input variable using GenerateCrossfadeFuzzySet<T>().
            //   -->  [implemented]   (original stub returned: new FuzzySet<FzInputSpeed>())
            // Crisp variable is speed in kph. Shoulders extend coverage to all speeds.

            set = GenerateCrossfadeFuzzySet<FzInputSpeed>(
                (0f, 49f),      // Slow: full DoM up to 49 kph
                71f,            // Medium: triangle peak at 71 kph
                92f,            // Fast: triangle peak at 92 kph
                (120f, 150f)    // VeryFast: full DoM from 120 kph up
            );

            return set;
        }

        // [helper method - not part of the original stub]
        private FuzzySet<FzInputSteerError> GetSteerErrorSet()
        {
            // Crisp variable is the signed angle (deg) from vehicle forward to the aim point.
            var set = GenerateCrossfadeFuzzySet<FzInputSteerError>(
                (-45f, -24f),   // HardLeft
                -9f,            // Left
                (-2f, 2f),      // Straight
                9f,             // Right
                (24f, 45f)      // HardRight
            );

            return set;
        }

        // [helper method - not part of the original stub]
        private FuzzySet<FzInputBend> GetBendSet()
        {
            // Crisp variable is the accumulated |road bend| (deg) over the upcoming window.
            var set = GenerateCrossfadeFuzzySet<FzInputBend>(
                (0f, 12f),      // Straight
                40f,            // Mild
                (75f, 120f)     // Sharp: genuinely tight (hairpin-grade) bend over the window
            );

            return set;
        }

        private FuzzySet<FzOutputThrottle> GetThrottleSet()
        {
            FuzzySet<FzOutputThrottle> set = null;

            // TODO: Define this fuzzy output variable using GenerateDiscreteFuzzySet<T>().
            //   -->  [implemented]   (original stub returned: new FuzzySet<FzOutputThrottle>())

            set = GenerateDiscreteFuzzySet<FzOutputThrottle>(-0.6f, 0f, 1f);

            return set;
        }

        private FuzzySet<FzOutputWheel> GetWheelSet()
        {
            FuzzySet<FzOutputWheel> set = null;

            // TODO: Define this fuzzy output variable using GenerateDiscreteFuzzySet<T>().
            //   -->  [implemented]   (original stub returned: new FuzzySet<FzOutputWheel>())

            set = GenerateDiscreteFuzzySet<FzOutputWheel>(-1f, -0.45f, 0f, 0.45f, 1f);

            return set;
        }


        private FuzzyRuleSet<FzOutputThrottle> GetThrottleRuleSet(FuzzySet<FzOutputThrottle> throttle)
        {

            FuzzyRule<FzOutputThrottle>[] rules =
            {
                // TODO: Add some rules.
                //   -->  [implemented]   (original stub had 3 speed-only example rules)
                // Speed x upcoming-curvature matrix; full coverage of both input axes.

                // Whenever we are slow and roughly aligned with the road, get back up to speed.
                // When we are pointed badly off-line (hard aim error), never accelerate:
                // coast if slow, brake otherwise - accelerating while misaligned is how a
                // recoverable excursion becomes a wipeout.
                If(And(FzInputSpeed.Slow,
                    Not(Or(FzInputSteerError.HardLeft, FzInputSteerError.HardRight)))).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Slow,
                    Or(FzInputSteerError.HardLeft, FzInputSteerError.HardRight))).Then(FzOutputThrottle.Coast),
                If(And(Not(FzInputSpeed.Slow),
                    Or(FzInputSteerError.HardLeft, FzInputSteerError.HardRight))).Then(FzOutputThrottle.Brake),

                // Medium speed: safe to keep accelerating unless a sharp bend is coming.
                If(And(FzInputSpeed.Medium, FzInputBend.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Medium, FzInputBend.Mild)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Medium, FzInputBend.Sharp)).Then(FzOutputThrottle.Brake),

                // Fast: only keep pushing on straights; ease off in bends, brake for sharp ones.
                If(And(FzInputSpeed.Fast, FzInputBend.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Fast, FzInputBend.Mild)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Fast, FzInputBend.Sharp)).Then(FzOutputThrottle.Brake),

                // Very fast (drag / long sweepers): full throttle only when dead straight.
                If(And(FzInputSpeed.VeryFast, FzInputBend.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.VeryFast, FzInputBend.Mild)).Then(FzOutputThrottle.Brake),
                If(And(FzInputSpeed.VeryFast, FzInputBend.Sharp)).Then(FzOutputThrottle.Brake),
            };

            return new FuzzyRuleSet<FzOutputThrottle>(throttle, rules);
        }

        private FuzzyRuleSet<FzOutputWheel> GetWheelRuleSet(FuzzySet<FzOutputWheel> wheel)
        {

            FuzzyRule<FzOutputWheel>[] rules =
            {
                // TODO: Add some rules.
                //   -->  [implemented]   (original stub had an empty rule array)
                // Steer toward the lookahead point: map the aim-error sets directly onto the
                // wheel sets. Crossfade overlap between adjacent error sets fires 2 rules at
                // once in most frames, so the defuzzed steering blends smoothly.

                If(FzInputSteerError.HardLeft).Then(FzOutputWheel.HardLeft),
                If(FzInputSteerError.Left).Then(FzOutputWheel.Left),
                If(FzInputSteerError.Straight).Then(FzOutputWheel.Straight),
                If(FzInputSteerError.Right).Then(FzOutputWheel.Right),
                If(FzInputSteerError.HardRight).Then(FzOutputWheel.HardRight),
            };

            return new FuzzyRuleSet<FzOutputWheel>(wheel, rules);
        }


        protected override void Awake()
        {
            base.Awake();

            StudentName = "Zonghao Hou";

            // DO NOT INITIALIZE FUZZY STUFF HERE!!! Use Start() instead.
        }

        protected override void Start()
        {
            base.Start();

            // TODO: You can initialize a bunch of Fuzzy stuff here like more fuzzy inputs
            //   -->  [implemented]   (added steer-error and curvature input sets)
            fzSpeedSet = this.GetSpeedSet();
            fzSteerErrorSet = this.GetSteerErrorSet();
            fzBendSet = this.GetBendSet();

            fzThrottleSet = this.GetThrottleSet();
            fzThrottleRuleSet = this.GetThrottleRuleSet(fzThrottleSet);

            fzWheelSet = this.GetWheelSet();
            fzWheelRuleSet = this.GetWheelRuleSet(fzWheelSet);
        }

        System.Text.StringBuilder strBldr = new System.Text.StringBuilder();

        override protected void Update()
        {

            // TODO Do all your input fuzzification here and then
            // pass your fuzzy rule sets to ApplyFuzzyRules()
            //   -->  [implemented]   (original stub used temporary fixed steering/throttle debug calls;
            //    both removed)

            // Crisp input 1: signed angle (deg) from our nose to a point on the centerline
            // ahead of us. Lookahead grows with speed so steering anticipates rather than reacts.
            Vector3 aimPoint = Racetrack.GetPointAhead(AimBaseDist + Speed * AimSpeedGain);
            Vector3 toAim = aimPoint - transform.position;
            toAim.y = 0f;
            float steerErrorDeg = Vector3.SignedAngle(transform.forward, toAim, Vector3.up);

            // Crisp input 2: accumulated |bend| (deg) of the road tangent across a speed-scaled
            // window ahead - the "tightness" of what we are about to drive through. Summing the
            // absolute tangent change between successive samples keeps S-curves from cancelling.
            float bendWindow = BendBaseDist + Speed * BendSpeedGain;
            float bendDeg = 0f;
            Vector3 prevDir = Racetrack.ClosestPointDirectionOnPath;
            prevDir.y = 0f;
            for (int i = 1; i <= BendSamples; i++)
            {
                Vector3 sampleDir = Racetrack.GetDirectionAhead(bendWindow * i / BendSamples);
                sampleDir.y = 0f;
                bendDeg += Mathf.Abs(Vector3.SignedAngle(prevDir, sampleDir, Vector3.up));
                prevDir = sampleDir;
            }

            // Fuzzification of vehicle state
            fzSpeedSet.Evaluate(Speed_kph, fzInputValueSet);
            fzSteerErrorSet.Evaluate(steerErrorDeg, fzInputValueSet);
            fzBendSet.Evaluate(bendDeg, fzInputValueSet);

            // ApplyFuzzyRules evaluates your rules and assigns Thottle and Steering accordingly
            // Also, some intermediate values are passed back for debugging purposes
            // Defuzzification output values as defined below are automatically assigned by ApplyFuzzyRules.
            // Throttle: [-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering: [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right
            // Note that you MUST use ApplyFuzzyRules(). You cannot direclty assign Throttle and Steering.

            ApplyFuzzyRules<FzOutputThrottle, FzOutputWheel>(
                fzThrottleRuleSet,
                fzWheelRuleSet,
                fzInputValueSet,
                // access to intermediate state for debugging
                out var throttleRuleOutput,
                out var wheelRuleOutput,
                ref mergedThrottle,
                ref mergedWheel
                );

            // (demo HUD/diagnostic output block removed for submission)

            // Keep the base Update call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (e.g. Throttle, Steering). base.Update() must be called or
            // autograder will fail.
            base.Update();
        }

    }
}
