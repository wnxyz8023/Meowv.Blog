﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Meowv.Blog.Application.HelloWorld
{
    public class HelloWorldService : MeowvBlogApplicationServiceBase, IHelloWorldService
    {
        public string HelloWorld()
        {
            return "HelloWorld";
        }
    }
}
