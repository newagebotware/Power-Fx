﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx
{
    [Obsolete("Remove this")]
    public static class SymbolTableExtensions
    {
        /// <summary>
        /// Adds a UserInfo object schema.
        /// Actual object is added in Runtime config service provider.
        /// </summary>
        public static void AddUserInfoObject(this SymbolTable symbolTable)
        {
            var userInfoType = RecordType.Empty()
                .Add("FullName", FormulaType.String)
                .Add("Email", FormulaType.String)
                .Add("Id", FormulaType.String);

            symbolTable.AddHostObject("User", userInfoType, GetUserInfoObject);
        }

        private static FormulaValue GetUserInfoObject(IServiceProvider serviceProvider)
        {
            var userInfo = (IUserInfo)serviceProvider.GetService(typeof(IUserInfo)) ?? throw new InvalidOperationException("UserInfo object was not added to service");

            RecordValue userRecord = FormulaValue.NewRecordFromFields(
                new NamedValue("FullName", FormulaValue.New(userInfo.FullName ?? string.Empty)),
                new NamedValue("Email", FormulaValue.New(userInfo.Email ?? string.Empty)),
                new NamedValue("Id", FormulaValue.New(userInfo.Id ?? string.Empty)));

            return userRecord;
        }
    }
}
