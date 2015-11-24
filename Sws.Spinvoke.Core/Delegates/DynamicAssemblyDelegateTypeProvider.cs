using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Reflection.Emit;

namespace Sws.Spinvoke.Core.Delegates
{
	public class DynamicAssemblyDelegateTypeProvider : IDelegateTypeProvider
	{
		private const string ModuleName = "DelegateGenerationModule";

		private const string DelegateTypeNamePrefix = "GeneratedDelegate";

		private const string DelegateInvokeMethodName = "Invoke";

		private readonly string _assemblyName;

		private readonly object _assemblyLock = new object();

		private ModuleBuilder _moduleBuilder;

		private int _typeCounter;

		public DynamicAssemblyDelegateTypeProvider(string assemblyName)
		{
			_assemblyName = assemblyName;
			_typeCounter = 0;
		}

		public Type GetDelegateType (DelegateSignature delegateSignature)
		{
			var callingConvention = delegateSignature.CallingConvention;
			var outputType = delegateSignature.OutputType;
			var inputTypes = delegateSignature.InputTypes;

			lock (_assemblyLock	) {
				var typeIndex = _typeCounter++;

				if (_moduleBuilder == null) {
					_moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName {
							Name = _assemblyName,
							Version = new Version (1, 0)
						},
						AssemblyBuilderAccess.RunAndSave)
					.DefineDynamicModule (ModuleName);
				}

				var typeBuilder = _moduleBuilder.DefineType (string.Concat (DelegateTypeNamePrefix, typeIndex.ToString ()),
									  TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
									  typeof(MulticastDelegate));

				typeBuilder.SetCustomAttribute (new CustomAttributeBuilder (typeof(UnmanagedFunctionPointerAttribute).GetConstructor(new Type[] { typeof(CallingConvention) }), new object[] { callingConvention }));

				var constructorBuilder = typeBuilder.DefineConstructor (MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(IntPtr) });

				constructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

				var methodBuilder = typeBuilder.DefineMethod (DelegateInvokeMethodName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, outputType, inputTypes);

				methodBuilder.SetImplementationFlags (MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

				return typeBuilder.CreateType ();
			}
		}
	}
}

