namespace Dreamteck.Splines.Editor
{
    public class ComputerEditorModule : EditorModule
    {
        public    EmptySplineHandler           repaintHandler;
        protected SplineComputer               spline;
        public    SplineEditorBase.UndoHandler undoHandler;

        public ComputerEditorModule(SplineComputer spline)
        {
            this.spline = spline;
        }

        protected override void RecordUndo(string title)
        {
            base.RecordUndo(title);
            if (undoHandler != null) undoHandler(title);
        }

        protected override void Repaint()
        {
            base.Repaint();
            if (repaintHandler != null) repaintHandler();
        }
    }
}