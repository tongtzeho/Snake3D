using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public class PosINT {
		public int x, z;
		public PosINT(int a, int b) {
			x = a;
			z = b;
		}
	}

	public class SnakeCell {
		public int targetx, targetz;
		public int prevx, prevz;
		public float posx, posz;
		public int dir;
		public GameObject gobj;

		public void getposfromdir() {
			if (dir == 0) {
				prevx = targetx - 1;
				prevz = targetz;
			} else if (dir == 1) {
				prevx = targetx;
				prevz = targetz - 1;
			} else if (dir == 2) {
				prevx = targetx + 1;
				prevz = targetz;
			} else {
				prevx = targetx;
				prevz = targetz + 1;
			}
			posx = (float)prevx;
			posz = (float)prevz;
		}

		public float move(float gamespeed) {
			float movelength = Time.deltaTime / gamespeed;
			if (dir == 0) {
				posx += movelength;
			} else if (dir == 1) {
				posz += movelength;
			} else if (dir == 2) {
				posx -= movelength;
			} else {
				posz -= movelength;
			}
			return Mathf.Sqrt ((posx - prevx) * (posx - prevx) + (posz - prevz) * (posz - prevz));
		}

		public void gettarget() {
			if (dir == 0) {
				targetx = prevx + 1;
				targetz = prevz;
			} else if (dir == 1) {
				targetx = prevx;
				targetz = prevz + 1;
			} else if (dir == 2) {
				targetx = prevx - 1;
				targetz = prevz;
			} else {
				targetx = prevx;
				targetz = prevz - 1;
			}
		}

	}

	public static bool gamestart;
	public int width, height;
	public int leftbound, rightbound, upbound, downbound;
	public float snakey;
	public int [,]board;
	/**
	 * 0-2 color
	 * 3 small obstacle
	 * 4 large obstacle
	 * 5 blue existed
	 * 6 head start pos
	 * 7 head/body on it
	 * 8-9 start dir
	 * 10 blue eaten
	 * 11 orange
	 * 12 add life
	 * */
	public int[,] belongbluelist;
	public List<PosINT>[] bluelist;
	public bool[] eatenblue, existedblue;
	public int[] objtype;
	public List<int>[] creator;
	public int snakenextdir;
	public GameObject[,] boardgobj;
	public int snakestartposx, snakestartposz, snakestartdir;
	public List<SnakeCell> snake;
	public Quaternion []dir_rotation; // 0 right 1 up 2 left 3 down
	public float gamespeed; // x second for 1 block
	public bool lockleft, lockright, lockr;
	public int snakeheadolddir;
	public GameObject boardquad, behindcamera;
	public bool judged, gameover, win;
	public int prevblueindex, bluecnt;
	public bool eraseblue, eraseother;
	private Mesh mh;
	private Vector2[] uvs;
	private Vector2[,] srcpos;
	public static bool ispause;
	public float holdon;
	public bool isoverlook;
	public UnityEngine.UI.Text stageandlifetext, scoretext, factortext;
	public RectTransform barfullrect;
	public UnityEngine.UI.Image clockimg;
	public int factor;
	public int needtoeat, eatennum;
	public int maxstep, stepcnt;
	public Sprite[] sprt;

	// Use this for initialization

	void getbound() {
		leftbound = -width / 2;
		rightbound = leftbound + width;
		downbound = -height / 2;
		upbound = downbound + height;
	}

	void drawboardquad() {
		boardquad = Instantiate (Resources.Load ("Board") as GameObject);
		boardquad.transform.position = new Vector3 (leftbound, 0, downbound);
		mh = boardquad.GetComponent<MeshFilter>().mesh;
		int i, j, k;
		Vector3[] vertes = new Vector3[width * height * 4 + 8];
		for (i = 0; i < width; i++)
			for (j = 0; j < height; j++) {
				int index = (i * height + j) * 4;
				vertes [index] = new Vector3 (i - 0.5f, j - 0.5f, 0f);
				vertes [index + 1] = new Vector3 (i - 0.5f, j + 0.5f, 0f);
				vertes [index + 2] = new Vector3 (i + 0.5f, j + 0.5f, 0f);
				vertes [index + 3] = new Vector3 (i + 0.5f, j - 0.5f, 0f);
			}
		vertes [width * height * 4] = new Vector3 (-0.5f, -0.5f, 0f);
		vertes [width * height * 4 + 1] = new Vector3 (-0.5f, height - 0.5f, 0f);
		vertes [width * height * 4 + 2] = new Vector3 (width - 0.5f, height - 0.5f, 0f);
		vertes [width * height * 4 + 3] = new Vector3 (width - 0.5f, -0.5f, 0f);
		vertes [width * height * 4 + 4] = new Vector3 (-1000.5f, -1000.5f, 0f);
		vertes [width * height * 4 + 5] = new Vector3 (-1000.5f, height + 1000.5f, 0f);
		vertes [width * height * 4 + 6] = new Vector3 (width + 1000.5f, height + 1000.5f, 0f);
		vertes [width * height * 4 + 7] = new Vector3 (width + 1000.5f, -1000.5f, 0f);
		mh.vertices = vertes;
		int[] triangles = new int[width * height * 6 + 24];
		for (i = 0; i < width * height; i++) {
			int dest = i * 6;
			int src = i * 4;
			triangles [dest] = src;
			triangles [dest + 1] = src + 1;
			triangles [dest + 2] = src + 2;
			triangles [dest + 3] = src;
			triangles [dest + 4] = src + 2;
			triangles [dest + 5] = src + 3;
		}
		triangles [width * height * 6] = width * height * 4;
		triangles [width * height * 6 + 1] = width * height * 4 + 4;
		triangles [width * height * 6 + 2] = width * height * 4 + 1;
		triangles [width * height * 6 + 3] = width * height * 4 + 1;
		triangles [width * height * 6 + 4] = width * height * 4 + 4;
		triangles [width * height * 6 + 5] = width * height * 4 + 5;
		triangles [width * height * 6 + 6] = width * height * 4 + 1;
		triangles [width * height * 6 + 7] = width * height * 4 + 5;
		triangles [width * height * 6 + 8] = width * height * 4 + 2;
		triangles [width * height * 6 + 9] = width * height * 4 + 2;
		triangles [width * height * 6 + 10] = width * height * 4 + 5;
		triangles [width * height * 6 + 11] = width * height * 4 + 6;
		triangles [width * height * 6 + 12] = width * height * 4 + 2;
		triangles [width * height * 6 + 13] = width * height * 4 + 6;
		triangles [width * height * 6 + 14] = width * height * 4 + 3;
		triangles [width * height * 6 + 15] = width * height * 4 + 3;
		triangles [width * height * 6 + 16] = width * height * 4 + 6;
		triangles [width * height * 6 + 17] = width * height * 4 + 7;
		triangles [width * height * 6 + 18] = width * height * 4 + 3;
		triangles [width * height * 6 + 19] = width * height * 4 + 7;
		triangles [width * height * 6 + 20] = width * height * 4;
		triangles [width * height * 6 + 21] = width * height * 4;
		triangles [width * height * 6 + 22] = width * height * 4 + 7;
		triangles [width * height * 6 + 23] = width * height * 4 + 4;
		mh.triangles = triangles;
		mh.RecalculateNormals();
		uvs = new Vector2[width * height * 4 + 8];
		srcpos = new Vector2[8, 4];
		for (i = 0; i < 7; i++) {
			srcpos [i, 0] = new Vector2 (0.125f * i + 0.001f, 0.504f);
			srcpos [i, 1] = new Vector2 (0.125f * i + 0.001f, 0.996f);
			srcpos [i, 2] = new Vector2 (0.125f * (i + 1) - 0.001f, 0.996f);
			srcpos [i, 3] = new Vector2 (0.125f * (i + 1) - 0.001f, 0.504f);
		}
		srcpos [7, 0] = new Vector2 (0.92f, 0.6f);
		srcpos [7, 1] = new Vector2 (0.92f, 0.9f);
		srcpos [7, 2] = new Vector2 (0.95f, 0.9f);
		srcpos [7, 3] = new Vector2 (0.95f, 0.6f);
		for (i = 0; i < width; i++)
			for (j = 0; j < height; j++) {
				int dest = (i * height + j) * 4;
				int src = board [i, j] & 7;
				for (k = 0; k < 4; k++) {
					uvs [dest + k] = srcpos [src, k];
				}
			}
		for (k = 0; k < 4; k++) {
			uvs [width * height * 4 + k] = srcpos [7, k];
		}
		uvs [width * height * 4 + 4] = new Vector2 (0.9f, 0.55f);
		uvs [width * height * 4 + 5] = new Vector2 (0.9f, 0.95f);
		uvs [width * height * 4 + 6] = new Vector2 (0.98f, 0.95f);
		uvs [width * height * 4 + 7] = new Vector2 (0.98f, 0.55f);
		mh.uv = uvs;
	}

	void getboardfromfile() {
		StreamReader sr = new StreamReader("Assets/Map/map"+UIClick.level.ToString()+".txt");
		string line;
		line = sr.ReadLine ().ToString ();
		height = int.Parse (line.Split (' ') [0]);
		width = int.Parse (line.Split (' ') [1]);
		getbound ();
		board = new int[width, height];
		belongbluelist = new int[width, height];
		boardgobj = new GameObject[width, height];
		string[] ele;
		int i, j;
		for (i = height - 1; i >= 0; i--) {
			ele = sr.ReadLine ().ToString ().Split (' ');
			for (j = 0; j < width; j++) {
				board [j, i] = int.Parse(ele [j]);
				belongbluelist [j, i] = -1;
				boardgobj [j, i] = null;
			}
		}
		ele = sr.ReadLine ().ToString ().Split (' ');
		bluelist = new List<PosINT>[int.Parse (ele [0])];
		needtoeat = int.Parse (ele [1]);
		maxstep = int.Parse (ele [2]);
		eatenblue = new bool[bluelist.Length];
		existedblue = new bool[bluelist.Length];
		creator = new List<int>[bluelist.Length];
		objtype = new int[bluelist.Length];
		for (i = 0; i < bluelist.Length; i++) {
			eatenblue [i] = false;
			existedblue [i] = false;
			ele = sr.ReadLine ().ToString ().Split (' ');
			int type = int.Parse (ele [0]);
			int creatornum;
			bluelist [i] = new List<PosINT> ();
			creator [i] = new List<int> ();
			if (type < 0) {
				objtype [i] = type;
				bluelist [i].Add (new PosINT (int.Parse (ele [1]), int.Parse (ele [2])));
				creatornum = int.Parse (ele [3]);
				for (j = 0; j < creatornum; j++) {
					creator [i].Add (int.Parse (ele [4 + j]));
				}
			} else {
				objtype [i] = 0;
				for (j = 0; j < type; j++) {
					bluelist [i].Add (new PosINT(int.Parse (ele [j * 2 + 1]), int.Parse (ele [j * 2 + 2])));
				}
				creatornum = int.Parse (ele [type * 2 + 1]);
				for (j = 0; j < creatornum; j++) {
					creator [i].Add (int.Parse (ele [type * 2 + 2 + j]));
				}
			}
		}
		sr.Close ();
	}

	void createblue() {
		Vector2[] bluesrcpos = new Vector2[4] {
			new Vector2 (0f, 0f),
			new Vector2 (0f, 0.5f),
			new Vector2 (0.125f, 0.5f),
			new Vector2 (0.125f, 0f)
		};
		int i, j, k;
		for (i = 0; i < bluelist.Length; i++) {
			if (!existedblue [i]) {
				bool create = true;
				if (creator [i].Count == 1 && creator [i] [0] == -1) {
					create = true;
				} else {
					for (j = 0; j < creator [i].Count; j++) {
						if (!eatenblue [creator [i] [j]]) {
							create = false;
							break;
						}
					}
				}
				if (create) {
					existedblue [i] = true;
					if (objtype [i] == 0) {
						for (j = 0; j < bluelist [i].Count; j++) {
							int posx = bluelist [i] [j].x;
							int posz = bluelist [i] [j].z;
							board [posx, posz] |= (1 << 5);
							belongbluelist [posx, posz] = i;
							if (boardgobj [posx, posz] != null) {
								Destroy (boardgobj [posx, posz]);
							}
							if (bluelist [i].Count > 1) {
								boardgobj [posx, posz] = Instantiate (Resources.Load ("Blue") as GameObject);
								boardgobj [posx, posz].transform.position = new Vector3 (posx + leftbound, 0.55f, posz + downbound);
								boardgobj [posx, posz].transform.rotation = Quaternion.Euler (0f, j * 45f, 45f);
								int index = (posx * height + posz) * 4;
								for (k = 0; k < 4; k++) {
									uvs [index + k] = bluesrcpos [k];
								}
							} else {
								boardgobj [posx, posz] = Instantiate (Resources.Load ("Green") as GameObject);
								boardgobj [posx, posz].transform.position = new Vector3 (posx + leftbound, 0.55f, posz + downbound);
							}
						}
					} else if (objtype [i] == -1) {
						int posx = bluelist [i] [0].x;
						int posz = bluelist [i] [0].z;
						board [posx, posz] |= (1 << 11);
						if (boardgobj [posx, posz] != null) {
							Destroy (boardgobj [posx, posz]);
						}
						boardgobj [posx, posz] = Instantiate (Resources.Load ("Orange") as GameObject);
						boardgobj [posx, posz].transform.position = new Vector3 (posx + leftbound, 0.55f, posz + downbound);
					} else if (objtype [i] == -2) {
						int posx = bluelist [i] [0].x;
						int posz = bluelist [i] [0].z;
						board [posx, posz] |= (1 << 12);
						if (boardgobj [posx, posz] != null) {
							Destroy (boardgobj [posx, posz]);
						}
						boardgobj [posx, posz] = Instantiate (Resources.Load ("AddLife") as GameObject);
						boardgobj [posx, posz].transform.position = new Vector3 (posx + leftbound, 0.5f, posz + downbound);
					}
				}
			}
		}
		mh.uv = uvs;
	}

	void resetblue(int x) {
		if (x >= 0 && x < bluelist.Length) {
			int i;
			for (i = 0; i < bluelist [x].Count; i++) {
				if ((board [bluelist [x] [i].x, bluelist [x] [i].z] & (1 << 10)) != 0) {
					board [bluelist [x] [i].x, bluelist [x] [i].z] -= (1 << 10);
				}
				boardgobj [bluelist [x] [i].x, bluelist [x] [i].z].transform.position = new Vector3 (boardgobj [bluelist [x] [i].x, bluelist [x] [i].z].transform.position.x, 0.55f, boardgobj [bluelist [x] [i].x, bluelist [x] [i].z].transform.position.z);
			}
		}
		factor = 1;
	}

	void getboard() {
		getboardfromfile ();
		drawboardquad ();
		int i, j;
		for (i = 0; i < width; i++) {
			for (j = 0; j < height; j++) {
				if ((board [i, j] & (1 << 6)) != 0) {
					snakestartposx = i;
					snakestartposz = j;
					snakestartdir = (board [i, j] >> 8) & 3;
				} else if ((board [i, j] & (1 << 3)) != 0) {
					boardgobj [i, j] = Instantiate (Resources.Load ("SmallObstacle") as GameObject);
					boardgobj [i, j].transform.position = new Vector3 (i + leftbound + 0.08f, -0.21f, j + downbound - 0.08f);
				} else if ((board [i, j] & (1 << 4)) != 0) {
					boardgobj [i, j] = Instantiate (Resources.Load ("BigObstacle") as GameObject);
					boardgobj [i, j].transform.position = new Vector3 (i + leftbound, 0.4f, j + downbound);
				}
			}
		}
		createblue ();
	}

	void getsnake() {
		snake = new List<SnakeCell> ();
		SnakeCell snakehead = new SnakeCell ();
		snakehead.targetx = snakestartposx;
		snakehead.targetz = snakestartposz;
		snakeheadolddir = snakehead.dir = snakestartdir;
		snakehead.getposfromdir ();
		if (snakehead.prevx >= 0 && snakehead.prevz >= 0 && snakehead.prevx < width && snakehead.prevz < height) {
			board [snakehead.prevx, snakehead.prevz] |= (1 << 7);
		}
		snakehead.gobj = Instantiate (Resources.Load ("SnakeHead") as GameObject);
		snakehead.gobj.transform.position = new Vector3 (snakehead.posx + leftbound, snakey, snakehead.posz + downbound);
		snakehead.gobj.transform.rotation = dir_rotation [snakehead.dir];
		snake.Add (snakehead);
		int i;
		for (i = 0; i < 4; i++) {
			SnakeCell snakebody = new SnakeCell ();
			SnakeCell snaketail = snake [snake.Count - 1];
			snakebody.targetx = snaketail.prevx;
			snakebody.targetz = snaketail.prevz;
			snakebody.dir = snaketail.dir;
			snakebody.getposfromdir ();
			if (snakebody.prevx >= 0 && snakebody.prevz >= 0 && snakebody.prevx < width && snakebody.prevz < height) {
				board [snakebody.prevx, snakebody.prevz] |= (1 << 7);
			}
			snakebody.gobj = Instantiate (Resources.Load ("SnakeBody") as GameObject);
			snakebody.gobj.name = "snakebody_" + snake.Count.ToString ();
			snakebody.gobj.transform.position = new Vector3 (snakebody.posx + leftbound, snakey, snakebody.posz + downbound);
			snakebody.gobj.transform.rotation = dir_rotation [snakebody.dir];
			snake.Add (snakebody);
		}
	}

	void Awake() {
		Application.targetFrameRate = 120;
	}

	void Start () {
		behindcamera = GameObject.Find ("Camera/BehindHead");
		Sunlight.light_strength = 0.5f;
		isoverlook = false;
		lockr = false;
		snakey = 0.25f;
		dir_rotation = new Quaternion[4]{Quaternion.Euler(0f, 180f, 90f), Quaternion.Euler(0f, 90f, 90f), Quaternion.Euler(0f, 0f, 90f), Quaternion.Euler(0f, 270f, 90f)};
		snakenextdir = 0;
		gamespeed = 0.17f;
		eatennum = 0;
		getboard ();
		getsnake ();
		setcamera ();
		gamestart = true;
		lockleft = lockright = false;
		judged = false;
		gameover = win = false;
		prevblueindex = -1;
		bluecnt = 0;
		eraseblue = eraseother = false;
		holdon = 0.3f;
		factor = 1;
		stepcnt = 0;
		getui ();
	}

	void movesnake() {
		int i, k;
		SnakeCell snakehead = snake [0];
		SnakeCell snakebody, snakebodyprev;
		float reach_rate = snakehead.move (gamespeed);
		if (reach_rate > 0.5) {
			if (reach_rate > 0.8) {
				if (eraseblue) {
					for (i = 0; i < bluecnt; i++) {
						Destroy (boardgobj [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z]);
						boardgobj [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] = null;
						if ((board [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] & (1 << 5)) != 0) {
							board [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] -= (1 << 5);
						}
						if ((board [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] & (1 << 10)) != 0) {
							board [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] -= (1 << 10);
						}
						if (bluecnt > 1) {
							int index = (bluelist [prevblueindex] [i].x * height + bluelist [prevblueindex] [i].z) * 4;
							for (k = 0; k < 4; k++) {
								uvs [index + k] = srcpos [board [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] & 7, k];
							}
						}
						belongbluelist [bluelist [prevblueindex] [i].x, bluelist [prevblueindex] [i].z] = -1;
					}
					if (bluecnt > 1) {
						mh.uv = uvs;
					}
					bluelist[prevblueindex].Clear();
					eatenblue [prevblueindex] = true;
					prevblueindex = -1;
					if (snake.Count < 20) {
						SnakeCell newsnaketail = new SnakeCell ();
						SnakeCell oldsnaketail = snake [snake.Count - 1];
						newsnaketail.targetx = newsnaketail.prevx = oldsnaketail.prevx;
						newsnaketail.targetz = newsnaketail.prevz = oldsnaketail.prevz;
						newsnaketail.dir = oldsnaketail.dir;
						newsnaketail.posx = oldsnaketail.posx;
						newsnaketail.posz = oldsnaketail.posz;
						newsnaketail.gobj = Instantiate (Resources.Load ("SnakeBody") as GameObject);
						newsnaketail.gobj.name = "snakebody_" + snake.Count.ToString ();
						newsnaketail.gobj.transform.position = new Vector3 (newsnaketail.posx + leftbound, snakey, newsnaketail.posz + downbound);
						newsnaketail.gobj.transform.rotation = dir_rotation [newsnaketail.dir];
						snake.Add (newsnaketail);
					}
					createblue ();
					eraseblue = false;
					if (bluecnt > 1) {
						UIClick.score += bluecnt * factor * 10;
						if (factor < 99) {
							factor++;
						}
					} else {
						UIClick.score += 500;
					}
					updateui ();
					eatennum++;
					updatebar ();
					if (eatennum == needtoeat) {
						win = true;
					}
					bluecnt = 0;
				}
				if (eraseother) {
					Destroy (boardgobj [snakehead.targetx, snakehead.targetz]);
					boardgobj [snakehead.targetx, snakehead.targetz] = null;
					eraseother = false;
				}
			}
			if (!judged) {
				if (snakehead.targetx >= 0 && snakehead.targetz >= 0 && snakehead.targetx < width && snakehead.targetz < height) {
					if ((board [snakehead.targetx, snakehead.targetz] & (1 << 7)) != 0) {
						// bite itself
						Sunlight.light_strength = 3f;
						gameover = true;
						return;
					}
					if ((board [snakehead.targetx, snakehead.targetz] & (1 << 4)) != 0) {
						// hit big obstacle
						Sunlight.light_strength = 1.3f;
						int cutlen = snake.Count >> 1;
						for (i = 0; i < cutlen; i++) {
							SnakeCell snaketail = snake [snake.Count - 1];
							if ((board [snaketail.prevx, snaketail.prevz] & (1 << 7)) != 0) {
								board [snaketail.prevx, snaketail.prevz] -= (1 << 7);
							}
							Destroy (snaketail.gobj);
							snake.Remove (snaketail);
							if (snake.Count < 3) {
								Sunlight.light_strength = 3f;
								gameover = true;
								return;
							}
						}
						snakehead.dir = (snakehead.dir + 3) % 4;
						snakehead.gettarget ();
						if ((board [snakehead.targetx, snakehead.targetz] & ((1 << 7) | (1 << 4))) == 0) {
							snakehead.posx = snakehead.prevx * 0.5f + snakehead.targetx * 0.5f;
							snakehead.posz = snakehead.prevz * 0.5f + snakehead.targetz * 0.5f;
							snakenextdir = 0;
							lockleft = lockright = false;
						} else {
							snakehead.dir = (snakehead.dir + 2) % 4;
							snakehead.gettarget ();
							if ((board [snakehead.targetx, snakehead.targetz] & ((1 << 7) | (1 << 4))) == 0) {
								snakehead.posx = snakehead.prevx * 0.5f + snakehead.targetx * 0.5f;
								snakehead.posz = snakehead.prevz * 0.5f + snakehead.targetz * 0.5f;
								snakenextdir = 0;
								lockleft = lockright = false;
							} else {
								snakehead.dir = (snakehead.dir + 1) % 4;
								snakehead.gettarget ();
								if ((board [snakehead.targetx, snakehead.targetz] & ((1 << 7) | (1 << 4))) == 0) {
									snakehead.posx = snakehead.prevx * 0.5f + snakehead.targetx * 0.5f;
									snakehead.posz = snakehead.prevz * 0.5f + snakehead.targetz * 0.5f;
									snakenextdir = 0;
									lockleft = lockright = false;
								} else {
									snakehead.dir = (snakehead.dir + 2) % 4;
									snakehead.gettarget ();
									Sunlight.light_strength = 3f;
									gameover = true;
									return;
								}
							}
						}
					}
					if ((board [snakehead.targetx, snakehead.targetz] & (1 << 3)) != 0) {
						// hit small obstacle
						Sunlight.light_strength = 0.9f;
						int cutlen = 2;
						for (i = 0; i < cutlen; i++) {
							SnakeCell snaketail = snake [snake.Count - 1];
							if ((board [snaketail.prevx, snaketail.prevz] & (1 << 7)) != 0) {
								board [snaketail.prevx, snaketail.prevz] -= (1 << 7);
							}
							Destroy (snaketail.gobj);
							snake.Remove (snaketail);
							if (snake.Count < 3) {
								Sunlight.light_strength = 3f;
								gameover = true;
								return;
							}
						}
						Destroy (boardgobj [snakehead.targetx, snakehead.targetz]);
						boardgobj [snakehead.targetx, snakehead.targetz] = null;
						board [snakehead.targetx, snakehead.targetz] -= (1 << 3);
					}
					if ((board [snakehead.targetx, snakehead.targetz] & (1 << 5)) != 0 && (board [snakehead.targetx, snakehead.targetz] & (1 << 10)) == 0) {
						if (belongbluelist [snakehead.targetx, snakehead.targetz] != prevblueindex) {
							bluecnt = 0;
							if (prevblueindex != -1) {
								resetblue (prevblueindex);
							}
							prevblueindex = belongbluelist [snakehead.targetx, snakehead.targetz];
						}
						bluecnt++;
						boardgobj [snakehead.targetx, snakehead.targetz].transform.position = new Vector3 (boardgobj [snakehead.targetx, snakehead.targetz].transform.position.x, -10, boardgobj [snakehead.targetx, snakehead.targetz].transform.position.z);
						board [snakehead.targetx, snakehead.targetz] |= (1 << 10);
						if (bluecnt == bluelist [prevblueindex].Count) {
							eraseblue = true;
						}
						if (bluelist [prevblueindex].Count > 1) {
							UIClick.score += factor * 10;
						}
					} else {
						if ((board [snakehead.targetx, snakehead.targetz] & (1 << 11)) != 0) {
							UIClick.score += 2000;
							eraseother = true;
						} else if ((board [snakehead.targetx, snakehead.targetz] & (1 << 12)) != 0) {
							UIClick.life++;
							UIClick.score += 100;
							stageandlifetext.text = "Stage: " + UIClick.level.ToString () + "\nLife: " + UIClick.life.ToString ();
							eraseother = true;
						}
						bluecnt = 0;
						if (prevblueindex != -1) {
							resetblue (prevblueindex);
						}
						prevblueindex = -1;
					}
					updateui ();
				}
				judged = true;
			}
			// Enter the next block, judge whether the head hits something
			if (reach_rate >= 1) {
				UIClick.score++;
				stepcnt++;
				int curclockid = stepcnt * 8 / maxstep;
				if (curclockid > 7) {
					curclockid = 7;
				}
				if (curclockid == 7) {
					Sunlight.light_strength = 0.7f;
				}
				int prevclockid = (stepcnt - 1) * 8 / maxstep;
				if (curclockid > prevclockid) {
					clockimg.sprite = sprt [curclockid];
				}
				if (stepcnt >= maxstep) {
					Sunlight.light_strength = 3f;
					gameover = true;
					return;
				}
				if (snakehead.prevx >= 0 && snakehead.prevz >= 0 && snakehead.prevx < width && snakehead.prevz < height) {
					if ((board [snakehead.prevx, snakehead.prevz] & (1 << 7)) != 0) {
						board [snakehead.prevx, snakehead.prevz] -= (1 << 7);
					}
				}
				snakehead.posx = snakehead.prevx = snakehead.targetx;
				snakehead.posz = snakehead.prevz = snakehead.targetz;
				if (snakehead.prevx >= 0 && snakehead.prevz >= 0 && snakehead.prevx < width && snakehead.prevz < height) {
					board [snakehead.prevx, snakehead.prevz] |= (1 << 7);
				}
				snakehead.gobj.transform.position = new Vector3 (snakehead.posx + leftbound, snakey, snakehead.posz + downbound);
				for (i = 1; i < snake.Count; i++) {
					snakebodyprev = snake [i - 1];
					snakebody = snake [i];
					if (snakebody.prevx >= 0 && snakebody.prevz >= 0 && snakebody.prevx < width && snakebody.prevz < height) {
						if ((board [snakebody.prevx, snakebody.prevz] & (1 << 7)) != 0) {
							board [snakebody.prevx, snakebody.prevz] -= (1 << 7);
						}
					}
					snakebody.posx = snakebody.prevx = snakebody.targetx;
					snakebody.posz = snakebody.prevz = snakebody.targetz;
					if (snakebody.prevx >= 0 && snakebody.prevz >= 0 && snakebody.prevx < width && snakebody.prevz < height) {
						board [snakebody.prevx, snakebody.prevz] |= (1 << 7);
					}
					snakebody.targetx = snakebodyprev.prevx;
					snakebody.targetz = snakebodyprev.prevz;
					if (snakebody.targetx == snakebody.prevx + 1) {
						snakebody.dir = 0;
					} else if (snakebody.targetz == snakebody.prevz + 1) {
						snakebody.dir = 1;
					} else if (snakebody.targetx == snakebody.prevx - 1) {
						snakebody.dir = 2;
					} else {
						snakebody.dir = 3;
					}
					snakebody.gobj.transform.position = new Vector3 (snakebody.posx + leftbound, snakey, snakebody.posz + downbound);
					snakebody.gobj.transform.rotation = dir_rotation [snakebody.dir];
				}
				snakeheadolddir = snakehead.dir;
				if (snakenextdir != 0) {
					snakehead.dir += snakenextdir;
					if (snakehead.dir == 4) {
						snakehead.dir = 0;
					} else if (snakehead.dir == -1) {
						snakehead.dir = 3;
					}
					snakenextdir = 0;
				}
				if (snakehead.dir == 0) {
					snakehead.targetx++;
				} else if (snakehead.dir == 1) {
					snakehead.targetz++;
				} else if (snakehead.dir == 2) {
					snakehead.targetx--;
				} else {
					snakehead.targetz--;
				}
				judged = false;
				updateui ();
				return;
			}
		}
		snakehead.gobj.transform.position = new Vector3 (snakehead.posx + leftbound, snakey, snakehead.posz + downbound);
		for (i = 1; i < snake.Count; i++) {
			snakebody = snake [i];
			snakebody.move (gamespeed);
			snakebody.gobj.transform.position = new Vector3 (snakebody.posx + leftbound, snakey, snakebody.posz + downbound);
		}
		if (snakehead.dir != snakeheadolddir) {
			if (snakehead.dir == snakeheadolddir + 1 || snakehead.dir == snakeheadolddir - 3) {
				snakehead.gobj.transform.rotation = Quaternion.Euler (0f, -90 * snakeheadolddir + 180 - 90 * Mathf.Min (1, reach_rate * 1.0f), 90f);
			} else {
				snakehead.gobj.transform.rotation = Quaternion.Euler (0f, -90 * snakeheadolddir + 180 + 90 * Mathf.Min (1, reach_rate * 1.0f), 90f);
			}
		} else {
			snakehead.gobj.transform.rotation = Quaternion.Euler (0f, -90 * snakeheadolddir + 180, 90f);
		}
	}

	void setcamera() {
		GameObject snakeheadobj = snake [0].gobj;
		float angle = snakeheadobj.transform.localEulerAngles.y;
		float radian = angle * Mathf.PI / 180f;
		if (!isoverlook) {
			behindcamera.transform.position = new Vector3 (snakeheadobj.transform.position.x + 1f * Mathf.Cos (radian), 5f, snakeheadobj.transform.position.z - 1f * Mathf.Sin (radian));
			behindcamera.transform.rotation = Quaternion.Euler (60f, angle - 90f, 0);
		} else {
			behindcamera.transform.position = new Vector3 (snakeheadobj.transform.position.x, 6f, snakeheadobj.transform.position.z);
			behindcamera.transform.rotation = Quaternion.Euler (90f, angle - 90f, 0);
		}
	}

	// Update is called once per frame
	void Update () {
		if (holdon > 0) {
			holdon -= Time.deltaTime;
			if (holdon <= 0) {
				if (gameover) {
					UIClick.gameover = true;
				} else if (win) {
					UIClick.win = true;
				}
				holdon = 0;
			}
		}
		if (holdon == 0 && !ispause) {
			if (Input.GetKey (KeyCode.R)) {
				if (!lockr) {
					isoverlook = !isoverlook;
					lockr = true;
				}
			} else {
				lockr = false;
			}
			if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
				if (!lockleft) {
					snakenextdir = 1;
					lockleft = true;
				}
			} else {
				lockleft = false;
				if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
					if (!lockright) {
						snakenextdir = -1;
						lockright = true;
					}
				} else {
					lockright = false;
				}
			}
			if ((!gameover) && (!win)) {
				movesnake ();
				setcamera ();
			} else {
				holdon = 1f;
			}
		}
	}

	void OnDestroy() {
		Destroy (boardquad);
		int i, j;
		for (i = 0; i < width; i++)
			for (j = 0; j < height; j++) {
				if (boardgobj [i, j] != null) {
					Destroy (boardgobj [i, j]);
				}
			}
		for (i = 0; i < snake.Count; i++) {
			Destroy (snake [i].gobj);
		}
	}

	void getui() {
		stageandlifetext = GameObject.Find ("Canvas/GameUp/StageAndLife").GetComponent<UnityEngine.UI.Text> ();
		scoretext = GameObject.Find ("Canvas/GameUp/Score").GetComponent<UnityEngine.UI.Text> ();
		factortext = GameObject.Find ("Canvas/GameUp/Factor").GetComponent<UnityEngine.UI.Text> ();
		barfullrect = GameObject.Find ("Canvas/GameUp/BarFull").GetComponent<RectTransform> ();
		clockimg = GameObject.Find ("Canvas/GameClock/Clock").GetComponent<UnityEngine.UI.Image> ();
		stageandlifetext.text = "Stage: " + UIClick.level.ToString () + "\nLife: " + UIClick.life.ToString ();
		sprt = new Sprite[8];
		int i;
		for (i = 0; i < 8; i++) {
			Texture2D img = Resources.Load ("Clock"+i.ToString()) as Texture2D;
			sprt[i] = Sprite.Create (img, new Rect (0, 0, img.width, img.height), new Vector2 (0.5f, 0.5f));
		}
		clockimg.sprite = sprt[0];
		updateui ();
		updatebar ();
	}

	void updateui() {
		if (UIClick.score >= 99999999) {
			scoretext.text = "99999999";
		} else {
			scoretext.text = (UIClick.score / 10000000).ToString () + (UIClick.score / 1000000 % 10).ToString () + (UIClick.score / 100000 % 10).ToString () + (UIClick.score / 10000 % 10).ToString () + (UIClick.score / 1000 % 10).ToString () + (UIClick.score / 100 % 10).ToString () + (UIClick.score / 10 % 10).ToString () + (UIClick.score % 10).ToString ();
		}
		factortext.text = factor.ToString ();
	}

	void updatebar() {
		barfullrect.localPosition = new Vector3 (-85f * eatennum / needtoeat, -16f, 0);
		barfullrect.localScale = new Vector3 (1.0f - 1.0f * eatennum / needtoeat, 1f, 1f);
	}
}
