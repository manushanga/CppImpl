// Guids.cs
// MUST match guids.h
using System;

namespace mx.CPPImpl
{
    static class GuidList
    {
        public const string guidCPPImplPkgString = "a78b0bc5-ad70-4945-abed-a4c6f03d77d8";
        public const string guidCPPImplCmdSetString = "5dd5a687-3093-4908-8e09-6473c1be4b0e";

        public static readonly Guid guidCPPImplCmdSet = new Guid(guidCPPImplCmdSetString);
    };
}