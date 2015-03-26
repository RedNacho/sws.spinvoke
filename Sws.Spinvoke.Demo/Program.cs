using System;

using Ninject;

using Sws.Spinvoke.Linux;
using Sws.Spinvoke.Interception;
using Sws.Spinvoke.Interception.DynamicProxy;
using Sws.Spinvoke.Ninject.Extensions;

namespace Sws.Spinvoke.Demo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Standard Ninject configuration for Linux and Castle DynamicProxy.

			SpinvokeNinjectExtensionsConfiguration.Configure (
				new LinuxNativeLibraryLoader(),
				new ProxyGenerator(new Castle.DynamicProxy.ProxyGenerator())
			);

			// Binding the real implementation of ICMath to libm.so.

			ICMath realImpl;

			using (var kernel = new StandardKernel ()) {
				// Application of the ToNative extension method.
				kernel.Bind<ICMath> ().ToNative ("libm.so").InSingletonScope ();
				// Resolve an instance.
				realImpl = kernel.Get<ICMath> ();
			}

			// Creating an equivalent fake implementation which uses .NET math functions (let's assume this is unusable in the wild for some reason, maybe it's really slow, although it's actually not).

			ICMath fakeImpl = new FakeCMath ();

			// Test the double and int overloads for each implementation.

			foreach (var implExample in new [] { Tuple.Create(fakeImpl, "Fake"), Tuple.Create(realImpl, "Real") }) {

				var cMath = implExample.Item1;

				var implementationName = implExample.Item2;

				Console.WriteLine ("Testing implementation: {0}", implementationName);

				Console.Write ("Enter the base (double): ");

				var doubleBase = double.Parse (Console.ReadLine ());

				Console.Write ("Enter the exponent (double): ");

				var doubleExponent = double.Parse (Console.ReadLine ());

				var doubleResult = cMath.Pow (doubleBase, doubleExponent);

				Console.WriteLine ("{0} ^ {1} is {2}", doubleBase, doubleExponent, doubleResult);

				Console.Write ("Enter the base (int): ");

				var intBase = int.Parse(Console.ReadLine ());

				Console.Write ("Enter the exponent (int): ");

				var intExponent = int.Parse(Console.ReadLine ());

				var intResult = cMath.Pow (intBase, intExponent);

				Console.WriteLine ("{0} ^ {1} is {2}", intBase, intExponent, intResult);
			}

			Console.WriteLine ("Press any key to exit.");

			Console.ReadKey ();
		}
	}

	// Maps into native functions in libm.so (or equivalent on a non-Linux system).
	public interface ICMath
	{
		// Maps directly into pow function.
		// Override attribute is present purely so we can use the capital "P" for our method name - if we called it "pow" like the C function it wouldn't be necessary.
		[NativeDelegateDefinitionOverride(FunctionName = "pow")]
		double Pow(double @base, double exponent);

		// Illustrates one way of changing the setup of the native delegate.
		// Arguments will be converted to doubles, passed to the native function, and then the output converted to an int.
		// This can also be done with the InputTypes and OutputType properties of the NativeDelegateDefinitionOverride.
		[NativeDelegateDefinitionOverride(FunctionName = "pow")]
		[return: DefaultNativeReturnDefinitionOverride(typeof(double))]
		int Pow(
			[DefaultNativeArgumentDefinitionOverride(typeof(double))] int @base,
			[DefaultNativeArgumentDefinitionOverride(typeof(double))] int exponent);
	}

	// Fake implementation
	public class FakeCMath : ICMath
	{
		public double Pow (double @base, double exponent)
		{
			return Math.Pow (@base, exponent);
		}

		public int Pow (int @base, int exponent)
		{
			return (int) Math.Pow((double) @base, (double) exponent);
		}
	}
}
