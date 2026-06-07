using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FanCtrl
{
    public class OSDGroup
    {
        public string Name { get; set; }

        public bool IsColor { get; set; } = false;

        public Color Color { get; set; } = Color.White;

        public int FontSize { get; set; } = 12;
        public string FontName { get; set; } = "Consolas";

        public int Digit { get; set; } = 5;

        private List<OSDItem> mItemList = new List<OSDItem>();
        public List<OSDItem> ItemList
        {
            get { return mItemList; }
        }

        public OSDGroup()
        {

        }

                public System.Collections.Generic.List<FanCtrl.OSDTextElement> getOSDElements(int maxNameLength)
        {
            var elements = new System.Collections.Generic.List<FanCtrl.OSDTextElement>();

            // Name
            string name = Name;
            for (int i = name.Length; i < maxNameLength; i++)
            {
                name += " ";
            }

            Color groupColor = this.IsColor ? this.Color : Color.White;
            elements.Add(new FanCtrl.OSDTextElement(name, groupColor, this.FontSize, this.FontName));

            // item list
            for (int i = 0; i < mItemList.Count; i++)
            {
                var item = mItemList[i];
                var element = item.getOSDElement(Digit);
                if (element != null)
                {
                    if (!item.IsColor) 
                    {
                        element.Color = groupColor; // Inherit group color
                    }
                    element.Text = new string(' ', maxNameLength) + element.Text; // Pad item
                    elements.Add(element);
                }
            }

            return elements;
        }

        public OSDGroup clone()
        {
            var group = new OSDGroup();
            group.Name = this.Name;
            group.IsColor = this.IsColor;
            group.Color = Color.FromArgb(this.Color.R, this.Color.G, this.Color.B);
            group.FontSize = this.FontSize;
            group.Digit = this.Digit;

            for (int i = 0; i < mItemList.Count; i++)
                group.ItemList.Add(mItemList[i].clone());

            return group;
        }
    }
}



