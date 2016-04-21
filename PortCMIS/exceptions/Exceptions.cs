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
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisBaseException() : base() { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// </summary>
        public CmisBaseException(string message) : base(message) { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        /// </summary>
        public CmisBaseException(string message, Exception inner) : base(message, inner) { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="code">The exception code. (Web Services only.)</param>
        /// </summary>
        public CmisBaseException(string message, long? code)
            : this(message)
        {
            Code = code;
        }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="errorContent">The error content</param>
        /// </summary>
        public CmisBaseException(string message, string errorContent)
            : this(message)
        {
            ErrorContent = errorContent;
        }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="errorContent">The error content</param>
        /// <param name="inner">The inner exception.</param>
        /// </summary>
        public CmisBaseException(string message, string errorContent, Exception inner)
            : this(message, inner)
        {
            ErrorContent = errorContent;
        }

        /// <summary>
        /// The exception code.
        /// </summary>
        /// <remarks>Only used by the Web Services binding.</remarks>
        /// <value>The exception code.</value>
        public long? Code { get; protected set; }

        /// <summary>
        /// The unparsed error message.
        /// </summary>
        /// <value>The unparsed error message.</value>
        public string ErrorContent { get; protected set; }

        /// <summary>
        /// The CMIS exception name.
        /// </summary>
        /// <value>The CMIS exception name.</value>
        public abstract string Name { get; }

    }

    /// <summary>
    /// Connection exception.
    /// </summary>
    public class CmisConnectionException : CmisBaseException
    {
        public CmisConnectionException() : base() { }
        public CmisConnectionException(string message) : base(message) { }
        public CmisConnectionException(string message, Exception inner) : base(message, inner) { }
        public CmisConnectionException(string message, long? code) : base(message) { }
        public CmisConnectionException(string message, string errorContent) : base(message) { }
        public CmisConnectionException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "connection";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Constraint exception.
    /// </summary>
    public class CmisConstraintException : CmisBaseException
    {
        public CmisConstraintException() : base() { }
        public CmisConstraintException(string message) : base(message) { }
        public CmisConstraintException(string message, Exception inner) : base(message, inner) { }
        public CmisConstraintException(string message, long? code) : base(message) { }
        public CmisConstraintException(string message, string errorContent) : base(message) { }
        public CmisConstraintException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "constraint";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Content Already Exists exception.
    /// </summary>
    public class CmisContentAlreadyExistsException : CmisBaseException
    {
        public CmisContentAlreadyExistsException() : base() { }
        public CmisContentAlreadyExistsException(string message) : base(message) { }
        public CmisContentAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
        public CmisContentAlreadyExistsException(string message, long? code) : base(message) { }
        public CmisContentAlreadyExistsException(string message, string errorContent) : base(message) { }
        public CmisContentAlreadyExistsException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "contentAlreadyExists";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Filter Not Valid exception.
    /// </summary>
    public class CmisFilterNotValidException : CmisBaseException
    {
        public CmisFilterNotValidException() : base() { }
        public CmisFilterNotValidException(string message) : base(message) { }
        public CmisFilterNotValidException(string message, Exception inner) : base(message, inner) { }
        public CmisFilterNotValidException(string message, long? code) : base(message) { }
        public CmisFilterNotValidException(string message, string errorContent) : base(message) { }
        public CmisFilterNotValidException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "filterNotValid";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Invalid Argument exception.
    /// </summary>
    public class CmisInvalidArgumentException : CmisBaseException
    {
        public CmisInvalidArgumentException() : base() { }
        public CmisInvalidArgumentException(string message) : base(message) { }
        public CmisInvalidArgumentException(string message, Exception inner) : base(message, inner) { }
        public CmisInvalidArgumentException(string message, long? code) : base(message) { }
        public CmisInvalidArgumentException(string message, string errorContent) : base(message) { }
        public CmisInvalidArgumentException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "invalidArgument";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Name Constraint Violation exception.
    /// </summary>
    public class CmisNameConstraintViolationException : CmisBaseException
    {
        public CmisNameConstraintViolationException() : base() { }
        public CmisNameConstraintViolationException(string message) : base(message) { }
        public CmisNameConstraintViolationException(string message, Exception inner) : base(message, inner) { }
        public CmisNameConstraintViolationException(string message, long? code) : base(message) { }
        public CmisNameConstraintViolationException(string message, string errorContent) : base(message) { }
        public CmisNameConstraintViolationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "nameConstraintViolation";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Not Supported exception.
    /// </summary>
    public class CmisNotSupportedException : CmisBaseException
    {
        public CmisNotSupportedException() : base() { }
        public CmisNotSupportedException(string message) : base(message) { }
        public CmisNotSupportedException(string message, Exception inner) : base(message, inner) { }
        public CmisNotSupportedException(string message, long? code) : base(message) { }
        public CmisNotSupportedException(string message, string errorContent) : base(message) { }
        public CmisNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "notSupported";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Object Not Found exception.
    /// </summary>
    public class CmisObjectNotFoundException : CmisBaseException
    {
        public CmisObjectNotFoundException() : base() { }
        public CmisObjectNotFoundException(string message) : base(message) { }
        public CmisObjectNotFoundException(string message, Exception inner) : base(message, inner) { }
        public CmisObjectNotFoundException(string message, long? code) : base(message) { }
        public CmisObjectNotFoundException(string message, string errorContent) : base(message) { }
        public CmisObjectNotFoundException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "objectNotFound";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Permission Denied exception.
    /// </summary>
    public class CmisPermissionDeniedException : CmisBaseException
    {
        public CmisPermissionDeniedException() : base() { }
        public CmisPermissionDeniedException(string message) : base(message) { }
        public CmisPermissionDeniedException(string message, Exception inner) : base(message, inner) { }
        public CmisPermissionDeniedException(string message, long? code) : base(message) { }
        public CmisPermissionDeniedException(string message, string errorContent) : base(message) { }
        public CmisPermissionDeniedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "permissionDenied";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Runtime exception.
    /// </summary>
    public class CmisRuntimeException : CmisBaseException
    {
        public CmisRuntimeException() : base() { }
        public CmisRuntimeException(string message) : base(message) { }
        public CmisRuntimeException(string message, Exception inner) : base(message, inner) { }
        public CmisRuntimeException(string message, long? code) : base(message) { }
        public CmisRuntimeException(string message, string errorContent) : base(message) { }
        public CmisRuntimeException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "runtime";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Storage exception.
    /// </summary>
    public class CmisStorageException : CmisBaseException
    {
        public CmisStorageException() : base() { }
        public CmisStorageException(string message) : base(message) { }
        public CmisStorageException(string message, Exception inner) : base(message, inner) { }
        public CmisStorageException(string message, long? code) : base(message) { }
        public CmisStorageException(string message, string errorContent) : base(message) { }
        public CmisStorageException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "storage";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Stream Not Supported exception.
    /// </summary>
    public class CmisStreamNotSupportedException : CmisBaseException
    {
        public CmisStreamNotSupportedException() : base() { }
        public CmisStreamNotSupportedException(string message) : base(message) { }
        public CmisStreamNotSupportedException(string message, Exception inner) : base(message, inner) { }
        public CmisStreamNotSupportedException(string message, long? code) : base(message) { }
        public CmisStreamNotSupportedException(string message, string errorContent) : base(message) { }
        public CmisStreamNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "streamNotSupported";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Update Conflict exception.
    /// </summary>
    public class CmisUpdateConflictException : CmisBaseException
    {
        public CmisUpdateConflictException() : base() { }
        public CmisUpdateConflictException(string message) : base(message) { }
        public CmisUpdateConflictException(string message, Exception inner) : base(message, inner) { }
        public CmisUpdateConflictException(string message, long? code) : base(message) { }
        public CmisUpdateConflictException(string message, string errorContent) : base(message) { }
        public CmisUpdateConflictException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "updateConflict";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Versioning exception.
    /// </summary>
    public class CmisVersioningException : CmisBaseException
    {
        public CmisVersioningException() : base() { }
        public CmisVersioningException(string message) : base(message) { }
        public CmisVersioningException(string message, Exception inner) : base(message, inner) { }
        public CmisVersioningException(string message, long? code) : base(message) { }
        public CmisVersioningException(string message, string errorContent) : base(message) { }
        public CmisVersioningException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        public const string ExceptionName = "versioning";

        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Unauthorized exception.
    /// </summary>
    public class CmisUnauthorizedException : CmisRuntimeException
    {
        public CmisUnauthorizedException() : base() { }
        public CmisUnauthorizedException(string message) : base(message) { }
        public CmisUnauthorizedException(string message, Exception inner) : base(message, inner) { }
        public CmisUnauthorizedException(string message, long? code) : base(message) { }
        public CmisUnauthorizedException(string message, string errorContent) : base(message) { }
        public CmisUnauthorizedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Proxy Authentication exception.
    /// </summary>
    public class CmisProxyAuthenticationException : CmisRuntimeException
    {
        public CmisProxyAuthenticationException() : base() { }
        public CmisProxyAuthenticationException(string message) : base(message) { }
        public CmisProxyAuthenticationException(string message, Exception inner) : base(message, inner) { }
        public CmisProxyAuthenticationException(string message, long? code) : base(message) { }
        public CmisProxyAuthenticationException(string message, string errorContent) : base(message) { }
        public CmisProxyAuthenticationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Service Unavailable exception.
    /// </summary>
    public class CmisServiceUnavailableException : CmisRuntimeException
    {
        public CmisServiceUnavailableException() : base() { }
        public CmisServiceUnavailableException(string message) : base(message) { }
        public CmisServiceUnavailableException(string message, Exception inner) : base(message, inner) { }
        public CmisServiceUnavailableException(string message, long? code) : base(message) { }
        public CmisServiceUnavailableException(string message, string errorContent) : base(message) { }
        public CmisServiceUnavailableException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Invalid Server Data exception.
    /// </summary>
    public class CmisInvalidServerData : InvalidOperationException
    {
        public CmisInvalidServerData() : base() { }
        public CmisInvalidServerData(string message) : base(message) { }
        public CmisInvalidServerData(string message, Exception inner) : base(message, inner) { }
    }
}
