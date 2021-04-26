using System.Collections.Generic;
using Quiz;
using SpacePhysic;

namespace XmlSaver
{
    /// <summary>
    /// 场景信息存储
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SceneBaseStruct<T> where T : AstralBody
    {
        public string                      sceneName;
        public List<AstralBodyDataDict<T>> astralBodyStructList;

    }
}