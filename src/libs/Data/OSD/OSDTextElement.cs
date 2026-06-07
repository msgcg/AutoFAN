using System.Drawing;

namespace FanCtrl
{
    public class OSDTextElement
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public bool IsBold { get; set; }

        public OSDTextElement(string text, Color color, int fontSize = 12, string fontName = "Consolas")
        {
            Text = text;
            Color = color;
            FontSize = fontSize;
            FontName = fontName;
            IsBold = true;
        }
    }
}
