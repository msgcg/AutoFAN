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

        public static bool update(System.Collections.Generic.List<FanCtrl.OSDTextElement> elements)
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
                            if (elements == null || elements.Count == 0)
                            {
                                mOverlayForm.Hide();
                            }
                            else
                            {
                                mOverlayForm.Show(elements, new Point(50, 50), 255, 0, AnimateMode.Blend, 0);
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
            update(null);
        }
    }
}


