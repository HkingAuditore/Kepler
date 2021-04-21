using System.Collections.Generic;
using XmlSaver;

namespace Quiz
{
    public class QuizBaseStruct : SceneBaseStruct<QuizAstralBody>
    {
        public string   quizName;
        public QuizType quizType;

        public static QuizBaseStruct FromSceneBaseStruct(SceneBaseStruct<Quiz.QuizAstralBody> sceneBaseStruct)
        {
            QuizBaseStruct quizBaseStruct = new QuizBaseStruct();
            quizBaseStruct.astralBodyStructList = sceneBaseStruct.astralBodyStructList.ConvertAll(q =>
                                                                                                  {
                                                                                                      QuizAstralBodyDataDict
                                                                                                         .FromAstralBodyDict(q);
                                                                                                  } );
        }
    }
}