﻿using commercetools.Sdk.Client;

namespace commercetools.Sdk.HttpApi
{
    public interface IRequestMessageBuilderFactory
    {
        IRequestMessageBuilder GetRequestMessageBuilder(ICommand command);
    }
}