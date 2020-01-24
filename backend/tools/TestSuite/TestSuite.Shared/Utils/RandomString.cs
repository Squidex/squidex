// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace TestSuite.Utils
{
    public static class RandomString
    {
        private static readonly Random Random = new Random();

        public static string Create(int length)
        {
            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                chars[i] = (char)Random.Next(48, 122);
            }

            return new string(chars);
        }
    }
}
