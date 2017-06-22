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
        protected CmisBaseException() : base() { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// </summary>
        protected CmisBaseException(string message) : base(message) { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        /// </summary>
        protected CmisBaseException(string message, Exception inner) : base(message, inner) { Code = null; }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="code">The exception code. (Web Services only.)</param>
        /// </summary>
        protected CmisBaseException(string message, long? code)
            : this(message)
        {
            Code = code;
        }

        /// <summary>
        /// Constructor.
        /// <param name="message">The exception message.</param>
        /// <param name="errorContent">The error content</param>
        /// </summary>
        protected CmisBaseException(string message, string errorContent)
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
        protected CmisBaseException(string message, string errorContent, Exception inner)
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

        /// <value>
        /// The CMIS exception name
        /// </value>
        public abstract string Name { get; }

    }

    /// <summary>
    /// Connection exception.
    /// </summary>
    public class CmisConnectionException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConnectionException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "connection";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Constraint exception.
    /// </summary>
    public class CmisConstraintException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisConstraintException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "constraint";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Content Already Exists exception.
    /// </summary>
    public class CmisContentAlreadyExistsException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisContentAlreadyExistsException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "contentAlreadyExists";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Filter Not Valid exception.
    /// </summary>
    public class CmisFilterNotValidException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisFilterNotValidException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "filterNotValid";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Invalid Argument exception.
    /// </summary>
    public class CmisInvalidArgumentException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidArgumentException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "invalidArgument";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Name Constraint Violation exception.
    /// </summary>
    public class CmisNameConstraintViolationException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNameConstraintViolationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "nameConstraintViolation";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Not Supported exception.
    /// </summary>
    public class CmisNotSupportedException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "notSupported";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Object Not Found exception.
    /// </summary>
    public class CmisObjectNotFoundException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisObjectNotFoundException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "objectNotFound";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Permission Denied exception.
    /// </summary>
    public class CmisPermissionDeniedException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisPermissionDeniedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "permissionDenied";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Runtime exception.
    /// </summary>
    public class CmisRuntimeException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRuntimeException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "runtime";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Storage exception.
    /// </summary>
    public class CmisStorageException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStorageException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "storage";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Stream Not Supported exception.
    /// </summary>
    public class CmisStreamNotSupportedException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisStreamNotSupportedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "streamNotSupported";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Update Conflict exception.
    /// </summary>
    public class CmisUpdateConflictException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUpdateConflictException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "updateConflict";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Versioning exception.
    /// </summary>
    public class CmisVersioningException : CmisBaseException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisVersioningException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }

        /// <summary>The CMIS exception name</summary>
        public const string ExceptionName = "versioning";

        /// <inheritdoc/>
        public override string Name { get { return ExceptionName; } }
    }

    /// <summary>
    /// Unauthorized exception.
    /// </summary>
    public class CmisUnauthorizedException : CmisRuntimeException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisUnauthorizedException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Proxy Authentication exception.
    /// </summary>
    public class CmisProxyAuthenticationException : CmisRuntimeException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisProxyAuthenticationException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Service Unavailable exception.
    /// </summary>
    public class CmisServiceUnavailableException : CmisRuntimeException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException(string message, long? code) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException(string message, string errorContent) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServiceUnavailableException(string message, string errorContent, Exception inner) : base(message, errorContent, inner) { }
    }

    /// <summary>
    /// Invalid Server Data exception.
    /// </summary>
    public class CmisInvalidServerDataException : InvalidOperationException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidServerDataException() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidServerDataException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisInvalidServerDataException(string message, Exception inner) : base(message, inner) { }
    }
}
