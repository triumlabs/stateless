using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class ExitActionBehavior
        {
            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args);

            protected ExitActionBehavior(Reflection.InvocationInfo description)
            {
                Description = description ?? throw new ArgumentNullException(nameof(description));
            }

            internal Reflection.InvocationInfo Description { get; }

            public class Sync : ExitActionBehavior
            {
                readonly Action<Transition, object[]> _action;

                public Sync(Action<Transition, object[]> action, Reflection.InvocationInfo description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    _action(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class SyncTo<TTriggerType> : Sync
            {
                internal TTriggerType Trigger { get; private set; }

                public SyncTo(TTriggerType trigger, Action<Transition, object[]> action, Reflection.InvocationInfo description)
                    : base(action, description)
                {
                    Trigger = trigger;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    if (transition.Trigger.Equals(Trigger))
                        base.Execute(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class Async : ExitActionBehavior
            {
                readonly Func<Transition, object[], Task> _action;

                public Async(Func<Transition, object[], Task> action, Reflection.InvocationInfo actionDescription) : base(actionDescription)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnExit event for '{transition.Source}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    return _action(transition, args);
                }
            }
        }
    }
}
