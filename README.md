**THE (POSSIBLY IMAGINARY) PROBLEM!**

It's all a bit vague at the moment... but the idea is this.

If you're a C# programmer and you want to call a native function, you can do one of two things:

1. Use DllImport and extern, if you know the library and function names at design time.  Personally I prefer to steer clear of this, as the library is just an arbitrary file, and I don't want to hard-code where it is.

2. Define a delegate type to match the function signature, load the library dynamically and get hold of the function pointer (e.g. using Kernel32 on Windows), then do some funky stuff with Marshal to create a delegate instance.

*Edit: You can also try some other things, like wrapping up the native code with some more friendly intermediary such as managed C++, but some of us prefer to be home by dinner time.*

It occurred to me the other day that both of these approaches are quite annoying when it comes to dependency injection.

In the first case, your methods must be static, which sort of makes sense, given that they actually are, but in order to abstract it away behind an interface, you need to repeat all your method definitions in the interface, then create a singleton implementation to wrap the extern calls, etc, etc...

In the second case, you've got a similar mountain of boilerplate.  You've got to define a delegate for each method, then you've got to instantiate the delegates, and then you've got to wrap them up in an implementing class (again, presumably a singleton).  You could probably make life easier for yourself by exposing the delegates directly, since in this scenario you can instantiate them however you want, but all the other developers will hate you.

**THE (POSSIBLY UNNECESSARY) SOLUTION!**

What you really want to be able to do is this:

1. Define all of the native methods for the library just once, in an interface, which you can then mock and stub and generally abuse.

2. Invoke some neat little gizmo with the name of the library to automagically wire up the interface to the real implementation.

I've been trying to write that gizmo.  At the moment it's very much at the PoC stage, but you can bind an interface to a native library as follows:

1. Use Ninject and Castle DynamicProxy.

2. Use Sws.Spinvoke.Ninject.Extensions.

3. Call SpinvokeNinjectExtensionsConfiguration.Configure with a native library loader for your OS (I've only included a Linux implementation but a Windows one is extremely easy - Google LoadLibrary Kernel32) and a proxy generator.  I'm leaving the native library loader up to the client for now, as I can't think of every OS in the universe, and even if I could I wouldn't be able to test my code on all of them.  You can also use your own proxy generator, although I'd recommend using the supplied Castle DynamicProxy implementation unless you have a good reason not to: Sws.Spinvoke.Interception.DynamicProxy.ProxyGenerator.

4. Call Bind<T>().ToNative(libraryName) to bind interface T to a native library which implements the required functions.

**UNTESTED EXAMPLE!**

C functions defined in libNativeCalculator.so:


```
#!c

int add(int x, int y);
int subtract(int x, int y);
int divide(int x, int y);
int multiply(int x, int y);
```


C# interface:


```
#!c#

public interface INativeCalculator
{
    int add(int x, int y);
    int subtract(int x, int y);
    int divide(int x, int y);
    int multiply(int x, int y);
}
```


Wiring:


```
#!c#

SpinvokeNinjectExtensionsConfiguration.Configure(
    new LinuxNativeLibraryLoader(),
    new ProxyGenerator(new Castle.DynamicProxy.ProxyGenerator()));

var kernel = new StandardKernel();
kernel.Bind<INativeCalculator>().ToNative("libNativeCalculator.so");

var nativeCalculator = kernel.Get<INativeCalculator>();

```

Usage:


```
#!c#

var nativeAddResult = nativeCalculator.add(4, 5);
var nativeSubtractResult = nativeCalculator.subtract(11, 2);
var nativeDivideResult = nativeCalculator.divide(18, 2);
var nativeMultiplyResult = nativeCalculator.multiply(3, 3);
```

**A NOTE ABOUT ATTRIBUTES**

I have added a number of attributes which allow the native delegate signature generated by the proxy to be manipulated in various ways, and pre- and post-processing to be applied. These are in Sws.Spinvoke.Interception, and are as follows:

* **NativeDelegateDefinitionOverride**.  Apply this to the delegate method to override various things (all of which are optional as they are inferred by default).  This includes the library name, the function name, the input and output types for the native delegate, the calling convention.  It also allows you specify an explicit delegate type (see below).

* **NativeArgumentDefinitionOverride**.  Abstract; subclasses can be applied to parameters to give finer control.  You can subclass this yourself as required, although the existing classes provide useful examples.

* **NativeReturnDefinitionOverride**.  Abstract; subclasses can be applied to the return value to give finer control.  You can subclass this yourself as required, although the existing classes provide useful examples.

* **DefaultNativeArgumentDefinitionOverride**.  Inherits NativeArgumentDefinitionOverride.  Default parameter settings if none specified.  Applies Convert.ChangeType as necessary to convert the parameter type on the interface to the specified input type.  In the implicit usage, the input type is either unchanged from the interface, or set to the input type specified in the NativeDelegateDefinitionOverride.

* **DefaultNativeReturnDefinitionOverride**.  Inherits NativeReturnDefinitionOverride.  Default return value settings if none specified.  Applies Convert.ChangeType as necessary to convert the expected output type to the return type on the interface.  In the implicit usage, the output type is either unchanged from the interface, or set to the output type specified in the NativeDelegateDefinitionOverride.

* **NativeArgumentAsStringPointer**.  Inherits NativeArgumentDefinitionOverride.  Assigns strings to IntPtr before passing the pointer to the native delegate.

* **NativeReturnsStringPointer**.  Inherits NativeReturnDefinitionOverride.  Expects IntPtr back from the delegate and marshals a string out of it.

* **NativeArgumentAsStructPointer**.  Inherits NativeArgumentDefinitionOverride.  Assigns value types to IntPtr before passing the pointer to the native delegate.

* **NativeReturnsStructPointer**.  Inherits NativeReturnDefinitionOverride.  Expects IntPtr back from the delegate and marshals a value type out of it.

**ANOTHER NOTE ABOUT ATTRIBUTES, AND ABOUT KNOWING WHEN YOU'RE BEATEN**

After experimenting with various techniques, I have come to the conclusion that it's impossible for me to fulfill all of your automatically generated delegate needs all of the time.  I have therefore added an attribute option which allows you to explicitly specify the delegate you want to use, in scenarios where the generated one won't cut it, or you just think it's easier.

I don't really like it, but it's still a damn sight quicker than doing all the boilerplate stuff I talked about at the beginning of the readme.

Usage:

```
#!c#

// Decorate this with all the shadazzle you know and love - MarshalAs, UnmanagedFunctionPointer, etc.
public delegate int ExplicitAddDelegate(int x, int y);

public interface INativeCalculator
{
    [NativeDelegateDefinitionOverride(ExplicitDelegateType = typeof(ExplicitAddDelegate))]
    int add(int x, int y);
    int subtract(int x, int y);
    int divide(int x, int y);
    int multiply(int x, int y);
}
```

**EXTENSION POINTS**

Most of the usage and examples I've given assume a certain default usage, which you don't have to follow.  I'll now explain what you have to change if this doesn't fit your requirements.

* **You don't have to use Linux.**  I've only provided the LinuxNativeLibraryLoader because I can't test anything else at the moment.  However, my day job is .NET development, and would you believe I use Windows.  A Windows implementation of INativeLibraryLoader is extremely simple - the interface methods map fairly directly onto Kernel32 functions.

* **You don't have to use Ninject.**  You'll have to assemble an Sws.Spinvoke.Core.INativeDelegateResolver implementation yourself, using the SpinvokeModule as a guide, but it should be fairly easy to follow.  Here's a poor man's DI example based on the current code:

```
#!c#

var resolver = new DefaultNativeDelegateResolver (
	               new LinuxNativeLibraryLoader (),
	               new CachedDelegateTypeProvider (
		               new DynamicAssemblyDelegateTypeProvider ("MyTempAssembly"),
		               new SimpleCompositeKeyedCache<Type> ()),
	               new FrameworkNativeDelegateProvider ());

```

You can then assemble an NativeDelegateInterceptor, wrap it in an Sws.Spinvoke.Interception.DynamicProxy.SpinvokeInterceptor (which adapts it to Castle DynamicProxy), and then use this with Castle DynamicProxy as you would any other interceptor.

* **You don't have to use Castle DynamicProxy.**  Instead of using Sws.Spinvoke.Interception.DynamicProxy.SpinvokeInterceptor, you can write an interceptor adapter for whatever proxy generator you want to use instead, and use that directly.  If you still want to use the Ninject extension methods, you can provide your own implementation of Sws.Spinvoke.Interception.IProxyGenerator, and pass this to the SpinvokeNinjectExtensionsConfiguration.Configure method.

* **You don't have to use interception at all.**  I have deliberately left the interception code out of the Core.  The Core itself is purely a native delegate generation and management library.  Provided you can create your own NativeDelegateDefinitions, you can use INativeDelegateResolver directly.

* **You don't have to use the default implementation for Sws.Spinvoke.Core.INativeDelegateResolver**.  As you can see from the poor man's DI code sample above, you can swap out INativeDelegateResolver or any component of it.  As an example, you might be able to think of a better way of generating the delegate types than me.  In that case, substitute whatever you want in place of the DynamicAssemblyDelegateTypeProvider.  You can then use this directly, or in conjunction with the Interception library, or with the Ninject extensions (I have added a Configure overload which lets you specify the INativeDelegateResolver implementation directly).

**HOW IT WORKS!**

In case you're incredibly nerdy, or you've tried to use my code and found some irritating problem, I will now attempt a coherent explanation of how Sws.Spinvoke works.

Firstly, the projects:

* **Sws.Spinvoke.Core**: This is the core library.  In a nutshell, it includes everything you need to generate a delegate for native code, given a NativeDelegateDefinition.  The standard entry point to this functionality is INativeDelegateResolver, which has Resolve and Release methods.  (Release can be used to allow a library to be unloaded when you're done with it.)

* **Sws.Spinvoke.Interception**: This is the interception library.  Everything here revolves around NativeDelegateInterceptor, which has the job of intercepting method calls, getting together a NativeDelegateDefinition, calling INativeDelegateResolver.Resolve, and then invoking the delegate.

* **Sws.Spinvoke.Interception.DynamicProxy**: This library ties the interception library up to Castle DynamicProxy (you can provide your own implementation if you wish).  There are two modes of operation: use SpinvokeInterceptor to adapt a Spinvoke interceptor to a Castle DynamicProxy interceptor (which you can then use in conjunction with DynamicProxy to create native code proxies), or use ProxyGenerator to adapt a Castle DynamicProxy proxy generator to the Spinvoke interception library's IProxyGenerator interface (which you can use to configure the Ninject library).

* **Sws.Spinvoke.Linux**: The Core code is dependent upon an INativeLibraryLoader interface, the implementation of which is platform-specific.  This is a Linux implementation of the interface.

* **Sws.Spinvoke.Ninject**: This is Ninject-specific code.  The primary entry point is the ToNative extension method, which binds an interface to a native code wrapper using the other libraries.  It uses SpinvokeModule, which you can also use directly if you want to use Sws.Spinvoke.Core without interception.

Secondly, here's a complete guide to what happens when you use the Ninject ToNative extension method:

Configuration:

1. When you call SpinvokeNinjectExtensionsConfiguration.Configure, the INativeLibraryLoader and IProxyGenerator implementations you supply (e.g. from Sws.Spinvoke.Linux and Sws.Spinvoke.Interception.DynamicProxy) are used to set up the ToNative method.  The native library loader is used in conjunction with the SpinvokeModule to wire up an INativeDelegateResolver.
2. SpinvokeModule specifies that the INativeDelegateResolver is an instance of Sws.Spinvoke.Core.DefaultNativeDelegateResolver.
3. DefaultNativeDelegateResolver has three arguments: An INativeLibraryLoader (which you supplied), an IDelegateTypeProvider, and an INativeDelegateProvider.  These objects can be used together to get a function pointer for a native function, create a delegate type, and combine the two to instantiate a delegate.
4. IDelegateTypeProvider has the job of getting a delegate type given a delegate signature.  It is bound to a DynamicAssemblyDelegateTypeProvider instance, through a wrapper which handles caching.  DynamicAssemblyDelegateTypeProvider uses Reflection.Emit to generate a type based on the delegate signature, which is stored in a dynamically generated assembly.
5. INativeDelegateProvider has the job of instantiating a native delegate given a delegate type and a function pointer.  It is bound to FrameworkNativeDelegateTypeProvider, which just uses Marshal.GetDelegateForFunctionPointer.

Binding to a native proxy:

1. When you call the ToNative method with a library name (and optionally a calling convention), a custom Ninject Provider is created which will handle the proxy resolution.

Resolving the native proxy:

1. When you call Get and the provider's CreateInstance method is called, an Sws.Spinvoke.Interception.NativeDelegateInterceptor is instantiated.
2. The NativeDelegateInterceptor has three arguments: The library name (passed through from the ToNative call), the calling convention (Winapi by default, or as specific in the ToNative call), and the INativeDelegateResolver which was resolved earlier.
3. The CreateInstance method then passes the interceptor to the proxy generator, which gives you a proxy into the native code.

Using the native proxy:

1. When you call one of the methods of the generated proxy, the invocation is intercepted by NativeDelegateInterceptor.
2. NativeDelegateInterceptor uses the MethodInfo to create a native delegate mapping.  This factors in the attributes discussed above, and ultimately provides all of the information required to pass through to native code.
3. Each argument in the invocation is processed through its associated IArgumentPreprocessor (which may have come from an attribute, or it might just be the default, which uses System.Convert to change the type of the argument to the required input type, if it is different).  If the IArgumentPreprocessor cannot handle the argument (for example, if you're using StringToPointer and the argument is not a string), an InvalidOperationException is thrown.
4. The input types from the native delegate mapping are then checked against the processed arguments.  If the processed arguments do not match the input types, the input type is changed to match the argument (in the hope that .NET can deal with it lower down).
5. The information is then put together into a NativeDelegateDefinition, as expected by the Core library.
6. The native delegate resolver (i.e. DefaultNativeDelegateResolver) is then called to resolve a delegate based on the NativeDelegateDefinition.
7. DefaultNativeDelegateResolver loads the library (if it is not already loaded) using its INativeLibraryLoader.
8. It then checks to see if it already has a delegate cached for this definition.  If so, this delegate is returned.
9. If there is no delegate cached, it gets a function pointer for the function using INativeLibraryLoader.
10. If no ExplicitDelegateType has been specified, it then goes to the delegate type provider (DynamicAssemblyDelegateTypeProvider, via a cache to improve performance) to build a type for the delegate.
11. As discussed earlier, DynamicAssemblyDelegateTypeProvider assembles a type for the delegate using Reflection.Emit.
12. The delegate type and the function pointer are then passed to the native delegate provider (FrameworkNativeDelegateProvider) to create a delegate instance for the native code.  As discussed earlier, this just uses Marshal.GetDelegateForFunctionPointer.
13. The delegate instance is cached and returned to the interceptor.
14. The interceptor then invokes the delegate with the processed arguments.  If all goes well, this calls into the native code, which does its stuff and returns a value of the expected output type.
15. The return value is then put through an IReturnPostprocessor (which may have come from an attribute, or it might just be the default, which uses ChangeType) to convert the value to the return value required by the method.  This is then set as the return value for the intercepted call.
16. Once the process is complete, each IArgumentPreprocessor has its ReleaseProcessedInput method called with the processed argument, in case it wants to do any tidying up.