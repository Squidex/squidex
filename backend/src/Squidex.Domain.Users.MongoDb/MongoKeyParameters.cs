// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoKeyParameters
    {
        public byte[] D { get; set; }

        public byte[] DP { get; set; }

        public byte[] DQ { get; set; }

        public byte[] Exponent { get; set; }

        public byte[] InverseQ { get; set; }

        public byte[] Modulus { get; set; }

        public byte[] P { get; set; }

        public byte[] Q { get; set; }

        public static MongoKeyParameters Create(RSAParameters source)
        {
            var mongoParameters = new MongoKeyParameters();

            mongoParameters.D = source.D;
            mongoParameters.DP = source.DP;
            mongoParameters.DQ = source.DQ;
            mongoParameters.Exponent = source.Exponent;
            mongoParameters.InverseQ = source.InverseQ;
            mongoParameters.Modulus = source.Modulus;
            mongoParameters.P = source.P;
            mongoParameters.Q = source.Q;

            return mongoParameters;
        }

        public RSAParameters ToParameters()
        {
            var parameters = default(RSAParameters);

            parameters.D = D;
            parameters.DP = DP;
            parameters.DQ = DQ;
            parameters.Exponent = Exponent;
            parameters.InverseQ = InverseQ;
            parameters.Modulus = Modulus;
            parameters.P = P;
            parameters.Q = Q;

            return parameters;
        }
    }
}
