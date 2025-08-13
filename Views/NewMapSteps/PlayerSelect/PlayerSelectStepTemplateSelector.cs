using System.Windows;
using System.Windows.Controls;
using WWXMapEditor.ViewModels;

namespace WWXMapEditor.Views.NewMapSteps.PlayerSelect
{
    // Selects which DataTemplate to use for the PlayerSelect mini-steps based on StepIndex.
    public class PlayerSelectStepTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? Step1Template { get; set; }
        public DataTemplate? Step2Template { get; set; }
        public DataTemplate? Step3Template { get; set; }
        public DataTemplate? Step4Template { get; set; }
        public DataTemplate? Step5Template { get; set; }
        public DataTemplate? Step6Template { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is PlayerSelectStepViewModel vm)
            {
                switch (vm.StepIndex)
                {
                    case 1: return Step1Template ?? Step2Template ?? Step3Template ?? Step4Template ?? Step5Template ?? Step6Template;
                    case 2: return Step2Template ?? Step1Template;
                    case 3: return Step3Template ?? Step1Template;
                    case 4: return Step4Template ?? Step1Template;
                    case 5: return Step5Template ?? Step1Template;
                    case 6: return Step6Template ?? Step1Template;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}