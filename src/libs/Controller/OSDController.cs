using System;
using System.Drawing;
using System.Windows.Forms;
using WinOSD;

namespace FanCtrl
{
    class OSDController
    {
        private static WinOSDForm mOverlayForm;
        private static Font mFont = new Font("Consolas", 12f, FontStyle.Bold);

        public static void Init()
        {
            if (mOverlayForm == null)
            {
                mOverlayForm = new WinOSDForm();
            }
        }

        public static void Dispose()
        {
            if (mOverlayForm != null)
            {
                mOverlayForm.Close();
                mOverlayForm = null;
            }
        }

        public static bool update(string osdString)
        {
            if (mOverlayForm == null)
                return false;

            try
            {
                Color displayColor = Color.White; // Default to white
                bool colorFound = false;

                var osdManager = OSDManager.getInstance();
                for (int i = 0; i < osdManager.getGroupCount(); i++)
                {
                    var group = osdManager.getGroup(i);
                    if (group != null)
                    {
                        if (group.IsColor)
                        {
                            displayColor = group.Color;
                            colorFound = true;
                            break;
                        }
                        
                        // Check items in group
                        for (int j = 0; j < group.ItemList.Count; j++)
                        {
                            var item = group.ItemList[j];
                            if (item.IsColor)
                            {
                                displayColor = item.Color;
                                colorFound = true;
                                break;
                            }
                        }
                        if (colorFound) break;
                    }
                }

                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].BeginInvoke(new Action(() =>
                    {
                        if (mOverlayForm != null)
                        {
                            if (string.IsNullOrEmpty(osdString))
                            {
                                mOverlayForm.Close();
                            }
                            else
                            {
                                mOverlayForm.Show(osdString, new Point(50, 50), 255, displayColor, mFont, 0, AnimateMode.Blend, 0);
                            }
                        }
                    }));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSDController.update : " + ex.Message);
                return false;
            }
        }

        public static void release()
        {
            update("");
        }
    }
}
