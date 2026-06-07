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
        private static Color mColor = Color.FromArgb(255, 128, 0);

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
                                mOverlayForm.Show(osdString, new Point(50, 50), 255, mColor, mFont, 0, AnimateMode.Blend, 0);
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
