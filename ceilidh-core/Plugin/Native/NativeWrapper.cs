using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Plugin.Native
{
    [Obsolete("The methods in this class can cause real damage, and you can't rely on their implicit contracts remaining consistent between versions.")]
    public static unsafe class NativeWrapper
    {
        /// <summary>
        ///     Implement a contract from an opaque pointer
        /// </summary>
        /// <typeparam name="TContract">The contract type to implement</typeparam>
        /// <param name="prototype">
        ///     The prototype memory: this memory begins with an array of function pointers, but can contain
        ///     any data. This is duplicated for every new instance of the contract impl.
        /// </param>
        /// <param name="size">The size of the opaque pointer.</param>
        /// <returns>A type that implements the specified contract.</returns>
        public static WrapperContainer<TContract> ImplementContract<TContract>(IntPtr prototype, int size) =>
            new WrapperContainer<TContract>(ImplementContract(typeof(TContract), prototype, size, IntPtr.Zero));

        /// <summary>
        ///     Implement a contract from an opaque pointer
        /// </summary>
        /// <typeparam name="TContract">The contract type to implement</typeparam>
        /// <param name="prototype">
        ///     The prototype memory: this memory begins with an array of function pointers, but can contain
        ///     any data. This is duplicated for every new instance of the contract impl.
        /// </param>
        /// <param name="size">The size of the opaque pointer.</param>
        /// <param name="constructor">A constructor function to be called on the newly copied memory block.</param>
        /// <returns>A type that implements the specified contract.</returns>
        public static WrapperContainer<TContract> ImplementContract<TContract>(IntPtr prototype, int size,
            IntPtr constructor) =>
            new WrapperContainer<TContract>(ImplementContract(typeof(TContract), prototype, size, constructor));

        /// <summary>
        ///     Implement a contract from an opaque pointer
        /// </summary>
        /// <param name="tContract">The contract type to implement</param>
        /// <param name="prototype">
        ///     The prototype memory: this memory begins with an array of function pointers, but can contain
        ///     any data. This is duplicated for every new instance of the contract impl.
        /// </param>
        /// <param name="size">The size of the opaque pointer.</param>
        /// <returns>A type that implements the specified contract.</returns>
        public static Type ImplementContract(Type tContract, IntPtr prototype, int size) =>
            ImplementContract(tContract, prototype, size, IntPtr.Zero);

        /// <summary>
        ///     Implement a contract from an opaque pointer
        /// </summary>
        /// <param name="tContract">The contract type to implement</param>
        /// <param name="prototype">
        ///     The prototype memory: this memory begins with an array of function pointers, but can contain
        ///     any data. This is duplicated for every new instance of the contract impl.
        /// </param>
        /// <param name="size">The size of the opaque pointer.</param>
        /// <param name="constructor">A constructor function to be called on the newly copied memory block.</param>
        /// <returns>A type that implements the specified contract.</returns>
        public static Type ImplementContract(Type tContract, IntPtr prototype, int size, IntPtr constructor)
        {
            if (tContract == null) throw new ArgumentNullException(nameof(tContract));
            if (prototype == IntPtr.Zero) throw new ArgumentNullException(nameof(prototype));
            if (size == 0) throw new ArgumentOutOfRangeException(nameof(size));

            if (!tContract.IsInterface)
                throw new ArgumentException("Passed contract type must be an interface", nameof(tContract));

            if (size < tContract.GetMethods(BindingFlags.Public | BindingFlags.Instance).Length * sizeof(IntPtr))
                throw new ArgumentOutOfRangeException(nameof(size),
                    "Structure size must be able to contain at least every function pointer");

            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()),
                AssemblyBuilderAccess.Run);
            var mb = asm.DefineDynamicModule(asm.GetName().Name);
            var tb = mb.DefineType($"{tContract.Name}Impl", TypeAttributes.Public | TypeAttributes.Class);
            tb.AddInterfaceImplementation(tContract);

            var thField = tb.DefineField("this", typeof(IntPtr), FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctorIl = ctor.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

            // load this
            ctorIl.Emit(OpCodes.Ldarg_0);

            // alloc this
            ctorIl.Emit(OpCodes.Ldc_I4, size);
            ctorIl.Emit(OpCodes.Call, typeof(Marshal).GetMethod(nameof(Marshal.AllocHGlobal), new[] {typeof(int)}));

            // store this
            ctorIl.Emit(OpCodes.Stfld, thField);


            // load this into dest
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldfld, thField);

            // source
            ctorIl.Emit(OpCodes.Ldc_I8, (long) prototype);
            ctorIl.Emit(OpCodes.Conv_I);

            // load src/dest size
            ctorIl.Emit(OpCodes.Ldc_I8, (long) size);

            // copy prototype into this
            ctorIl.Emit(OpCodes.Cpblk);

            // If we have a ctor, call it with the new this ptr
            if (constructor != IntPtr.Zero)
            {
                // Load this as arg
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Ldfld, thField);

                // invoke native ctor
                ctorIl.Emit(OpCodes.Ldc_I8, (long) constructor);
                ctorIl.Emit(OpCodes.Conv_I);
                ctorIl.EmitCalli(OpCodes.Calli, CallingConvention.Cdecl, typeof(void), new[] {typeof(void*)});
            }

            ctorIl.Emit(OpCodes.Ret);

            var pos = 0;
            foreach (var implMethod in tContract.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var implB = tb.DefineMethod(implMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                    implMethod.ReturnType, implMethod.GetParameters().Select(x => x.ParameterType).ToArray());
                var mGen = implB.GetILGenerator();

                // load this ptr
                mGen.Emit(OpCodes.Ldarg_0);
                mGen.Emit(OpCodes.Ldfld, thField);

                for (var i = 0; i < implMethod.GetParameters().Length; i++)
                    mGen.Emit(OpCodes.Ldarg, i + 1);

                // load method ptr
                mGen.Emit(OpCodes.Ldarg_0);
                mGen.Emit(OpCodes.Ldfld, thField);
                mGen.Emit(OpCodes.Ldc_I4, sizeof(IntPtr) * pos++);
                mGen.Emit(OpCodes.Add);
                mGen.Emit(OpCodes.Ldind_I);

                var param = implMethod.GetParameters().Select(x => x.ParameterType).Prepend(typeof(IntPtr)).ToArray();
                mGen.EmitCalli(OpCodes.Calli, CallingConvention.Cdecl, implMethod.ReturnType, param);
                mGen.Emit(OpCodes.Ret);

                tb.DefineMethodOverride(implB, implMethod);
            }

            return tb.CreateType();
        }

        public class WrapperContainer<TContract>
        {
            private readonly Type _implType;

            internal WrapperContainer(Type type)
            {
                _implType = type;
                if (!typeof(TContract).IsAssignableFrom(_implType))
                    throw new ArgumentException("Generated type must implement the contract interface", nameof(type));
            }

            public TContract CreateImplementation() => (TContract) Activator.CreateInstance(_implType);
        }
    }
}