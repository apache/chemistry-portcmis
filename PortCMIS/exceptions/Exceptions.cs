/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements. See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership. The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* Kind, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using System;

namespace PortCMIS.Exceptions
{
    /// <summary>
    /// Base exception for all CMIS exceptions.
    /// </summary>
    public abstract class CmisBaseException : Exception
    {
        public CmisBaseException() : base() { Code = null; }
        public CmisBaseException(string message) : base(message) { Code = null; }
        public CmisBaseException(string message, Exception inner) : base(message, inner) { Code = null; }
        public CmisBaseException(string message, long? code)
            : this(message)
        {
            Code = code;
        }
        public CmisBaseException(string message, string errorContent)
            : this(message)
        {
            ErrorContent = errorContent;
        }
        public CmisBaseException(string message, string errorContent, Exception inner)
            : this(message, inner)
        {
            ErrorContent = errorContent;
        }
        public long? Code { get; protected set; }
        public string ErrorContent { get; protected set; }

        public abstract string GetExtensionName();
    }

    public class CmisConnectionException : CmisBaseException
    {
        public CmisConnectionException() : base() { }
        public CmisConnectionException(string message) : base(message) { }
        public CmisConnectionException(string message, Exception inner) : base(message, inner) { }
        public CmisConnectionException(string message, long? code) : base(message) { }
        public CmisConnectionException(string message, string errorContent) : base(message) { }
        public CmisConnectionException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "connection";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisConstraintException : CmisBaseException
    {
        public CmisConstraintException() : base() { }
        public CmisConstraintException(string message) : base(message) { }
        public CmisConstraintException(string message, Exception inner) : base(message, inner) { }
        public CmisConstraintException(string message, long? code) : base(message) { }
        public CmisConstraintException(string message, string errorContent) : base(message) { }
        public CmisConstraintException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "constraint";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisContentAlreadyExistsException : CmisBaseException
    {
        public CmisContentAlreadyExistsException() : base() { }
        public CmisContentAlreadyExistsException(string message) : base(message) { }
        public CmisContentAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
        public CmisContentAlreadyExistsException(string message, long? code) : base(message) { }
        public CmisContentAlreadyExistsException(string message, string errorContent) : base(message) { }
        public CmisContentAlreadyExistsException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "contentAlreadyExists";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisFilterNotValidException : CmisBaseException
    {
        public CmisFilterNotValidException() : base() { }
        public CmisFilterNotValidException(string message) : base(message) { }
        public CmisFilterNotValidException(string message, Exception inner) : base(message, inner) { }
        public CmisFilterNotValidException(string message, long? code) : base(message) { }
        public CmisFilterNotValidException(string message, string errorContent) : base(message) { }
        public CmisFilterNotValidException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "filterNotValid";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisInvalidArgumentException : CmisBaseException
    {
        public CmisInvalidArgumentException() : base() { }
        public CmisInvalidArgumentException(string message) : base(message) { }
        public CmisInvalidArgumentException(string message, Exception inner) : base(message, inner) { }
        public CmisInvalidArgumentException(string message, long? code) : base(message) { }
        public CmisInvalidArgumentException(string message, string errorContent) : base(message) { }
        public CmisInvalidArgumentException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "invalidArgument";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisNameConstraintViolationException : CmisBaseException
    {
        public CmisNameConstraintViolationException() : base() { }
        public CmisNameConstraintViolationException(string message) : base(message) { }
        public CmisNameConstraintViolationException(string message, Exception inner) : base(message, inner) { }
        public CmisNameConstraintViolationException(string message, long? code) : base(message) { }
        public CmisNameConstraintViolationException(string message, string errorContent) : base(message) { }
        public CmisNameConstraintViolationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "nameConstraintViolation";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisNotSupportedException : CmisBaseException
    {
        public CmisNotSupportedException() : base() { }
        public CmisNotSupportedException(string message) : base(message) { }
        public CmisNotSupportedException(string message, Exception inner) : base(message, inner) { }
        public CmisNotSupportedException(string message, long? code) : base(message) { }
        public CmisNotSupportedException(string message, string errorContent) : base(message) { }
        public CmisNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "notSupported";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisObjectNotFoundException : CmisBaseException
    {
        public CmisObjectNotFoundException() : base() { }
        public CmisObjectNotFoundException(string message) : base(message) { }
        public CmisObjectNotFoundException(string message, Exception inner) : base(message, inner) { }
        public CmisObjectNotFoundException(string message, long? code) : base(message) { }
        public CmisObjectNotFoundException(string message, string errorContent) : base(message) { }
        public CmisObjectNotFoundException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "objectNotFound";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisPermissionDeniedException : CmisBaseException
    {
        public CmisPermissionDeniedException() : base() { }
        public CmisPermissionDeniedException(string message) : base(message) { }
        public CmisPermissionDeniedException(string message, Exception inner) : base(message, inner) { }
        public CmisPermissionDeniedException(string message, long? code) : base(message) { }
        public CmisPermissionDeniedException(string message, string errorContent) : base(message) { }
        public CmisPermissionDeniedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "permissionDenied";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisRuntimeException : CmisBaseException
    {
        public CmisRuntimeException() : base() { }
        public CmisRuntimeException(string message) : base(message) { }
        public CmisRuntimeException(string message, Exception inner) : base(message, inner) { }
        public CmisRuntimeException(string message, long? code) : base(message) { }
        public CmisRuntimeException(string message, string errorContent) : base(message) { }
        public CmisRuntimeException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "runtime";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisStorageException : CmisBaseException
    {
        public CmisStorageException() : base() { }
        public CmisStorageException(string message) : base(message) { }
        public CmisStorageException(string message, Exception inner) : base(message, inner) { }
        public CmisStorageException(string message, long? code) : base(message) { }
        public CmisStorageException(string message, string errorContent) : base(message) { }
        public CmisStorageException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "storage";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisStreamNotSupportedException : CmisBaseException
    {
        public CmisStreamNotSupportedException() : base() { }
        public CmisStreamNotSupportedException(string message) : base(message) { }
        public CmisStreamNotSupportedException(string message, Exception inner) : base(message, inner) { }
        public CmisStreamNotSupportedException(string message, long? code) : base(message) { }
        public CmisStreamNotSupportedException(string message, string errorContent) : base(message) { }
        public CmisStreamNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "streamNotSupported";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisUpdateConflictException : CmisBaseException
    {
        public CmisUpdateConflictException() : base() { }
        public CmisUpdateConflictException(string message) : base(message) { }
        public CmisUpdateConflictException(string message, Exception inner) : base(message, inner) { }
        public CmisUpdateConflictException(string message, long? code) : base(message) { }
        public CmisUpdateConflictException(string message, string errorContent) : base(message) { }
        public CmisUpdateConflictException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "updateConflict";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisVersioningException : CmisBaseException
    {
        public CmisVersioningException() : base() { }
        public CmisVersioningException(string message) : base(message) { }
        public CmisVersioningException(string message, Exception inner) : base(message, inner) { }
        public CmisVersioningException(string message, long? code) : base(message) { }
        public CmisVersioningException(string message, string errorContent) : base(message) { }
        public CmisVersioningException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "versioning";

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisUnauthorizedException : CmisRuntimeException
    {
        public CmisUnauthorizedException() : base() { }
        public CmisUnauthorizedException(string message) : base(message) { }
        public CmisUnauthorizedException(string message, Exception inner) : base(message, inner) { }
        public CmisUnauthorizedException(string message, long? code) : base(message) { }
        public CmisUnauthorizedException(string message, string errorContent) : base(message) { }
        public CmisUnauthorizedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisProxyAuthenticationException : CmisRuntimeException
    {
        public CmisProxyAuthenticationException() : base() { }
        public CmisProxyAuthenticationException(string message) : base(message) { }
        public CmisProxyAuthenticationException(string message, Exception inner) : base(message, inner) { }
        public CmisProxyAuthenticationException(string message, long? code) : base(message) { }
        public CmisProxyAuthenticationException(string message, string errorContent) : base(message) { }
        public CmisProxyAuthenticationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisServiceUnavailableException : CmisRuntimeException
    {
        public CmisServiceUnavailableException() : base() { }
        public CmisServiceUnavailableException(string message) : base(message) { }
        public CmisServiceUnavailableException(string message, Exception inner) : base(message, inner) { }
        public CmisServiceUnavailableException(string message, long? code) : base(message) { }
        public CmisServiceUnavailableException(string message, string errorContent) : base(message) { }
        public CmisServiceUnavailableException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public override string GetExtensionName()
        {
            return ExceptionName;
        }
    }

    public class CmisInvalidServerData : InvalidOperationException
    {
        public CmisInvalidServerData() : base() { }
        public CmisInvalidServerData(string message) : base(message) { }
        public CmisInvalidServerData(string message, Exception inner) : base(message, inner) { }
    }
}
