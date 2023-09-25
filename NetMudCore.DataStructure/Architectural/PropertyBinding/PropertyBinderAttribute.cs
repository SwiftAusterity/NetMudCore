namespace NetMudCore.DataStructure.Architectural.PropertyBinding
{
    public abstract class PropertyBinderAttribute : Attribute
    {
        public abstract object Convert(object input);
    }
}
