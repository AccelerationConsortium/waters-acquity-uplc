using System.Reflection;
using System.Windows.Forms;

namespace AstraClient
{
   public partial class AboutDialog : Form
   {
      public AboutDialog()
      {
         InitializeComponent();
         
         product.Text = Application.ProductName + " " + Application.ProductVersion;

         Assembly assembly = Assembly.GetExecutingAssembly();
         AssemblyCopyrightAttribute copyrightAttr;
         copyrightAttr = (AssemblyCopyrightAttribute)AssemblyCopyrightAttribute.GetCustomAttribute(
            assembly, typeof(AssemblyCopyrightAttribute));
         copyright.Text = copyrightAttr.Copyright;
      }
   }
}