using System.Collections.Generic;
using XmlSaver;

namespace Quiz
{
    /// <summary>
    /// 问题信息存储
    /// </summary>
    public class QuizBaseStruct : SceneBaseStruct<QuizAstralBody>
    {
        public QuizType quizType;

        // public static QuizBaseStruct FromSceneBaseStruct(SceneBaseStruct<Quiz.QuizAstralBody> sceneBaseStruct)
        // {
        //     QuizBaseStruct quizBaseStruct = new QuizBaseStruct();
        //     quizBaseStruct.astralBodyStructList = sceneBaseStruct.astralBodyStructList;
        //     quizBaseStruct.quizName             = "UnNamed";
        //     quizBaseStruct.quizType             = QuizType.Mass;
        //     return quizBaseStruct;
        // }
    }
}