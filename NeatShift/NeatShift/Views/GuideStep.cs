using System;

namespace NeatShift.Views
{
    public class GuideStep
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public GuideStep()
        {
        }

        public GuideStep(int number, string title, string description)
        {
            Number = number;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
} 