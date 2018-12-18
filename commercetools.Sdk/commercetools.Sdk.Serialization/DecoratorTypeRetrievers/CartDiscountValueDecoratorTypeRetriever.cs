﻿using commercetools.Sdk.Registration;
using commercetools.Sdk.Domain;

namespace commercetools.Sdk.Serialization
{
    public class CartDiscountValueDecoratorTypeRetriever : DecoratorTypeRetriever<CartDiscountValue>
    {
        public CartDiscountValueDecoratorTypeRetriever(ITypeRetriever typeRetriever) : base(typeRetriever)
        {
        }
    }
}