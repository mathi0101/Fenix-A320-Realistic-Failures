using System.Windows.Controls;

namespace RealFenixFailures.UI.Views.Panels;
/// <summary>
/// Interaction logic for LogPanel.xaml
/// LOG EN TIEMPO REAL con scroll automático hacia el último mensaje.
/// </summary>
public partial class LogPanel : UserControl {
    public LogPanel() {
        InitializeComponent();
    }
    /// <summary>
    /// Mantiene el scroll pegado al final cuando se agregan nuevas entradas de log.
    /// Solo autoscrollea si el contenido creció (ExtentHeightChange > 0), de modo que
    /// el usuario todavía puede desplazarse manualmente hacia arriba sin ser interrumpido.
    /// </summary>
    private void LogScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
        if (e.ExtentHeightChange > 0 && sender is ScrollViewer viewer) {
            viewer.ScrollToEnd();
        }
    }
}
