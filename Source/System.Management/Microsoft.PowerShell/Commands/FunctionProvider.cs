﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
    [CmdletProvider("Function", ProviderCapabilities.ShouldProcess)]
    public sealed class FunctionProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Function";

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            PSDriveInfo item = new PSDriveInfo("Function", base.ProviderInfo, string.Empty, string.Empty, null);
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            collection.Add(item);
            return collection;
        }

        protected override object NewItemDynamicParameters(string path, string type, object newItemValue)
        {
            return new FunctionProviderDynamicParameters();
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            return new FunctionProviderDynamicParameters();
        }

        internal override bool CanRenameItem(object item)
        {
            throw new NotImplementedException();
        }

        internal override object GetSessionStateItem(string name)
        {
            return SessionState.Function.Get(name);
        }

        internal override IDictionary GetSessionStateTable()
        {
            return SessionState.Function.GetAll();
        }

        internal override object GetValueOfItem(object item)
        {
            object scriptBlock = item;
            FunctionInfo info = item as FunctionInfo;
            if (info != null)
            {
                return info.ScriptBlock;
            }
            else
            {
                // TODO: the item can be of FilterInfo type
            }
            return scriptBlock;

        }

        internal override void RemoveSessionStateItem(string name)
        {
            throw new NotImplementedException();
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            throw new NotImplementedException();
        }

        protected override void GetItem(string name)
        {
            WriteItemObject(GetSessionStateItem(name), name, false);
        }
    }
}
