// Licensed to the Apache Software Foundation(ASF) under one
// or more contributor license agreements.See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol.Entities;

namespace Thrift.Protocol
{
    /**
    * TMessageValidatorProtocol is a protocol-independent concrete decorator that allows a Thrift
    * client to communicate with a Server and validate that the messaged received is matching the
    * send seqid. If it does not match we keep reading the buffer or throw an exception.
    * This Protocol Decorator provides the most advantage with a framed transport.
    * When used with THROW_EXCEPTION mode the behavior is the same as when calling the service with
    * validate_message. The difference is that the protocol level validation allows for care free
    * service call by for example library users.
    */

    // ReSharper disable once InconsistentNaming
    public class TMessageValidatorProtocol : TProtocolDecorator
    {
        public enum ValidationMode {
            KEEP_READING,
            THROW_EXCEPTION
        }
        public ValidationMode Mode { get; set; }
        public TMessageValidatorProtocol(TProtocol protocol, ValidationMode mode)
            : base(protocol)
        {
            Mode = mode;
        }

        public override async ValueTask<TMessage> ReadMessageBeginAsync(TMessage original_message, CancellationToken cancellationToken) {
            TMessage new_message = await ReadMessageBeginAsync(cancellationToken);
            while (original_message.SeqID != new_message.SeqID)
            {
                if (Mode == ValidationMode.KEEP_READING)
                    new_message = await ReadMessageBeginAsync(cancellationToken);
                else if (Mode == ValidationMode.THROW_EXCEPTION)
                    throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "Received SeqID and sent one do not match.");
                else
                    throw new InvalidProgramException("This is an unreachable situation");
            }
            return new_message;
        }
    }
}
