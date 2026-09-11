using System.Runtime.CompilerServices;

// NSubstitute builds its substitutes through Castle DynamicProxy, which emits the generated proxy
// types into a dynamic assembly named DynamicProxyGenAssembly2. Making that assembly a friend is
// what lets Substitute.For<T>() proxy an internal interface; without it the call throws
// System.ArgumentException ("... because it is not accessible"). Guarded by
// InternalInterfaceSubstitutionTests, so deleting this line names its own cause.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
