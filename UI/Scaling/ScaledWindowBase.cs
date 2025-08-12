using System.Windows;
using WWXMapEditor.UI.Scaling;

namespace WWXMapEditor.UI.Scaling
{
    public class ScaledWindowBase : Window
    {
        public ScaledWindowBase()
        {
            Loaded += (_, __) => ScaleService.Instance.Attach(this);
        }
    }
}