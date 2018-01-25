using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandLine;
using LinqToDB;
using Newtonsoft.Json.Linq;

namespace SBM_CommitFix
{
    class Program
    {
        const int MaxLength = 50;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                using (var db = new VisionSuiteDB(options))
                {
                    var count = db.Commits.Count();
                    Console.WriteLine($"There are {count:N0} commits in the {options.Database} database.");

                    int updateCount = 0;

                    var commits = GetCommitsToCheck(options);
                    foreach (var commit in commits)
                    {
                        updateCount = HandleCommit(commit, options, updateCount, db);
                    }

                    Console.WriteLine($"Updated {updateCount} commits.");
                }
            });
        }

        private static int HandleCommit(Commit commit, Options options, int updateCount, VisionSuiteDB db)
        {
            var payload = commit.Payload
                .Decompress()
                .AsString()
                .AsJArray();

            foreach (var @event in payload)
            {
                updateCount = HandleEvent(commit, options, updateCount, db, @event, payload);
            }
            return updateCount;
        }

        private static int HandleEvent(Commit commit, Options options, int updateCount, VisionSuiteDB db, JToken @event,
            JArray payload)
        {
            var eventType = @event.EventType();

            if (eventType.IndexOf("SpecifyPieceOfEquipmentTypeEvent", StringComparison.InvariantCulture) != -1)
            {
                var pieceOfEquipmentCode = @event.Body()["PieceOfEquipmentCode"].ToString();
                var equipmentType = @event.Body()["Type"].ToString();

                if (equipmentType.Length > MaxLength)
                {
                    updateCount = TruncateEquipmentType(commit, options, updateCount, db, @event, payload, equipmentType, pieceOfEquipmentCode);
                }
            }
            return updateCount;
        }

        private static int TruncateEquipmentType(Commit commit, Options options, int updateCount, VisionSuiteDB db,
            JToken @event, JArray payload, string equipmentType, string pieceOfEquipmentCode)
        {
            Console.WriteLine($"Truncating '{equipmentType}' for equipment '{pieceOfEquipmentCode}'");

            @event.Body()["Type"] = equipmentType.Substring(0, MaxLength);

            var newPayload = payload
                .ToString()
                .AsBytes()
                .Compress();

            Expression<Func<Commit, bool>> updatePredicate = c => c.CheckpointNumber == commit.CheckpointNumber;

            if (options.WhatIf)
            {
                updateCount += db.Commits
                    .Count(updatePredicate);
            }
            else
            {
                updateCount += db.Commits
                    .Where(updatePredicate)
                    .Set(c => c.Payload, newPayload)
                    .Update();
            }
            return updateCount;
        }

        private static List<Commit> GetCommitsToCheck(Options options)
        {
            using (var db = new VisionSuiteDB(options))
            {
                var commits =
                    db.Commits
                        .OrderBy(c => c.CheckpointNumber)
                        .ToList();
                return commits;
            }
        }
    }
}