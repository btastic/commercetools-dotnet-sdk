﻿using commercetools.Sdk.Domain;

namespace commercetools.Sdk.Client
{
    public abstract class SignUpCommand<T> : Command<SignInResult<T>>
    {
        public SignUpCommand(IDraft<T> entity)
        {
            this.Entity = entity;
        }

        public SignUpCommand(IDraft<T> entity, IAdditionalParameters<T> additionalParameters)
        {
            this.Entity = entity;
            this.AdditionalParameters = additionalParameters;
        }

        public IDraft<T> Entity { get; }

        public override System.Type ResourceType => typeof(T);
    }
}