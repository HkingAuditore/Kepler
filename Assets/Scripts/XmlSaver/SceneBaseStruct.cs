using System.Collections.Generic;
using Quiz;
using SpacePhysic;

namespace XmlSaver
{
    public class SceneBaseStruct<T> where T : IAstralBodyDictale
    {
        public List<T>  astralBodyStructList;

    }
}