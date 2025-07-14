using OneScript.DebugProtocol;

namespace VSCode.DebugAdapter
{
    public class ThreadStateContainer
    {
        private readonly Handles<StackFrame> _framesHandles = new Handles<StackFrame>();
        private readonly Handles<IVariableLocator> _variableHandles = new Handles<IVariableLocator>();

        public void Reset()
        {
            _framesHandles.Reset();
            _variableHandles.Reset();
        }

        public int RegisterFrame(StackFrame processFrame) => _framesHandles.Create(processFrame);

        public StackFrame GetFrameById(int id) => _framesHandles.Get(id, null);

        public int RegisterVariableContainer(StackFrame frame) => _variableHandles.Create(frame);
        
        public int RegisterVariableContainer(IVariableLocator parent) => _variableHandles.Create(parent);
        
        public IVariableLocator GetVariableContainerById(int id) => _variableHandles.Get(id, null);
    }
}