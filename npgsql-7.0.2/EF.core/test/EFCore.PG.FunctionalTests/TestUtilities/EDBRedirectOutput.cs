using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    // EnterpriseDB Team
    // Helper used to redirect console messages to xUnit ITestOutputHelper
    // Use as : Console.SetOut(new EDBRedirectOutput(testOutputHelper));
    public class EDBRedirectOutput : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public EDBRedirectOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding { get; } // set some if required

        public override void WriteLine(string value)
        {
            try
            {
                _output.WriteLine(value);
            }
            catch (Exception)
            {
                //Console.WriteLine(value);
            }
            
        }
    }
}
