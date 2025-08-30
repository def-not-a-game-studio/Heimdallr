using Core.Path;
using UnityEngine;
using UnityRO.Core.GameEntity;

namespace Heimdallr.Core.Game.Controllers
{
    public partial class GameEntityMovementController
    {
        private CPathInfo pathInfo;

        private int pathStartCellIndex;
        private int pathPreviousCellIndex;

        private Direction direction;
        private long m_lastProcessStateTime;
        private long m_lastServerTime;
        private bool m_isNeverAnimation;
        
        private Vector3 MoveStartPosition;
        private Vector3 MoveEndPosition;
        private float Distance;
        
        private void ProcessState() {
            var serverTime = GameManager.Tick;
            
            if (Entity.State == EntityState.Walk) {
                var serverDirection = 0;
                var nextCellPosition = Vector3.zero;
                var nextCellTime = serverTime;

                var previousServerDirection = 0;
                var previousCellPosition = Vector3.zero;
                var prevTime = 0L;
                var lastPosition = transform.position;

                pathStartCellIndex = pathInfo.GetNextCellInfo(
                    serverTime,
                    ref nextCellTime,
                    ref nextCellPosition,
                    ref serverDirection,
                    PathFinder.GetCellHeight
                );

                pathPreviousCellIndex = pathInfo.GetPrevCellInfo(
                    serverTime,
                    ref prevTime,
                    ref previousCellPosition,
                    ref previousServerDirection,
                    PathFinder.GetCellHeight
                );

                var passedTime = serverTime - prevTime;
                var cellTime = nextCellTime - prevTime;

                if (pathPreviousCellIndex == 0) {
                    previousCellPosition = MoveStartPosition;
                }

                if (passedTime > cellTime) {
                    passedTime = cellTime;
                }

                if (passedTime >= 0 && cellTime > 0) {
                    var distance = nextCellPosition - previousCellPosition;
                    // The key calculation - this should be frame-rate independent
                    var interpolationRatio = (float)passedTime / (float)cellTime;
                    var position = Vector3.Lerp(previousCellPosition, nextCellPosition, interpolationRatio);
                    
                    CheckDirection(nextCellPosition, previousCellPosition);
                    transform.position = position;
                }

                AddDistance(lastPosition);

                if (pathStartCellIndex == -1 && serverTime >= nextCellTime) {
                    transform.position = nextCellPosition;
                    StopMoving();
                }
            }

            m_lastProcessStateTime = GameManager.Tick;
            m_lastServerTime = serverTime;
        }

        private void AddDistance(Vector3 lastPosition)
        {
            var progress = transform.position - lastPosition;
            progress.y = 0;
            var deltaDistance = progress.magnitude;
            Distance += deltaDistance / 0.25f;
        }

        private void CheckDirection(Vector3 position, Vector3 prevPos) {
            var direction = PathFinder.GetDirectionForOffset(position, prevPos);
            if (this.direction != direction) {
                this.direction = direction;
                Entity.ChangeDirection(direction);
            }
        }
        
        private bool FindPath(int startX, int startY, int endX, int endY, long tick) {
            if (PathFinder == null) {
                PathFinder = FindObjectOfType<PathFinder>();
            }
            
            if (PathFinder is null) return false;

            return PathFinder.FindPath(
                tick,
                startX, startY,
                endX, endY,
                Entity.Status.MoveSpeed,
                pathInfo
            );
        }

        public float GetDistance()
        {
            return Distance;
        }
    }
}