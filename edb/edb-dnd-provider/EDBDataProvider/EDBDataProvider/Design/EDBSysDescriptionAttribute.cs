using System;
using System.Reflection;
using System.Resources;
using System.ComponentModel;
using System.Windows.Forms;

namespace EnterpriseDB.EDBClient.Design {
  [AttributeUsage(AttributeTargets.Property)]
  internal class EDBSysDescriptionAttribute : DescriptionAttribute{
    private bool replaced = false;
    private ResourceManager resman;

    public EDBSysDescriptionAttribute(string ResourceName, Type ResourceClass) : base(ResourceName){
      this.resman = new ResourceManager(ResourceClass);
    }

    public override string Description {
      get {
        if(this.replaced == false){
          base.DescriptionValue = this.resman.GetString(base.Description);
          this.replaced = true;
        }
        return base.Description;
      }
    }
  }
}
