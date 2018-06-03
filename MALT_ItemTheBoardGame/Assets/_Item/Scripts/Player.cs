﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HedgehogTeam.EasyTouch;
using System;
using DG.Tweening;

namespace AppAdvisory.Item {

	public class Player : MonoBehaviour {
		public string playerName;
		public string picURL;


		private ModelGrid modelGrid;
        private OptimizedGrid optiGrid;

        public BallColor color {
			get {
				return ballPrefab.Color;
			}
		}
		public Ball ballPrefab;

		public uint ballCount = 10;
		private bool isTweening = false;

		public event Action<Vector2> OnPhase1TurnFinished;
		public event Action<List<Vector2>> OnPhase2TurnFinished;

		private bool hasAlreadyJumpedOnce = false;
		private Cell currentCell;
		private List<Cell> currentCellsToMove;
		private List<Cell> currentCellsToJump;
		private List<Vector2> movements;

		private Vector3 deltaPosition;
		private int fingerIndex;
		private Ball currentBall;

		public bool showHelp = false;

        public void Reset()
        {
            ballCount = 10;
            isTweening = false;
            hasAlreadyJumpedOnce = false;
            currentCell = null;
            currentBall = null;
            currentCellsToMove = new List<Cell>();
            currentCellsToJump = new List<Cell>();
            movements = new List<Vector2>();
        }

		public void StartTurn() {
            Debug.Log("playerstartturn");
			EasyTouch.On_TouchUp += OnTouchUp;
            FindObjectOfType<GridManager>().numberOfTurnsPlayer1++;
//			if (ballCount > 0) {
//				EasyTouch.On_Drag += On_Drag;
//				EasyTouch.On_DragStart += On_DragStart;
//				EasyTouch.On_DragEnd += On_DragEnd;
//			} else {
//				EasyTouch.On_TouchUp += OnTouchUp;
//			}
		}


        public void EndTurn()
        {
            Debug.Log("playerendturn");
			EasyTouch.On_TouchUp -= OnTouchUp;

//			if (ballCount > 0) {
//				EasyTouch.On_Drag -= On_Drag;
//				EasyTouch.On_DragStart -= On_DragStart;
//				EasyTouch.On_DragEnd -= On_DragEnd;
//			} else {
//				EasyTouch.On_TouchUp -= OnTouchUp;
//			}
		}

		void On_Drag(Gesture gesture) {
			if (fingerIndex != gesture.fingerIndex)
				return;

			if (!currentBall)
				return;

			if (currentBall.gameObject != gesture.pickedObject)
				return;

			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			gesture.pickedObject.transform.position = position - deltaPosition;


		}

		void On_DragStart(Gesture gesture) {
			if (!gesture.pickedObject)
				return;

			Ball ball = gesture.pickedObject.GetComponent<Ball> ();

			if (!ball)
				return;

			if (ball.Color != color)
				return;

			if (ballCount > 0) {
				On_DragStartPhase1 (gesture, ball);
			} else {
				On_DragStartPhase2 (gesture, ball);
			}
		}


		void On_DragStartPhase1(Gesture gesture, Ball ball) {
			if (ball.owner != null)
				return;

			RegisterBall (gesture, ball);
		}

		void On_DragStartPhase2(Gesture gesture, Ball ball) {
			if (!ball.owner)
				return;

			RegisterBall (gesture, ball);
			SelectBall (ball.owner);
		}

		void On_DragEndPhase1(Cell pickedCell) {
			if (pickedCell.HasBall ()) {
				currentBall.ResetPosition ();
				return;
			}

			EndTurn ();
			ballCount--;
			currentBall.DOPlace (pickedCell);
			currentBall = null;

			SendTurnDataPhase1 (pickedCell);
		}

		void On_DragEndPhase2(Cell pickedCell) {
			//MoveBall (pickedCell);


			//return;
            
			if (!currentCell) {
				if(!pickedCell.HasBall(color))
					return;

				SelectBall (pickedCell);
			} 
			else {
				MoveBall (pickedCell);
			}
		}

		void On_DragEnd(Gesture gesture) {
			if (fingerIndex != gesture.fingerIndex)
				return;

			if (!currentBall)
				return;

			if (currentBall.gameObject != gesture.pickedObject)
				return;

			RaycastHit2D hit = Physics2D.CircleCast (gesture.pickedObject.transform.position, 0.25f, Vector2.zero, 1f, 1 << 9);

			if (!hit) {
				currentBall.ResetPosition ();
				return;
			}

			if (!hit.collider) {
				currentBall.ResetPosition ();
				return;
			}

			Cell pickedCell = hit.collider.GetComponent<Cell> ();


			if (!pickedCell) {
				currentBall.ResetPosition ();
				return;
			}
				

			if (ballCount > 0) {
				On_DragEndPhase1 (pickedCell);
			} else {
				On_DragEndPhase2 (pickedCell);
			}
		}
        
		void OnTouchUp(Gesture gesture) {
			if (isTweening)
				return;

			if (!gesture.pickedObject)
				return;

            Debug.Log("ontouchup");

			Cell pickedCell = gesture.pickedObject.GetComponent<Cell> (); 

			Ball pickedBall = gesture.pickedObject.GetComponent<Ball> ();

			if (pickedBall)
				if (pickedBall.owner)
					pickedCell = pickedBall.owner;

			if (ballCount > 0) {

				if (currentBall) {
					if (!pickedCell) {

						if (pickedBall && pickedBall.Color == color) {
							currentBall.HideHighlight ();
							currentBall = pickedBall;
							currentBall.ShowHighlight ();
						}
						return;
					}

					if (pickedCell.HasBall ()) {
						currentBall.ResetPosition ();
						return;
					}


					EndTurn ();
					ballCount--;
					currentBall.DOPlace (pickedCell);
					currentBall.HideHighlight ();
					currentBall = null;

					SendTurnDataPhase1 (pickedCell);


				} else {
					if (!pickedBall)
						return;

					if (pickedBall.Color != color)
						return;

					On_DragStartPhase1 (gesture, pickedBall);
				
				}
				if (!pickedCell)
					return;

				if (pickedCell.HasBall ())
					return;


				EndTurn ();
				ballCount--;
				SendTurnDataPhase1 (pickedCell);

			} else {
				if (!pickedCell)
					return;

				if (!currentCell) {
					if(!pickedCell.HasBall(color))
						return;

					SelectBall (pickedCell);
				} 
				else {
					MoveBall (pickedCell);
				}
			}
		}

		public void SelectBall(Cell pickedCell) {

			Cell destinationCell;

			List<Cell> neighbours = modelGrid.GetCellNeighbours (pickedCell);
			currentCellsToMove = new List<Cell> ();
			currentCellsToJump = new List<Cell> ();
			currentCell = pickedCell;

			foreach (Cell cell in neighbours) {
				if (!cell.HasBall()) {
					if(!hasAlreadyJumpedOnce)
						currentCellsToMove.Add (cell);
				} else {
					destinationCell = modelGrid.GetCellFromDirection (pickedCell, cell);
					if (destinationCell) {
						if (!destinationCell.HasBall ())
							currentCellsToJump.Add (destinationCell);
					}

				}
			}


			//Utils.ResetCellsColor (grid);


			if (showHelp) {
				//Utils.HighlighCell (pickedCell, Color.red);
				//Utils.HighlighCells (currentCellsToMove, Color.green);
				//Utils.HighlighCells (currentCellsToJump, Color.yellow);
			}

			currentBall = currentCell.ball;
			currentBall.ShowHighlight ();
		}

		void ResetCurrentCells() {
			currentCell = null;
			currentCellsToJump = null;
			currentCellsToMove = null;
		}

//		public void PlaceBall(Cell pickedCell) {
//			Ball ball = Instantiate(ballPrefab);
//			pickedCell.ball = ball;
//			ball.transform.position = pickedCell.transform.position;
//
//			ballCount--;
//		}

		public void ChangeBallPosition(Cell firstCell, Cell secondCell) {

            Ball ball = firstCell.ball;

			secondCell.ball = ball;
			firstCell.ball = null;

			ball.owner = secondCell;
            
            Move move = new Move();
            move.fromX = firstCell.y;
            move.fromY = firstCell.x;
            move.toX = secondCell.y;
            move.toY = secondCell.x;
            move.color = (CellColor)ball.Color;

            optiGrid.DoMove(move);

			isTweening = true;
            ball.GetComponent<Animation>().Play();
			ball.transform.DOMove (secondCell.transform.position, 1f).OnComplete (() => {
				ball.transform.position = secondCell.transform.position;
				//ball.SetStartPosition ();
				isTweening = false;
			});
		}

		void SendTurnDataPhase1(Cell pickedCell) {
			List<Vector2> movements = new List<Vector2> ();
			movements.Add (new Vector2(pickedCell.x, pickedCell.y));

			if (OnPhase1TurnFinished != null)
				OnPhase1TurnFinished (new Vector2 (pickedCell.x, pickedCell.y));
		}

		void SendTurnDataPhase2() {
			hasAlreadyJumpedOnce = false;
			print("player phase 2 turn over");

			if (OnPhase2TurnFinished != null)
				OnPhase2TurnFinished (movements);
		}

		public void MoveBall(Cell pickedCell) {
			if (pickedCell == currentCell) {


				currentBall.ResetPosition ();
				//Utils.ResetCellsColor (grid);

				currentBall.HideHighlight ();

				if (hasAlreadyJumpedOnce) {
					currentBall.HideHighlight ();
					EndTurn ();
					SendTurnDataPhase2 ();
				}														

				ResetCurrentCells ();
				return;
			}


			if (pickedCell.HasBall ()) {
				currentBall.ResetPosition ();
				return;
			}
				

			//Utils.ResetCellsColor (grid);

			if (currentCellsToMove.Contains (pickedCell)) {
				movements = new List<Vector2> ();
				movements.Add (new Vector2 (currentCell.x, currentCell.y));
				movements.Add (new Vector2 (pickedCell.x, pickedCell.y));

				ChangeBallPosition (currentCell, pickedCell);
				currentBall.HideHighlight ();
				ResetCurrentCells ();
				EndTurn ();
				SendTurnDataPhase2 ();

			} else if (currentCellsToJump.Contains (pickedCell)) {
				if (hasAlreadyJumpedOnce)
					movements.Add (new Vector2 (pickedCell.x, pickedCell.y));
				else {
					movements = new List<Vector2> ();
					movements.Add (new Vector2 (currentCell.x, currentCell.y));
					movements.Add (new Vector2 (pickedCell.x, pickedCell.y));
				}

				ChangeBallPosition (currentCell, pickedCell);
				ResetCurrentCells ();
				hasAlreadyJumpedOnce = true;
				SelectBall (pickedCell);
			} else {
				currentBall.ResetPosition ();
				//Utils.ResetCellsColor (grid);
				return;
			}
				

			Utils.CheckWin (modelGrid, pickedCell);
		}

		void RegisterBall(Gesture gesture, Ball ball)  {
			currentBall = ball;
			currentBall.ShowHighlight ();

			fingerIndex = gesture.fingerIndex;

			// the world coordinate from touch
			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			deltaPosition = position - gesture.pickedObject.transform.position;
		}

		public void SetGrid(ModelGrid modelGrid, OptimizedGrid optiGrid) {
			this.modelGrid = modelGrid;
            this.optiGrid = optiGrid;
		}

		void OnDestroy(){
			EndTurn ();
		}
	}
}