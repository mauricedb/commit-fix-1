using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToDB;
using Newtonsoft.Json.Linq;

namespace SBM_CommitFix
{
    internal class FixCommits
    {
        const int MaxLength = 50;

        private readonly Options _options;
        private readonly Logger _logger;
        private int _updateCount;

        public FixCommits(Options options, Logger logger)
        {
            _options = options;
            _logger = logger;
        }

        public void Execute()
        {
            using (var db = new VisionSuiteDB(_options))
            {
                _logger.WriteLine($"Truncating Type properties on SpecifyPieceOfEquipmentTypeEvent that are longer then {MaxLength} characters");

                var commits = GetCommitsToCheck(_options);
                foreach (var commit in commits)
                {
                    _logger.PrintProgress();

                    HandleCommit(commit, db);
                }

                if (_options.WhatIf)
                {
                    _logger.WriteLine($"Would have updated {_updateCount} commits.");
                }
                else
                {
                    _logger.WriteLine($"Updated {_updateCount} commits.");
                }
            }
        }
        private void HandleCommit(Commit commit, VisionSuiteDB db)
        {
            var payload = commit.Payload
                .Decompress()
                .AsString()
                .AsJArray();

            foreach (var @event in payload)
            {
                HandleEvent(commit, db, @event, payload);
            }
        }

        private void HandleEvent(Commit commit, VisionSuiteDB db, JToken @event, JArray payload)
        {
            var eventType = @event.EventType();

            if (eventType.IndexOf("SpecifyPieceOfEquipmentTypeEvent", StringComparison.InvariantCulture) != -1)
            {
                var pieceOfEquipmentCode = @event.Body()["PieceOfEquipmentCode"].ToString();
                var equipmentType = @event.Body()["Type"].ToString();

                if (equipmentType.Length > MaxLength)
                {
                    TruncateEquipmentType(commit, db, @event, payload, equipmentType, pieceOfEquipmentCode);
                }
            }
        }

        private void TruncateEquipmentType(Commit commit, VisionSuiteDB db,
            JToken @event, JArray payload, string equipmentType, string pieceOfEquipmentCode)
        {
            _logger.WriteLine($"Truncating '{equipmentType}' for equipment '{pieceOfEquipmentCode}'");

            @event.Body()["Type"] = equipmentType.Substring(0, MaxLength);

            var newPayload = payload
                .ToString()
                .AsBytes()
                .Compress();

            Expression<Func<Commit, bool>> updatePredicate = c => c.CheckpointNumber == commit.CheckpointNumber;

            if (_options.WhatIf)
            {
                _updateCount += db.Commits
                    .Count(updatePredicate);
            }
            else
            {
                _updateCount += db.Commits
                    .Where(updatePredicate)
                    .Set(c => c.Payload, newPayload)
                    .Update();
            }
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