﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Core.App.ErrorContainers;
using Microsoft.PowerFx.Core.Binding;
using Microsoft.PowerFx.Core.Errors;
using Microsoft.PowerFx.Core.Functions;
using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Syntax;

namespace Microsoft.PowerFx.Core.Texl.Builtins
{
    // Mid(source:s, start:n, [count:n])
    internal sealed class MidFunction : BuiltinFunction
    {
        public override bool IsSelfContained => true;

        public MidFunction()
            : base("Mid", TexlStrings.AboutMid, FunctionCategories.Text, DType.String, 0, 2, 3, DType.String, DType.Number, DType.Number)
        {
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new[] { TexlStrings.StringFuncArg1, TexlStrings.StringFuncArg2 };
            yield return new[] { TexlStrings.StringFuncArg1, TexlStrings.StringFuncArg2, TexlStrings.StringFuncArg3 };
        }
    }

    // Mid(source:s|*[s], start:n|*[n], [count:n|*[n]])
    internal sealed class MidTFunction : BuiltinFunction
    {
        public override bool IsSelfContained => true;

        public MidTFunction()
            : base("Mid", TexlStrings.AboutMidT, FunctionCategories.Table, DType.EmptyTable, 0, 2, 3)
        {
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new[] { TexlStrings.StringTFuncArg1, TexlStrings.StringFuncArg2 };
            yield return new[] { TexlStrings.StringTFuncArg1, TexlStrings.StringFuncArg2, TexlStrings.StringFuncArg3 };
        }

        public override string GetUniqueTexlRuntimeName(bool isPrefetching = false)
        {
            return GetUniqueTexlRuntimeName(suffix: "_T");
        }

        public override bool CheckTypes(CheckTypesContext context, TexlNode[] args, DType[] argTypes, IErrorContainer errors, out DType returnType, out Dictionary<TexlNode, DType> nodeToCoercedTypeMap)
        {
            Contracts.AssertValue(args);
            Contracts.AssertAllValues(args);
            Contracts.AssertValue(argTypes);
            Contracts.Assert(args.Length == argTypes.Length);
            Contracts.AssertValue(errors);
            Contracts.Assert(MinArity <= args.Length && args.Length <= MaxArity);

            var fValid = base.CheckTypes(context, args, argTypes, errors, out returnType, out nodeToCoercedTypeMap);

            var type0 = argTypes[0];
            var type1 = argTypes[1];

            // Arg0 should be either a string or a column of strings.
            // Its type dictates the function return type.
            if (type0.IsTableNonObjNull)
            {
                // Ensure we have a one-column table of strings
                fValid &= CheckStringColumnType(context, args[0], type0, errors, ref nodeToCoercedTypeMap, out returnType);
            }
            else
            {
                returnType = DType.CreateTable(new TypedName(DType.String, GetOneColumnTableResultName(context.Features)));
                if (!CheckType(context, args[0], type0, DType.String, errors, ref nodeToCoercedTypeMap))
                {
                    fValid = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[0], TexlStrings.ErrStringExpected);
                }
            }

            // Arg1 should be either a number or a column of numbers.
            if (type1.IsTableNonObjNull)
            {
                fValid &= CheckNumericColumnType(context, args[1], type1, errors, ref nodeToCoercedTypeMap);
            }
            else if (!CheckType(context, args[1], type1, DType.Number, errors, ref nodeToCoercedTypeMap))
            { 
                fValid = false;
                errors.EnsureError(DocumentErrorSeverity.Severe, args[1], TexlStrings.ErrNumberExpected);
            }

            // Arg2 should be either a number or a column of numbers, if it exists.
            if (argTypes.Length > 2)
            {
                var type2 = argTypes[2];
                if (type2.IsTableNonObjNull)
                {
                    fValid &= CheckNumericColumnType(context, args[2], type2, errors, ref nodeToCoercedTypeMap);
                }
                else if (!CheckType(context, args[2], type2, DType.Number, errors, ref nodeToCoercedTypeMap))
                {
                    fValid = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[2], TexlStrings.ErrNumberExpected);
                }
            }

            // At least one arg has to be a table.
            if (!type0.IsTableNonObjNull && !type1.IsTableNonObjNull && (argTypes.Length <= 2 || !argTypes[2].IsTableNonObjNull))
            {
                fValid = false;
                errors.EnsureError(DocumentErrorSeverity.Severe, args[0], TexlStrings.ErrTypeError);
                errors.EnsureError(DocumentErrorSeverity.Severe, args[1], TexlStrings.ErrTypeError);
                if (args.Length > 2)
                {
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[2], TexlStrings.ErrTypeError);
                }
            }

            return fValid;
        }
    }
}
