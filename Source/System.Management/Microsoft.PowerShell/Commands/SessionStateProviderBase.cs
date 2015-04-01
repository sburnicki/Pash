﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Management;
using System.Management.Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        protected SessionStateProviderBase()
        {
        }

        protected override void ClearItem(string path) { throw new NotImplementedException(); }
        protected override void CopyItem(string path, string copyPath, bool recurse) { throw new NotImplementedException(); }

        protected override void GetChildItems(string path, bool recurse)
        {
            path = NormalizePath(path);

            if (string.IsNullOrEmpty(path))
            {
                IDictionary sessionStateTable = GetSessionStateTable();

                foreach (DictionaryEntry entry in sessionStateTable)
                {
                    WriteItemObject(entry.Value, (string)entry.Key, false);
                }
            }
            else
            {
                object item = GetSessionStateItem(path);

                if (item != null)
                {
                    WriteItemObject(item, path, false);
                }
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            path = NormalizePath(path);

            // check for a named child
            if (path.Length > 0)
            {
                object item = GetSessionStateItem(path);
                if (item != null)
                {
                    WriteItemObject(path, path, false);
                }
                return;
            }
            // otherwise names of all children
            IDictionary sessionStateTable = GetSessionStateTable();
            foreach (DictionaryEntry entry in sessionStateTable)
            {
                var name = (string)entry.Key;
                WriteItemObject(name, name, false);
            }
        }

        protected override void GetItem(string path) {
            path = NormalizePath(path);
            if (path.Length == 0)
            {
                WriteItemObject(PSDriveInfo, path, true);
            }
            object item = GetSessionStateItem(path);
            if (item != null)
            {
                WriteItemObject(item, path, false);
            }
        }

        protected override bool HasChildItems(string path)
        {
            path = NormalizePath(path);
            // we don't support multilevel hierarchy for session state items. so only the root itself has items
            return path.Length == 0;
        }
        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
        {
            if (string.IsNullOrEmpty(path) || new Path(path).IsRootPath())
            {
                return true;
            }

            return null != GetSessionStateItem(path);
        }

        protected override void NewItem(string path, string type, object newItem) { throw new NotImplementedException(); }
        protected override void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }
        protected override void RenameItem(string name, string newName) { throw new NotImplementedException(); }
        protected override void SetItem(string path, object value) { throw new NotImplementedException(); }

        // internals
        internal virtual bool CanRenameItem(object item)
        {
            return true;
        }

        internal abstract object GetSessionStateItem(string name);
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        {
            if (item is DictionaryEntry)
            {
                return ((DictionaryEntry)item).Value;
            }
            return item;
        }
        //TODO: remove these functions from all subclasses - they are never in use!
        internal abstract void RemoveSessionStateItem(string name);
        internal abstract void SetSessionStateItem(string name, object value, bool writeItem);

        #region IContentCmdletProvider Members

        public void ClearContent(string path)
        {
        }

        public object ClearContentDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentReader GetContentReader(string path)
        {
            return new SessionStateContentReader(this, path);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentWriter GetContentWriter(string path)
        {
            return new SessionStateContentWriter(this, path);
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        private string NormalizePath(string path)
        {
            return new Path(path).NormalizeSlashes().TrimEndSlash().ToString();
        }
    }
}
