// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Text;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class Result
    {
        public Exception? Exception { get; private init; }

        public string? Dump { get; private set; }

        public RuleResult Status { get; private set; }

        public void Enrich(TimeSpan elapsed)
        {
            var dumpBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Dump))
            {
                dumpBuilder.AppendLine(Dump);
            }

            if (Status == RuleResult.Timeout)
            {
                dumpBuilder.AppendLine();
                dumpBuilder.AppendLine("Action timed out.");
            }

            if (Exception != null)
            {
                dumpBuilder.AppendLine();
                dumpBuilder.Append("Error: ");
                dumpBuilder.AppendLine(Exception.Message);
            }

            dumpBuilder.AppendLine();
            dumpBuilder.AppendFormat(CultureInfo.InvariantCulture, "Elapsed {0}.", elapsed);
            dumpBuilder.AppendLine();

            Dump = dumpBuilder.ToString();
        }

        public static Result Ignored()
        {
            return Success("Ignored");
        }

        public static Result Complete()
        {
            return Success("Completed");
        }

        public static Result Create(string? dump, RuleResult result)
        {
            return new Result { Dump = dump, Status = result };
        }

        public static Result Success(string? dump)
        {
            return new Result { Dump = dump, Status = RuleResult.Success };
        }

        public static Result Failed(Exception? ex)
        {
            return Failed(ex, ex?.Message);
        }

        public static Result SuccessOrFailed(Exception? ex, string? dump)
        {
            if (ex != null)
            {
                return Failed(ex, dump);
            }
            else
            {
                return Success(dump);
            }
        }

        public static Result Failed(Exception? ex, string? dump)
        {
            var result = new Result { Exception = ex, Dump = dump ?? ex?.Message };

            if (ex is OperationCanceledException || ex is TimeoutException)
            {
                result.Status = RuleResult.Timeout;
            }
            else
            {
                result.Status = RuleResult.Failed;
            }

            return result;
        }
    }
}
