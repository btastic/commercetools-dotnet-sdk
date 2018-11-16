﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace commercetools.Sdk.Linq
{
    public abstract class FilterVisitor
    {
        public List<string> Accessors { get; protected set; }
        public abstract string Render();
        public abstract string RenderValue();
    }
}
