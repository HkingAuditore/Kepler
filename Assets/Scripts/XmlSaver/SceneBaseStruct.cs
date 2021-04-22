using System.Collections.Generic;
using Quiz;
using SpacePhysic;

namespace XmlSaver
{
    public class SceneBaseStruct<T> where T : AstralBody
    {
        public string                      sceneName;
        public List<AstralBodyDataDict<T>> astralBodyStructList;

    }
}