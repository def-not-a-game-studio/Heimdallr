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
        
        private void ProcessState() {
            var serverTime = GameManager.Tick;
            if (Entity.State == EntityState.Walk) {
                var serverDirection = 0;
                var nextCellPosition = Vector3.zero;
                var nextCellTime = serverTime;

                var previousServerDirection = 0;
                var previousCellPosition = Vector3.zero;
                var prevTime = 0L;

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
                    var position = previousCellPosition + distance * passedTime / cellTime;
                    CheckDirection(nextCellPosition, previousCellPosition);

                    transform.position = position;
                }

                if (pathStartCellIndex == -1 && serverTime >= nextCellTime) {
                    transform.position = nextCellPosition;

                    StopMoving();
                }
            }

            m_lastProcessStateTime = GameManager.Tick;
            m_lastServerTime = serverTime;
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
    }
}