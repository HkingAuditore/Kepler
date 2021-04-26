namespace CustomUI.Tutorial
{
    class SatelliteTutorialUI : TutorialUI
    {
        public int       challengeIndex;
        public LabModeUI labModeUI;

        public override bool CheckAwake()
        {
            return this.awakePanel.activeSelf && labModeUI.satelliteSceneIndex == this.challengeIndex;
        }
    }
}