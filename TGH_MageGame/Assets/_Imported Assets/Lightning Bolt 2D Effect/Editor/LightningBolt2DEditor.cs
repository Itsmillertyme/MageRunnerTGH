using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace LightningBolt2D
{
	[CustomEditor(typeof(LightningBolt2D))]
	public class LightningBolt2DEditor : Editor
	{
		LightningBolt2D script;
		Plane objectPlane; //Mostly to position controls using plane's normal
		Rect windowRect; //For start and end points positions window

		#region Create and initialize the object

		[MenuItem("GameObject/Effects/Lightning Bolt 2D")]
		private static void Create()
		{
			var go = new GameObject();
			go.AddComponent<LightningBolt2D>();
			go.name = "Lightning Bolt 2D";
			var sc = SceneView.lastActiveSceneView != null
				? SceneView.lastActiveSceneView
				: SceneView.sceneViews[0] as SceneView;
			go.transform.position = new Vector3(sc.pivot.x, sc.pivot.y, 0f);
			if (Selection.activeGameObject != null) go.transform.parent = Selection.activeGameObject.transform;
			Selection.activeGameObject = go;
		}

		private void Awake()
		{
			script = (LightningBolt2D)target;
		}

		#endregion

		#region Inspector window

		public override void OnInspectorGUI()
		{
			var forceRepaint = false;
			if (Event.current.type == EventType.ValidateCommand)
			{
				if (Event.current.commandName == "UndoRedoPerformed")
				{
					forceRepaint = true;
				}
			}

			var arcCount = EditorGUILayout.IntSlider(new GUIContent("Arc count",
				"How many lightning arcs to draw"), script.arcCount, 1, 30);
			if (arcCount != script.arcCount)
			{
				Undo.RecordObject(script, "Change arc count");
				script.arcCount = arcCount;
			}

			var pointCount = EditorGUILayout.IntSlider(new GUIContent("Point count",
					"How many points each individual lightning consists of, including start and end"),
				script.pointCount, 3, 50);
			if (pointCount != script.pointCount)
			{
				Undo.RecordObject(script, "Change point count");
				script.pointCount = pointCount;
			}

			GUILayout.Space(8);

			var lineColor = EditorGUILayout.ColorField(new GUIContent("Line color",
				"Color of the lightning"), script.lineColor);
			if (script.lineColor != lineColor)
			{
				Undo.RecordObject(script, "Change line color");
				script.lineColor = lineColor;
			}

			var lineWidth = EditorGUILayout.FloatField(new GUIContent("Line width",
				"Thickness of the lightning line"), script.lineWidth);
			if (lineWidth != script.lineWidth)
			{
				lineWidth = Mathf.Max(0f, lineWidth);
				Undo.RecordObject(script, "Change line width");
				script.lineWidth = lineWidth;
			}

			var glowColor = EditorGUILayout.ColorField(new GUIContent("Glow color",
				"Color of the glow around the lightning"), script.glowColor);
			if (script.glowColor != glowColor)
			{
				Undo.RecordObject(script, "Change glow color");
				script.glowColor = glowColor;
				script.glowEdgeColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
				EditorUtility.SetDirty(script);
			}

			var glowWidth = EditorGUILayout.FloatField(new GUIContent("Glow width",
					"The gradient outside of the line. Setting it to zero removes its geometry"),
				script.glowWidth);
			if (glowWidth != script.glowWidth)
			{
				glowWidth = Mathf.Max(0f, glowWidth);
				Undo.RecordObject(script, "Change glow width");
				script.glowWidth = glowWidth;
			}

			GUILayout.Space(8);

			var distort = EditorGUILayout.FloatField(new GUIContent("Distort",
					"How big the distortion spikes will be relative to the distance between endpoints. Set to zero to make completely smooth line"),
				script.distort);
			if (distort != script.distort)
			{
				distort = Mathf.Max(0f, distort);
				Undo.RecordObject(script, "Change distort");
				script.distort = distort;
			}

			var jitter = EditorGUILayout.FloatField(new GUIContent("Jitter",
				"Random displacements of the points when the scene is in play mode"), script.jitter);
			if (jitter != script.jitter)
			{
				jitter = Mathf.Max(0f, jitter);
				Undo.RecordObject(script, "Change jitter");
				script.jitter = jitter;
			}

			GUILayout.Space(8);

			var bend = EditorGUILayout.FloatField(new GUIContent("Bend amount",
				"How far to push the curve, relative to distance between your two points"), script.bend);
			if (bend != script.bend)
			{
				bend = Mathf.Max(0f, bend);
				Undo.RecordObject(script, "Change bend amount");
				script.bend = bend;
			}

			var bendSpeed = EditorGUILayout.FloatField(new GUIContent("Bend speed",
					"Speed of bending animation while the scene is in play mode. Zero will remove the animation"),
				script.bendSpeed);
			if (bendSpeed != script.bendSpeed)
			{
				bendSpeed = Mathf.Max(0f, bendSpeed);
				Undo.RecordObject(script, "Change bend speed");
				script.bendSpeed = bendSpeed;
			}

			GUILayout.Space(8);

			var arcLifetimeMin = EditorGUILayout.FloatField(
				new GUIContent("Arc lifetime min", "Lower limit of randomized arc lifetime"), script.arcLifetimeMin);
			if (arcLifetimeMin != script.arcLifetimeMin)
			{
				arcLifetimeMin = Mathf.Max(0f, arcLifetimeMin);
				arcLifetimeMin = Mathf.Min(arcLifetimeMin, script.arcLifetimeMax);
				Undo.RecordObject(script, "Change arc lifetime minimum");
				script.arcLifetimeMin = arcLifetimeMin;
			}

			var arcLifetimeMax = EditorGUILayout.FloatField(
				new GUIContent("Arc lifetime max", "Lower limit of randomized arc lifetime"), script.arcLifetimeMax);
			if (arcLifetimeMax != script.arcLifetimeMax)
			{
				arcLifetimeMax = Mathf.Max(0f, arcLifetimeMax);
				arcLifetimeMax = Mathf.Max(arcLifetimeMax, script.arcLifetimeMin);
				Undo.RecordObject(script, "Change arc lifetime maximum");
				script.arcLifetimeMax = arcLifetimeMax;
			}

			GUILayout.Space(8);

			var FPSLimit = EditorGUILayout.FloatField(
				new GUIContent("FPS limit",
					"How many times per second this object is allowed to update. Zero means that there's no limit"),
				script.FPSLimit);
			if (FPSLimit != script.FPSLimit)
			{
				FPSLimit = Mathf.Max(0f, FPSLimit);
				Undo.RecordObject(script, "Change FPS limit");
				script.FPSLimit = FPSLimit;
			}

			//Show triangle count
			GUILayout.Box(new GUIContent(script.triangleCount > 1
					? "The mesh has " + script.triangleCount.ToString() + " triangles"
					: (script.triangleCount == 1 ? "The mesh is just one triangle" : "The mesh has no triangles")),
				EditorStyles.helpBox);

			//Turn on and off
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Playback",
				"Turn effect on and off. If set to off and there is still an animation going, it will finish it and then stop"));
			var switchState = GUILayout.Toolbar(script.isPlaying ? 0 : 1, PlayButtons.GetNames(typeof(PlayButtons)));
			if (switchState != (script.isPlaying ? 0 : 1))
			{
				GUI.FocusControl(null);
				script.isPlaying = (switchState == 0 ? true : false);
			}

			EditorGUILayout.EndHorizontal();

			//Fire once
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			EditorGUI.BeginDisabledGroup(script.isPlaying);
			if (GUILayout.Button(new GUIContent("Fire once",
					"Create lightning(s), let them play out to the end and do not create new ones")))
			{
				script.FireOnce();
				forceRepaint = true;
			}

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			//Clear and regenerate
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			if (GUILayout.Button(
					new GUIContent("Regenerate", "Clear currently generated mesh and generate the new one")))
			{
				script.ScheduleRegenerate();
				forceRepaint = true;
			}

			EditorGUILayout.EndHorizontal();

			//Sprite sorting
			GUILayout.Space(10);
			//Get sorting layers
			var layerIDs = GetSortingLayerUniqueIDs();
			var layerNames = GetSortingLayerNames();
			//Get selected sorting layer
			int selected = -1;
			for (int i = 0; i < layerIDs.Length; i++)
			{
				if (layerIDs[i] == script.sortingLayer)
				{
					selected = i;
				}
			}

			//Select Default layer if no other is selected
			if (selected == -1)
			{
				for (int i = 0; i < layerIDs.Length; i++)
				{
					if (layerIDs[i] == 0)
					{
						selected = i;
					}
				}
			}

			//Sorting layer dropdown
			EditorGUI.BeginChangeCheck();
			var dropdown = new GUIContent[layerNames.Length + 2];
			for (int i = 0; i < layerNames.Length; i++)
			{
				dropdown[i] = new GUIContent(layerNames[i]);
			}

			dropdown[layerNames.Length] = new GUIContent();
			dropdown[layerNames.Length + 1] = new GUIContent("Add Sorting Layer...");
			selected = EditorGUILayout.Popup(new GUIContent("Sorting Layer",
				"Name of the Renderer's sorting layer"), selected, dropdown);
			if (EditorGUI.EndChangeCheck())
			{
				if (selected == layerNames.Length + 1)
				{
					SettingsService.OpenProjectSettings("Project/Tags and Layers");
				}
				else
				{
					Undo.RecordObject(script, "Change sorting layer");
					script.sortingLayer = layerIDs[selected];
				}
			}

			//Order in layer field
			EditorGUI.BeginChangeCheck();
			var order = EditorGUILayout.IntField(new GUIContent("Order in Layer",
				"Renderer's order within a sorting layer"), script.orderInLayer);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(script, "Change order in layer");
				script.orderInLayer = order;
			}

			if (GUI.changed || forceRepaint)
			{
				if (!Application.isPlaying)
				{
					script.Generate();
				}

				SceneView.RepaintAll();
			}
		}

		#endregion

		#region Scene view

		private void OnSceneGUI()
		{
			var et = Event.current.type; //Need to save this because it can be changed to Used by other functions
			objectPlane = new Plane(
				script.transform.TransformPoint(new Vector3(0, 0, 0)),
				script.transform.TransformPoint(new Vector3(0, 1, 0)),
				script.transform.TransformPoint(new Vector3(1, 0, 0))
			);
			//Movable handles for start and end points of the lightning
			Handles.color = Color.white;
			EditorGUI.BeginChangeCheck();
			var startPoint = script.startPoint;
			var endPoint = script.endPoint;
			DrawMoveHandle(ref startPoint);
			DrawMoveHandle(ref endPoint);
			var changed = EditorGUI.EndChangeCheck();
			if (changed)
			{
				Undo.RecordObject(script, "Change point position");
				script.startPoint = startPoint;
				script.endPoint = endPoint;
				if (!Application.isPlaying)
				{
					script.Generate();
				}
			}

			//If object is being dragged, offset start and end points by the drag amount since those point use absolute coordinates
			if ((et == EventType.MouseDrag || et == EventType.MouseUp) ||
				script.lastPosition != script.transform.position)
			{
				Undo.RecordObject(script, "Auto reposition");
				var diff = script.transform.position - script.lastPosition;
				script.startPoint += (Vector2)diff;
				script.endPoint += (Vector2)diff;
				script.lastPosition = script.transform.position;
			}

			DrawPointsPropertiesWindow();
		}

		private void DrawDebugLines()
		{
			var size = HandleUtility.GetHandleSize(script.transform.position) * 0.1f;
			//Just for debugging. Displays points, lines and bezier handles
			for (var a = 0; a < script.Arcs.Count; a++)
			{
				//Draw arc's bezier handles
				Handles.DrawLine(script.transform.TransformPoint(script.Arcs[a].Point1),
					script.transform.TransformPoint(script.Arcs[a].Handle1));
				Handles.DrawLine(script.transform.TransformPoint(script.Arcs[a].Point2),
					script.transform.TransformPoint(script.Arcs[a].Handle2));
				Handles.DrawSolidDisc(script.transform.TransformPoint(script.Arcs[a].Handle1), Vector3.back,
					size * 0.4f);
				Handles.DrawSolidDisc(script.transform.TransformPoint(script.Arcs[a].Handle2), Vector3.back,
					size * 0.4f);
				for (int p = 0; p < script.Arcs[a].Points.Length - 1; p++)
				{
					//Draw the generated line
					Handles.color = Color.gray;
					Handles.DrawLine(
						script.transform.TransformPoint(script.Arcs[a].Points[p]),
						script.transform.TransformPoint(script.Arcs[a].Points[p + 1])
					);
					if (p != 0)
					{
						Handles.DrawWireDisc(script.transform.TransformPoint(script.Arcs[a].Points[p]),
							Vector3.back, size * 0.4f);
					}
				}
			}
		}

		private void DrawMoveHandle(ref Vector2 position)
		{
			var size = HandleUtility.GetHandleSize(position) * 0.1f;
			
			#if UNITY_2022_1_OR_NEWER
			var point = Handles.FreeMoveHandle(position, size, Vector3.zero, Handles.CircleHandleCap);
			#else
			var point = Handles.FreeMoveHandle(position, script.transform.rotation, size, Vector3.zero, Handles.CircleHandleCap);
			#endif
			
			position = (Vector2)(point);
			if (Vector2.Distance(position, GetMouseWorldPosition()) < size)
			{
				SetCursor(UnityEditor.MouseCursor.Arrow);
			}
		}

		private void DrawPointsPropertiesWindow()
		{
			var et = Event.current.type;
			if (et == EventType.Repaint)
			{
				return;
			}

			var windowSize = new Vector2(176, 70);
			var pixelRect = Camera.current.pixelRect;
			windowRect = new Rect(
				pixelRect.width / EditorGUIUtility.pixelsPerPoint - windowSize.x - 3,
				pixelRect.height / EditorGUIUtility.pixelsPerPoint - windowSize.y - 2,
				windowSize.x,
				windowSize.y
			);
			var windowStyle = new GUIStyle(GUI.skin.window) { margin = new RectOffset(5, 5, 5, 5) };
			var options = new GUILayoutOption[]
			{
				GUILayout.MaxWidth(200), // Max width to prevent overly wide windows
				GUILayout.MinHeight(50) // Minimum height to accommodate content
			};
			var cid = GUIUtility.GetControlID(FocusType.Passive);
			GUILayout.Window(cid, windowRect, (id) =>
			{
				var startPoint = script.startPoint;
				var endPoint = script.endPoint;
				EditorGUI.BeginChangeCheck();
				//Start point
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 35;
				EditorGUILayout.PrefixLabel(new GUIContent("Start",
					"Start of the line. Accessible through scripting as Vector2 named startPoint"));
				EditorGUIUtility.labelWidth = 20;
				startPoint.x = EditorGUILayout.FloatField("X", startPoint.x, GUILayout.Width(64));
				startPoint.y = EditorGUILayout.FloatField("Y", startPoint.y, GUILayout.Width(64));
				EditorGUILayout.EndHorizontal();
				//End point
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 35;
				EditorGUILayout.PrefixLabel(new GUIContent("End",
					"Ending of the line. Accessible through scripting as Vector2 named endPoint"));
				EditorGUIUtility.labelWidth = 20;
				endPoint.x = EditorGUILayout.FloatField("X", endPoint.x, GUILayout.Width(64));
				endPoint.y = EditorGUILayout.FloatField("Y", endPoint.y, GUILayout.Width(64));
				EditorGUILayout.EndHorizontal();
				//Redraw on change
				var changed = EditorGUI.EndChangeCheck();
				if (changed)
				{
					Undo.RecordObject(script, "Change point position");
					script.startPoint = startPoint;
					script.endPoint = endPoint;
					if (!Application.isPlaying)
					{
						script.Generate();
					}
				}

				//Make window draggable but don't actually allow to drag it. It's a hack so a window wouldn't disappear on click.
				if (Event.current.type != EventType.MouseDrag)
				{
					GUI.DragWindow();
				}
			}, "Start and end points", windowStyle, options);
			GUI.FocusWindow(cid);
		}

		#endregion

		#region Utilities

		private Vector2 GetMouseWorldPosition()
		{
			try
			{
				var mRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				if (objectPlane.Raycast(mRay, out var mRayDist)) return (Vector2)mRay.GetPoint(mRayDist);
				return Vector2.zero;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				return Vector2.zero;
			}
		}

		private static void SetCursor(UnityEditor.MouseCursor cursor)
		{
			EditorGUIUtility.AddCursorRect(Camera.current.pixelRect, cursor);
		}

		private static int[] GetSortingLayerUniqueIDs()
		{
			var internalEditorUtilityType = typeof(InternalEditorUtility);
			var sortingLayerUniqueIDsProperty =
				internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",
					BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayerUniqueIDsProperty.GetValue(null, Array.Empty<object>());
		}

		private static string[] GetSortingLayerNames()
		{
			var internalEditorUtilityType = typeof(InternalEditorUtility);
			var sortingLayersProperty =
				internalEditorUtilityType.GetProperty("sortingLayerNames",
					BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, Array.Empty<object>());
		}

		private enum PlayButtons
		{
			On,
			Off
		}

		#endregion
	}
}