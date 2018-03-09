using System;
using System.Reflection;
using JetBrains.Annotations;
using NLog;

namespace CED
{
    internal class DelegatingHook
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [NotNull] private readonly EventInfo _eventInfo;
        [CanBeNull] private readonly object _producerInstance;
        [NotNull] private readonly MethodInfo _methodInfo;
        [CanBeNull] private readonly Delegate _delegate;

        private bool _isHooked;

        internal DelegatingHook([NotNull] EventInfo eventInfo, [CanBeNull]object producerInstance, [NotNull] MethodInfo methodInfo, [CanBeNull]object consumerInstance, bool throwOnFailure)
        {
            _eventInfo = eventInfo ?? throw new ArgumentNullException(nameof(eventInfo));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _producerInstance = producerInstance;
            


            // Try creating a delegate
            _delegate = consumerInstance == null
                ? Delegate.CreateDelegate(_eventInfo.EventHandlerType, methodInfo, throwOnFailure)
                : Delegate.CreateDelegate(_eventInfo.EventHandlerType, consumerInstance, methodInfo, throwOnFailure);
        }

        internal bool Hook()
        {
            try
            {
                _eventInfo.GetAddMethod().Invoke(_producerInstance, new object[] {_delegate});
                _isHooked = true;
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"{nameof(Hook)}: This exception will be dismissed at this point.");
                return false;
            }
        }

        internal bool Unhook()
        {
            if (!_isHooked) return true;

            try
            {
                _isHooked = false;
                _eventInfo.GetRemoveMethod().Invoke(_producerInstance, new object[] {_delegate});

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"{nameof(Unhook)}: This exception will be dismissed at this point.");
                return false;
            }
        }
        
        public bool CreatedSucessfully => _delegate != null; // add others if needed

        public string DebugName =>
            $"{_eventInfo.DeclaringType.FullName} ({_eventInfo.Name}) += {_methodInfo.DeclaringType.FullName} ({_methodInfo.Name})";

    }
}