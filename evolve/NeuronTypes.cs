namespace evolve
{
    public interface IActivatable
    {
        public float Activate(Node node);
    }

    public interface ISensor : IActivatable
    {
    }
    
    public interface ISink
    {
        public void UpdateWeight(float weight);
        public void Reset();
    }

    public interface IInnerNeuron : IActivatable, ISink
    {
    }

    public interface IAction : ISink
    {
        public void Act(Node node);
    }
}