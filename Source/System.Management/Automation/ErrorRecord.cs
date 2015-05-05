﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    [Serializable]
    public class ErrorRecord : ISerializable
    {
        protected ErrorRecord(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public ErrorRecord(Exception exception, string errorId, ErrorCategory errorCategory, object targetObject)
        {
            Exception = exception;
            ErrorId = errorId;
            TargetObject = targetObject;
            CategoryInfo = new ErrorCategoryInfo(exception, errorCategory);
        }

        internal ErrorRecord(ErrorRecord errorRecord, Exception exception)
        {
            Exception = exception;
            ErrorId = errorRecord.ErrorId;
            TargetObject = errorRecord.TargetObject;
        }

        internal string ErrorId { get; set; }

        public ErrorCategoryInfo CategoryInfo { get; set; }

        public ErrorDetails ErrorDetails { get; set; }
        public Exception Exception { get; internal set; }
        public string FullyQualifiedErrorId { get { return ErrorId; } } // for now it's simply the errorId
        // public InvocationInfo InvocationInfo { get; }
        public object TargetObject { get; internal set; }

        public override string ToString()
        {
            // TODO: implement ErrorRecord.ToString
            return Exception.ToString();
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
