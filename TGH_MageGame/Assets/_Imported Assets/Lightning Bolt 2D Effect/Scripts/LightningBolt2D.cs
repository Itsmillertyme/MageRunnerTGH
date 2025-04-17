using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LightningBolt2D
{
	[ExecuteInEditMode]
	public class LightningBolt2D : MonoBehaviour
	{
		#region Configurable variables

		public Vector2 startPoint = Vector2.left * 3f;
		public Vector2 endPoint = Vector2.right * 3f;

		public float arcLifetimeMin = 0.1f;
		public float arcLifetimeMax = 1f;

		public int arcCount = 1;
		public int pointCount = 20;
		public float lineWidth = 0.15f;
		public float glowWidth = 0.1f;

		public Color lineColor;
		public Color glowColor;
		public Color glowEdgeColor;

		public float distort = 0.03f;
		public float jitter = 0.1f;
		public float bend = 0.2f;
		public float bendSpeed = 0.05f;

		public float FPSLimit = 60f;

		public bool isPlaying = true;

		public int sortingLayer = 0;
		public int orderInLayer = 0;

		[SerializeField] int instanceID = 0;

		#endregion

		#region Non-configurable variables

		private long _lastTime = System.DateTime.Now.Ticks;
		public Vector3 lastPosition;

		//Component settings
		[SerializeField] private bool isCanvas = false;
		[SerializeField] private CanvasRenderer cr;
		[SerializeField] private MeshFilter mf;
		[SerializeField] private MeshRenderer mr;
		public Material useMaterial = null;

		//For mesh generation
		[SerializeField] private Mesh mesh;
		[SerializeField] private List<Vector3> vertices = new List<Vector3>(200);
		[SerializeField] private List<Vector3> uvs = new List<Vector3>(200);
		[SerializeField] private List<Color> colors = new List<Color>(200);
		[SerializeField] private int[] triangles;
		public int triangleCount; //Keeps triangle count for displaying in inspector

		private bool _regenerate = false;

		public List<Arc> Arcs { get; private set; } = new List<Arc>(5);

		public class Arc
		{
			public int Seed; //Seed for the random generator
			public float TimeStart; //Keep time when arc was created
			public float TimeEnd; //Keep time when arc will be removed
			public float TimeRatio; //Arc's lifetime in 0 to 1 ratio
			public Vector2 Point1; //Start point in relative space
			public Vector2 Point2; //End point in relative space
			public Vector2 Handle1; //Bezier handle of start point
			public Vector2 Handle2; //Bezier handle of end point
			public Vector2 Handle1direction; //Direction in which start point handle will move when being animated
			public Vector2 Handle2direction; //Direction in which end point handle will move when being animated
			public float SegmentLength; //Holds average segment width
			public Vector2[] Points; //Holds points of the line we calculate (not final vertices but an original line)
			public Vector2[] Normals; //Holds normals for each segment
			public float[] Widths; //Holds width of each segment
			public Vector2[] Miters; //Needed to calculate corners. It's a normalized vector
			public float[,] MitersLength; //Lengths of meter for points. 4 per point
		}

		#endregion

		#region Manage components and updates

		private void Awake()
		{
			//Check if object is a copy
			if (instanceID != GetInstanceID())
			{
				if (instanceID == 0)
				{
					instanceID = GetInstanceID();
				}
				else
				{
					instanceID = GetInstanceID();
					if (instanceID < 0)
					{
						Arcs = new List<Arc>(5);
						mesh = null;
					}
				}
			}

			//Get material to use
			if (useMaterial == null) useMaterial = (Material)Resources.Load("LightningBolt2D", typeof(Material));
			//Check there's a Canvas in parents of the object
			if (isCanvas != IsChildOfCanvas(transform)) isCanvas = !isCanvas;
			//Generate lightning color if not yet set
			if (lineColor.a == 0f)
			{
				glowColor = RandomMildColor();
				glowEdgeColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
				lineColor = Color.Lerp(glowColor, Color.white, 0.8f);
			}
			//Adds the components
			TakeCareOfComponenets();
			//If object starts in no play mode, we clear the mesh jsut in case
			if (Application.isPlaying && !isPlaying)
			{
				Arcs.Clear();
				if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0) mf.sharedMesh.Clear();
			}
			//Calculates and generates the geometry
			Generate();
		}

		/*
			Runs the Generate() function every frame
		*/

		private void Update()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying && lastPosition != transform.position)
			{
				PrefabUtility.RecordPrefabInstancePropertyModifications(this);
				Vector3 diff = transform.position - lastPosition;
				startPoint += (Vector2)diff;
				endPoint += (Vector2)diff;
				lastPosition = transform.position;
			}
			#endif
			if (FPSLimit > 0)
			{
				if (System.DateTime.Now.Ticks < _lastTime + System.TimeSpan.FromSeconds(1f / FPSLimit).Ticks)
				{
					return;
				}
				else
				{
					_lastTime = System.DateTime.Now.Ticks;
				}
			}

			//Get material to use
			if (useMaterial == null) useMaterial = (Material)Resources.Load("LightningBolt2D", typeof(Material));
			//Check there's a Canvas in parents of the object
			if (!Application.isPlaying)
			{
				if (isCanvas != IsChildOfCanvas(transform))
				{
					isCanvas = !isCanvas;
					TakeCareOfComponenets();
				}
			}
			//Generate only if scene is in play mode. In edit mode this function gets called by editor script
			if (Application.isPlaying)
			{
				Generate();
			}
		}

		/*
			Add/remove components depending on parent
		*/

		private void TakeCareOfComponenets()
		{
			if (isCanvas)
			{
				DestroyImmediate(GetComponent<MeshFilter>());
				DestroyImmediate(GetComponent<MeshRenderer>());
				if (GetComponent<CanvasRenderer>() == null) gameObject.AddComponent<CanvasRenderer>();
				cr = GetComponent<CanvasRenderer>();
				cr.SetMaterial(useMaterial, null);
			}
			else
			{
				DestroyImmediate(GetComponent<CanvasRenderer>());
				if (GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
				if (GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>();
				mf = GetComponent<MeshFilter>();
				mr = GetComponent<MeshRenderer>();
				mr.sharedMaterial = useMaterial;
				sortingLayer = mr.sortingLayerID;
				orderInLayer = mr.sortingOrder;
			}
		}

		#endregion

		#region Actual calculation and mesh generation

		/*
			Starts generating lightnings and immediately stops producing new ones
			so the ones that are created will play their animation to the end but
			the new ones will not be created
		*/

		public void FireOnce()
		{
			if (!isPlaying)
			{
				isPlaying = true;
				Generate();
				isPlaying = false;
			}
		}

		public void ScheduleRegenerate()
		{
			_regenerate = true;
		}

		/*
			This function calculates positions of the points based on the configuration.
			The result of this function is arcs[].points arrays for each arc
		*/

		public void Generate()
		{
			//If number of arcs was changed, we regenerate arc objects
			if ((Arcs.Count != arcCount && (isPlaying || !Application.isPlaying)) || _regenerate)
			{
				Arcs = new List<Arc>(5);
				for (var a = 0; a < arcCount; a++)
				{
					Arcs.Add(new Arc());
				}
				_regenerate = false;
			}
			//Iterate through each arc, restart the expired ones, count which ones are active
			var activeArcs = 0;
			for (var a = 0; a < Arcs.Count; a++)
			{
				//Regenerate the arc if it reached it's end time or wasn't generated yet
				if (Arcs[a].Seed == 0 || (Application.isPlaying && isPlaying && Arcs[a].TimeEnd < Time.time))
				{
					Arcs[a].Seed = (int)System.DateTime.Now.Ticks + a;
					Arcs[a].TimeStart = Time.time;
					Arcs[a].TimeEnd = Time.time + Random.Range(arcLifetimeMin, arcLifetimeMax);
					//Also update this object's position in case the points were moved
					transform.position = Vector3.Lerp((Vector3)startPoint, (Vector3)endPoint, 0.5f);
					lastPosition = transform.position;
				}

				//Check if this arc can be considered as active and increment if so
				if (Arcs[a].TimeEnd >= Time.time || !Application.isPlaying)
					activeArcs++;
			}

			//If there are active arcs, we calculate their geometry for this frame
			if (activeArcs > 0)
			{
				for (var a = 0; a < Arcs.Count; a++)
				{
					if (Arcs[a].TimeEnd >= Time.time || !Application.isPlaying)
					{
						Random.InitState(Arcs[a].Seed); //Seed random to get consistent results every frame
						Arcs[a].Point1 =
							transform.InverseTransformPoint(
								startPoint); //Set end and start for the arc in case they were moved
						Arcs[a].Point2 = transform.InverseTransformPoint(endPoint);
						var direction = Arcs[a].Point2 - Arcs[a].Point1;
						var length = direction.magnitude;
						Arcs[a].SegmentLength =
							length / (pointCount - 1); //Length of a single segment without distortions
						//Generate handles which spread points evently and multiply them by curvePower
						Arcs[a].Handle1 = Vector2.Lerp(Arcs[a].Point1, Arcs[a].Point2, 0.3333333f) +
										  Random.insideUnitCircle * (length * bend * Random.value);
						Arcs[a].Handle2 = Vector2.Lerp(Arcs[a].Point1, Arcs[a].Point2, 0.6666666f) +
										  Random.insideUnitCircle * (length * bend * Random.value);
						//Set directions for bending animation
						Arcs[a].Handle1direction = Random.insideUnitCircle;
						Arcs[a].Handle2direction = Random.insideUnitCircle;
						//Animate bending the arc by moving bezier handles
						if (Application.isPlaying)
						{
							Arcs[a].Handle1 += Arcs[a].Handle1direction * ((Time.time - Arcs[a].TimeStart) * length * bendSpeed);
							Arcs[a].Handle2 += Arcs[a].Handle2direction * ((Time.time - Arcs[a].TimeStart) * length * bendSpeed);
						}
						//If number of points changed, we regenrate point data from scratch
						if (Arcs[a].Points == null || Arcs[a].Points.Length != pointCount)
						{
							Arcs[a].Points = new Vector2[pointCount];
							Arcs[a].Normals = new Vector2[pointCount];
							Arcs[a].Widths = new float[pointCount];
							Arcs[a].Miters = new Vector2[pointCount];
							Arcs[a].MitersLength = new float[pointCount, 4];
						}
						//Create points based on bezier curve and add distortion
						for (var i = 0; i < pointCount; i++)
						{
							Arcs[a].Points[i] = CalculateBezierPoint(((float)i) / (pointCount - 1), Arcs[a].Point1,
								Arcs[a].Handle1, Arcs[a].Handle2, Arcs[a].Point2);
							Arcs[a].Points[i] += Random.insideUnitCircle * (length * distort * Parabola((float)i / (pointCount - 1), 0.5f));
						}
					}
				}
				//Jitter only affects the object when the scene is in play mode
				if (Application.isPlaying)
				{
					Random.InitState((int)System.DateTime.Now.Ticks);
					for (var a = 0; a < Arcs.Count; a++)
					{
						for (var i = 0; i < pointCount; i++)
						{
							Arcs[a].Points[i] += Random.insideUnitCircle * (jitter * Parabola((float)i / (pointCount - 1), 0.5f));
						}
					}
				}
				//Build the geometry based on points we calculated
				BuildMesh();
			}
			else if (mesh != null && mesh.vertexCount > 0)
			{
				mesh.Clear();
				if (cr != null)
				{
					cr.SetMesh(mesh);
				}
			}
		}

		/*
			This function builds the mesh, combining all lines into one geometrical object (mesh).
			It is also responsible for calculation of line width based on all of the parameters
			and conditions (segment length, arc lifetime)
		*/

		private void BuildMesh()
		{
			//Reset and clear everything to rebuild from scratch
			if (mesh == null)
			{
				mesh = new Mesh { name = "Lightning Bolt 2D" };
			}
			mesh.Clear();
			vertices.Clear();
			uvs.Clear();
			colors.Clear();

			//Do all the math needed to create vertices of the mesh
			var activeArcs = 0; //Keeps the number of arcs we want to render
			for (var a = 0; a < Arcs.Count; a++)
			{
				if (Arcs[a].TimeEnd >= Time.time || !Application.isPlaying)
				{
					activeArcs++;
					//Calculate normals for all line segments
					for (var p = 1; p < pointCount; p++)
					{
						//Calculate normal for a line segment before this point
						Arcs[a].Normals[p] = (Arcs[a].Points[p - 1] - Arcs[a].Points[p]).normalized;
						Arcs[a].Normals[p] = new Vector2(Arcs[a].Normals[p].y, -Arcs[a].Normals[p].x);
						//For the zero point we use normal of the first point
						if (p == 1) Arcs[a].Normals[p - 1] = Arcs[a].Normals[p];
					}
					//Calculate widths for line at every point
					Arcs[a].TimeRatio = (Time.time - Arcs[a].TimeStart) / (Arcs[a].TimeEnd - Arcs[a].TimeStart);
					for (var p = 0; p < pointCount; p++)
					{
						var position = (float)p / (pointCount - 1);
						Arcs[a].Widths[p] = (lineWidth / 2);
						Arcs[a].Widths[p] *= Parabola(position, 0.5f); //Make line thinner towards the ends
						//Modify by lifetime
						if (Application.isPlaying)
						{
							Arcs[a].Widths[p] *= EaseIn(Arcs[a].TimeRatio, 0.1f); //Thin in the beginning
							Arcs[a].Widths[p] *= EaseOut(Arcs[a].TimeRatio, 5f); //Thin in the end
							Arcs[a].Widths[p] *= 1f - (Parabola(position, 2f) * (Arcs[a].TimeRatio * Arcs[a].TimeRatio * Arcs[a].TimeRatio)); //Thin in the middle towards the end
						}
					}
					//Calculate miters for each corner. It's a normalized vector that shows direction of the corner point
					for (var p = 1; p < pointCount - 1; p++)
					{
						//Calculate a normalized miter
						Arcs[a].Miters[p] = ((Arcs[a].Points[p + 1] - Arcs[a].Points[p]).normalized +
											 (Arcs[a].Points[p] - Arcs[a].Points[p - 1]).normalized).normalized;
						Arcs[a].Miters[p] = new Vector2(-Arcs[a].Miters[p].y, Arcs[a].Miters[p].x);
						//Calculate miter length for bottom line vertex
						Arcs[a].MitersLength[p, 0] =
							Arcs[a].Widths[p] / Vector2.Dot(Arcs[a].Miters[p], -Arcs[a].Normals[p]);
						if (Mathf.Abs(Arcs[a].MitersLength[p, 0]) > lineWidth * 2)
							Arcs[a].MitersLength[p, 0] =
								lineWidth * 2 * Mathf.Sign(Arcs[a].MitersLength[p, 0]); //Don't allow very sharp edges
						//Calculate miter length for top line vertex
						Arcs[a].MitersLength[p, 1] =
							Arcs[a].Widths[p] / Vector2.Dot(Arcs[a].Miters[p], Arcs[a].Normals[p]);
						if (Mathf.Abs(Arcs[a].MitersLength[p, 1]) > lineWidth * 2)
							Arcs[a].MitersLength[p, 1] =
								lineWidth * 2 * Mathf.Sign(Arcs[a].MitersLength[p, 1]); //Don't allow very sharp edges
						//Miter length for the bottom glow vertex
						Arcs[a].MitersLength[p, 2] =
							(Arcs[a].Widths[p] + glowWidth * (Arcs[a].Widths[p] / (lineWidth / 2))) /
							Vector2.Dot(Arcs[a].Miters[p], -Arcs[a].Normals[p]);
						if (Mathf.Abs(Arcs[a].MitersLength[p, 2]) > (lineWidth * 2 + glowWidth * 2))
							Arcs[a].MitersLength[p, 2] =
								(lineWidth * 2 + glowWidth * 2) *
								Mathf.Sign(Arcs[a].MitersLength[p, 2]); //Don't allow very sharp edges
						//Miter length for the top glow vertex
						Arcs[a].MitersLength[p, 3] =
							(Arcs[a].Widths[p] + glowWidth * (Arcs[a].Widths[p] / (lineWidth / 2))) /
							Vector2.Dot(Arcs[a].Miters[p], Arcs[a].Normals[p]);
						if (Mathf.Abs(Arcs[a].MitersLength[p, 3]) > (lineWidth * 2 + glowWidth * 2))
							Arcs[a].MitersLength[p, 3] =
								(lineWidth * 2 + glowWidth * 2) *
								Mathf.Sign(Arcs[a].MitersLength[p, 3]); //Don't allow very sharp edges
					}
				}
			}

			//Create vertices for the glow
			if (glowWidth > 0)
			{
				for (var a = 0; a < Arcs.Count; a++)
				{
					if (Arcs[a].TimeEnd >= Time.time || !Application.isPlaying)
					{
						for (var p = 0; p < pointCount; p++)
						{
							vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 2]);
							vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 0]);
							colors.Add(glowEdgeColor);
							colors.Add((p > 0 && p < pointCount - 1) ? glowColor : glowEdgeColor);
						}
						for (var p = 0; p < pointCount; p++)
						{
							vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 1]);
							vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 3]);
							colors.Add((p > 0 && p < pointCount - 1) ? glowColor : glowEdgeColor);
							colors.Add(glowEdgeColor);
						}
					}
				}
			}

			//Create vertices and colros for the lines
			for (var a = 0; a < Arcs.Count; a++)
			{
				if (Arcs[a].TimeEnd >= Time.time || !Application.isPlaying)
				{
					for (var p = 1; p < pointCount - 1; p++)
					{
						//Add the first point since we're skipping it in the for loop
						if (p == 1)
						{
							vertices.Add(Arcs[a].Points[p - 1] - Arcs[a].Normals[p - 1] * Arcs[a].Widths[p - 1]);
							vertices.Add(Arcs[a].Points[p - 1] + Arcs[a].Normals[p - 1] * Arcs[a].Widths[p - 1]);
							colors.Add(lineColor);
							colors.Add(lineColor);
						}
						vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 0]);
						vertices.Add(Arcs[a].Points[p] + Arcs[a].Miters[p] * Arcs[a].MitersLength[p, 1]);
						colors.Add(lineColor);
						colors.Add(lineColor);
						//If we're on second to last point, we're also adding the last one
						if (p == pointCount - 2)
						{
							vertices.Add(Arcs[a].Points[p + 1] - Arcs[a].Normals[p + 1] * Arcs[a].Widths[p + 1]);
							vertices.Add(Arcs[a].Points[p + 1] + Arcs[a].Normals[p + 1] * Arcs[a].Widths[p + 1]);
							colors.Add(lineColor);
							colors.Add(lineColor);
						}
					}
				}
			}

			//Create triangles
			var lines = (activeArcs * (lineWidth > 0 ? 1 : 0)) + (activeArcs * (glowWidth > 0 ? 2 : 0));
			triangleCount = lines * (pointCount - 1) * 2;
			if (triangles == null)
			{
				triangles = new int[triangleCount * 3];
			}
			else if (triangles.Length != triangleCount * 3)
			{
				System.Array.Resize(ref triangles, triangleCount * 3);
			}

			for (var l = 0; l < lines; l++)
			{
				var ls = (l * (pointCount - 1) * 6);
				var vs = l * pointCount * 2;
				for (var i = 0; i < pointCount - 1; i++)
				{
					triangles[ls + (i * 6) + 0] = vs + i * 2 + 0;
					triangles[ls + (i * 6) + 1] = vs + i * 2 + 1;
					triangles[ls + (i * 6) + 2] = vs + i * 2 + 3;
					triangles[ls + (i * 6) + 3] = vs + i * 2 + 0;
					triangles[ls + (i * 6) + 4] = vs + i * 2 + 3;
					triangles[ls + (i * 6) + 5] = vs + i * 2 + 2;
				}
			}

			mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetColors(colors);
			mesh.SetTriangles(triangles, 0);
			mesh.RecalculateBounds();
			if (isCanvas)
			{
				if (cr == null) cr = GetComponent<CanvasRenderer>();
				cr.SetMesh(mesh);
			}
			else
			{
				if (mf == null) mf = GetComponent<MeshFilter>();
				mf.sharedMesh = mesh;
			}
			if (mr != null)
			{
				mr.sortingLayerID = sortingLayer;
				mr.sortingOrder = orderInLayer;
			}
		}

		#endregion

		#region Utility functions

		private static bool IsChildOfCanvas(Transform t)
		{
			while (true)
			{
				if (t.GetComponent<Canvas>() != null)
				{
					return true;
				}
				if (t.parent != null)
				{
					t = t.parent;
				}
				else
				{
					return false;
				}
			}
		}

		private static Color RandomMildColor()
		{
			var hue = Random.Range(0f, 1f);
			while (hue * 360f >= 236f && hue * 360f <= 246f)
			{
				hue = Random.Range(0f, 1f);
			}

			return Color.HSVToRGB(hue, Random.Range(0.2f, 0.7f), Random.Range(0.8f, 1f));
		}
		private static float Parabola(float x, float p)
		{
			return Mathf.Pow(4f * x * (1f - x), p);
		}
		private static float EaseIn(float x, float p)
		{
			return Mathf.Pow(x, p);
		}
		private static float EaseOut(float x, float p)
		{
			float f = x - 1f;
			return 1f - Mathf.Pow(x, p);
		}
		private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			var u = 1 - t;
			var tt = t * t;
			var uu = u * u;
			var uuu = uu * u;
			var ttt = tt * t;
			var p = uuu * p0;
			p += 3 * uu * t * p1;
			p += 3 * u * tt * p2;
			p += ttt * p3;
			return p;
		}

		#endregion
	}
}