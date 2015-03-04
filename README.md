**THE (POSSIBLY IMAGINARY) PROBLEM!**

It's all a bit vague at the moment... but the idea is this.

If you're a C# programmer and you want to call a native function, you can do one of two things:

1. Use DllImport and extern, if you know the library and function names at design time.  Personally I prefer to steer clear of this, as the library is just an arbitrary file, and I don't want to hard-code where it is.

2. Define a delegate type to match the function signature, load the library dynamically and get hold of the function pointer (e.g. using Kernel32 on Windows), then do some funky stuff with Marshal to create a delegate instance.

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

3. Call BindingToSyntaxExtensions.Configure with a native library loader for your OS (I've only included a Linux implementation but a Windows one is extremely easy - Google LoadLibrary Kernel32).  I'm leaving this up to the client for now, as I can't think of every OS in the universe, and even if I could I wouldn't be able to test my code on all of them.

4. Call Bind<T>().ToNative(libraryName) to bind interface T to a native library which implements the required functions.

There is a NativeDelegateDefinitionOverride attribute which you can add to the interface methods to change stuff like the function name and the calling convention, but it's a work in progress.

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

SpinvokeNinjectExtensionsConfiguration.Configure(new LinuxNativeLibraryLoader());

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
