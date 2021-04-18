using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [Serializable]
    public struct S_Vector3
    {
        public float x, y, z;


        public S_Vector3(Vector3 input)
        {
            x = input.x;
            y = input.y;
            z = input.z;
        }

        public Vector3 vector
        {
            get => new Vector3(x, y, z);
            set { }
        }
    }

    [Serializable]
    public struct S_Color
    {
        public float r, g, b, a;

        public S_Color(Color input)
        {
            r = input.r;
            g = input.g;
            b = input.b;
            a = input.a;
        }

        public Color color
        {
            get => new Color(r, g, b, a);
            set { }
        }
    }

    [Serializable]
    public class SplinePreset
    {
        private static string path = "";

        public bool        isClosed;
        public string      filename    = "";
        public string      name        = "";
        public string      description = "";
        public Spline.Type type        = Spline.Type.Bezier;


        [NonSerialized] protected SplineComputer computer;

        [NonSerialized] public Vector3 origin = Vector3.zero;

        private S_Color[]          points_color    = new S_Color[0];
        private S_Vector3[]        points_normal   = new S_Vector3[0];
        private S_Vector3[]        points_position = new S_Vector3[0];
        private float[]            points_size     = new float[0];
        private S_Vector3[]        points_tangent2 = new S_Vector3[0];
        private S_Vector3[]        points_tanget   = new S_Vector3[0];
        private SplinePoint.Type[] points_type     = new SplinePoint.Type[0];

        public SplinePreset(SplinePoint[] p, bool closed, Spline.Type t)
        {
            points_position = new S_Vector3[p.Length];
            points_tanget   = new S_Vector3[p.Length];
            points_tangent2 = new S_Vector3[p.Length];
            points_normal   = new S_Vector3[p.Length];
            points_color    = new S_Color[p.Length];
            points_size     = new float[p.Length];
            points_type     = new SplinePoint.Type[p.Length];
            for (var i = 0; i < p.Length; i++)
            {
                points_position[i] = new S_Vector3(p[i].position);
                points_tanget[i]   = new S_Vector3(p[i].tangent);
                points_tangent2[i] = new S_Vector3(p[i].tangent2);
                points_normal[i]   = new S_Vector3(p[i].normal);
                points_color[i]    = new S_Color(p[i].color);
                points_size[i]     = p[i].size;
                points_type[i]     = p[i].type;
            }

            isClosed = closed;
            type     = t;
            path     = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
        }

        public SplinePoint[] points
        {
            get
            {
                var p = new SplinePoint[points_position.Length];
                for (var i = 0; i < p.Length; i++)
                {
                    p[i].type     = points_type[i];
                    p[i].position = points_position[i].vector;
                    p[i].tangent  = points_tanget[i].vector;
                    p[i].tangent2 = points_tangent2[i].vector;
                    p[i].normal   = points_normal[i].vector;
                    p[i].color    = points_color[i].color;
                    p[i].size     = points_size[i];
                }

                return p;
            }
        }

        public void Save(string name)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var formatter = new BinaryFormatter();
            var file      = File.Create(path + "/" + name + ".dsp");
            formatter.Serialize(file, this);
            file.Close();
        }

        public static void Delete(string filename)
        {
            path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
            if (!Directory.Exists(path))
            {
                Debug.LogError("Directory " + path + " does not exist");
                return;
            }

            File.Delete(path + "/" + filename);
        }

        public static SplinePreset[] LoadAll()
        {
            path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
            if (!Directory.Exists(path))
            {
                Debug.LogError("Directory " + path + " does not exist");
                return null;
            }

            var files   = Directory.GetFiles(path, "*.dsp");
            var presets = new SplinePreset[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                var bf = new BinaryFormatter();
                bf.Binder = new DSP2Binder();
                var file = File.Open(files[i], FileMode.Open);
                presets[i]          = (SplinePreset) bf.Deserialize(file);
                presets[i].filename = new FileInfo(files[i]).Name;
                file.Close();
            }

            return presets;
        }
    }

    internal class DSP2Binder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            typeName     = typeName.Replace("Dreamteck.Splines", "Dreamteck.Splines.Editor");
            assemblyName = assemblyName.Replace("Dreamteck.Splines", "Dreamteck.Splines.Editor");
            return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        }
    }
}